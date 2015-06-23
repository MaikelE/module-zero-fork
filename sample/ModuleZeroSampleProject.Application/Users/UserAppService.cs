using System.Collections.Generic;
using System.Linq;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ModuleZeroSampleProject.Users.Dto;
using System.Linq;

namespace ModuleZeroSampleProject.Users
{
    public class UserAppService : ApplicationService, IUserAppService
    {
        private readonly UserManager _userManager;

        public UserAppService(UserManager userManager)
        {
            _userManager = userManager;
        }

        public ListResultOutput<UserDto> GetUsers()
        {
            return new ListResultOutput<UserDto>
                   {
                       Items = _userManager.Users.Where(u => u.UserInTenants.Any(ut => ut.TenantId == AbpSession.TenantId)).ToList().MapTo<List<UserDto>>(),
                       //Items = _userManager
                       //    .GetAllList(u => u.UserInTenants.Any(ut => ut.TenantId == CurrentSession.TenantId)) //TODO: DataFilter?
                       //    .MapTo<List<UserDto>>()
                   };
        }
    }
}