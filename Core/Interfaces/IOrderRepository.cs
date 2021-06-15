using Core.Entities.OrderAggregate;
using Core.Entities.OrderNeo4j;

using Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> Add(Order order);
        Task<IReadOnlyList<Order>> ListAsync(ISpecification<OrderNeo4j> spec);
        Task<Order> GetEntityWithSpec(ISpecification<OrderNeo4j> spec);
        Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
        Task<bool> CheckUserBuyProduct(string buyerEmail, string productID);
    }
}
