using GardenGroup.Models;
using GardenGroup.Repositories;
using GardenGroup.Services;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using GardenGroup.Migrations;
    
namespace GardenGroup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                });
            builder.Services.AddAuthorization();
            

            var connectionString = builder.Configuration.GetConnectionString("MongoDb");
            builder.Services.AddScoped<ITicketQueries, TicketQueries>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("The connection string is missing. Set it with: dotnet user-secrets set \"ConnectionStrings:MongoDb\" \"<your-mongodb-connection-string>\"");
                Environment.Exit(1);
            }

            var client = new MongoClient(connectionString);

            builder.Services.AddSingleton<IMongoClient>(client);

            builder.Services.AddSingleton(client.GetDatabase("helpdesk"));

            builder.Services.AddScoped<ITicketPrioritySorter, TicketPrioritySorter>();
            builder.Services.AddScoped<ITicketSearcher, TicketSearcher>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();
            builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                MongoIndexInitializer.EnsureIndexes(scope.ServiceProvider.GetRequiredService<IMongoDatabase>());
                EmployeePasswordMigration.Run(
                    scope.ServiceProvider.GetRequiredService<IMongoDatabase>(),
                    scope.ServiceProvider.GetRequiredService<IPasswordHasher<Employee>>());
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

            app.UseAuthentication();
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
