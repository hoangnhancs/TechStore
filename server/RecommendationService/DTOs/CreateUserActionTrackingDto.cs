namespace RecommendationService.DTOs
{
    public class CreateUserActionTrackingDto
    {
        public required string ProductId { get; set; }
        public required string ActionType { get; set; }
    }
}
