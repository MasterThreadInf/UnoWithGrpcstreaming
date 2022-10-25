using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GrpcServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
      
            const string corsPolicy = "_corsPolicy";
            services.AddGrpc();
            services.AddCors(options =>
                {
                    options.AddPolicy(name: corsPolicy,
                                      policy =>
                                      {
                                          policy.WithOrigins("https://localhost:44366",
                                                             "http://localhost:5000")
                                                .AllowAnyHeader()
                                                .AllowAnyMethod();
                                      });
                });
            //services.AddSingleton<Resolverfactory>
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseCors(cors => cors
            //   .AllowAnyMethod()
            //   .AllowAnyHeader()
            //   //.SetIsOriginAllowed(origin => true)
            //   .AllowCredentials()
            //   .WithOrigins("https://localhost:44366",
            //                 "http://localhost:5000")
            //   );
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ch20Ex01 v1"));
            }
            //app.UseHttpsRedirection();
            app.UseRouting();
            //app.UseCors(builder =>
            //{
            //    builder.WithOrigins("https://localhost:44366", "http://localhost:5000")
            //    .AllowCredentials()  
            //    .AllowAnyHeader()
            //    .AllowAnyMethod();
            //});
            app.UseCors();
            app.UseGrpcWeb();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<DefaultFooService>().EnableGrpcWeb()
                    .RequireCors(cors => cors.AllowAnyHeader().AllowAnyMethod()
                        .WithOrigins("https://localhost:44366", "http://localhost:5000"));

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
