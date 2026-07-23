using Fruitables.Constants;
using Fruitables.Models;
using Fruitables.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Fruitables.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly ISettingsService _settingsService;

    private const string SUPPORT_EMAIL = "support@fruitables.com";
    private const string COMPANY_NAME = "Fruitables";

    public EmailService(ILogger<EmailService> logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
    }

    public async Task<bool> SendAccountLockedEmailAsync(
        string customerEmail,
        string customerName,
        string violationType,
        string reason,
        string lockType,
        DateTime? expiresAt)
    {
        try
        {
            var subject = $"[{COMPANY_NAME}] Thông báo khóa tài khoản";
            var body = GenerateAccountLockedEmailBody(customerName, violationType, reason, lockType, expiresAt);
            return await SendEmailAsync(customerEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send account locked email to {Email}", customerEmail);
            return false;
        }
    }

    public async Task<bool> SendAccountUnlockedEmailAsync(
        string customerEmail,
        string customerName,
        string reason)
    {
        try
        {
            var subject = $"[{COMPANY_NAME}] Thông báo mở khóa tài khoản";
            var body = GenerateAccountUnlockedEmailBody(customerName, reason);
            return await SendEmailAsync(customerEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send account unlocked email to {Email}", customerEmail);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
    {
        try
        {
            var subject = $"[{COMPANY_NAME}] Đặt lại mật khẩu";
            var body = GeneratePasswordResetEmailBody(resetLink);
            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            return false;
        }
    }
    public async Task<bool> SendTemporaryPasswordEmailAsync(string email, string customerName, string temporaryPassword)
    {
        try
        {
            var subject = $"[{COMPANY_NAME}] Mật khẩu tạm thời của bạn";
            var body = GenerateTemporaryPasswordEmailBody(customerName, temporaryPassword);
            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send temporary password email to {Email}", email);
            return false;
        }
    }

    private string GenerateTemporaryPasswordEmailBody(string customerName, string temporaryPassword)
    {
        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Mật khẩu tạm thời</title>
</head>
<body style=""margin:0; padding:0; background-color:#f5f5f5; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f5f5f5; padding:20px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px; width:100%;"">
                    <tr>
                        <td style=""background:linear-gradient(135deg,#e67e22,#f39c12); padding:35px 30px; text-align:center; border-radius:12px 12px 0 0;"">
                            <h1 style=""color:white; margin:0; font-size:28px;"">🔑 {COMPANY_NAME}</h1>
                            <p style=""color:rgba(255,255,255,0.9); margin:8px 0 0 0; font-size:15px;"">Mật khẩu tạm thời</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background:white; padding:30px; border-left:1px solid #e8e8e8; border-right:1px solid #e8e8e8;"">
                            <p style=""margin:0 0 15px 0; font-size:15px;"">Xin chào <strong>{System.Net.WebUtility.HtmlEncode(customerName)}</strong>,</p>
                            <p style=""margin:0 0 20px 0; color:#555; font-size:14px;"">Bạn đã yêu cầu đặt lại mật khẩu. Dưới đây là mật khẩu tạm thời để đăng nhập vào hệ thống:</p>

                            <div style=""background:#fff3cd; border:2px dashed #f39c12; border-radius:10px; padding:20px; text-align:center; margin:20px 0;"">
                                <p style=""margin:0 0 8px 0; color:#856404; font-size:13px; text-transform:uppercase; letter-spacing:1px;"">Mật khẩu tạm thời</p>
                                <p style=""margin:0; font-size:28px; font-weight:bold; color:#e67e22; letter-spacing:3px; font-family:monospace;"">{System.Net.WebUtility.HtmlEncode(temporaryPassword)}</p>
                            </div>

                            <div style=""background:#fff8e1; border-left:4px solid #ff9800; padding:15px; margin:20px 0; border-radius:4px;"">
                                <p style=""margin:0 0 8px 0; font-weight:bold; color:#e65100;"">⚠️ Lưu ý quan trọng:</p>
                                <ul style=""margin:0; padding-left:20px; color:#555; font-size:14px;"">
                                    <li style=""margin-bottom:5px;"">Mật khẩu này chỉ có hiệu lực trong <strong>15 phút</strong>.</li>
                                    <li style=""margin-bottom:5px;"">Sau khi đăng nhập, hãy <strong>đổi mật khẩu ngay</strong> trong phần cài đặt tài khoản.</li>
                                    <li>Không chia sẻ mật khẩu này cho bất kỳ ai.</li>
                                </ul>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background:#2c3e50; color:#bdc3c7; padding:25px 30px; text-align:center; border-radius:0 0 12px 12px; font-size:13px;"">
                            <p style=""margin:0 0 8px 0;""><strong style=""color:white;"">{COMPANY_NAME}</strong></p>
                            <p style=""margin:0; color:#95a5a6;"">Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                            <p style=""margin:8px 0 0 0; color:#7f8c8d; font-size:12px;"">Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public async Task<bool> SendOrderConfirmationEmailAsync(Order order)
    {
        try
        {
            // Xác định email người nhận: ưu tiên email user đăng nhập, fallback địa chỉ snapshot
            var toEmail = order.User?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                // Lấy email từ ShippingSnapshot nếu có (guest checkout)
                if (!string.IsNullOrWhiteSpace(order.ShippingSnapshot))
                {
                    try
                    {
                        var snapshot = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(order.ShippingSnapshot);
                        if (snapshot.TryGetProperty("Email", out var emailProp))
                            toEmail = emailProp.GetString();
                    }
                    catch { /* ignore parsing errors */ }
                }
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Cannot send order confirmation email for order {OrderNumber}: no email found", order.OrderNumber);
                return false;
            }

            var customerName = order.User?.Name ?? order.Address?.FullName ?? "Quý khách";
            var subject = $"[{COMPANY_NAME}] Xác nhận đơn hàng #{order.OrderNumber}";
            var body = GenerateOrderConfirmationEmailBody(order, customerName);
            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderNumber}", order.OrderNumber);
            return false;
        }
    }

    // Template HTML email xác nhận đơn hàng
    private string GenerateOrderConfirmationEmailBody(Order order, string customerName)
    {
        var paymentMethodText = order.PaymentMethod switch
        {
            PaymentMethod.COD => "Thanh toán khi nhận hàng (COD)",
            PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
            PaymentMethod.Paypal => "PayPal",
            PaymentMethod.Check => "Séc",
            _ => "Không xác định"
        };

        var shippingMethodText = order.ShippingMethod switch
        {
            ShippingMethod.Free => "Miễn phí vận chuyển",
            ShippingMethod.FlatRate => "Vận chuyển tiêu chuẩn",
            ShippingMethod.LocalPickup => "Nhận tại cửa hàng",
            _ => "Tiêu chuẩn"
        };

        // Build items table rows
        var itemsHtml = new System.Text.StringBuilder();
        foreach (var item in order.Items)
        {
            var itemTotal = item.Price * item.Quantity;
            itemsHtml.Append($@"
                <tr>
                    <td style=""padding: 12px 8px; border-bottom: 1px solid #f0f0f0;"">
                        <strong>{System.Net.WebUtility.HtmlEncode(item.ProductName)}</strong>
                    </td>
                    <td style=""padding: 12px 8px; border-bottom: 1px solid #f0f0f0; text-align: center;"">{item.Quantity}</td>
                    <td style=""padding: 12px 8px; border-bottom: 1px solid #f0f0f0; text-align: right;"">{item.Price:N0}₫</td>
                    <td style=""padding: 12px 8px; border-bottom: 1px solid #f0f0f0; text-align: right; font-weight: bold;"">{itemTotal:N0}₫</td>
                </tr>");
        }

        // Shipping address block
        var addressBlock = "";
        if (order.Address != null)
        {
            addressBlock = $@"
            <div style=""background:#f8fffe; border-left:4px solid #28a745; padding:15px; margin:20px 0; border-radius:4px;"">
                <h3 style=""color:#28a745; margin:0 0 10px 0; font-size:16px;"">📍 Địa chỉ giao hàng</h3>
                <p style=""margin:4px 0;""><strong>{System.Net.WebUtility.HtmlEncode(order.Address.FullName)}</strong></p>
                <p style=""margin:4px 0; color:#666;"">📞 {System.Net.WebUtility.HtmlEncode(order.Address.Phone ?? "")}</p>
                <p style=""margin:4px 0; color:#666;"">🏠 {System.Net.WebUtility.HtmlEncode(order.Address.StreetAddress ?? "")}
                    {(string.IsNullOrEmpty(order.Address.CommuneName) ? "" : ", " + System.Net.WebUtility.HtmlEncode(order.Address.CommuneName))}
                    {(string.IsNullOrEmpty(order.Address.ProvinceName) ? "" : ", " + System.Net.WebUtility.HtmlEncode(order.Address.ProvinceName))}
                </p>
            </div>";
        }

        var discountRow = order.Discount > 0
            ? $@"<tr><td style=""padding:6px 0; color:#28a745;"">Giảm giá</td><td style=""text-align:right; color:#28a745;"">-{order.Discount:N0}₫</td></tr>"
            : "";

        var shippingRow = order.ShippingFee > 0
            ? $@"<tr><td style=""padding:6px 0; color:#666;"">Phí vận chuyển</td><td style=""text-align:right; color:#666;"">{order.ShippingFee:N0}₫</td></tr>"
            : $@"<tr><td style=""padding:6px 0; color:#28a745;"">Phí vận chuyển</td><td style=""text-align:right; color:#28a745;"">Miễn phí</td></tr>";

        var createdAt = order.CreatedAt.ToString("HH:mm dd/MM/yyyy");

        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Xác nhận đơn hàng</title>
</head>
<body style=""margin:0; padding:0; background-color:#f5f5f5; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f5f5f5; padding:20px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px; width:100%;"">

                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #2ecc71, #27ae60); padding:35px 30px; text-align:center; border-radius:12px 12px 0 0;"">
                            <h1 style=""color:white; margin:0; font-size:28px; letter-spacing:1px;"">🛒 {COMPANY_NAME}</h1>
                            <p style=""color:rgba(255,255,255,0.9); margin:8px 0 0 0; font-size:15px;"">Đặt hàng thành công!</p>
                        </td>
                    </tr>

                    <!-- Success Banner -->
                    <tr>
                        <td style=""background:#e8f8f0; padding:20px 30px; text-align:center; border-left:1px solid #d4efdf; border-right:1px solid #d4efdf;"">
                            <p style=""margin:0; font-size:18px; color:#27ae60;"">✅ Đơn hàng của bạn đã được xác nhận!</p>
                            <p style=""margin:6px 0 0 0; color:#666; font-size:14px;"">Chúng tôi sẽ xử lý và giao hàng sớm nhất có thể.</p>
                        </td>
                    </tr>

                    <!-- Order Info -->
                    <tr>
                        <td style=""background:white; padding:25px 30px; border-left:1px solid #e8e8e8; border-right:1px solid #e8e8e8;"">
                            <p style=""margin:0 0 15px 0; font-size:15px;"">Xin chào <strong>{System.Net.WebUtility.HtmlEncode(customerName)}</strong>,</p>
                            <p style=""margin:0 0 20px 0; color:#555; font-size:14px;"">Cảm ơn bạn đã mua sắm tại <strong>{COMPANY_NAME}</strong>. Dưới đây là thông tin chi tiết đơn hàng của bạn.</p>

                            <div style=""background:#f8f9fa; border-radius:8px; padding:15px 20px; margin-bottom:20px;"">
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    <tr>
                                        <td style=""padding:5px 0; font-size:14px;""><strong>Mã đơn hàng:</strong></td>
                                        <td style=""text-align:right; font-size:14px; color:#2ecc71; font-weight:bold;"">#{order.OrderNumber}</td>
                                    </tr>
                                    <tr>
                                        <td style=""padding:5px 0; font-size:14px;""><strong>Ngày đặt:</strong></td>
                                        <td style=""text-align:right; font-size:14px; color:#555;"">{createdAt}</td>
                                    </tr>
                                    <tr>
                                        <td style=""padding:5px 0; font-size:14px;""><strong>Thanh toán:</strong></td>
                                        <td style=""text-align:right; font-size:14px; color:#555;"">{paymentMethodText}</td>
                                    </tr>
                                    <tr>
                                        <td style=""padding:5px 0; font-size:14px;""><strong>Vận chuyển:</strong></td>
                                        <td style=""text-align:right; font-size:14px; color:#555;"">{shippingMethodText}</td>
                                    </tr>
                                </table>
                            </div>

                            {addressBlock}

                            <!-- Products Table -->
                            <h3 style=""color:#333; margin:20px 0 12px 0; font-size:16px;"">🛍️ Sản phẩm đặt mua</h3>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse; border:1px solid #e8e8e8; border-radius:8px; overflow:hidden;"">
                                <thead>
                                    <tr style=""background:#2ecc71;"">
                                        <th style=""padding:12px 8px; color:white; text-align:left; font-size:13px;"">Sản phẩm</th>
                                        <th style=""padding:12px 8px; color:white; text-align:center; font-size:13px;"">SL</th>
                                        <th style=""padding:12px 8px; color:white; text-align:right; font-size:13px;"">Đơn giá</th>
                                        <th style=""padding:12px 8px; color:white; text-align:right; font-size:13px;"">Thành tiền</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                </tbody>
                            </table>

                            <!-- Order Total -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:15px;"">
                                <tr><td colspan=""2""><hr style=""border:none; border-top:1px solid #e8e8e8; margin:0 0 10px 0;""></td></tr>
                                <tr><td style=""padding:6px 0; color:#666;"">Tạm tính</td><td style=""text-align:right; color:#666;"">{order.Subtotal:N0}₫</td></tr>
                                {shippingRow}
                                {discountRow}
                                <tr><td colspan=""2""><hr style=""border:none; border-top:2px solid #e8e8e8; margin:8px 0;""></td></tr>
                                <tr>
                                    <td style=""font-size:17px; font-weight:bold; color:#333;"">Tổng cộng</td>
                                    <td style=""text-align:right; font-size:20px; font-weight:bold; color:#e74c3c;"">{order.Total:N0}₫</td>
                                </tr>
                            </table>

                            <!-- Note -->
                            {(string.IsNullOrEmpty(order.Notes) ? "" : $@"<div style=""background:#fff8e1; border-left:4px solid #ffc107; padding:12px 15px; margin-top:20px; border-radius:4px;""><p style=""margin:0; font-size:14px;""><strong>📝 Ghi chú:</strong> {System.Net.WebUtility.HtmlEncode(order.Notes)}</p></div>")}
                        </td>
                    </tr>

                    <!-- CTA -->
                    <tr>
                        <td style=""background:white; padding:0 30px 25px 30px; text-align:center; border-left:1px solid #e8e8e8; border-right:1px solid #e8e8e8;"">
                            <p style=""margin:0 0 15px 0; color:#555; font-size:14px;"">Bạn có thể theo dõi trạng thái đơn hàng trong tài khoản của mình.</p>
                            <a href=""/OrderHistory"" style=""display:inline-block; background:linear-gradient(135deg,#2ecc71,#27ae60); color:white; padding:12px 30px; border-radius:25px; text-decoration:none; font-weight:bold; font-size:15px;"">📦 Xem lịch sử đơn hàng</a>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background:#2c3e50; color:#bdc3c7; padding:25px 30px; text-align:center; border-radius:0 0 12px 12px; font-size:13px;"">
                            <p style=""margin:0 0 8px 0;""><strong style=""color:white;"">{COMPANY_NAME}</strong> — Trái cây tươi ngon mỗi ngày 🍎</p>
                            <p style=""margin:0; color:#95a5a6;"">Nếu có thắc mắc, liên hệ: <a href=""mailto:{SUPPORT_EMAIL}"" style=""color:#2ecc71;"">{SUPPORT_EMAIL}</a></p>
                            <p style=""margin:8px 0 0 0; color:#7f8c8d; font-size:12px;"">Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    // Gửi email qua SMTP (MailKit), cấu hình đọc từ DB thông qua ISettingsService
    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var host = await _settingsService.GetSettingAsync(SettingKeys.SmtpHost);
        var portStr = await _settingsService.GetSettingAsync(SettingKeys.SmtpPort);
        var username = await _settingsService.GetSettingAsync(SettingKeys.SmtpUsername);
        var password = await _settingsService.GetSettingAsync(SettingKeys.SmtpPassword);
        var enableSslStr = await _settingsService.GetSettingAsync(SettingKeys.SmtpEnableSsl);
        var senderName = await _settingsService.GetSettingAsync(SettingKeys.SmtpSenderName) ?? COMPANY_NAME;
        var senderEmail = await _settingsService.GetSettingAsync(SettingKeys.SmtpSenderEmail);

        // Nếu chưa cấu hình SMTP thì bỏ qua, log để debug
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("SMTP chưa được cấu hình. Email tới {ToEmail} không được gửi.", toEmail);
            _logger.LogInformation("[DEV] Subject: {Subject} | To: {To}", subject, toEmail);
            return true;
        }

        int port = int.TryParse(portStr, out var p) ? p : 587;
        bool enableSsl = !bool.TryParse(enableSslStr, out var ssl) || ssl;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail ?? username));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        var secureOption = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(host, port, secureOption);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email gửi thành công tới {ToEmail}", toEmail);
        return true;
    }

    // Template HTML email thông báo khóa tài khoản
    private string GenerateAccountLockedEmailBody(
        string customerName,
        string violationType,
        string reason,
        string lockType,
        DateTime? expiresAt)
    {
        var lockTypeText = lockType == "Temporary" ? "tạm thời" : "vĩnh viễn";
        var expirationText = expiresAt.HasValue
            ? $"<p><strong>Thời gian hết hạn:</strong> {expiresAt.Value:dd/MM/yyyy HH:mm}</p>"
            : "<p><strong>Thời gian hết hạn:</strong> Không xác định (khóa vĩnh viễn)</p>";

        return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo khóa tài khoản</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border: 1px solid #dee2e6; }}
        .info-box {{ background-color: #fff; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0; }}
        .appeal-section {{ background-color: #e7f3ff; border-left: 4px solid #0d6efd; padding: 15px; margin: 20px 0; }}
        .footer {{ background-color: #343a40; color: #adb5bd; padding: 20px; text-align: center; font-size: 12px; border-radius: 0 0 8px 8px; }}
        h1 {{ margin: 0; font-size: 24px; }}
        h2 {{ color: #dc3545; font-size: 18px; }}
        .highlight {{ color: #dc3545; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>⚠️ Thông báo khóa tài khoản</h1>
    </div>

    <div class=""content"">
        <p>Xin chào <strong>{customerName}</strong>,</p>
        <p>Chúng tôi rất tiếc phải thông báo rằng tài khoản của bạn tại <strong>{COMPANY_NAME}</strong>
        đã bị <span class=""highlight"">khóa {lockTypeText}</span>.</p>

        <div class=""info-box"">
            <h2>📋 Chi tiết khóa tài khoản</h2>
            <p><strong>Loại vi phạm:</strong> {violationType}</p>
            <p><strong>Lý do chi tiết:</strong></p>
            <p style=""padding-left: 15px; border-left: 2px solid #ccc;"">{reason}</p>
            <p><strong>Loại khóa:</strong> {(lockType == "Temporary" ? "Tạm thời" : "Vĩnh viễn")}</p>
            {expirationText}
        </div>

        <div class=""appeal-section"">
            <h2>📝 Hướng dẫn khiếu nại</h2>
            <p>Nếu bạn cho rằng đây là một sự nhầm lẫn hoặc muốn khiếu nại quyết định này,
            vui lòng liên hệ với chúng tôi qua:</p>
            <ul>
                <li><strong>Email:</strong> <a href=""mailto:{SUPPORT_EMAIL}"">{SUPPORT_EMAIL}</a></li>
                <li><strong>Tiêu đề email:</strong> [Khiếu nại] Yêu cầu xem xét khóa tài khoản - {customerName}</li>
            </ul>
            <p>Trong email khiếu nại, vui lòng cung cấp:</p>
            <ol>
                <li>Họ tên đầy đủ và email đăng ký</li>
                <li>Lý do bạn cho rằng quyết định khóa là không chính xác</li>
                <li>Bất kỳ bằng chứng hoặc thông tin bổ sung nào</li>
            </ol>
            <p><em>Chúng tôi sẽ xem xét và phản hồi trong vòng 3-5 ngày làm việc.</em></p>
        </div>

        <p>Chúng tôi hiểu rằng đây có thể là tin không vui, nhưng chúng tôi cam kết
        duy trì một môi trường an toàn và công bằng cho tất cả khách hàng.</p>

        <p>Trân trọng,<br><strong>Đội ngũ {COMPANY_NAME}</strong></p>
    </div>

    <div class=""footer"">
        <p>© {DateTime.Now.Year} {COMPANY_NAME}. Tất cả quyền được bảo lưu.</p>
        <p>Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
        <p>Nếu bạn cần hỗ trợ, hãy liên hệ: {SUPPORT_EMAIL}</p>
    </div>
</body>
</html>";
    }

    // Template HTML email thông báo mở khóa tài khoản
    private string GenerateAccountUnlockedEmailBody(string customerName, string reason)
    {
        return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo mở khóa tài khoản</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border: 1px solid #dee2e6; }}
        .info-box {{ background-color: #fff; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
        .cta-section {{ text-align: center; margin: 30px 0; }}
        .cta-button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .footer {{ background-color: #343a40; color: #adb5bd; padding: 20px; text-align: center; font-size: 12px; border-radius: 0 0 8px 8px; }}
        h1 {{ margin: 0; font-size: 24px; }}
        h2 {{ color: #28a745; font-size: 18px; }}
        .highlight {{ color: #28a745; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>✅ Thông báo mở khóa tài khoản</h1>
    </div>

    <div class=""content"">
        <p>Xin chào <strong>{customerName}</strong>,</p>
        <p>Chúng tôi vui mừng thông báo rằng tài khoản của bạn tại <strong>{COMPANY_NAME}</strong>
        đã được <span class=""highlight"">mở khóa thành công</span>!</p>

        <div class=""info-box"">
            <h2>📋 Chi tiết mở khóa</h2>
            <p><strong>Lý do mở khóa:</strong></p>
            <p style=""padding-left: 15px; border-left: 2px solid #ccc;"">{reason}</p>
            <p><strong>Thời gian mở khóa:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
        </div>

        <p>Bạn có thể đăng nhập và sử dụng tất cả các dịch vụ của {COMPANY_NAME} như bình thường.</p>

        <div class=""cta-section"">
            <a href=""#"" class=""cta-button"">Đăng nhập ngay</a>
        </div>

        <div class=""info-box"" style=""border-left-color: #ffc107;"">
            <h2 style=""color: #856404;"">⚠️ Lưu ý quan trọng</h2>
            <p>Để tránh việc tài khoản bị khóa trong tương lai, vui lòng:</p>
            <ul>
                <li>Tuân thủ <a href=""#"">Điều khoản sử dụng</a> của {COMPANY_NAME}</li>
                <li>Không thực hiện các hành vi vi phạm chính sách</li>
                <li>Liên hệ hỗ trợ nếu có bất kỳ thắc mắc nào</li>
            </ul>
        </div>

        <p>Cảm ơn bạn đã là khách hàng của {COMPANY_NAME}. Chúng tôi rất vui được phục vụ bạn!</p>

        <p>Trân trọng,<br><strong>Đội ngũ {COMPANY_NAME}</strong></p>
    </div>

    <div class=""footer"">
        <p>© {DateTime.Now.Year} {COMPANY_NAME}. Tất cả quyền được bảo lưu.</p>
        <p>Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
        <p>Nếu bạn cần hỗ trợ, hãy liên hệ: {SUPPORT_EMAIL}</p>
    </div>
</body>
</html>";
    }

    // Template HTML email đặt lại mật khẩu
    private string GeneratePasswordResetEmailBody(string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Đặt lại mật khẩu</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #198754; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border: 1px solid #dee2e6; }}
        .cta-section {{ text-align: center; margin: 30px 0; }}
        .cta-button {{ display: inline-block; background-color: #198754; color: white; padding: 14px 36px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px; }}
        .note-box {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ background-color: #343a40; color: #adb5bd; padding: 20px; text-align: center; font-size: 12px; border-radius: 0 0 8px 8px; }}
        h1 {{ margin: 0; font-size: 22px; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🔐 Đặt lại mật khẩu</h1>
    </div>

    <div class=""content"">
        <p>Bạn vừa yêu cầu đặt lại mật khẩu tài khoản <strong>{COMPANY_NAME}</strong>.</p>
        <p>Nhấn vào nút bên dưới để tạo mật khẩu mới. Liên kết này sẽ <strong>hết hạn sau 15 phút</strong>.</p>

        <div class=""cta-section"">
            <a href=""{resetLink}"" class=""cta-button"">Đặt lại mật khẩu</a>
        </div>

        <div class=""note-box"">
            <strong>⚠️ Lưu ý bảo mật:</strong> Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này. Mật khẩu hiện tại của bạn sẽ không thay đổi.
        </div>

        <p>Hoặc copy đường link sau vào trình duyệt:<br>
        <small style=""word-break: break-all; color: #666;"">{resetLink}</small></p>

        <p>Trân trọng,<br><strong>Đội ngũ {COMPANY_NAME}</strong></p>
    </div>

    <div class=""footer"">
        <p>© {DateTime.Now.Year} {COMPANY_NAME}. Tất cả quyền được bảo lưu.</p>
        <p>Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
        <p>Nếu bạn cần hỗ trợ, hãy liên hệ: {SUPPORT_EMAIL}</p>
    </div>
</body>
</html>";
    }
}
