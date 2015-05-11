using System.Collections.Generic;
using Abp.Configuration;
using Abp.Localization;

namespace Abp.Zero.Configuration
{
    public class AbpZeroSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new List<SettingDefinition>
                   {
                       new SettingDefinition(
                           AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin,
                           "false",
                           new FixedLocalizableString("Is email confirmation required for login."),
                           scopes: SettingScopes.Application | SettingScopes.Tenant
                           ),
                        new SettingDefinition(
                           AbpZeroSettingNames.TenantManagement.IsTenantNameRequiredWithLogin,
                           "false",
                           new FixedLocalizableString("Is tenant name required for login."),
                           scopes: SettingScopes.Application
                           ),
                        new SettingDefinition(
                           AbpZeroSettingNames.TenantManagement.HostDisplayName,
                           "Host application",
                           new FixedLocalizableString("Host displayname."),
                           scopes: SettingScopes.Application
                           ),
                        new SettingDefinition(
                           AbpZeroSettingNames.UserManagement.IsTwoFactorEnabledForLogin,
                           "false",
                           new FixedLocalizableString("Two factor login"),
                           scopes: SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User
                           ),
                        new SettingDefinition(
                           AbpZeroSettingNames.UserManagement.IsTwoFactorRequiredForLogin,
                           "false",
                           new FixedLocalizableString("Host displayname."),
                           scopes: SettingScopes.Application | SettingScopes.Tenant
                           )

                   };
        }
    }
}
