namespace IdentityMicroservice.Services.Contracts.Google
{
    public interface IRecaptchaV2Service
    {
        Task<bool> ValidateReCaptchaResponse(string response);
    }
}