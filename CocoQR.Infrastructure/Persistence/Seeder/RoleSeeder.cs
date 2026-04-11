using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Roles.Requests;
using CocoQR.Application.DTOs.Seed;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Entities;
using CocoQR.Domain.Exceptions;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CocoQR.Infrastructure.Persistence.Seeder
{
    public class RoleSeeder
    {
        private readonly IWebHostEnvironment _env;
        private readonly CocoQRDbContext _context;
        private readonly IIdGenerator _idGenerator;

        public RoleSeeder(IWebHostEnvironment env, CocoQRDbContext context, IIdGenerator idGenerator)
        {
            _env = env;
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<RoleSyncPreviewRes> PreviewAsync()
        {
            var (filePath, roles) = await ReadSeedFileAsync();
            var fileInfo = new FileInfo(filePath);

            var dbRoles = await _context.Roles
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Name);

            var jsonCodes = roles.Select(x => x.Name).ToHashSet();
            var changes = new List<RoleSyncDiffItem>();

            foreach (var item in roles)
            {
                if (!dbRoles.TryGetValue(item.Name, out var existing))
                {
                    changes.Add(new RoleSyncDiffItem
                    {
                        Action = RoleSyncAction.Insert,
                        RoleName = item.Name,
                        Diffs = []
                    });
                }
                else
                {
                    var diffs = DetectDiffs(existing, item);

                    changes.Add(new RoleSyncDiffItem
                    {
                        Action = diffs.Count > 0 ? RoleSyncAction.Update : RoleSyncAction.Unchanged,
                        RoleName = item.Name,
                        Diffs = diffs
                    });
                }
            }

            // DELETE
            foreach (var db in dbRoles.Values.Where(x => !jsonCodes.Contains(x.Name)))
            {
                changes.Add(new RoleSyncDiffItem
                {
                    Action = RoleSyncAction.Delete,
                    RoleName = db.Name,
                    Diffs = []
                });
            }

            return new RoleSyncPreviewRes
            {
                SourceFile = Path.GetFileName(filePath),
                FileLastModified = fileInfo.LastWriteTimeUtc,
                Summary = new RoleSyncSummary
                {
                    TotalInFile = roles.Count,
                    TotalInDb = dbRoles.Count,
                    ToInsert = changes.Count(x => x.Action == RoleSyncAction.Insert),
                    ToUpdate = changes.Count(x => x.Action == RoleSyncAction.Update),
                    ToDelete = changes.Count(x => x.Action == RoleSyncAction.Delete),
                    Unchanged = changes.Count(x => x.Action == RoleSyncAction.Unchanged)
                },
                Changes = changes
                    .Where(x => x.Action != RoleSyncAction.Unchanged) // chỉ trả về có thay đổi
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
                                            FileStorage.Folders.Seed,
                                            FileStorage.Folders.Data,
                                                "roles_v1.json");

                    if (!File.Exists(filePath))
                    {
                        throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");
                    }

                    var json = await File.ReadAllTextAsync(filePath);
                    var roles = JsonSerializer.Deserialize<List<PostRoleJsonReq>>(json);

                    if (roles is null || roles.Count == 0)
                        throw new DomainException(ErrorCode.InternalError, "Seed file is empty or invalid.");

                    var dbRoles = await _context.Roles.ToDictionaryAsync(x => x.Name.ToLower());
                    var jsonCodes = roles.Select(x => x.Name.ToLower()).ToHashSet();

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
                                Status = true
                            };

                            role.Initialize(_idGenerator.NewId(), Guid.Empty);
                            _context.Roles.Add(role);
                        }
                        else
                        {
                            var isDirty = existing.Name != item.Name;

                            if (isDirty)
                            {
                                existing.Name = key;
                                existing.NameUpperCase = item.Name.ToUpper();

                                _context.Entry(existing).State = EntityState.Modified;
                            }
                        }
                    }

                    #region Delete 
                    var toDelete = dbRoles.Values.Where(x => !jsonCodes.Contains(x.Name.ToLower())).ToList();

                    _context.Roles.RemoveRange(toDelete);
                    #endregion

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
        private async Task<(string filePath, List<PostRoleJsonReq> roles)> ReadSeedFileAsync()
        {
            var filePath = Path.Combine(_env.ContentRootPath,
                                    FileStorage.Folders.Seed,
                                    FileStorage.Folders.Data,
                                        "roles_v1.json");

            if (!File.Exists(filePath))
                throw new DomainException(ErrorCode.NotFound, $"Seed file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            var roles = JsonSerializer.Deserialize<List<PostRoleJsonReq>>(json);

            if (roles is null || roles.Count == 0)
                throw new DomainException(ErrorCode.InternalError, "Seed file is empty or invalid.");

            return (filePath, roles);
        }

        private static List<RoleFieldDiff> DetectDiffs(Role existing, PostRoleJsonReq incoming)
        {
            var diffs = new List<RoleFieldDiff>();

            Check(nameof(existing.Name), existing.Name, incoming.Name);

            return diffs;

            void Check(string field, string? oldVal, string? newVal)
            {
                if (oldVal != newVal)
                    diffs.Add(new RoleFieldDiff { Field = field, OldValue = oldVal, NewValue = newVal });
            }
        }
    }
}