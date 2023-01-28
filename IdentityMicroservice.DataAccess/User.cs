using System.ComponentModel.DataAnnotations;

namespace IdentityMicroservice.DataAccess
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
    }
}
