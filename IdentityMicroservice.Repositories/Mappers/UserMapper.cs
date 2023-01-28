using IdentityMicroservice.Domain.Entities;
using IdentityMicroservice.Repositories.Contracts.Mappers;

namespace IdentityMicroservice.Repositories.Mappers
{
    public class UserMapper : IUserMapper
    {
        public DataAccess.User FillFromDomain(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return new DataAccess.User
            {
                UserId = user.UserId,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                Email = user.Email
            };
        }

        public User FillFromDataAccess(DataAccess.User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return new User
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };
        }
    }
}
