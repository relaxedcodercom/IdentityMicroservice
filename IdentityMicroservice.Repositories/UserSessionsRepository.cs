using IdentityMicroservice.DataAccess;
using IdentityMicroservice.Repositories.Contracts;
using IdentityMicroservice.Repositories.Contracts.Mappers;
using Microsoft.EntityFrameworkCore;
using UserSession = IdentityMicroservice.Domain.Entities.UserSession;

namespace IdentityMicroservice.Repositories
{
    public class UserSessionsRepository : BaseRepository, IUserSessionsRepository
    {
        private readonly IUserSessionMapper userSessionMapper;

        public UserSessionsRepository(DataContext context, IUserSessionMapper userSessionMapper) : base(context)
        {
            this.userSessionMapper = userSessionMapper;
        }

        public async Task<bool> IsRefreshTokenValidForUser(string refreshToken, Guid userId)
        {
            return await DataContext.UserSessions.AnyAsync(userSession => userSession.UserId == userId &&
                    userSession.RefreshToken == refreshToken && userSession.ExpirationDate > DateTime.UtcNow);
        }

        public async Task Add(UserSession userSession)
        {
            await DataContext.UserSessions.AddAsync(userSessionMapper.FillFromDomain(userSession));

            await DataContext.SaveChangesAsync();
        }

        public async Task DeleteByRefreshTokenAndUser(string refreshToken, Guid userId)
        {
            var userSession = await DataContext.UserSessions.FirstOrDefaultAsync(userSession => userSession.RefreshToken == refreshToken
                    && userSession.UserId == userId);

            if (userSession != null)
            {
                DataContext.UserSessions.Remove(userSession);
                await DataContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserId(Guid userId)
        {
            var expiredUserSessions = await DataContext.UserSessions.Where(userSession => userSession.UserId == userId).ToListAsync();

            if (expiredUserSessions.Any())
            {
                DataContext.UserSessions.RemoveRange(expiredUserSessions);
                await DataContext.SaveChangesAsync();
            }
        }

        public async Task DeleteExpiredByUserId(Guid userId)
        {
            var expiredUserSessions = await DataContext.UserSessions.Where(userSession => userSession.UserId == userId &&
                    userSession.ExpirationDate < DateTime.UtcNow).ToListAsync();

            if (expiredUserSessions.Any())
            {
                DataContext.UserSessions.RemoveRange(expiredUserSessions);
                await DataContext.SaveChangesAsync();
            }
        }
    }
}
