namespace Microsoft.Bot.Sample.AzureSql.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSetup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SqlBotDataEntities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BotStoreType = c.Int(nullable: false),
                        BotId = c.String(),
                        ChannelId = c.String(maxLength: 200),
                        ConversationId = c.String(maxLength: 200),
                        UserId = c.String(maxLength: 200),
                        Data = c.Binary(),
                        ETag = c.String(),
                        ServiceUrl = c.String(),
                    Timestamp = c.DateTimeOffset(nullable: false, precision: 7, defaultValueSql: "GETUTCDATE()"),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.BotStoreType, t.ChannelId, t.ConversationId }, name: "idxStoreChannelConversation")
                .Index(t => new { t.BotStoreType, t.ChannelId, t.ConversationId, t.UserId }, name: "idxStoreChannelConversationUser")
                .Index(t => new { t.BotStoreType, t.ChannelId, t.UserId }, name: "idxStoreChannelUser");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SqlBotDataEntities", "idxStoreChannelUser");
            DropIndex("dbo.SqlBotDataEntities", "idxStoreChannelConversationUser");
            DropIndex("dbo.SqlBotDataEntities", "idxStoreChannelConversation");
            DropTable("dbo.SqlBotDataEntities");
        }
    }
}
