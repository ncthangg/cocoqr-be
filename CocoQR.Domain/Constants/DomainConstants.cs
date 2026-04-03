namespace CocoQR.Domain.Constants
{
    public static class TimeZone
    {
        public const string HoChiMinh = "Asia/Ho_Chi_Minh";
        public const string NewYork = "America/New_York";
    }

    public static class SuccessCode
    {
        public const string Success = "SUCCESS";
    }
    public static class ErrorCode
    {
        // Client Errors (4xx)
        public const string BadRequest = "BAD_REQUEST";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string UnprocessableEntity = "UNPROCESSABLE_ENTITY";

        // Domain Errors
        public const string InvalidEmail = "INVALID_EMAIL";
        public const string InvalidProjectState = "INVALID_PROJECT_STATE";
        public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
        public const string DuplicateEntry = "DUPLICATE_ENTRY";

        // Server Errors (5xx)
        public const string InternalError = "INTERNAL_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string DatabaseError = "DATABASE_ERROR";
    }

    public static class ErrorCategory
    {
        public const string InvalidArgument = "Invalid Argument";
        public const string DomainRuleViolation = "Domain Rule Violation";
        public const string ApplicationUseCaseFailure = "Application Use Case Failure";
        public const string SystemError = "System Error";
    }

    /// <summary>
    /// Validation messages (Vietnamese)
    /// </summary>
    public static class ValidationMessages
    {
        // Required fields
        public const string RequiredField = "Trường này là bắt buộc";
        public const string RequiredEmail = "Email là bắt buộc";
        public const string RequiredPassword = "Mật khẩu là bắt buộc";
        public const string RequiredUsername = "Tên người dùng là bắt buộc";
        public const string RequiredToEmail = "ToEmail là bắt buộc";
        public const string RequiredHost = "Host là bắt buộc";
        public const string RequiredFromName = "FromName là bắt buộc";
        public const string RequiredSubject = "Subject là bắt buộc";
        public const string RequiredBody = "Content là bắt buộc";
        public const string RequiredPath = "Path là bắt buộc";
        public const string RequiredAccountNumber = "AccountNumber là bắt buộc";
        public const string RequiredBankCode = "BankCode là bắt buộc";
        public const string RequiredPhone = "Phone là bắt buộc";

        // Format validation
        public const string InvalidEmail = "Email không hợp lệ";
        public const string InvalidPhoneNumber = "Số điện thoại không hợp lệ";
        public const string InvalidUrl = "URL không hợp lệ";
        public const string InvalidDate = "Ngày tháng không hợp lệ";
        public const string InvalidPort = "Port không hợp lệ";
        public const string InvalidSmtpType = "Type không hợp lệ";
        public const string RequiredSmtpType = "Type is required";
        public const string InvalidUserId = "Invalid userId ID";
        public const string InvalidQrId = "Invalid qr ID";

        // Length validation
        public const string PasswordTooShort = "Mật khẩu phải có ít nhất {0} ký tự";
        public const string PasswordTooLong = "Mật khẩu không được vượt quá {0} ký tự";
        public const string UsernameTooShort = "Tên người dùng phải có ít nhất {0} ký tự";
        public const string UsernameTooLong = "Tên người dùng không được vượt quá {0} ký tự";

        // Business rule violations
        public const string MaxProfileExceeded = "Bạn đã đạt giới hạn số lượng Profile ({0})";
        public const string MaxProjectsExceeded = "Bạn đã đạt giới hạn số lượng dự án ({0})";
        public const string MaxSkillsExceeded = "Bạn đã đạt giới hạn số lượng kỹ năng ({0})";
        public const string DuplicateProject = "Dự án này đã tồn tại";
        public const string DuplicateSkill = "Kỹ năng này đã tồn tại";
        public const string DuplicateContact = "Liên lạc này đã tồn tại";
    }

    /// <summary>
    /// SUCCESS messages (Vietnamese)
    /// </summary>
    public static class SuccessMessages
    {
        // CRUD operations
        public const string CreateSuccess = "Tạo thành công";
        public const string UpdateSuccess = "Cập nhật thành công";
        public const string DeleteSuccess = "Xóa thành công";
        public const string DeleteForeverSuccess = "Xóa vĩnh viễn thành công";
        public const string SaveSuccess = "Lưu thành công";

        // User operations
        public const string SigninSuccess = "Đăng nhập thành công";
        public const string SignoutSuccess = "Đăng xuất thành công";
        public const string SignupSuccess = "Đăng ký thành công";
        public const string PasswordChanged = "Đổi mật khẩu thành công";
        public const string PasswordReseted = "Đổi mật khẩu thành công";
        public const string SentOTP = "Gửi mã OTP thành công.";
        public const string CheckedOTP = "Xác thực mã OTP thành công.";
        public const string AddVerificationMethod = "Thêm phương thức xác thực thành công";
        public const string DeleteVerificationMethod = "Đã xóa phương thức xác thực thành công";

        // Profile operations
        public const string ProjectCreated = "Dự án đã được tạo";
        public const string SkillAdded = "Kỹ năng đã được thêm";
        public const string ProfilePublished = "Hồ sơ đã được công khai";
    }

    /// <summary>
    /// Error messages (Vietnamese)
    /// </summary>
    public static class ErrorMessages
    {
        // Required object
        public const string EntityRequired = "Dối tượng này là bắt buộc";
        public const string FieldRequired = "Trường thông tin này là bắt buộc";

        // Authentication
        public const string InvalidCredentials = "Tên đăng nhập hoặc mật khẩu không đúng";
        public const string AccountLocked = "Tài khoản đã bị khóa";
        public const string AccountNotVerified = "Tài khoản chưa được xác thực";
        public const string Unauthorized = "Bạn không có quyền truy cập";

        // Not found
        public const string EntityNotFound = "Không tồn tại đối tượng";
        public const string UserIDNotFoundInTheContext = "Context không chứa UserID";
        public const string UserNotFound = "Không tìm thấy người dùng";
        public const string ProfileNotFound = "Không tìm thấy profile";
        public const string ContactNotFound = "Không tìm thấy thông tin liên lạc";
        public const string ProjectNotFound = "Không tìm thấy dự án";
        public const string SkillNotFound = "Không tìm thấy kỹ năng";
        public const string SmtpSettingInactive = "Cấu hình SMTP đang tắt (IsActive = false), không thể gửi email.";
        public const string SmtpSettingByTypeNotFound = "Không tìm thấy cấu hình SMTP cho type {0}.";
        public const string SmtpConnectionFailed = "Không thể kết nối SMTP server {0}:{1}. Vui lòng kiểm tra Host/Port/SSL hoặc firewall mạng.";
        public const string SmtpSendFailed = "Gửi mail test thất bại trên SMTP server {0}:{1}. Vui lòng kiểm tra tài khoản SMTP và cấu hình bảo mật.";
        public const string SmtpProtocolFailed = "Gửi mail test thất bại trên SMTP server {0}:{1}. Vui lòng kiểm tra phản hồi của server SMTP.";
        public const string SmtpAuthenticationFailed = "Xác thực SMTP thất bại với server {0}:{1}. Vui lòng kiểm tra Username/Password.";
        public const string CloudinaryStorageNotConfigured = "Cloudinary storage chưa được cấu hình đúng.";
        public const string CloudinaryUploadFailed = "Upload file lên Cloudinary thất bại.";
        public const string CloudinaryDeleteFailed = "Xóa file trên Cloudinary thất bại.";
        public const string DigitalOceanStorageNotConfigured = "DigitalOcean storage chưa được cấu hình đúng.";
        public const string InvalidOrigin = "Origin không hợp lệ";
        public const string ConfigurationValueRequired = "Giá trị cấu hình '{0}' là bắt buộc.";
        public const string ConnectionStringNotFound = "Connection string '{0}' không tìm thấy trong cấu hình.";
        public const string AccountByIdNotFound = "Account {0} not found";
        public const string QrHistoryNotFound = "QR history không tồn tại.";
        public const string StyleLibraryNotFound = "Style library not found";
        public const string StyleLibraryInactive = "Style library is inactive";
        public const string StylePermissionDenied = "No permission for selected style";
        public const string AccountNotFound = "Account không tồn tại.";
        public const string AccountAccessDenied = "Không có quyền truy cập account.";
        public const string AccountInactive = "Account inactive.";
        public const string BankNotFound = "Bank không tồn tại.";
        public const string UnsupportedMode = "Mode không hỗ trợ";
        public const string ProviderNotFound = "Provider không tồn tại.";
        public const string PaymentMethodMaintenance = "Phương thức thanh toán đang bảo trì.";

        // General errors
        public const string UnexpectedError = "Đã xảy ra lỗi không mong muốn";
        public const string DatabaseError = "Lỗi kết nối cơ sở dữ liệu";
        public const string NetworkError = "Lỗi kết nối mạng";
    }
}
