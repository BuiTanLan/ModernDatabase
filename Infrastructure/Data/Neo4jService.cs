//using Microsoft.AspNetCore.Http;
////using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Options;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Core.Entities;
//using Neo4jClient;

//namespace Infrastructure.Data
//{
//    public class Neo4jService
//    {
//        private readonly AppSettings _appSettings;
//        public GraphClient client;

//        public Neo4jService(IOptions<AppSettings> appSettings)
//        {
//            _appSettings = appSettings.Value;
//            client = new GraphClient(new Uri(_appSettings.Neo4jServer), _appSettings.Neo4jUserName, _appSettings.Neo4jPassword);
//            nẽo
//        }
//    }

//}