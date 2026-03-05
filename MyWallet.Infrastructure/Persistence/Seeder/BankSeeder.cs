using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.DTOs.Request;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Entities;
using MyWallet.Infrastructure.Persistence.MyDbContext;
using System.Text.Json;
using DomainException = MyWallet.Domain.Exceptions.DomainException;

namespace MyWallet.Infrastructure.Persistence.Seeder
{
    public class BankSeeder
    {
        private readonly IWebHostEnvironment _env;
        private readonly MyWalletDbContext _context;
        private readonly IIdGenerator _idGenerator;
        private readonly IFileStorageService _fileStorageService;

        public BankSeeder(IWebHostEnvironment env, MyWalletDbContext context, IIdGenerator idGenerator, IFileStorageService fileStorageService)
        {
            _env = env;
            _context = context;
            _idGenerator = idGenerator;
            _fileStorageService = fileStorageService;
        }

        public async Task SeedAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath,
                                        "Seed",
                                        "Data",
                                        "banks_v1.json");

            if (!File.Exists(filePath))
            {
                throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var banks = JsonSerializer.Deserialize<List<PostBankInfoJsonReq>>(json);

            var dbBanks = await _context.BankInfos.ToDictionaryAsync(x => x.BankCode);

            _context.ChangeTracker.AutoDetectChangesEnabled = false; //Tắt check mỗi khi Add/Update/Delete

            foreach (var item in banks)
            {
                if (!dbBanks.TryGetValue(item.BankCode, out var existing))
                {
                    var bank = new BankInfo
                    {
                        BankCode = item.BankCode,
                        NapasCode = item.NapasCode,
                        SwiftCode = item.SwiftCode,
                        BankName = item.BankName,
                        ShortName = item.ShortName,
                        LogoUrl = item.LogoUrl,
                        IsActive = item.IsActive
                    };

                    bank.Initialize(_idGenerator.NewId(), Guid.Empty);

                    _context.BankInfos.Add(bank);
                }
                else
                {
                    // UPDATE
                    existing.NapasCode = item.NapasCode;
                    existing.SwiftCode = item.SwiftCode;
                    existing.BankName = item.BankName;
                    existing.ShortName = item.ShortName;
                    existing.LogoUrl = item.LogoUrl;
                    existing.IsActive = item.IsActive;
                }
            }

            // DELETE nếu JSON không còn
            var jsonCodes = banks.Select(x => x.BankCode).ToHashSet();
            var toDelete = dbBanks.Values.Where(x => !jsonCodes.Contains(x.BankCode)).ToList();

            foreach (var item in toDelete)
            {
                if (!string.IsNullOrEmpty(item.LogoUrl))
                    await _fileStorageService.DeleteFileAsync(item.LogoUrl);
            }

            _context.BankInfos.RemoveRange(toDelete);

            await _context.SaveChangesAsync();

            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }
}
