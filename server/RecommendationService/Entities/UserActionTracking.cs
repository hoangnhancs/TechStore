using Microsoft.CodeAnalysis;
using Shared.Core.EF.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecommendationService.Entities
{
    public class UserActionTracking : BaseEntity<int>
    {
        public required string UserId { get; set; }
        public required string ProductId { get; set; }
        [Column(TypeName = "varchar(30)")]
        public UserActionType ActionType { get; set; }
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;
        public string? MetaData { get; set; }
        public enum UserActionType
        {
            View,
            AddToCart,
            Purchase
        }
    }
}
