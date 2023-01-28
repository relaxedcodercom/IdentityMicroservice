using FluentValidation;
using IdentityMicroservice.Domain.Entities;

namespace IdentityMicroservice.Domain.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.Username)
                .NotEmpty()
                .Length(1, 256);
            RuleFor(user => user.Email)
                .NotEmpty()
                .Length(1, 256)
                .EmailAddress();
        }
    }
}
