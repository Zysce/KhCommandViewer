using KhCommand.Data;
using KhCommand.Data.Utils;
using KHCommandExtractor;
using KhCommandViewer.Components;
using Microsoft.EntityFrameworkCore;

namespace KhCommandViewer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder
            .Services
            .AddDbContext<CommandDbContext>(opts =>
                opts.UseInMemoryDatabase("KhCommand"));

        builder
            .Services
            .AddScoped<Extractor>()
            .AddSingleton<ImageExtractor>();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        using (var scope = app.Services.CreateScope())
        {
            var extractor = scope.ServiceProvider.GetRequiredService<Extractor>();
            await extractor.Extract();            
        }

        await app.RunAsync();
    }
}
