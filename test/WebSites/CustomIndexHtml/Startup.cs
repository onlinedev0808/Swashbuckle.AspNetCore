﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.Swagger.Model;

namespace CustomIndexHtml
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "API V1", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            });
        }
    }
}
