namespace CocoQR.Application.DTOs.Base.BaseRes
{
    public class BaseResponseModel<T>
    {
        public string Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public object? AdditionalData { get; set; }

        public BaseResponseModel(string code, T? data, object? additionalData = null, string? message = null)
        {
            Code = code;
            Data = data;
            AdditionalData = additionalData;
            Message = message;
        }

        public BaseResponseModel(string code, string? message)
        {
            Code = code;
            Message = message;
        }
    }
}
