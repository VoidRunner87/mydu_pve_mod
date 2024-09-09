using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mod.DynamicEncounters.Api.Config;
using Mod.DynamicEncounters.Features;
using NQutils.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mod.DynamicEncounters.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var mvcBuilder = services.AddControllersWithViews();
        
        mvcBuilder.ConfigureApplicationPartManager(apm =>
        {
            // Removes other assemblies that we don't want controller parts
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Backend"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Backend.PubSub"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Backend.Telemetry"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "BotLib"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Interfaces"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "NQutils"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Prometheus.AspNetCore"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Router.Orleans"));
        });
        
        services.RegisterModFeatures();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        
        services.AddControllers(options =>
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            options.InputFormatters.Add(new YamlInputFormatter(deserializer));  
            options.OutputFormatters.Add(new YamlOutputFormatter(serializer));  
            options.FormatterMappings.SetMediaTypeMappingForFormat("yaml", MediaTypeHeaderValues.ApplicationYaml); 
        });

        services.AddLogging(logging => logging.Setup(logWebHostInfo: true));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        var allowsAllCors = Environment.GetEnvironmentVariable("CORS_ALLOW_ALL") is "1" or "true";

        if (allowsAllCors)
        {
            Console.WriteLine("WARNING: CORS IS SET TO ALLOW ALL");
            app.UseCors("AllowAll");
        }
        
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}