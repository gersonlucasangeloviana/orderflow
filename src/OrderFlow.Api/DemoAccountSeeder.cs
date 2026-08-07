using Microsoft.AspNetCore.Identity;
using OrderFlow.Infrastructure;

public static class DemoAccountSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("DemoSeed:Enabled")) return;
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        await CreateIfMissingAsync(users, configuration["DemoSeed:CustomerEmail"] ?? "customer@orderflow.local", configuration["DemoSeed:CustomerPassword"] ?? "OrderFlow!Demo2026", "Cliente Demonstração", "Customer");
        await CreateIfMissingAsync(users, configuration["DemoSeed:AdminEmail"] ?? "admin@orderflow.local", configuration["DemoSeed:AdminPassword"] ?? "OrderFlow!Admin2026", "Administrador Demonstração", "Admin");
    }

    private static async Task CreateIfMissingAsync(UserManager<ApplicationUser> users, string email, string password, string name, string role)
    {
        if (await users.FindByEmailAsync(email) is not null) return;
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, DisplayName = name, EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded) throw new InvalidOperationException($"Could not seed {role} account: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        await users.AddToRoleAsync(user, role);
    }
}
