using Core.Entities.CommentCassandra;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private ICommentRepository _commentRepo;
        private IOrderRepository _orderRepo;

        public CommentService(ICommentRepository commentRepository, IOrderRepository orderRepository)
        {
            _commentRepo = commentRepository;
            _orderRepo = orderRepository;
        }

        public async Task<Comment> AddComment(Comment comment)
        {
            if (await _orderRepo.CheckUserBuyProduct(comment.userName, comment.productID))
            {
                return await _commentRepo.Add(comment);
            }

            throw new Exception("User has not bought this item");
        }

        public Task<List<Comment>> GetAllCommentsByProduct(string productID)
        {
            return _commentRepo.GetAllByProduct(productID);
        }
    }
}
