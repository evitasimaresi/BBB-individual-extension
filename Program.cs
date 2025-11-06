using BBB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        
        // Add session support
        builder.Services.AddDistributedMemoryCache(); // Required for session
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });


        // Add database context
        builder.Services.AddSingleton<DisableWalInterceptor>();
        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseLazyLoadingProxies()
                   .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(serviceProvider.GetRequiredService<DisableWalInterceptor>());
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();
        app.UseSession();
        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();


        app.Run();
    }
}