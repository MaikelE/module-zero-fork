namespace ModuleZeroSampleProject.Migrations.App
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateConfigurationModuleZeroSample : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        QuestionId = c.Int(nullable: false),
                        IsAccepted = c.Boolean(nullable: false),
                        CreationTime = c.DateTime(nullable: false),
                        CreatorUserId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User_Ref", t => t.CreatorUserId)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId)
                .Index(t => t.CreatorUserId);
            
            CreateTable(
                "dbo.User_Ref",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        IsDeleted = c.Boolean(nullable: false),
                        DeleterUserId = c.Long(),
                        DeletionTime = c.DateTime(),
                        LastModificationTime = c.DateTime(),
                        LastModifierUserId = c.Long(),
                        CreationTime = c.DateTime(nullable: false),
                        CreatorUserId = c.Long(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_User_Ref_SoftDelete", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User_Ref", t => t.CreatorUserId)
                .ForeignKey("dbo.User_Ref", t => t.DeleterUserId)
                .ForeignKey("dbo.User_Ref", t => t.LastModifierUserId)
                .Index(t => t.DeleterUserId)
                .Index(t => t.LastModifierUserId)
                .Index(t => t.CreatorUserId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Text = c.String(),
                        VoteCount = c.Int(nullable: false),
                        AnswerCount = c.Int(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        CreationTime = c.DateTime(nullable: false),
                        CreatorUserId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User_Ref", t => t.CreatorUserId)
                .Index(t => t.CreatorUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Questions", "CreatorUserId", "dbo.User_Ref");
            DropForeignKey("dbo.Answers", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.Answers", "CreatorUserId", "dbo.User_Ref");
            DropForeignKey("dbo.User_Ref", "LastModifierUserId", "dbo.User_Ref");
            DropForeignKey("dbo.User_Ref", "DeleterUserId", "dbo.User_Ref");
            DropForeignKey("dbo.User_Ref", "CreatorUserId", "dbo.User_Ref");
            DropIndex("dbo.Questions", new[] { "CreatorUserId" });
            DropIndex("dbo.User_Ref", new[] { "CreatorUserId" });
            DropIndex("dbo.User_Ref", new[] { "LastModifierUserId" });
            DropIndex("dbo.User_Ref", new[] { "DeleterUserId" });
            DropIndex("dbo.Answers", new[] { "CreatorUserId" });
            DropIndex("dbo.Answers", new[] { "QuestionId" });
            DropTable("dbo.Questions");
            DropTable("dbo.User_Ref",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_User_Ref_SoftDelete", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Answers");
        }
    }
}
