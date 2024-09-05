using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mod.DynamicEncounters.Features;

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
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Backend.Telemetry"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "BotLib"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Interfaces"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "NQutils"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Prometheus.AspNetCore"));
            apm.ApplicationParts.Remove(apm.ApplicationParts.Single(p => p.Name == "Router.Orleans"));
        });
        
        services.RegisterModFeatures();
        
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
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