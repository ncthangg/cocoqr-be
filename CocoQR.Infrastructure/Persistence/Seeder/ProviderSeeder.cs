using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Providers.Requests;
using CocoQR.Application.DTOs.Seed;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Entities;
using CocoQR.Domain.Exceptions;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CocoQR.Infrastructure.Persistence.Seeder
{
    public class ProviderSeeder
    {
        private readonly IWebHostEnvironment _env;
        private readonly CocoQRDbContext _context;
        private readonly IIdGenerator _idGenerator;

        public ProviderSeeder(IWebHostEnvironment env, CocoQRDbContext context, IIdGenerator idGenerator)
        {
            _env = env;
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<ProviderSyncPreviewRes> PreviewAsync()
        {
            var (filePath, providers) = await ReadSeedFileAsync();
            var fileInfo = new FileInfo(filePath);

            var dbProviders = await _context.Providers
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Code);

            var jsonCodes = providers.Select(x => x.Code).ToHashSet();
            var changes = new List<ProviderSyncDiffItem>();

            foreach (var item in providers)
            {
                if (!dbProviders.TryGetValue(item.Code, out var existing))
                {
                    changes.Add(new ProviderSyncDiffItem
                    {
                        Action = ProviderSyncAction.Insert,
                        ProviderCode = item.Code,
                        ProviderName = item.Name,
                        Diffs = new List<ProviderFieldDiff>()
                    });
                }
                else
                {
                    var diffs = DetectDiffs(existing, item);

                    changes.Add(new ProviderSyncDiffItem
                    {
                        Action = diffs.Count > 0 ? ProviderSyncAction.Update : ProviderSyncAction.Unchanged,
                        ProviderCode = item.Code,
                        ProviderName = item.Name,
                        Diffs = diffs
                    });
                }
            }

            // DELETE
            foreach (var db in dbProviders.Values.Where(x => !jsonCodes.Contains(x.Code)))
            {
                changes.Add(new ProviderSyncDiffItem
                {
                    Action = ProviderSyncAction.Delete,
                    ProviderCode = db.Code,
                    ProviderName = db.Name,
                    Diffs = new List<ProviderFieldDiff>()
                });
            }

            return new ProviderSyncPreviewRes
            {
                SourceFile = Path.GetFileName(filePath),
                FileLastModified = fileInfo.LastWriteTimeUtc,
                Summary = new ProviderSyncSummary
                {
                    TotalInFile = providers.Count,
                    TotalInDb = dbProviders.Count,
                    ToInsert = changes.Count(x => x.Action == ProviderSyncAction.Insert),
                    ToUpdate = changes.Count(x => x.Action == ProviderSyncAction.Update),
                    ToDelete = changes.Count(x => x.Action == ProviderSyncAction.Delete),
                    Unchanged = changes.Count(x => x.Action == ProviderSyncAction.Unchanged)
                },
                Changes = changes
                    .Where(x => x.Action != ProviderSyncAction.Unchanged) // chỉ trả về có thay đổi
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
                                                "providers_v1.json");

                    if (!File.Exists(filePath))
                    {
                        throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");
                    }

                    var json = await File.ReadAllTextAsync(filePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var providers = JsonSerializer.Deserialize<List<PostProviderJsonReq>>(json, options);

                    if (providers is null || providers.Count == 0)
                        throw new DomainException(ErrorCode.InternalError, "Seed file is empty or invalid.");

                    var dbProviders = await _context.Providers.ToDictionaryAsync(x => x.Code);
                    var jsonCodes = providers.Select(x => x.Code).ToHashSet();

                    _context.ChangeTracker.AutoDetectChangesEnabled = false; //Tắt check mỗi khi Add/Update/Delete

                    foreach (var item in providers)
                    {
                        if (!dbProviders.TryGetValue(item.Code, out var existing))
                        {
                            var provider = new Provider
                            {
                                Code = item.Code,
                                Name = item.Name,
                                IsActive = false,
                            };

                            provider.Initialize(_idGenerator.NewId(), Guid.Empty);
                            _context.Providers.Add(provider);
                        }
                        else
                        {
                            var isDirty = existing.Name != item.Name;

                            if (isDirty)
                            {
                                existing.Name = item.Name;

                                _context.Entry(existing).State = EntityState.Modified;
                            }
                        }
                    }

                    #region Delete 
                    var toDelete = dbProviders.Values.Where(x => !jsonCodes.Contains(x.Code)).ToList();

                    _context.Providers.RemoveRange(toDelete);
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
        private async Task<(string filePath, List<PostProviderJsonReq>)> ReadSeedFileAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Seed", "Data", "providers_v1.json");

            if (!File.Exists(filePath))
                throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var providers = JsonSerializer.Deserialize<List<PostProviderJsonReq>>(json, options);

            if (providers is null || providers.Count == 0)
                throw new DomainException(ErrorCode.InternalError, "Seed file is empty or invalid.");

            return (filePath, providers);
        }

        private static List<ProviderFieldDiff> DetectDiffs(Provider existing, PostProviderJsonReq incoming)
        {
            var diffs = new List<ProviderFieldDiff>();

            Check(nameof(existing.Name), existing.Name, incoming.Name);

            return diffs;

            void Check(string field, string? oldVal, string? newVal)
            {
                if (oldVal != newVal)
                    diffs.Add(new ProviderFieldDiff { Field = field, OldValue = oldVal, NewValue = newVal });
            }
        }
    }
}