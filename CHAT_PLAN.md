# SignalR Chat Khách/Admin Trên Trang Contact

## Summary
- Thêm chat realtime giữa khách đăng nhập hoặc khách vãng lai với `Admin`/`SuperAdmin`.
- Khách chat tại trang `Contact`; khách đăng nhập tự lấy tên/sđt từ tài khoản, khách vãng lai nhập tên và sđt trước khi chat.
- Admin có trang `/Admin/Chat` để xem danh sách khách, chọn từng cuộc chat, nhắn tin realtime và xem lịch sử sau khi refresh.

## Key Changes
- Mở rộng `EcommerceHub` hiện có tại `/hubs/ecommerce`:
  - Admin/SuperAdmin tự join group `Admins`.
  - Khách join group theo `Conversation:{id}`.
  - Hub methods chính: `StartContactChat(name, phone)`, `SendCustomerMessage(conversationId, message)`, `JoinAdminConversation(conversationId)`, `SendAdminMessage(conversationId, message)`, `MarkConversationRead(conversationId)`.
  - Validate quyền: khách chỉ gửi vào conversation thuộc user/session của mình; admin chỉ dùng được khi role là `Admin` hoặc `SuperAdmin`.

- Thêm lưu DB:
  - `ChatConversation`: lưu `UserId?`, `GuestSessionId`, `CustomerName`, `CustomerPhone`, `Status`, `LastMessageAt`, `LastMessagePreview`, `UnreadByAdminCount`, `UnreadByCustomerCount`, `CreatedAt`, `UpdatedAt`.
  - `ChatMessage`: lưu `ConversationId`, `SenderType` (`Customer`/`Admin`), `SenderUserId?`, `Message`, `CreatedAt`, `IsRead`.
  - Thêm `DbSet`, relationship, indexes theo `LastMessageAt`, `UserId`, `GuestSessionId`, `ConversationId`.

- Thêm service/controller:
  - Tạo `IChatService`/`ChatService` xử lý tạo/lấy conversation, lưu message, load danh sách admin, load lịch sử chat.
  - Tạo `Areas/Admin/Controllers/ChatController` với `[Authorize(Roles = "Admin,SuperAdmin")]`.
  - View admin hiển thị layout 2 cột: list khách bên trái, khung chat bên phải, badge tin chưa đọc.

- UI khách tại `Views/Contact/Index.cshtml`:
  - Thêm khung chat dưới hoặc cạnh form contact hiện tại.
  - Nếu khách chưa đăng nhập: hiển thị form nhỏ gồm `Tên`, `Số điện thoại`, nút bắt đầu chat.
  - Nếu khách đã đăng nhập: tự fill tên/sđt từ `User` và cho mở chat ngay.
  - Tin nhắn realtime qua `window.ecommerceHub`; khi admin trả lời thì append ngay vào khung chat.

- UI admin:
  - Thêm menu sidebar “Chat khách hàng”.
  - Admin thấy danh sách khách gồm tên, sđt, thời gian tin cuối, preview, trạng thái online nếu có.
  - Khi khách gửi tin mới: admin đang ở trang chat nhận realtime; nếu chưa chọn conversation thì list cập nhật và tăng badge.

## Test Plan
- Chạy migration tạo bảng chat và build project.
- Test khách vãng lai: nhập tên/sđt, gửi tin, admin thấy khách trong list và trả lời được.
- Test khách đăng nhập: tên/sđt lấy từ tài khoản, refresh vẫn thấy lịch sử.
- Test phân quyền: customer/guest không gọi được admin methods; user không thuộc conversation không đọc/gửi được.
- Test realtime: mở 2 browser, gửi qua lại không cần refresh; admin refresh vẫn load lại list/lịch sử từ DB.

## Assumptions
- Chat được lưu DB theo lựa chọn “Lưu DB”.
- `Admin` và `SuperAdmin` đều được chat với khách.
- V1 chỉ hỗ trợ text message, chưa hỗ trợ file/ảnh, emoji picker, typing indicator, hoặc phân công admin cụ thể.
- Với khách vãng lai, định danh bằng `SessionId` hiện có trong `ContactController`; nếu session hết hạn thì hệ thống tạo conversation mới.
