using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.DTOs
{
    public class OrderStatusHistoryWithShipmentDto
    {
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();
        public ShipmentDto? Shipment { get; set; }
    }
}