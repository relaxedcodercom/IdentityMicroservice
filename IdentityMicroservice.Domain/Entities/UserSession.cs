using FluentValidation;
using IdentityMicroservice.Domain.Validators;

namespace IdentityMicroservice.Domain.Entities
{
    public class UserSession
    {
        private static UserSessionValidator userSessionValidator = new UserSessionValidator();

        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string IpAddress { get; set; }

        public void ValidateAndThrow()
        {
            userSessionValidator.ValidateAndThrow(this);
        }
    }
}
