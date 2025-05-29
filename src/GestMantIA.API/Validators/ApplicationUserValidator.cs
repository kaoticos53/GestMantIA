using FluentValidation;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.API.Validators
{
    public class ApplicationUserValidator : AbstractValidator<ApplicationUser>
    {
        public ApplicationUserValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio")
                .Length(3, 50).WithMessage("El nombre de usuario debe tener entre 3 y 50 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio")
                .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido")
                .MaximumLength(100).WithMessage("El correo electrónico no puede tener más de 100 caracteres");
        }
    }
}
