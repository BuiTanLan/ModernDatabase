using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.CommentCassandra
{
    public class Comment
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string ProductId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CommentAt { get; set; }
    }
}
