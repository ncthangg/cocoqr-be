using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Auths.Requests;
using CocoQR.Application.DTOs.Auths.Responses;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using System.Security.Claims;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenConfiguration _tokenConfiguration;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;

        private readonly ITokenService _tokenService;

        public AuthService(IUnitOfWork unitOfWork,
            ITokenConfiguration tokenConfiguration,
            IUserContext userContext,
            IIdGenerator idGenerator,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenConfiguration = tokenConfiguration;
            _userContext = userContext;
            _idGenerator = idGenerator;
            _tokenService = tokenService;
        }

        public async Task<GetUserRes> Me()
        {
            var userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

            User? user = await _unitOfWork.Users.GetByIdAsync(userId)
                      ?? throw new ApplicationException(ErrorCode.Unauthorized, "Tên người dùng này chưa có tài khoản! Vui lòng đăng ký!");

            return new GetUserRes
            {
                Email = user.Email,
                FullName = user.FullName,
                PictureUrl = user.PictureUrl,
            };
        }

        public async Task<SignInGoogleRes> SignInGoogle(HttpContext context)
        {
            var auth = await context.AuthenticateAsync(Google.OAuthTempConfigPath);

            if (!auth.Succeeded || auth.Principal == null)
                throw new ApplicationException(ErrorCode.BadRequest, "Đăng nhập Google thất bại hoặc callback chưa hoàn tất!");

            var claims = auth.Principal.Claims;

            string email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            string name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
            string googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
            string picture = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value ?? "";

            if (string.IsNullOrWhiteSpace(email))
                throw new ApplicationException(ErrorCode.BadRequest, "Không tìm thấy email Google!");

            User? user = await _unitOfWork.Users.GetByEmailAsync(email);

            if (user == null)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    user = new User
                    {
                        Email = email,
                        FullName = name,
                        GoogleId = googleId,
                        PictureUrl = picture,
                        SecurityStamp = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.UtcNow
                    };
                    var userId = _idGenerator.NewId();
                    user.Initialize(userId, userId);
                    await _unitOfWork.Users.AddAsync(user);

                    var rolesExisted = await _unitOfWork.Roles.GetByNameAsync(RoleCategory.USER.ToString())
                        ?? throw new ApplicationException(ErrorCode.NotFound, "Default role not found");
                    await _unitOfWork.UserRoles.AddUserToRoleAsync(_idGenerator.NewId(), user.Id, rolesExisted.Id);

                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            else
            {
                if (user.Status == false)
                    throw new ApplicationException(ErrorCode.Unauthorized, "Tài khoản đang bị tạm khóa. Vui lòng liên hệ Admin để biết thêm chi tiết.");
            }

            var roles = (await _unitOfWork.UserRoles.GetRolesByUserIdAsync(user.Id)).ToList();

            if (!roles.Any())
                throw new ApplicationException(ErrorCode.BadRequest, "Đăng nhập thất bại, Role of user not found");

            TokenRes? jwt = null;
            if (roles.Count == 1)
            {
                jwt = await _tokenService.GenerateTokens(user.Id, roles, null);
            }

            return new SignInGoogleRes
            {
                UserId = user.Id,
                UserRes = new()
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    PictureUrl = user.PictureUrl,
                },
                TokenRes = jwt,
                RoleRes = roles.Select(p => RoleMapper.ToGetRoleRes(p)).ToList()
            };
        }

        public async Task<SwitchRoleRes> SwitchRoleAsync(SwitchRoleReq request)
        {
            if (request.UserId == Guid.Empty || request.RoleId == Guid.Empty)
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid userId/roleId");

            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.UserNotFound);

            if (user.Status == false)
                throw new ApplicationException(ErrorCode.Unauthorized, "Tài khoản đang bị tạm khóa. Vui lòng liên hệ Admin để biết thêm chi tiết.");

            var roles = (await _unitOfWork.UserRoles.GetRolesByUserIdAsync(request.UserId)).ToList();
            var selectedRole = roles.FirstOrDefault(x => x.Id == request.RoleId)
                ?? throw new ApplicationException(ErrorCode.Forbidden, "Role không thuộc về user.");

            var token = await _tokenService.GenerateTokens(user.Id, new[] { selectedRole }, null);

            return new SwitchRoleRes
            {
                TokenRes = token,
                RoleRes = RoleMapper.ToGetRoleRes(selectedRole)
            };
        }
    }
}
