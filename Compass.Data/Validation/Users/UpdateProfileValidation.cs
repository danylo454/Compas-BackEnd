using Compass.Data.Data.ViewModels.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Validation.Users
{
    public class UpdateProfileValidation: AbstractValidator<UpdateProfileVM>
    {
        public UpdateProfileValidation()
        {
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.Surname).NotEmpty();
            RuleFor(r => r.Email).NotEmpty();
            RuleFor(r => r.Phone).NotEmpty();
        }
    }
}
