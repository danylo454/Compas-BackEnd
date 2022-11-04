using Compass.Data.Data.ViewModels.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Validation.Users
{
    public class ChangePasswordValidation : AbstractValidator<ChangePasswordVM>
    {
        public ChangePasswordValidation()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.OldPassword).NotEmpty().MinimumLength(6);
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).NotEmpty().MinimumLength(6);
            //RuleFor(x => x.NewPassword).Equal(c => c.ConfirmPassword).WithMessage("Passwords do not match");
        }
    }
}
