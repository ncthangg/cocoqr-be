using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Banks.Requests;
using CocoQR.Application.DTOs.Seed;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using System.Text.Json;
using DomainException = CocoQR.Domain.Exceptions.DomainException;

namespace CocoQR.Infrastructure.Persistence.Seeder
{
    public class BankSeeder
    {
        private readonly IWebHostEnvironment _env;
        private readonly CocoQRDbContext _context;
        private readonly IIdGenerator _idGenerator;
        private readonly IFileStorageService _fileStorageService;

        public BankSeeder(IWebHostEnvironment env, CocoQRDbContext context, IIdGenerator idGenerator, IFileStorageService fileStorageService)
        {
            _env = env;
            _context = context;
            _idGenerator = idGenerator;
            _fileStorageService = fileStorageService;
        }

        public async Task<BankSyncPreviewRes> PreviewAsync()
        {
            var (filePath, banks) = await ReadSeedFileAsync();
            var fileInfo = new FileInfo(filePath);

            var dbBanks = await _context.BankInfos
                .AsNoTracking()
                .ToDictionaryAsync(x => x.BankCode);

            var jsonCodes = banks.Select(x => x.BankCode).ToHashSet();
            var changes = new List<BankSyncDiffItem>();

            foreach (var item in banks)
            {
                if (!dbBanks.TryGetValue(item.BankCode, out var existing))
                {
                    changes.Add(new BankSyncDiffItem
                    {
                        Action = BankSyncAction.Insert,
                        BankCode = item.BankCode,
                        BankName = item.BankName,
                        IsActive = item.IsActive,
                        Diffs = []
                    });
                }
                else
                {
                    var diffs = DetectDiffs(existing, item);

                    changes.Add(new BankSyncDiffItem
                    {
                        Action = diffs.Count > 0 ? BankSyncAction.Update : BankSyncAction.Unchanged,
                        BankCode = item.BankCode,
                        BankName = item.BankName,
                        IsActive = item.IsActive,
                        Diffs = diffs
                    });
                }
            }

            // DELETE
            foreach (var db in dbBanks.Values.Where(x => !jsonCodes.Contains(x.BankCode)))
            {
                changes.Add(new BankSyncDiffItem
                {
                    Action = BankSyncAction.Delete,
                    BankCode = db.BankCode,
                    BankName = db.BankName,
                    Diffs = []
                });
            }

            return new BankSyncPreviewRes
            {
                SourceFile = Path.GetFileName(filePath),
                FileLastModified = fileInfo.LastWriteTimeUtc,
                Summary = new BankSyncSummary
                {
                    TotalInFile = banks.Count,
                    TotalInDb = dbBanks.Count,
                    ToInsert = changes.Count(x => x.Action == BankSyncAction.Insert),
                    ToUpdate = changes.Count(x => x.Action == BankSyncAction.Update),
                    ToDelete = changes.Count(x => x.Action == BankSyncAction.Delete),
                    Unchanged = changes.Count(x => x.Action == BankSyncAction.Unchanged)
                },
                Changes = changes
                    .Where(x => x.Action != BankSyncAction.Unchanged) // chỉ trả về có thay đổi
                    .OrderBy(x => x.Action)
                    .ToList()
            };
        }

        public async Task SeedAsync()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
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

                    if (banks is null || banks.Count == 0)
                        throw new DomainException(ErrorCode.InternalError, "Seed file is empty or invalid.");

                    var dbBanks = await _context.BankInfos.ToDictionaryAsync(x => x.BankCode);
                    var jsonCodes = banks.Select(x => x.BankCode).ToHashSet();

                    _context.ChangeTracker.AutoDetectChangesEnabled = false; //Tắt check mỗi khi Add/Update/Delete

                    foreach (var item in banks)
                    {
                        if (!dbBanks.TryGetValue(item.BankCode, out var existing))
                        {
                            var bank = new BankInfo
                            {
                                BankCode = item.BankCode,
                                NapasBin = item.NapasBin,
                                SwiftCode = item.SwiftCode,
                                BankName = item.BankName,
                                ShortName = item.ShortName,
                                IsActive = item.IsActive
                            };

                            bank.Initialize(_idGenerator.NewId(), Guid.Empty);
                            _context.BankInfos.Add(bank);
                        }
                        else
                        {
                            var isDirty =
                                existing.NapasBin != item.NapasBin ||
                                existing.SwiftCode != item.SwiftCode ||
                                existing.BankName != item.BankName ||
                                existing.ShortName != item.ShortName ||
                                existing.IsActive != item.IsActive;

                            if (isDirty)
                            {
                                existing.NapasBin = item.NapasBin;
                                existing.SwiftCode = item.SwiftCode;
                                existing.BankName = item.BankName;
                                existing.ShortName = item.ShortName;
                                existing.IsActive = item.IsActive;

                                _context.Entry(existing).State = EntityState.Modified;
                            }
                        }
                    }

                    #region Delete           
                    var toDelete = dbBanks.Values
                                          .Where(x => !jsonCodes.Contains(x.BankCode))
                                          .ToList();

                    foreach (var item in toDelete)
                    {
                        if (!string.IsNullOrEmpty(item.LogoUrl))
                            await _fileStorageService.DeleteFileAsync(item.LogoUrl);
                    }

                    _context.BankInfos.RemoveRange(toDelete);
                    #endregion

                    _context.ChangeTracker.DetectChanges();

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                finally
                {
                    _context.ChangeTracker.AutoDetectChangesEnabled = true;
                }
            });
        }

        // ── Helpers ────────────────────────────────────────────────
        private async Task<(string filePath, List<PostBankInfoJsonReq> banks)> ReadSeedFileAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Seed", "Data", "banks_v1.json");

            if (!File.Exists(filePath))
                throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            var banks = JsonSerializer.Deserialize<List<PostBankInfoJsonReq>>(json);

            if (banks is null || banks.Count == 0)
                throw new DomainException(ErrorCode.InternalError, "Seed file is empty or invalid.");

            return (filePath, banks);
        }

        private static List<BankFieldDiff> DetectDiffs(BankInfo existing, PostBankInfoJsonReq incoming)
        {
            var diffs = new List<BankFieldDiff>();

            Check(nameof(existing.NapasBin), existing.NapasBin, incoming.NapasBin);
            Check(nameof(existing.SwiftCode), existing.SwiftCode, incoming.SwiftCode);
            Check(nameof(existing.BankName), existing.BankName, incoming.BankName);
            Check(nameof(existing.ShortName), existing.ShortName, incoming.ShortName);
            Check(nameof(existing.IsActive), existing.IsActive.ToString(), incoming.IsActive.ToString());

            return diffs;

            void Check(string field, string? oldVal, string? newVal)
            {
                if (oldVal != newVal)
                    diffs.Add(new BankFieldDiff { Field = field, OldValue = oldVal, NewValue = newVal });
            }
        }
    }
}
