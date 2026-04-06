using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;
namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class BankInfoRepository : BaseRepository<BankInfo>, IBankInfoRepository
    {
        public BankInfoRepository(IUnitOfWork _unitOfWork)
            : base(_unitOfWork, "BankInfos")
        {
        }
        public async Task<(IEnumerable<BankInfo>, int totalCount)> GetBankInfosAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue, bool isAdmin)
        {
            var orderBy = isAdmin ? "ShortName ASC"
                                  : "IsActive DESC, ShortName ASC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

                var field = sortField switch
                {
                    "shortName" => $"ShortName {dir}",
                    "bankCode" => $"BankCode {dir}",
                    _ => "ShortName ASC"
                };
                orderBy = isAdmin ? field : $"IsActive DESC, {field}";
            }

            string sql;
            if (isAdmin)
            {
                sql = $@"
        SELECT 
            Id, BankCode, NapasBin, SwiftCode, 
            BankName, ShortName, LogoUrl, IsActive,
            Status, 
            CreatedAt, CreatedBy, 
            UpdatedAt, UpdatedBy, 
            DeletedAt, DeletedBy
        FROM BankInfos
        WHERE 
            (@IsActive IS NULL OR IsActive = @IsActive)
            AND (
                @SearchValue IS NULL 
                OR BankName LIKE '%' + @SearchValue + '%'
                OR ShortName LIKE '%' + @SearchValue + '%'
                OR BankCode LIKE '%' + @SearchValue + '%'
            )
        ORDER BY {orderBy}
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM BankInfos
        WHERE 
            (@IsActive IS NULL OR IsActive = @IsActive);
        ";
            }
            else
            {
                sql = $@"
        SELECT 
            Id, BankCode, NapasBin, SwiftCode, BankName, ShortName, LogoUrl, IsActive
        FROM BankInfos
        WHERE 
            DeletedAt IS NULL
            AND Status = 1
            AND (
                @SearchValue IS NULL 
                OR BankName LIKE '%' + @SearchValue + '%'
                OR ShortName LIKE '%' + @SearchValue + '%'
                OR BankCode LIKE '%' + @SearchValue + '%'
            )
        ORDER BY {orderBy}
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM BankInfos
        WHERE 
            DeletedAt IS NULL
            AND Status = 1;
        ";
            }

            return await QueryPagedAsync<BankInfo>(sql, new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsActive = isActive,
                SearchValue = searchValue
            });
        }

        public async Task<BankInfo> GetByBankCodeAsync(string bankCode)
        {
            var sql = $@"SELECT
                     Id, BankCode, NapasBin, SwiftCode, BankName, ShortName, LogoUrl, IsActive

                FROM BankInfos 
                WHERE BankCode = @BankCode
                     AND DeletedAt IS NULL
                     AND Status = 1
                ";

            return await QueryFirstOrDefaultAsync<BankInfo>(sql,
            new
            {
                BankCode = bankCode,
            });
        }
    }
}
