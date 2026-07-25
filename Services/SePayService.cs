using Fruitables.Models;
using Fruitables.Services.Interfaces;

namespace Fruitables.Services;

public class SePayService : ISePayService
{
    private readonly IConfiguration _configuration;

    public SePayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetQrCodeUrl(Order order)
    {
        var bankCode = _configuration["SePay:BankCode"] ?? "NCB";
        var accountNumber = _configuration["SePay:AccountNumber"] ?? "";
        var amount = (long)order.Total;
        
        // Dùng ID số nguyên làm nội dung chuyển khoản, tránh dấu gạch ngang bị ngân hàng lọc bỏ
        var content = $"DH{order.Id}";

        return $"https://qr.sepay.vn/img?bank={bankCode}" +
               $"&acc={accountNumber}" +
               $"&template=compact" +
               $"&amount={amount}" +
               $"&des={Uri.EscapeDataString(content)}" +
               $"&download=false";
    }

    public bool VerifyWebhook(HttpRequest request)
    {
        var apiKey = _configuration["SePay:ApiKey"];
        
        // Nếu chưa cấu hình ApiKey thì bỏ qua bước xác thực (dev mode)
        if (string.IsNullOrWhiteSpace(apiKey))
            return true;

        var authHeader = request.Headers["Authorization"].ToString();
        return authHeader == $"Apikey {apiKey}";
    }
}
