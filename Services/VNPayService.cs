using Fruitables.Helpers;
using Fruitables.Models;
using Fruitables.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Fruitables.Services;

public class VNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VNPayService> _logger;

    public VNPayService(IConfiguration configuration, ILogger<VNPayService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string CreatePaymentUrl(Order order, HttpContext context)
    {
        var vnpay = new VnPayLibrary();
        
        var tmnCode = _configuration["VNPAY:TmnCode"]?.Trim();
        var hashSecret = _configuration["VNPAY:HashSecret"]?.Trim();
        var baseUrl = _configuration["VNPAY:BaseUrl"]?.Trim();
        
        if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl))
            throw new InvalidOperationException("Thiếu cấu hình VNPAY (TmnCode/HashSecret/BaseUrl).");
        
        // VNPAY yêu cầu thời gian GMT+7, không phụ thuộc timezone của máy chạy app.
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var createDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        var expireDate = createDate.AddMinutes(15);
        
        // TxnRef phải duy nhất mỗi giao dịch — dùng OrderId
        var txnRef = order.Id.ToString();
        
        // Số tiền VNPAY: VND * 100, chỉ phần nguyên
        var amount = ((long)Math.Round(order.Total * 100m, MidpointRounding.AwayFromZero)).ToString();
        
        vnpay.AddRequestData("vnp_Version", _configuration["VNPAY:Version"]!);
        vnpay.AddRequestData("vnp_Command", _configuration["VNPAY:Command"]!);
        vnpay.AddRequestData("vnp_TmnCode", tmnCode);
        vnpay.AddRequestData("vnp_Amount", amount);
        vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", _configuration["VNPAY:CurrCode"]!);
        vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(context));
        vnpay.AddRequestData("vnp_Locale", _configuration["VNPAY:Locale"]!);
        // VNPAY quy định OrderInfo là tiếng Việt không dấu, không ký tự đặc biệt.
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang " + order.Id);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss"));
        
        string returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/Checkout/PaymentCallback";
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        
        vnpay.AddRequestData("vnp_TxnRef", txnRef);

        string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
        _logger.LogInformation("VNPAY URL created for order {OrderId}, amount={Amount}, tmn={TmnCode}", order.Id, amount, tmnCode);
        
        return paymentUrl;
    }

    public bool PaymentExecute(IQueryCollection collections, out int orderId)
    {
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }
        
        string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
        string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        string vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
        
        orderId = 0;
        if (int.TryParse(vnp_TxnRef, out var id))
        {
            orderId = id;
        }

        var hashSecret = _configuration["VNPAY:HashSecret"]?.Trim();
        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret!);
        
        if (checkSignature)
        {
            if (vnp_ResponseCode == "00")
            {
                return true;
            }
            return false;
        }
        
        return false;
    }
}
