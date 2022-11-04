using AutoMapper;
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Data.AutoMapper
{
    public class AutoMapperUserProfile : Profile
    {
        public AutoMapperUserProfile()
        {
            CreateMap<AppUser, RegisterUserVM>();
            CreateMap<RegisterUserVM, AppUser>().ForMember(dst => dst.UserName, act => act.MapFrom(src => src.Email));
            CreateMap<AppUser, AllUsersVM>();
            CreateMap<UpdateProfileVM, AppUser>();
            CreateMap<AppUser, EditUserVM>();
            CreateMap<EditUserVM, AppUser>();
        }
    }
}
