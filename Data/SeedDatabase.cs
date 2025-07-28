using API.Entity;
using Microsoft.AspNetCore.Identity;

namespace API.Data;

public static class SeedDatabase
{
    public static async void Initialize(IApplicationBuilder app)
    {
        var userManager = app.ApplicationServices
        .CreateScope()
        .ServiceProvider
        .GetRequiredService<UserManager<AppUser>>();

        var roleManager = app.ApplicationServices
        .CreateScope()
        .ServiceProvider
        .GetRequiredService<RoleManager<AppRole>>();

        if (!roleManager.Roles.Any())
        {
            var customer = new AppRole { Name = "customer" };
            var admin = new AppRole { Name = "Admin" };

            await roleManager.CreateAsync(customer);
            await roleManager.CreateAsync(admin);
        }

        if (!userManager.Users.Any())
        {
            var customer = new AppUser { Name = "deneme", UserName = "deneme", Email = "deneme@gmail.com" };
            var admin = new AppUser { Name = "denemeadmin", UserName = "denemeadmin", Email = "admin@gmail.com" };

            await userManager.CreateAsync(customer, "Customer_123");
            await userManager.AddToRoleAsync(customer, "customer");
            await userManager.CreateAsync(admin, "Admin_123");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "customer" });
        }

    }
}