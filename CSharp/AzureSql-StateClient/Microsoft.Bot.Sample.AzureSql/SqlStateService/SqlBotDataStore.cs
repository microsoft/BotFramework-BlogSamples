using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.AzureSql.SqlStateService
{
    public class SqlBotDataStore : IBotDataStore<BotData>
    {
        string _connectionStringName { get; set; }
        public SqlBotDataStore(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        async Task<BotData> IBotDataStore<BotData>.LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            using (var context = new SqlBotDataContext(_connectionStringName))
            {
                try
                {
                    SqlBotDataEntity entity = SqlBotDataEntity.GetSqlBotDataEntity(key, botStoreType, context);

                    if (entity == null)
                        return new BotData(eTag: String.Empty, data: null);
                    
                    return new BotData(entity.ETag, entity.GetData());
                }               
                catch (Exception ex)
                {
                    throw new HttpException((int)HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        async Task IBotDataStore<BotData>.SaveAsync(IAddress key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            SqlBotDataEntity entity = new SqlBotDataEntity(botStoreType, key.BotId, key.ChannelId, key.ConversationId, key.UserId, botData.Data)
            {
                ETag = botData.ETag,
                ServiceUrl = key.ServiceUrl
            };

            using (var context = new SqlBotDataContext(_connectionStringName))
            {
                try
                {
                    if (String.IsNullOrEmpty(entity.ETag))
                    {
                        context.BotData.Add(entity);
                    }
                    else if (entity.ETag == "*")
                    {
                        var foundData = SqlBotDataEntity.GetSqlBotDataEntity(key, botStoreType, context);
                        if (botData.Data != null)
                        {
                            if (foundData == null)
                                context.BotData.Add(entity);
                            else
                            {
                                foundData.Data = entity.Data;
                                foundData.ServiceUrl = entity.ServiceUrl;
                            }
                        }
                        else
                        {
                            if (foundData != null)
                                context.BotData.Remove(foundData);
                        }
                    }
                    else
                    {
                        var foundData = SqlBotDataEntity.GetSqlBotDataEntity(key, botStoreType, context);
                        if (botData.Data != null)
                        {
                            if (foundData == null)
                                context.BotData.Add(entity);
                            else
                            {
                                foundData.Data = entity.Data;
                                foundData.ServiceUrl = entity.ServiceUrl;
                                foundData.ETag = entity.ETag;
                            }
                        }
                        else
                        {
                            if (foundData != null)
                                context.BotData.Remove(foundData);
                        }
                    }
                    context.SaveChanges();
                }
                catch (System.Data.SqlClient.SqlException err)
                {
                    throw new HttpException((int)HttpStatusCode.InternalServerError, err.Message);
                }
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }
    }
}