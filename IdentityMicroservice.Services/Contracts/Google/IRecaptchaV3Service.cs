namespace IdentityMicroservice.Services.Contracts.Google
{
    public interface IRecaptchaV3Service
    {
        Task<bool> ValidateReCaptchaResponse(string response);
    }
}