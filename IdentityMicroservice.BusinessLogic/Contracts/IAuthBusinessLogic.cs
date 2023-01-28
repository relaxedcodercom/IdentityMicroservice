using IdentityMicroservice.Domain.Entities;

namespace IdentityMicroservice.BusinessLogic.Contracts
{
    public interface IAuthBusinessLogic
    {
        Task<User> ProcessAuthenticate(LoginCredentials loginCredentials);
        Task ProcessLogout(string refreshToken, Guid userId);
        Task ProcessLogoutEverywhere(Guid userId);
        Task<TokenModel> ProcessRefreshToken(TokenModel tokenModel);
        Task<User> ProcessRegisterUser(RegisterUser registerUser);
    }
}