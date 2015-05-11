namespace Abp.Authorization.Users
{
    public enum AbpLoginResultType
    {
        Success = 1,

        InvalidUserNameOrEmailAddress,
        InvalidPassword,
        UserIsNotActive,
        UserEmailIsNotConfirmed,

        InvalidTenancyName,
        NoTenancyNameProvided,
        TenantIsNotActive,
        UserNotWithTenant,
        UserNotActiveWithTenant,
        UserNeedsToChooseTenant
    }
}