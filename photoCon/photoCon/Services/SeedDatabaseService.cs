using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace photoCon.Services
{
    public class SeedDatabaseService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDatabaseService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void SeedData()
        {
            // Check if roles already exist
            if (!_roleManager.Roles.Any())
            {
                var roles = new List<IdentityRole>
                {
                    new IdentityRole { Name = "SYSADMIN", NormalizedName = "SYSADMIN".ToUpper() },
                    new IdentityRole { Name = "APPADMIN", NormalizedName = "APPADMIN".ToUpper() },
                    new IdentityRole { Name = "ECDUSER", NormalizedName = "ECDUSER".ToUpper() },
                    new IdentityRole { Name = "JUDGE", NormalizedName = "JUDGE".ToUpper() }
                };

                foreach (var role in roles)
                {
                    _roleManager.CreateAsync(role).GetAwaiter().GetResult();
                }
            }

            // Add users if they do not exist
            AddUserWithRoleIfNotExists("Rainier Anthony", "Sitjar", "T", "External Communications Dept.", "WDO II", "11", "02-0555", "02-0555", "RainierAnthony.Sitjar@Pagcor.ph", "RAINIERANTHONY.SITJAR@PAGCOR.PH", true, null, false, false, null, true, 0, "APPADMIN");
            AddUserWithRoleIfNotExists("Juan Philippe", "Manuel", "M", "External Communications Dept.", "SWE", "11", "11-0124", "11-0124", "JuanPhilippe.Manuel@Pagcor.ph", "JUANPHILIPPE.MANUEL@PAGCOR.PH", true, null, false, false, null, true, 0, "ECDUSER");
            AddUserWithRoleIfNotExists("JUAN", "ONE", "UNO", null, null, null, "juanone1", "juanone1", "juanone1@sample.com", "juanone1@sample.com", true, null, false, false, null, true, 0, "JUDGE");
            AddUserWithRoleIfNotExists("DUHA", "TWO", "DOS", null, null, null, "duhatwo2", "duhatwo2", "duhatwo2@sample.com", "duhatwo2@sample.com", true, null, false, false, null, true, 0, "JUDGE");
        }

        private void AddUserWithRoleIfNotExists(string firstName, string lastName, string middleName, string department, string position, string payClass, string userName, string normalizedUserName, string email, string normalizedEmail, bool emailConfirmed, string phoneNumber, bool phoneNumberConfirmed, bool twoFactorEnabled, DateTimeOffset? lockoutEnd, bool lockoutEnabled, int accessFailedCount, string roleName)
        {
            // Check if the user already exists
            if (_userManager.FindByNameAsync(userName).Result == null)
            {
                var passwordHasher = new PasswordHasher<ApplicationUser>();

                var user = new ApplicationUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    Department = department,
                    Position = position,
                    PayClass = payClass,
                    UserName = userName,
                    NormalizedUserName = normalizedUserName,
                    Email = email,
                    NormalizedEmail = normalizedEmail,
                    EmailConfirmed = emailConfirmed,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = phoneNumberConfirmed,
                    TwoFactorEnabled = twoFactorEnabled,
                    LockoutEnd = lockoutEnd,
                    LockoutEnabled = lockoutEnabled,
                    AccessFailedCount = accessFailedCount
                };

                // Set password
                user.PasswordHash = passwordHasher.HashPassword(user, "Password01");

                // Create user
                _userManager.CreateAsync(user).GetAwaiter().GetResult();

                // Assign role to the user
                var role = _roleManager.Roles.SingleOrDefault(r => r.Name == roleName);
                if (role != null)
                {
                    _userManager.AddToRoleAsync(user, role.Name).GetAwaiter().GetResult();
                }
            }
        }
    }
}
