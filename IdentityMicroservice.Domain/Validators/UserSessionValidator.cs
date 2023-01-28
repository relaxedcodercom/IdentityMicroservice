using FluentValidation;
using IdentityMicroservice.Domain.Entities;

namespace IdentityMicroservice.Domain.Validators
{
    public class UserSessionValidator : AbstractValidator<UserSession>
    {
        public UserSessionValidator()
        {
            RuleFor(session => session.IpAddress)
                .Length(0, 64);
        }
    }
}
