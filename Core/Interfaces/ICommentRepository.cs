using Core.Entities.CommentCassandra;
using System.Collections.Generic;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;

namespace Core.Interfaces
{
    public interface ICommentRepository
    {
        public Task DeleteMany(string productId);
        public Task<Comment> Add(Comment comment);
        public Task<List<Comment>> GetAll();
        public Task<List<Comment>> GetAllByProduct(string productID);
        public Task<Comment> Update(Comment comment);
    }
}
