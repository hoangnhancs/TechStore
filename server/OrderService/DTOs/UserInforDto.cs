namespace OrderService.DTOs
{
    public class UserInforDto
    { 
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public string? UserDisplayName { get; set; }
        public required string UserEmail { get; set; }
        public string? UserImageUrl { get; set; }
        public long TotalSpent { get; set; }
    }
}
