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
        private readonly ICluster _cluster;

        public CommentRepository(ICluster cluster)
        {
            _cluster = cluster;
        }

        public async Task DeleteMany(string productId)
        {
            var query = "delete comment where product = ?";

            await ExecuteWithoutResultAsync(query, new[] { productId });
        }

        public async Task<Comment> Update(Comment comment)
        {
            var query = "update comment set username = ? where productid = ? and userid = ?";

            await ExecuteWithoutResultAsync(query, new[] { comment.Email, comment.ProductId, comment.Content, comment.UserName });

            return comment; 
        }

        public async Task<Comment> Add(Comment comment)
        {
            var query = "insert into comment (userid, productid, content, commentat, username) values (?, ?, ?, ?, ?)";

            await ExecuteWithoutResultAsync(query, new object[] { comment.Email, comment.ProductId, comment.Content, comment.CommentAt, comment.UserName });

            return comment;
        }

        public async Task<List<Comment>> GetAllByProduct(string product)
        {
            var rs = await ExecuteSimpleAsync("select * from comment");

           // var rs = await ExecuteSimpleAsync("select * from comment where productid = ?", new []{product});

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
            var session = await _cluster.ConnectAsync("skinet_comment");
            var statement = new SimpleStatement(query, value);
            var rs = await session.ExecuteAsync(statement);
            return rs;                         
        }

        private async Task ExecuteWithoutResultAsync(string query, object[] value = null)
        {
            var session = await _cluster.ConnectAsync("skinet_comment");
            var ps = await session.PrepareAsync(query);
            var statement = ps.Bind(value);
            await session.ExecuteAsync(statement);
        }

        private Comment Mapping(Row row)
        {
            var temp = new Comment
            {
                ProductId = row.GetValue<string>("productid"),
                Email = row.GetValue<string>("userid"),
                UserName = row.GetValue<string>("username"),
                Content = row.GetValue<string>("content"),
                CommentAt = row.GetValue<long>("commentat")
            };
            return temp;
        }
    }
}
