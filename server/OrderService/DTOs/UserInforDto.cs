namespace OrderService.DTOs
{
    public class UserInforDto
    { 
        public required string UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public long TotalSpent { get; set; }
    }
}
