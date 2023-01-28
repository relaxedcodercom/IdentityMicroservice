namespace IdentityMicroservice.Services.Contracts
{
    public interface ICryptoService
    {
        string GetPasswordHash(string password);
        bool VerifyPasswordHash(string hashedPassword, string password);
    }
}