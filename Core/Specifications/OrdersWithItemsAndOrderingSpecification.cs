using System;
using System.Linq.Expressions;
using Core.Entities.OrderAggregate;
using Core.Entities.OrderNeo4j;

namespace Core.Specifications
{
    public class OrdersWithItemsAndOrderingSpecification: BaseSpecification<OrderNeo4j>
    {
        public OrdersWithItemsAndOrderingSpecification(string email ) 
            : base(ord =>ord.BuyerEmail == email)
        {
            //AddIncluded(o => o.OrderItems);
            //AddIncluded(o => o.DeliveryMethod);
        }
        public OrdersWithItemsAndOrderingSpecification(string id, string email)
            : base(ord => ord.uuid == id && ord.BuyerEmail ==email)
        {
            //AddIncluded(o => o.OrderItems);
            //AddIncluded(o => o.DeliveryMethod);
        }
    }
}