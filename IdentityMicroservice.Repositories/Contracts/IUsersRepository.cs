using IdentityMicroservice.Domain.Entities;

namespace IdentityMicroservice.Repositories.Contracts
{
    public interface IUsersRepository
    {
        Task<User> Add(User user);
        Task<bool> EmailExists(string email);
        Task<User> GetByEmailWithPassword(string email);
        Task<User> GetByUsernameWithPassword(string username);
        Task<bool> UsernameExists(string username);
    }
}