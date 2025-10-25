using Microsoft.AspNetCore.Identity;

namespace DevelPrueba.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            // Datos del admin inicial
            const string adminEmail = "admin@acme.local";
            const string adminPass = "Admin123!";

            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, adminPass);
                if (!result.Succeeded)
                {
                    throw new Exception("No se pudo crear el usuario admin: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
