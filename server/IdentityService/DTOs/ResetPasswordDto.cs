using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
