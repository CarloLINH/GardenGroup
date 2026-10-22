using GardenGroup.Repositories;
using GardenGroup.Services;
using MongoDB.Bson;
using MongoDB.Bson.IO;
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
            

            var connectionString = builder.Configuration.GetConnectionString("MongoDb");
            //builder.Services.AddSingleton<IMongoDatabase>(_ => MongoConnection.Create(connectionString!, "helpdesk"));
            builder.Services.AddScoped<ITicketQueries, TicketQueries>();

            if (connectionString == null)
            {
                Console.WriteLine("You must set your 'MONGODB_URI' environment variable. To learn how to set it, see https://www.mongodb.com/docs/drivers/csharp/current/get-started/create-connection-string");
                Environment.Exit(0);
            }

            var client = new MongoClient(connectionString);

            builder.Services.AddSingleton<IMongoClient>(client);

            builder.Services.AddSingleton(client.GetDatabase("helpdesk"));


            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<ITicketService, TicketService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                MongoIndexInitializer.EnsureIndexes(scope.ServiceProvider.GetRequiredService<IMongoDatabase>());
            }

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
