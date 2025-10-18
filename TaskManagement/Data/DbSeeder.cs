using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            // Create roles if they don't exist
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "Employee", "Super Admin" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create users if they don't exist
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = serviceProvider.GetRequiredService<TaskDbContext>();

            // Admin user
            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            if (adminUser == null)
            {
                var newAdminUser = new IdentityUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newAdminUser, "Admin!23456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                    var adminEmployee = new Employee { Name = "Admin User", Email = "admin@gmail.com", PhoneNo = "1234567890" };
                    dbContext.Employees.Add(adminEmployee);
                }
                else
                {
                    // Log errors if user creation failed
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin user: {error.Description}");
                    }
                }
            }

            // Employee user
            var employeeUser = await userManager.FindByEmailAsync("employee@gmail.com");
            if (employeeUser == null)
            {
                var newEmployeeUser = new IdentityUser
                {
                    UserName = "employee@gmail.com",
                    Email = "employee@gmail.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newEmployeeUser, "Employee!23");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newEmployeeUser, "Employee");
                    var employee = new Employee { Name = "Employee User", Email = "employee@gmail.com", PhoneNo = "0987654321" };
                    dbContext.Employees.Add(employee);
                }
                else
                {
                    // Log errors if user creation failed
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating employee user: {error.Description}");
                    }
                }
            }
             var managerUser = await userManager.FindByEmailAsync("superadmin@gmail.com");
            if (managerUser == null)
            {
                var newManagerUser = new IdentityUser
                {
                    UserName = "superadmin@gmail.com",
                    Email = "superadmin@gmail.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newManagerUser, "Superadmin!23");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newManagerUser, "SuperAdmin");
                    var managerEmployee = new Employee { Name = "Super Admin User", Email = "superadmin@gmail.com", PhoneNo = "1122334455" };
                    dbContext.Employees.Add(managerEmployee);
                }
                else
                {
                    // Log errors if user creation failed
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating manager user: {error.Description}");
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
