using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.DTOs.Roles.Requests;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Entities;
using MyWallet.Domain.Exceptions;
using MyWallet.Infrastructure.Persistence.MyDbContext;
using System.Text.Json;

namespace MyWallet.Infrastructure.Persistence.Seeder
{
    public class RoleSeeder
    {
        private readonly IWebHostEnvironment _env;
        private readonly MyWalletDbContext _context;
        private readonly IIdGenerator _idGenerator;

        public RoleSeeder(IWebHostEnvironment env, MyWalletDbContext context, IIdGenerator idGenerator)
        {
            _env = env;
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task SeedAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath,
                                        "Seed",
                                        "Data",
                                        "roles_v1.json");

            if (!File.Exists(filePath))
            {
                throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var roles = JsonSerializer.Deserialize<List<PostRoleJsonReq>>(json) ?? [];

            var dbRoles = await _context.Roles.ToDictionaryAsync(x => x.Name.ToLower());

            _context.ChangeTracker.AutoDetectChangesEnabled = false; //Tắt check mỗi khi Add/Update/Delete

            foreach (var item in roles)
            {
                var key = item.Name.ToLower();
                if (!dbRoles.TryGetValue(key, out var existing))
                {
                    var role = new Role
                    {
                        Name = key,
                        NameUpperCase = item.Name.ToUpper(),
                        Status = item.Status
                    };

                    role.Initialize(_idGenerator.NewId(), Guid.Empty);

                    _context.Roles.Add(role);
                }
                else
                {
                    // UPDATE
                    existing.Name = key;
                    existing.NameUpperCase = item.Name.ToUpper();
                    existing.Status = item.Status;
                }
            }

            // DELETE nếu JSON không còn
            var jsonCodes = roles.Select(x => x.Name.ToLower()).ToHashSet();
            var toDelete = dbRoles.Values.Where(x => !jsonCodes.Contains(x.Name)).ToList();

            _context.Roles.RemoveRange(toDelete);

            await _context.SaveChangesAsync();

            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }
}
