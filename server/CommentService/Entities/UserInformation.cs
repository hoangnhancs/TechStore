using Shared.Core.EF.Domain.Entities;

namespace CommentService.Entities
{
    public class UserInformation : BaseEntity<int>
    {
        public required string UserId { get; set; }
        public required string DisplayName { get; set; }
        public string? ImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public UserInformation() : base() { }
    }
}
