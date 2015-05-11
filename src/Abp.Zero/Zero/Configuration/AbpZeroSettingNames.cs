namespace Abp.Zero.Configuration
{
    public static class AbpZeroSettingNames
    {
        public static class UserManagement
        {
            /// <summary>
            /// "Abp.Zero.UserManagement.IsEmailConfirmationRequiredForLogin".
            /// </summary>
            public const string IsEmailConfirmationRequiredForLogin = "Abp.Zero.UserManagement.IsEmailConfirmationRequiredForLogin";

            /// <summary>
            /// "Abp.Zero.UserManagement.IsEmailConfirmationRequiredForLogin".
            /// </summary>
            public const string IsTwoFactorRequiredForLogin = "Abp.Zero.UserManagement.IsTwoFactorRequiredForLogin";

            /// <summary>
            /// "Abp.Zero.UserManagement.IsEmailConfirmationRequiredForLogin".
            /// </summary>
            public const string IsTwoFactorEnabledForLogin = "Abp.Zero.UserManagement.IsTwoFactorEnabledForLogin";

        }

        public static class TenantManagement
        {

            /// <summary>
            /// "Abp.Zero.TenantManagement.IsTenantNameRequiredWithLogin".
            /// </summary>
            public const string IsTenantNameRequiredWithLogin = "Abp.Zero.TenantManagement.IsTenantNameRequiredWithLogin";

            /// <summary>
            /// "Abp.Zero.TenantManagement.IsTenantNameRequiredWithLogin".
            /// </summary>
            public const string HostDisplayName = "Abp.Zero.TenantManagement.HostDisplayName";
        }
    }
}