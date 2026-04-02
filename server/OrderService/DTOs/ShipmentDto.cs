using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Entities;

namespace OrderService.DTOs
{
    public class ShipmentDto
    {
        public required string CarrierName { get; set; }
        public required string TrackingNumber { get; set; }
        public decimal ShippingCost { get; set; }
        public ShipmentStatus Status { get; private set; } = ShipmentStatus.Preparing; 
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }
        public string? Note { get; set; }
        public required string OrderId { get; set; }
    }
}