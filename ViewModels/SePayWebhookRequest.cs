using Fruitables.Models;

namespace Fruitables.ViewModels;

/// <summary>Payload mà SePay gửi về khi có giao dịch phát sinh</summary>
public class SePayWebhookRequest
{
    public int Id { get; set; }
    public string Gateway { get; set; } = string.Empty;
    public string TransactionDate { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    
    /// <summary>Mã tra cứu — SePay trích từ nội dung chuyển khoản (khớp với OrderNumber)</summary>
    public string Code { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    /// <summary>"in" = tiền vào, "out" = tiền ra</summary>
    public string TransferType { get; set; } = string.Empty;
    public long TransferAmount { get; set; }
    public long Accumulated { get; set; }
    public string? SubAccount { get; set; }
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
}
