using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using MyWallet.Application.Common.Mapper;
using MyWallet.Application.Contracts.IConfigs;
using MyWallet.Application.Contracts.IContext;
using MyWallet.Application.Contracts.IServices;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Application.DTOs.Response;
using MyWallet.Domain.Constants;
using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories;
using MyWallet.Domain.Interface.IUnitOfWork;
using System.Security.Claims;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;

namespace MyWallet.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenConfiguration _tokenConfiguration;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        //private readonly IRedisService _redisService;

        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        public AuthService(IUnitOfWork unitOfWork,
            ITokenConfiguration tokenConfiguration,
            IUserContext userContext,
            IIdGenerator idGenerator,
            //IRedisService redisService,
            ITokenService tokenService,
            IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _tokenConfiguration = tokenConfiguration;
            _userContext = userContext;
            _idGenerator = idGenerator;
            //_redisService = redisService;

            _tokenService = tokenService;
            _userRepository = userRepository;
        }
        public async Task<GetUserRes> Me()
        {
            var userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

            User? user = await _userRepository.GetWithAccountsAsync(userId)
                      ?? throw new ApplicationException(ErrorCode.Unauthorized, "Tên người dùng này chưa có tài khoản! Vui lòng đăng ký!");

            return new GetUserRes
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                GoogleId = user.GoogleId,
                PictureUrl = user.PictureUrl,
                SecurityStamp = user.SecurityStamp ?? ""                                                    
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
             
            User? user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
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

                var rolesExisted = await _unitOfWork.Roles.GetByNameAsync(RoleCategory.User.ToString())
                    ?? throw new ApplicationException(ErrorCode.NotFound, "Default role not found");
                await _unitOfWork.UserRoles.AddUserToRoleAsync(_idGenerator.NewId(), user.Id, rolesExisted.Id);

                await _unitOfWork.CommitAsync();
            }

            var roles = await _unitOfWork.UserRoles.GetRolesByUserIdAsync(user.Id)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Role of user not found");

            // 4. Generate JWT for this user
            TokenRes jwt = await _tokenService.GenerateTokens(user.Id, roles, null);

            return new SignInGoogleRes
            {
                UserRes = new()
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    GoogleId = user.GoogleId,
                    SecurityStamp = user.SecurityStamp ?? "",
                    PictureUrl = user.PictureUrl,
                },
                TokenRes = jwt,
                RoleRes = roles.Select(p => RoleMapper.ToGetRoleRes(p)).ToList()
            };
        }
    }
}
