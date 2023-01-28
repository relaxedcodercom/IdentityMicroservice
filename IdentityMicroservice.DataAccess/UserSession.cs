using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityMicroservice.DataAccess
{
    public class UserSession
    {
        [Key]
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string IpAddress { get; set; }
    }
}
