namespace IdentityMicroservice.Domain.Entities
{
    public class TokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
    }
}
