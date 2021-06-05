using Core.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Core.Entities.OrderNeo4j
{
    public class OrderNeo4j
    {
        public OrderNeo4j()
        {

        }
        public OrderNeo4j(Order order)
        {
            uuid = Guid.NewGuid().ToString();
            BuyerEmail = order.BuyerEmail;
            OrderDate = order.OrderDate;
            ShipToAddress = JsonSerializer.Serialize<Address>(order.ShipToAddress);
            Subtotal = order.Subtotal;
            Status = order.Status.ToString();
            PaymentIntentId = order.PaymentIntentId;
            Total = order.Total;
        }
        public string uuid { get; set; }
        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string ShipToAddress { get; set; }
        public decimal Subtotal { get; set; }
        public string Status { get; set; }
        public string PaymentIntentId { get; set; }
        public decimal Total { get; set; }
        //public List<ProductNeo4j> pros { get; set; }
    }

}
