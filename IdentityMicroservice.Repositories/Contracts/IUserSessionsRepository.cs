using IdentityMicroservice.Domain.Entities;

namespace IdentityMicroservice.Repositories.Contracts
{
    public interface IUserSessionsRepository
    {
        Task Add(UserSession userSession);
        Task DeleteByRefreshTokenAndUser(string refreshToken, Guid userId);
        Task DeleteByUserId(Guid userId);
        Task DeleteExpiredByUserId(Guid userId);
        Task<bool> IsRefreshTokenValidForUser(string refreshToken, Guid userId);
    }
}