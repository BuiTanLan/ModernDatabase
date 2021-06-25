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
        private readonly IGraphClient _client;
        public OrderRepository(IGraphClient client)
        {
            _client = client;
        }
        public async Task<Order> Add(Order order)
        {

            await _client.ConnectAsync();
            if (!_client.IsConnected) throw new Exception();

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

            try
            {
                if (tempOrder != null)
                {
                    var tempId = tempOrder.uuid;
                    var ret = await _client.Cypher.Match("(ord:ORDER), (method:DELIVERYMETHOD)")
                                                  .Where("ord.uuid = $uuid and method.Id = $Id")
                                                  .Create("(ord)-[r:USE]->(method)")
                                                  .WithParams(new { uuid = tempId, Id = order.DeliveryMethod.Id })
                                                  .Return(r => r.As<string>())
                                                  .ResultsAsync;

                    if (ret.SingleOrDefault() == null)
                    {
                        await _client.Cypher.Match("(ord:ORDER)-[r]->()")
                                            .Where<OrderNeo4j>(ord => ord.uuid == tempId)
                                            .Delete("ord, r")
                                            .ExecuteWithoutResultsAsync();
                        return null;
                    }

                    var retUser = await _client.Cypher.Match("(ord:ORDER), (user:USER)")
                                                      .Where("ord.uuid = $uuid and user.BuyerEmail = $email")
                                                      .Create("(ord)<-[r:ARRANGE]-(user)")
                                                      .WithParams(new { uuid = tempId, email = order.BuyerEmail })
                                                      .Return(r => r.As<string>())
                                                      .ResultsAsync;

                    if (retUser.SingleOrDefault() == null)
                    {
                        await _client.Cypher.Match("(ord:ORDER)-[r]->()")
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
                                                          .Return(r => r.As<ContainRelationshipNeo4j>())
                                                          .ResultsAsync;

                        if (result1.SingleOrDefault() == null)
                        {
                            await _client.Cypher.Match("(ord:ORDER)-[r]->()")
                                                .Where<OrderNeo4j>(ord => ord.uuid == tempId)
                                                .Delete("ord, r")
                                                .ExecuteWithoutResultsAsync();
                            return null;
                        }

                        var result2 = await _client.Cypher.Match("(user:USER)", "(pro: PRODUCT)")
                                                          .Where("user.BuyerEmail = $email and pro.ProductItemId = $mongoID")
                                                          .Merge("(user)-[r:BUY]->(pro)")
                                                          .WithParams(new
                                                          {
                                                              email = order.BuyerEmail,
                                                              mongoID = tempProID
                                                          })
                                                          .Return(r => r.As<string>())
                                                          .ResultsAsync;
                        if (result2.SingleOrDefault() == null)
                        {
                            await _client.Cypher.Match("(ord:ORDER)-[r]->()")
                                                .Where<OrderNeo4j>(ord => ord.uuid == tempId)
                                                .Delete("ord, r")
                                                .ExecuteWithoutResultsAsync();
                            return null;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                await _client.Cypher.Match("(ord:ORDER)-[r]->()")
                                    .Where<OrderNeo4j>(ord => ord.uuid == tempOrder.uuid)
                                    .Delete("ord, r")
                                    .ExecuteWithoutResultsAsync();
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

            //if (result.Count() == 0)
            //    return null;

            var temp = result.SingleOrDefault();
            if (temp == null)
                return null;

            var ret = mappingOrder(temp.Order, temp.Method, temp.Users, temp.ContainRelated.ToList(), temp.Pros.ToList());
            return ret;
        }

        public async Task<IReadOnlyList<Order>> ListAsync(string buyerEmail)
        {
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return null;


            var result = await _client.Cypher.Match("(ord:ORDER)")
                                                    .Where("ord.BuyerEmail = $email")
                                                    .WithParams(new { email = buyerEmail })
                                                     .Return((ord) => new
                                                     {
                                                         Order = ord.As<OrderNeo4j>()
                                                         //Method = method.As<DeliveryMethod>(),
                                                         // ContainRelated = r.CollectAs<ContainRelationshipNeo4j>(),
                                                         // Pros = pro.CollectAs<ProductNeo4j>(),
                                                         // Users = user.As<UserNeo4j>()
                                                     })
                                                   .ResultsAsync;
            var listOrder = result.Select(x => new Order
            {
                Id = x.Order.uuid,
                BuyerEmail = x.Order.BuyerEmail,
                OrderDate = x.Order.OrderDate,
                Total = x.Order.Total,
                Status = x.Order.Status
            }).ToList();
            return listOrder;
            //var result = await _client.Cypher.Match("(ord:ORDER)-[r:CONTAIN]->(pro), (ord:ORDER)-[:USE]->(method), (user:USER)-[:ARRANGE]->(ord:ORDER)")
            //                           .Return((ord, pro, r, method, user) => new {
            //                               Order = ord.As<OrderNeo4j>(),
            //                               Method = method.As<DeliveryMethod>(),
            //                               ContainRelated = r.CollectAs<ContainRelationshipNeo4j>(),
            //                               Pros = pro.CollectAs<ProductNeo4j>(),
            //                               Users = user.As<UserNeo4j>()
            //                           })
            //                           .ResultsAsync;
            //var temp = result.ToList();

            //var ret = new List<Order>();
            //foreach (var i in temp)
            //{
            //    var tempItem = mappingOrder(i.Order, i.Method, i.Users, i.ContainRelated.ToList(), i.Pros.ToList());
            //    ret.Add(tempItem);
            //}
            //return ret;
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

        public async Task<bool> CheckUserBuyProduct(string buyerEmail, string productID)
        {
            await _client.ConnectAsync();
            if (!_client.IsConnected)
                return false;

            var result2 = await _client.Cypher.Match("((user:USER)-[:BUY]->(pro: PRODUCT))")
                                              .Where("user.BuyerEmail = $email and pro.ProductItemId = $mongoID")
                                              .WithParams(new { email = buyerEmail, mongoID = productID })
                                              .Return(user => user.As<UserNeo4j>().BuyerEmail)
                                              .ResultsAsync;
            var temp = result2.SingleOrDefault();
            return (temp != null);
        }


        private Order mappingOrder(OrderNeo4j sourceOrder,
                                  DeliveryMethod deliveryMethod,
                                  UserNeo4j user,
                                  List<ContainRelationshipNeo4j> sourceListRelatation,
                                  List<ProductNeo4j> sourceListPro)
        {
            var ret = new Order
            {
                Id = sourceOrder.uuid,
                BuyerEmail = sourceOrder.BuyerEmail,
                DeliveryMethod = deliveryMethod,
                OrderDate = sourceOrder.OrderDate,
                PaymentIntentId = sourceOrder.PaymentIntentId,
                ShipToAddress = new Address
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    State = user.State,
                    Street = user.Street,
                    ZipCode = user.ZipCode
                }
            };

            OrderStatus tempStatus;
            // if (Enum.TryParse<OrderStatus>(sourceOrder.Status, out tempStatus))
            // {
            //     ret.Status = tempStatus;
            // }
            ret.Subtotal = sourceOrder.Subtotal;
            ret.Total = sourceOrder.Total;
            ret.OrderItems = new List<OrderItem>();

            //var tempProList = temp.Pros.ToList();
            //var tempRelationshipList = temp.ContainRelated.ToList();
            var tempItemsList = new List<OrderItem>();
            for (int i = 0; i < sourceListPro.Count(); i++)
            {
                var tempItem = new OrderItem();
                tempItem.Id = sourceListPro[i].uuid;
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
