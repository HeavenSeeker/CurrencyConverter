using CurrencyConverter.WebAPI.Constants;
using Microsoft.AspNetCore.Identity;

namespace CurrencyConverter.WebAPI.Data
{
    public class DbSeeder
    {
        public static async Task SeedData(IApplicationBuilder app)
        {
            // Create a scoped service provider to resolve dependencies
            using var scope = app.ApplicationServices.CreateScope();

            // resolve the logger service
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

            try
            {
                // resolve other dependencies
                var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                // Check if any users exist to prevent duplicate seeding
                if (!userManager.Users.Any())
                {

                    //
                    //add admin user
                    //
                    var adminUser = new IdentityUser
                    {
                        //Name = "Admin",
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com",
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    // Create Admin role if it doesn't exist
                    if (!(await roleManager.RoleExistsAsync(Roles.Admin)))
                    {
                        logger.LogInformation("Admin role is creating");
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

                        if (!roleResult.Succeeded)
                        {
                            var roleErros = roleResult.Errors.Select(e => e.Description);
                            logger.LogError($"Failed to create admin role. Errors : {string.Join(",", roleErros)}");

                            return;
                        }
                        logger.LogInformation("Admin role is created");
                    }

                    // Attempt to create admin user
                    var createUserResult = await userManager.CreateAsync(user: adminUser, password: "admin");

                    // Validate user creation
                    if (!createUserResult.Succeeded)
                    {
                        var errors = createUserResult.Errors.Select(e => e.Description);
                        logger.LogError(
                            $"Failed to create admin user. Errors: {string.Join(", ", errors)}"
                        );
                        return;
                    }

                    // adding role to user
                    var addUserToRoleResult = await userManager.AddToRoleAsync(user: adminUser, role: Roles.Admin);

                    if (!addUserToRoleResult.Succeeded)
                    {
                        var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                        logger.LogError($"Failed to add admin role to user. Errors : {string.Join(",", errors)}");
                    }
                    logger.LogInformation("Admin user is created");

                    //
                    //add normal user
                    //
                    var normalUser = new IdentityUser
                    {
                        //Name = "user",
                        UserName = "user@gmail.com",
                        Email = "user@gmail.com",
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    // Create User role if it doesn't exist
                    if (!(await roleManager.RoleExistsAsync(Roles.User)))
                    {
                        logger.LogInformation("User role is creating");
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(Roles.User));

                        if (!roleResult.Succeeded)
                        {
                            var roleErros = roleResult.Errors.Select(e => e.Description);
                            logger.LogError($"Failed to create User role. Errors : {string.Join(",", roleErros)}");

                            return;
                        }
                        logger.LogInformation("User role is created");
                    }

                    // Attempt to create normal user
                    var createUserResult2 = await userManager.CreateAsync(user: normalUser, password: "user");

                    // Validate user creation
                    if (!createUserResult2.Succeeded)
                    {
                        var errors = createUserResult2.Errors.Select(e => e.Description);
                        logger.LogError(
                            $"Failed to create normal user. Errors: {string.Join(", ", errors)}"
                        );
                        return;
                    }

                    // adding role to user
                    var addUserToRoleResult2 = await userManager.AddToRoleAsync(user: normalUser, role: Roles.User);

                    if (!addUserToRoleResult2.Succeeded)
                    {
                        var errors = addUserToRoleResult2.Errors.Select(e => e.Description);
                        logger.LogError($"Failed to add 'User' role to user. Errors : {string.Join(",", errors)}");
                    }
                    logger.LogInformation("'User' user is created");

                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.Message);
            }
        }
    }
}
