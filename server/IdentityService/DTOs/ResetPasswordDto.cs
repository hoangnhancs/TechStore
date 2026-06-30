using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string ResetCode { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
