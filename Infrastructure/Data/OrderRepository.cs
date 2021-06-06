using Core.Entities.OrderAggregate;
using Core.Entities.OrderNeo4j;
using Core.Interfaces;
using Core.Specifications;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class OrderRepository : IOrderRepository
    {
        public IGraphClient _client;
        public OrderRepository(IGraphClient client)
        {
            _client = client;
        }
        public async Task<Order> Add(Order order)
        {

            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return null;

            var newOrder = new OrderNeo4j(order);
           
            //var proList = new List<ProductNeo4j>();
            var containRelationship = new ContainRelationshipNeo4j();
            var method = order.DeliveryMethod;

            //await _client.Cypher.Merge("(order:ORDER {id: {uuid})")
            //                    .OnCreate()
            //                    .Set("order = {newOrder}")
            //                    .WithParams(new { newOrder.id, newOrder})
            //                    .Match("(pro:PRODUCT) {id: {uuid}}")
            //                    .Merge("()")


            var orderID = await _client.Cypher.Create("(ord:ORDER)")
                                             .Set("ord = $order")
                                             .WithParams(new { order = newOrder})
                                             .Return(ord => ord.As<OrderNeo4j>())
                                             .ResultsAsync;


            var tempOrder = orderID.SingleOrDefault();


            if (tempOrder != null)
            {
                var tempId = tempOrder.uuid;
                var ret = await _client.Cypher.Match("(ord:ORDER), (method:DELIVERYMETHOD)")
                                              .Where("ord.uuid = $uuid and method.Id = $Id")
                                              .Create("(ord)-[r:USE]->(method)")
                                              .WithParams(new { uuid = tempId, Id = order.DeliveryMethod.Id })
                                              .Return(r => r.As<string>())
                                              .ResultsAsync;

                if (ret == null)
                {
                    await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->()")
                                        .Where<OrderNeo4j>(ord => ord.uuid == tempId)
                                        .Delete("ord, r")
                                        .ExecuteWithoutResultsAsync();
                    return null;
                }

                var retUser = await _client.Cypher.Match("(ord:ORDER), (user:USER)")
                                                  .Where("ord.uuid = $uuid and user.BuyerEmail = $email")
                                                  .Create("(ord)<-[r:ARRANGE]-(user)")
                                                  .WithParams(new { uuid = tempId, user = order.BuyerEmail })
                                                  .Return(r => r.As<string>())
                                                  .ResultsAsync;

                if (retUser == null)
                {
                    await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->()")
                                        .Where<OrderNeo4j>(ord => ord.uuid == tempId)
                                        .Delete("ord, r")
                                        .ExecuteWithoutResultsAsync();
                    return null;
                }


                foreach (var i in order.OrderItems)
                {
                    var tempProID = i.ItemOrdered.ProductItemId;
                    var result1 = await _client.Cypher.Match("(ord:ORDER)", "(pro: PRODUCT)")
                                                      .Where("ord.uuid = $uuid and pro.ProductItemId = $mongoID")
                                                      .Create("(ord)-[r:CONTAIN]->(pro)")
                                                      .Set("r = $rela")
                                                      .WithParams(new
                                                      {
                                                          uuid = tempId,
                                                          mongoID = tempProID,
                                                          rela = new ContainRelationshipNeo4j(i.Quantity)
                                                      })
                                                      .Return(r => r.As<ContainRelationshipNeo4j>().Quantity)
                                                      .ResultsAsync;
                    if (result1 == null)
                    {
                        await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->()")
                                            .Where<OrderNeo4j>(ord => ord.uuid == tempId)
                                            .Delete("ord, r")
                                            .ExecuteWithoutResultsAsync();
                        return null;
                    }
                }
            }
                                       
            return order;
        }

        public async Task<Order> GetEntityWithSpec(ISpecification<OrderNeo4j> spec)
        {
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return null;

            var result = await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->(pro), (ord:ORDER)-[:USE]->(method), (ord:ORDER)<-[:ARRANGE]-(user:USER)")
                                                   .Where(spec.Criteria)
                                                   .Return((ord, pro, r, method, user) => new {
                                                       Order = ord.As<OrderNeo4j>(),
                                                       Method = method.As<DeliveryMethod>(),
                                                       ContainRelated = r.CollectAs<ContainRelationshipNeo4j>(),
                                                       Pros = pro.CollectAs<ProductNeo4j>(),
                                                       Users = user.As<UserNeo4j>()
                                                   })
                                                   .ResultsAsync;
            var temp = result.Single();
            if (result == null || temp == null)
                return null;

            var ret = mappingOrder(temp.Order, temp.Method, temp.Users, temp.ContainRelated.ToList(), temp.Pros.ToList());
            return ret;
        }

        public async Task<IReadOnlyList<Order>> ListAsync(ISpecification<OrderNeo4j> spec)
        {
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return null;


            var result = await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->(pro), (ord:ORDER)-[:USE]->(method), (user:USER)-[:ARRANGE]->(ord:ORDER)")
                                                   .Where(spec.Criteria)
                                                   .Return((ord, pro, r, method, user) => new {
                                                       Order = ord.As<OrderNeo4j>(),
                                                       Method = method.As<DeliveryMethod>(),
                                                       ContainRelated = r.CollectAs<ContainRelationshipNeo4j>(),
                                                       Pros = pro.CollectAs<ProductNeo4j>(),
                                                       Users = user.As<UserNeo4j>()
                                                   })
                                                   .ResultsAsync;
            var temp = result.ToList();
            if (result == null || temp == null)
                return null;

            var ret = new List<Order>();
            foreach (var i in temp)
            {
                var tempItem = mappingOrder(i.Order,i.Method,i.Users, i.ContainRelated.ToList(), i.Pros.ToList());
                ret.Add(tempItem);
            }
            return ret;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return null;

            var result = await _client.Cypher.Match("(n:DELIVERYMETHOD)")
                                                   .Return(n => n.As<DeliveryMethod>())
                                                   .ResultsAsync;
            return result.ToList();
        }

        private Order mappingOrder(OrderNeo4j sourceOrder,
                                  DeliveryMethod deliveryMethod,
                                  UserNeo4j user,
                                  List<ContainRelationshipNeo4j> sourceListRelatation,
                                  List<ProductNeo4j> sourceListPro)
        {
            var ret = new Order();
            ret.id = sourceOrder.uuid;
            ret.BuyerEmail = sourceOrder.BuyerEmail;
            ret.DeliveryMethod = deliveryMethod;
            ret.OrderDate = sourceOrder.OrderDate;
            ret.PaymentIntentId = sourceOrder.PaymentIntentId;
            ret.ShipToAddress = new Address();
            ret.ShipToAddress.FirstName = user.FirstName;
            ret.ShipToAddress.LastName = user.LastName;
            ret.ShipToAddress.State = user.State;
            ret.ShipToAddress.Street = user.Street;
            ret.ShipToAddress.ZipCode = user.ZipCode;

            OrderStatus tempStatus;
            if (Enum.TryParse<OrderStatus>(sourceOrder.Status, out tempStatus))
            {
                ret.Status = tempStatus;
            }
            ret.Subtotal = sourceOrder.Subtotal;
            ret.Total = sourceOrder.Total;
            ret.OrderItems = new List<OrderItem>();

            //var tempProList = temp.Pros.ToList();
            //var tempRelationshipList = temp.ContainRelated.ToList();
            var tempItemsList = new List<OrderItem>();
            for (int i = 0; i < sourceListPro.Count(); i++)
            {
                var tempItem = new OrderItem();
                tempItem.id = sourceListPro[i].uuid;
                tempItem.ItemOrdered = new ProductItemOrdered(sourceListPro[i].ProductItemId, sourceListPro[i].ProductName, sourceListPro[i].PictureUrl);
                tempItem.Price = sourceListPro[i].Price;
                tempItem.Quantity = sourceListRelatation[i].Quantity;

                tempItemsList.Add(tempItem);
            }
            ret.OrderItems = tempItemsList;

            return ret;
        }


    }
}
