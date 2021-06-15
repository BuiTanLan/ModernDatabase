using Cassandra;
using Core.Entities.CommentCassandra;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class CommentRepository : ICommentRepository
    {
        private ICluster _cluster;

        public CommentRepository(ICluster cluster)
        {
            _cluster = cluster;
        }

        public async Task DeleteMany(string productID)
        {
            var query = "delete comment where product = ?";

            await ExecuteWithoutResultAsync(query, new[] { productID });
        }

        public async Task<Comment> Update(Comment comment)
        {
            var query = "update comment set username = ? where productid = ? and userid = ?";

            await ExecuteWithoutResultAsync(query, new[] { comment.userID, comment.productID, comment.content, comment.userName });

            return comment; 
        }

        public async Task<Comment> Add(Comment comment)
        {
            var query = "insert into comment (userid, productid, content, commentat, username) values (?, ?, ?, toTimestamp(now()), ?)";

            await ExecuteWithoutResultAsync(query, new[] { comment.userID, comment.productID, comment.content, comment.userName });

            return comment;
        }

        public async Task<List<Comment>> GetAllByProduct(string product)
        {
            var rs = await ExecuteSimpleAsync("select * from comment where productid = ?", new []{product});

            var ret = new List<Comment>();
            foreach (var row in rs)
            {
                var temp = Mapping(row);
                ret.Add(temp);
            }

            return ret;
        }

        public async Task<List<Comment>> GetAll()
        {
            var rs = await ExecuteSimpleAsync("select * from comment");

            var ret = new List<Comment>();
            foreach(var row in rs)
            {
                var temp = Mapping(row);
                ret.Add(temp);
            }

            return ret;
        }

        private async Task<RowSet> ExecuteSimpleAsync(string query, object[] value=null)
        {
            //var cluster = Cluster.Builder()
            //                        .AddContactPoints("localhost")
            //                        .Build();

            var session = await _cluster.ConnectAsync("skinet_comment");

            var statement = new SimpleStatement(query, value);

            //var statement = new SimpleStatement(query);

            var rs = await session.ExecuteAsync(statement);
            return rs;                         
        }

        private async Task ExecuteWithoutResultAsync(string query, object[] value = null)
        {
            //var cluster = Cluster.Builder()
            //                        .AddContactPoints("localhost")
            //                        .Build();

            var session = await _cluster.ConnectAsync("skinet_comment");

            var ps = session.Prepare(query);

            var statement = ps.Bind(value);

            await session.ExecuteAsync(statement);
        }

        private Comment Mapping(Row row)
        {
            var temp = new Comment();
            temp.productID = row.GetValue<string>("productid");
            temp.userID = row.GetValue<string>("userid");
            temp.userName = row.GetValue<string>("username");
            temp.content = row.GetValue<string>("content");
            temp.CommentAt = row.GetValue<DateTimeOffset>("commentat");
            return temp;
        }
    }
}
