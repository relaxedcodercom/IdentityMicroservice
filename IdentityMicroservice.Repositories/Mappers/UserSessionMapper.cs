using IdentityMicroservice.Domain.Entities;
using IdentityMicroservice.Repositories.Contracts.Mappers;

namespace IdentityMicroservice.Repositories.Mappers
{
    public class UserSessionMapper : IUserSessionMapper
    {
        public DataAccess.UserSession FillFromDomain(UserSession userSession)
        {
            if (userSession is null)
            {
                throw new ArgumentNullException(nameof(userSession));
            }

            return new DataAccess.UserSession
            {
                SessionId = userSession.SessionId,
                UserId = userSession.UserId,
                RefreshToken = userSession.RefreshToken,
                ExpirationDate = userSession.ExpirationDate,
                IpAddress = userSession.IpAddress
            };
        }
    }
}
