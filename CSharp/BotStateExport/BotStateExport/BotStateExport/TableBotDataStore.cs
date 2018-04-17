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
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.Web;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Azure
{

    /// <summary>
    /// <see cref="IBotDataStore{T}"/> Implementation using Azure Storage Table 
    /// </summary>
    public class TableBotDataStore : IBotDataStore<BotData>
    {
        private static HashSet<string> checkedTables = new HashSet<string>();

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the azure table storage.
        /// </summary>
        /// <param name="connectionString">The storage connection string.</param>
        /// <param name="tableName">The name of table.</param>
        public TableBotDataStore(string connectionString, string tableName = "botdata")
            : this(CloudStorageAccount.Parse(connectionString), tableName)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the azure table storage.
        /// </summary>
        /// <param name="storageAccount">The storage account.</param>
        /// <param name="tableName">The name of table.</param>
        public TableBotDataStore(CloudStorageAccount storageAccount, string tableName = "botdata")
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            this.Table = tableClient.GetTableReference(tableName);

            lock (checkedTables)
            {
                if (!checkedTables.Contains(tableName))
                {
                    this.Table.CreateIfNotExists();
                    checkedTables.Add(tableName);
                }
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the azure table storage.
        /// </summary>
        /// <param name="table">The cloud table.</param>
        public TableBotDataStore(CloudTable table)
        {
            this.Table = table;
        }

        /// <summary>
        /// The <see cref="CloudTable"/>.
        /// </summary>
        public CloudTable Table { get; private set; }

        async Task<BotData> IBotDataStore<BotData>.LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            var entityKey = BotDataEntity.GetEntityKey(key, botStoreType);
            try
            {
                var result = await this.Table.ExecuteAsync(TableOperation.Retrieve<BotDataEntity>(entityKey.PartitionKey, entityKey.RowKey));
                BotDataEntity entity = (BotDataEntity)result.Result;
                if (entity == null)
                    // empty record ready to be saved
                    return new BotData(eTag: String.Empty, data: null);

                // return botdata 
                return new BotData(entity.ETag, entity.GetData());
            }
            catch (StorageException err)
            {
                throw new HttpException(err.RequestInformation.HttpStatusCode, err.RequestInformation.HttpStatusMessage);
            }
        }

        async Task IBotDataStore<BotData>.SaveAsync(IAddress key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            var entityKey = BotDataEntity.GetEntityKey(key, botStoreType);
            BotDataEntity entity = new BotDataEntity(key.BotId, key.ChannelId, key.ConversationId, key.UserId, botData.Data)
            {
                ETag = botData.ETag
            };
            entity.PartitionKey = entityKey.PartitionKey;
            entity.RowKey = entityKey.RowKey;

            try
            {
                if (String.IsNullOrEmpty(entity.ETag))
                    await this.Table.ExecuteAsync(TableOperation.Insert(entity));
                else if (entity.ETag == "*")
                {
                    if (botData.Data != null)
                        await this.Table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
                    else
                        await this.Table.ExecuteAsync(TableOperation.Delete(entity));
                }
                else
                {
                    if (botData.Data != null)
                        await this.Table.ExecuteAsync(TableOperation.Replace(entity));
                    else
                        await this.Table.ExecuteAsync(TableOperation.Delete(entity));
                }
            }
            catch (StorageException err)
            {
                if ((HttpStatusCode)err.RequestInformation.HttpStatusCode == HttpStatusCode.Conflict)
                    throw new HttpException((int)HttpStatusCode.PreconditionFailed, err.RequestInformation.HttpStatusMessage);

                throw new HttpException(err.RequestInformation.HttpStatusCode, err.RequestInformation.HttpStatusMessage);
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }

    }

    internal class EntityKey
    {
        public EntityKey(string partition, string row)
        {
            PartitionKey = partition;
            RowKey = row;
        }

        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }

    }

    internal class BotDataEntity : TableEntity
    {
        private static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public BotDataEntity()
        {
        }

        internal BotDataEntity(string botId, string channelId, string conversationId, string userId, object data)
        {
            this.BotId = botId;
            this.ChannelId = channelId;
            this.ConversationId = conversationId;
            this.UserId = userId;
            this.Data = Serialize(data);
        }

        private byte[] Serialize(object data)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            using (var streamWriter = new StreamWriter(stream))
            {
                var serializedJSon = JsonConvert.SerializeObject(data, serializationSettings);
                streamWriter.Write(serializedJSon);
                streamWriter.Close();
                stream.Close();
                return cmpStream.ToArray();
            }
        }

        private object Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gz))
            {
                return JsonConvert.DeserializeObject(streamReader.ReadToEnd());
            }
        }


        internal static EntityKey GetEntityKey(IAddress key, BotStoreType botStoreType)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    return new EntityKey($"{key.ChannelId}:conversation", key.ConversationId.SanitizeForAzureKeys());

                case BotStoreType.BotUserData:
                    return new EntityKey($"{key.ChannelId}:user", key.UserId.SanitizeForAzureKeys());

                case BotStoreType.BotPrivateConversationData:
                    return new EntityKey($"{key.ChannelId}:private", $"{key.ConversationId.SanitizeForAzureKeys()}:{key.UserId.SanitizeForAzureKeys()}");

                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }

        internal ObjectT GetData<ObjectT>()
        {
            return ((JObject)Deserialize(this.Data)).ToObject<ObjectT>();
        }

        internal object GetData()
        {
            return Deserialize(this.Data);
        }

        public string BotId { get; set; }

        public string ChannelId { get; set; }

        public string ConversationId { get; set; }

        public string UserId { get; set; }

        public byte[] Data { get; set; }
    }

}