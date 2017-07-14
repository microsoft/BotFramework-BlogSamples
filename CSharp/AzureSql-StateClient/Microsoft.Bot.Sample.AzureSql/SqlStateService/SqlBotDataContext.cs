using System.Data.Entity;


namespace Microsoft.Bot.Sample.AzureSql.SqlStateService
{
    public class SqlBotDataContext : DbContext
    {
        public SqlBotDataContext()
            : this("BotDataContextConnectionString")
        {
        }
        public SqlBotDataContext(string connectionStringName)
            : base(connectionStringName)
        {
        }
        public DbSet<SqlBotDataEntity> BotData { get; set; }
    }
}