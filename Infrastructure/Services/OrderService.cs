using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Neo4jClient;
using Microsoft.AspNetCore;
using Core.Entities.OrderNeo4j;
using System.Text.Json;
using System;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IGraphClient _client;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;

        public OrderService(IBasketRepository basketRepo, IGraphClient client,
            IPaymentService paymentService, IProductService productService,
            IOrderRepository orderRepository)
        {
            _paymentService = paymentService;
            _client = client;
            _basketRepo = basketRepo;
            _productService = productService;
            _orderRepository = orderRepository;
        }
        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {
            //get basket from repo
            var basket = await _basketRepo.GetBasketAsync(basketId);
            //get item from produtc repo
            var items = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                var index = item.PictureUrl.IndexOf("images/products/", StringComparison.Ordinal);
                var pictureUrl = item.PictureUrl[index..];
                var productItem = await _productService.GetByIdAsync(item.Id);
                var itemOrdered = new ProductItemOrdered(productItem._id, productItem.Name, pictureUrl);
                var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
                items.Add(orderItem);
            }

            //get delivery method from repo
            //var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            //calc subtotal
            var subtotal = items.Sum(item => item.Quantity * item.Price);

            // check  to see if order exists
            // var spec = new OrderByPaymentIntentIdWithItemsSpecification(basket.PaymentIntentId);
            // var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
            // if (existingOrder != null)
            // {
            //     _unitOfWork.Repository<Order>().Delete(existingOrder);
            //     await _paymentService.CreateOrUpdatePaymentIntent(basket.PaymentIntentId);
            // }

            //create order 
            var order = new Order(items, buyerEmail, shippingAddress, null, subtotal);
            // var order = new Order();
            order.Total = subtotal;//+ order.DeliveryMethod.Price;

            await _orderRepository.Add(order);
            //_unitOfWork.Repository<Order>().Add(order);

            // save to db
            //var result = await _unitOfWork.Complete();
            //if (result <= 0) return null;

            //  delete basket

            // return db
            return order;

        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            //await _client.ConnectAsync();
            //if (!_client.IsConnected)
            //    return null;
            //var result = await _client.Cypher.Match(@"n:DELIVERYMETHOD")
            //                                 .Return(n => n.As<DeliveryMethod>())
            //                                 .ResultsAsync;

            //return result.ToList();
            return await _orderRepository.GetDeliveryMethodsAsync();
        }

        public async Task<Order> GetOrderByIdAsync(string id, string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);
            return await _orderRepository.GetEntityWithSpec(spec);

            ////return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
            //return new Order();
            //await _client.ConnectAsync();
            //if (!_client.IsConnected)
            //    return null;


            //var result = await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->(pro)")
            //                                       .Where<OrderNeo4j>(e => e.id == id && e.BuyerEmail == buyerEmail)
            //                                       .Return((ord, pro, r) => new {
            //                                           Order = ord.As<OrderNeo4j>(),
            //                                           ContainRelated = r.CollectAs<ContainRelationshipNeo4j>(),
            //                                           Pros = pro.CollectAs<ProductNeo4j>()
            //                                       })
            //                                       .ResultsAsync;
            //var temp = result.Single();
            //if (result == null)
            //    return null;

            //var ret = new Order();
            //ret.id = temp.Order.id;
            //ret.BuyerEmail = temp.Order.BuyerEmail;
            //ret.DeliveryMethod = JsonSerializer.Deserialize<DeliveryMethod>(temp.Order.DeliveryMethod);
            //ret.OrderDate = temp.Order.OrderDate;
            //ret.PaymentIntentId = temp.Order.PaymentIntentId;
            //ret.ShipToAddress = JsonSerializer.Deserialize<Address>(temp.Order.ShipToAddress);
            //OrderStatus tempStatus;
            //if (Enum.TryParse<OrderStatus>(temp.Order.Status, out tempStatus))
            //{
            //    ret.Status = tempStatus;
            //}
            //ret.Subtotal = temp.Order.Subtotal;
            //ret.Total = temp.Order.Total;
            //ret.OrderItems = new List<OrderItem>();

            //var tempProList = temp.Pros.ToList();
            //var tempRelationshipList = temp.ContainRelated.ToList();
            //var tempItemsList = new List<OrderItem>();
            //for (int i = 0; i < tempProList.Count(); i++)
            //{
            //    var tempItem = new OrderItem();
            //    tempItem.id = tempProList[i].id;
            //    tempItem.ItemOrdered = JsonSerializer.Deserialize<ProductItemOrdered>(tempProList[i].ItemOrdered);
            //    tempItem.Price = tempProList[i].Price;
            //    tempItem.Quantity = tempRelationshipList[i].Quantity;

            //    tempItemsList.Add(tempItem);
            //}
            //ret.OrderItems = tempItemsList;


            //return ret;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);
            //return await _unitOfWork.Repository<Order>().ListAsync(spec);
            return await _orderRepository.ListAsync(spec);
        }
    }
}