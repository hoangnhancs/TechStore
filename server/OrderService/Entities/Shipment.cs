using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace OrderService.Entities
{
    public class Shipment : BaseEntity<int>
    {
        public required string CarrierName { get; set; }
        public required string TrackingNumber { get; set; }
        public decimal ShippingCost { get; set; }
        [Column(TypeName = "varchar(20)")]
        public ShipmentStatus Status { get; private set; } = ShipmentStatus.Preparing; 
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }
        public string? Note { get; set; }
        public required string OrderId { get; set; }
        public Order? Order { get; set; }
        public void UpdateStatus(ShipmentStatus newStatus)
        {
            // Add any business rules for status transitions here
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum ShipmentStatus
    {
        Preparing,      // đang chuẩn bị giao
        Picked,         // shipper đã lấy hàng
        InTransit,      // đang vận chuyển
        Delivered,      // giao thành công
        Failed          // giao thất bại
    }
}