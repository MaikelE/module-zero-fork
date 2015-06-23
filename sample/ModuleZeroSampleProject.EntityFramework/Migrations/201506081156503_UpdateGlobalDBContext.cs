namespace ModuleZeroSampleProject.Migrations.GlobalDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateGlobalDBContext : DbMigration
    {
        public override void Up()
        {
            //AddForeignKey("dbo.AbpUsersTenants", "UserId", "dbo.AbpUsers", "Id", cascadeDelete: true);
            DropForeignKey("dbo.AbpUsersTenants", "TenantId", "dbo.AbpTenants");
            DropIndex("dbo.AbpUsersTenants", new[] { "TenantId" });
            AlterTableAnnotations(
                "dbo.AbpUsersTenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        TenantId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeleterUserId = c.Long(),
                        DeletionTime = c.DateTime(),
                        LastModificationTime = c.DateTime(),
                        LastModifierUserId = c.Long(),
                        CreationTime = c.DateTime(nullable: false),
                        CreatorUserId = c.Long(),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_UserTenant_MustHaveTenant",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
            AddColumn("dbo.AbpUsers", "AuthenticationSource", c => c.String(maxLength: 64));
            AddColumn("dbo.AbpUsers", "LastTenantId", c => c.Int());
            AlterColumn("dbo.AbpUserLogins", "LoginProvider", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.AbpUserLogins", "ProviderKey", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.AbpUsersTenants", "TenantId", c => c.Int());
            CreateIndex("dbo.AbpUsersTenants", "TenantId");
            
            AddForeignKey("dbo.AbpUsersTenants", "TenantId", "dbo.AbpTenants", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AbpUsersTenants", "TenantId", "dbo.AbpTenants");
            DropForeignKey("dbo.AbpUsersTenants", "UserId", "dbo.AbpUsers");
            DropIndex("dbo.AbpUsersTenants", new[] { "TenantId" });
            AlterColumn("dbo.AbpUsersTenants", "TenantId", c => c.Int(nullable: false));
            AlterColumn("dbo.AbpUserLogins", "ProviderKey", c => c.String());
            AlterColumn("dbo.AbpUserLogins", "LoginProvider", c => c.String());
            DropColumn("dbo.AbpUsers", "LastTenantId");
            DropColumn("dbo.AbpUsers", "AuthenticationSource");
            AlterTableAnnotations(
                "dbo.AbpUsersTenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        TenantId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeleterUserId = c.Long(),
                        DeletionTime = c.DateTime(),
                        LastModificationTime = c.DateTime(),
                        LastModifierUserId = c.Long(),
                        CreationTime = c.DateTime(nullable: false),
                        CreatorUserId = c.Long(),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_UserTenant_MustHaveTenant",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            CreateIndex("dbo.AbpUsersTenants", "TenantId");
            AddForeignKey("dbo.AbpUsersTenants", "TenantId", "dbo.AbpTenants", "Id", cascadeDelete: true);
        }
    }
}
