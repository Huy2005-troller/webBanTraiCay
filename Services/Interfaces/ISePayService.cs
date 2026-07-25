using Fruitables.Models;

namespace Fruitables.Services.Interfaces;

public interface ISePayService
{
    /// <summary>Tạo URL ảnh QR Code từ SePay</summary>
    string GetQrCodeUrl(Order order);
    
    /// <summary>Xác thực API Key trong header từ webhook SePay</summary>
    bool VerifyWebhook(HttpRequest request);
}
