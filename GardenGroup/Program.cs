using GardenGroup.Models;
using MongoDB.Driver;

namespace GardenGroup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            
            var connectionString = builder.Configuration.GetConnectionString("MongoDb")
                ?? throw new InvalidOperationException("The MongoDb connection string is not configured.");
            var databaseName = builder.Configuration["MongoDb:Database"] ?? "helpdesk";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            builder.Services.AddSingleton(database.GetCollection<Employee>("employees"));

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

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
