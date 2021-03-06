using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Helpers;
using API.Middlewares;
using AutoMapper;
using Cassandra;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Neo4jClient;
using StackExchange.Redis;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;

        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
      
            services.AddAutoMapper(typeof(MappingProfiles));
            services.AddControllers();

            services.AddDbContext<StoreContext>(opt =>
            {
                opt.UseSqlServer(_config.GetConnectionString("DefaultConnection"));
            });
            services.AddDbContext<AppIdentityDbContext>(opt =>
            {
                opt.UseSqlServer(_config.GetConnectionString("DefaultConnection"));
            });
            services.Configure<AppSettings>(_config.GetSection("AppSettings"));

            services.AddSingleton<IConnectionMultiplexer>(c =>
            {
                var configuration = ConfigurationOptions.Parse(_config.GetConnectionString("Redis"), true);
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddSingleton<MongoDbService>();


            // add neo4j
            var info = _config.GetSection("Neo4j");
            //var neo4jClient = GraphDatabase.Driver(new Uri(info["Neo4jServer"]), info["Neo4jUserName"], info["Neo4jPassword"]);
            var driver = GraphDatabase.Driver(info["Neo4jServer"], AuthTokens.Basic(info["Neo4jUserName"], info["Neo4jPassword"]));
            var neo4jClient = new BoltGraphClient(driver);
            services.AddSingleton<IGraphClient>(neo4jClient);


            // add cassandra
            info = _config.GetSection("Cassandra");
            var cluster = Cluster.Builder()
                                 .AddContactPoints(info["ContactPoint"])
                                 .Build();

            services.AddSingleton<ICluster>(cluster);

            //var session = cluster.Connect();

            services.AddApplicationServices();
            services.AddIdentityServices(_config);
            services.AddSwaggerDocumentation();
            services.AddCors(opt => 
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Content")),
                RequestPath = "/content"
            });
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwaggerDocumention();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }
}
