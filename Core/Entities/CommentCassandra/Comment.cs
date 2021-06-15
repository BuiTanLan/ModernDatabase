using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.CommentCassandra
{
    public class Comment
    {
        public string userID { get; set; }
        public string userName { get; set; }
        public string productID { get; set; }
        public string content { get; set; }
        public DateTimeOffset CommentAt { get; set; }
    }
}
