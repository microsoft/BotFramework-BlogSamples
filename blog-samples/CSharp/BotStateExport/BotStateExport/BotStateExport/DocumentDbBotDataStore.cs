// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{

    /// <summary>
    /// <see cref="IBotDataStore{T}"/> Implementation using Azure DocumentDb
    /// </summary>
    public class DocumentDbBotDataStore : IBotDataStore<BotData>
    {
       

        private const string entityKeyParameterName = "@entityKey";

        private static readonly TimeSpan MaxInitTime = TimeSpan.FromSeconds(5);

        private readonly IDocumentClient documentClient;
        private readonly string databaseId;
        private readonly string collectionId;

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the Azure DocumentDb.
        /// </summary>
        /// <param name="documentClient">The DocumentDb client to use.</param>
        /// <param name="databaseId">The name of the DocumentDb database to use.</param>
        /// <param name="collectionId">The name of the DocumentDb collection to use.</param>
        public DocumentDbBotDataStore(IDocumentClient documentClient, string databaseId = "botdb", string collectionId = "botcollection")
        {
            SetField.NotNull(out this.databaseId, nameof(databaseId), databaseId);
            SetField.NotNull(out this.collectionId, nameof(collectionId), collectionId);

            this.documentClient = documentClient;
            this.databaseId = databaseId;
            this.collectionId = collectionId;

            CreateDatabaseIfNotExistsAsync().GetAwaiter().GetResult();
            CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the Azure DocumentDb.
        /// </summary>
        /// <param name="serviceEndpoint">The service endpoint to use to create the client.</param>
        /// <param name="authKey">The authorization key or resource token to use to create the client.</param>
        /// <param name="databaseId">The name of the DocumentDb database to use.</param>
        /// <param name="collectionId">The name of the DocumentDb collection to use.</param>
        /// <remarks>The service endpoint can be obtained from the Azure Management Portal. If you
        /// are connecting using one of the Master Keys, these can be obtained along with
        /// the endpoint from the Azure Management Portal If however you are connecting as
        /// a specific DocumentDB User, the value passed to authKeyOrResourceToken is the
        /// ResourceToken obtained from the permission feed for the user.
        /// Using Direct connectivity, wherever possible, is recommended.</remarks>
        public DocumentDbBotDataStore(Uri serviceEndpoint, string authKey, string databaseId = "botdb", string collectionId = "botcollection")
            : this(new DocumentClient(serviceEndpoint, authKey), databaseId, collectionId) { }

        async Task<BotData> IBotDataStore<BotData>.LoadAsync(IAddress key, BotStoreType botStoreType,
            CancellationToken cancellationToken)
        {
            try
            {
                var entityKey = DocDbBotDataEntity.GetEntityKey(key, botStoreType);

                // query to retrieve the document if it exists
                SqlQuerySpec querySpec = new SqlQuerySpec(
                                                queryText: $"SELECT * FROM {collectionId} b WHERE (b.id = {entityKeyParameterName})",
                                                parameters: new SqlParameterCollection()
                                                {
                                                    new SqlParameter(entityKeyParameterName, entityKey)
                                                });
                var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
                var query = documentClient.CreateDocumentQuery(collectionUri, querySpec)
                                          .AsDocumentQuery();
                var feedResponse = await query.ExecuteNextAsync<Document>(CancellationToken.None);
                Document document = feedResponse.FirstOrDefault();

                if (document != null)
                {
                    // The document, of type IDynamicMetaObjectProvider, has a dynamic nature, 
                    // similar to DynamicTableEntity in Azure storage. When casting to a static type, properties that exist in the static type will be 
                    // populated from the dynamic type.
                    DocDbBotDataEntity entity = (dynamic)document;
                    return new BotData(document?.ETag, entity?.Data);
                }
                else
                {
                    // the document does not exist in the database, return an empty BotData object
                    return new BotData(string.Empty, null);
                }
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode.HasValue && e.StatusCode.Value == HttpStatusCode.NotFound)
                {
                    return new BotData(string.Empty, null);
                }

                throw new HttpException(e.StatusCode.HasValue ? (int)e.StatusCode.Value : 0, e.Message, e);
            }
        }

        async Task IBotDataStore<BotData>.SaveAsync(IAddress key, BotStoreType botStoreType, BotData botData,
            CancellationToken cancellationToken)
        {
            try
            {
                var requestOptions = new RequestOptions()
                {
                    AccessCondition = new AccessCondition()
                    {
                        Type = AccessConditionType.IfMatch,
                        Condition = botData.ETag
                    }
                };

                var entity = new DocDbBotDataEntity(key, botStoreType, botData);
                var entityKey = DocDbBotDataEntity.GetEntityKey(key, botStoreType);

                if (string.IsNullOrEmpty(botData.ETag))
                {
                    await documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), entity, requestOptions);
                }
                else if (botData.ETag == "*")
                {
                    if (botData.Data != null)
                    {
                        await documentClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), entity, requestOptions);
                    }
                    else
                    {
                        await documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, entityKey), requestOptions);
                    }
                }
                else
                {
                    if (botData.Data != null)
                    {
                        await documentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, entityKey), entity, requestOptions);
                    }
                    else
                    {
                        await documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, entityKey), requestOptions);
                    }
                }
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode.HasValue && e.StatusCode.Value == HttpStatusCode.Conflict)
                {
                    throw new HttpException((int)HttpStatusCode.PreconditionFailed, e.Message, e);
                }

                throw new HttpException(e.StatusCode.HasValue ? (int)e.StatusCode.Value : 0, e.Message, e);
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await documentClient.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await documentClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await documentClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseId),
                        new DocumentCollection { Id = collectionId });
                }
                else
                {
                    throw;
                }
            }
        }
    }

    internal class BotDataDocDbKey
    {
        public BotDataDocDbKey(string partition, string row)
        {
            PartitionKey = partition;
            RowKey = row;
        }

        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }

    }

    internal class DocDbBotDataEntity
    {
        internal const int MAX_KEY_LENGTH = 254;
        public DocDbBotDataEntity() { }

        internal DocDbBotDataEntity(IAddress key, BotStoreType botStoreType, BotData botData)
        {
            this.Id = GetEntityKey(key, botStoreType);
            this.BotId = key.BotId;
            this.ChannelId = key.ChannelId;
            this.ConversationId = key.ConversationId;
            this.UserId = key.UserId;
            this.Data = botData.Data;
        }

        public static string GetEntityKey(IAddress key, BotStoreType botStoreType)
        {
            string entityKey;
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    entityKey = $"{key.ChannelId}:conversation{key.ConversationId.SanitizeForAzureKeys()}";
                    return TruncateEntityKey(entityKey);

                case BotStoreType.BotUserData:
                    entityKey = $"{key.ChannelId}:user{key.UserId.SanitizeForAzureKeys()}";
                    return TruncateEntityKey(entityKey);

                case BotStoreType.BotPrivateConversationData:
                    entityKey = $"{key.ChannelId}:private{key.ConversationId.SanitizeForAzureKeys()}:{key.UserId.SanitizeForAzureKeys()}";
                    return TruncateEntityKey(entityKey);

                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }

        private static string TruncateEntityKey(string entityKey)
        {
            if (entityKey.Length > MAX_KEY_LENGTH)
            {
                var hash = entityKey.GetHashCode().ToString("x");
                entityKey = entityKey.Substring(0, MAX_KEY_LENGTH - hash.Length) + hash;
            }

            return entityKey;
        }

        internal static class StringExtensions
        {
           
        }

[JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "botId")]
        public string BotId { get; set; }

        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }
}