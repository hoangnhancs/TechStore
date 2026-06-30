using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class VerifyEmailDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
