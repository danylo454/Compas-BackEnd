using Compass.Data.Data.ViewModels.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Data.Validation.Users
{
    public class EditUserValidation : AbstractValidator<EditUserVM>
    {
        public EditUserValidation()
        {
            RuleFor(r => r.Name).NotEmpty().MinimumLength(3);
            RuleFor(r => r.Surname).NotEmpty().MinimumLength(3);
        }
    }
}
