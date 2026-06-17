using LogisticsManagementSystem.Data;
using LogisticsManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace LogisticsManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // FIX: SINGLE NAMED CLIENT USED BY ALL CONTROLLERS
            builder.Services.AddHttpClient("LogisticsApi", client =>
            {
                var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
                             ?? "http://localhost:7193/";
                client.BaseAddress = new Uri(apiUrl);
            });

            builder.Services.AddDbContext<LogisticDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // FIX: CURRENCY SERVICE USES SAME NAMED CLIENT NOT ITS OWN
            builder.Services.AddHttpClient<CurrencyService>("LogisticsApi");

            // FIX: REMOVED bare AddHttpClient() AND SystemApiClient - THEY WERE CREATING DEFAULT CLIENT
            builder.Services.AddScoped<FileValidationService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // FIX: REMOVED UseHttpsRedirection - CAUSES REDIRECT LOOP IN DOCKER
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}