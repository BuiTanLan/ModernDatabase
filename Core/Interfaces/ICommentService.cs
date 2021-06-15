using Core.Entities.CommentCassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ICommentService
    {
        public Task<Comment> AddComment(Comment comment);
        public Task<List<Comment>> GetAllCommentsByProduct(string productID);
    }
}
