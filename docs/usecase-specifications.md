# Đặc Tả Use Case — Hệ Thống Fruitables

---

## UC01 — Đăng Ký Tài Khoản

| | |
|---|---|
| **Name** | Đăng ký tài khoản | **Code** | UC01 |
| **Description** | Khách vãng lai điền thông tin để tạo tài khoản khách hàng mới trên hệ thống |
| **Actor** | Khách vãng lai (Guest) | **Trigger** | Người dùng truy cập trang Đăng ký và nhấn nút "Tạo tài khoản" |
| **Pre-condition** | Người dùng chưa đăng nhập, email chưa được đăng ký trong hệ thống |
| **Post-condition** | Tài khoản mới được tạo, người dùng được chuyển đến trang đăng nhập |
| **Error situations** | 1. Email đã tồn tại trong hệ thống<br>2. Mật khẩu không đủ độ mạnh<br>3. Thông tin nhập không hợp lệ (thiếu trường bắt buộc) |
| **System state in error situations** | Hiển thị thông báo lỗi tương ứng, giữ nguyên form đăng ký |
| **Standard flow/process** | 1. Người dùng truy cập trang Đăng ký<br>2. Nhập họ tên, email, mật khẩu<br>3. Hệ thống kiểm tra email chưa tồn tại<br>4. Hệ thống mã hóa mật khẩu bằng BCrypt<br>5. Tạo bản ghi User mới với Role = Customer<br>6. Chuyển hướng đến trang đăng nhập |
| **Alternative Flow 1** | Email đã tồn tại: Hệ thống hiển thị "Email này đã được sử dụng" |
| **Alternative Flow 2** | Người dùng chọn "Đăng nhập bằng Google" → chuyển sang luồng Google OAuth |

---

## UC02 — Xem & Tìm Kiếm Sản Phẩm

| | |
|---|---|
| **Name** | Xem & tìm kiếm sản phẩm | **Code** | UC02 |
| **Description** | Người dùng duyệt danh sách sản phẩm, tìm kiếm theo từ khóa hoặc lọc theo danh mục, giá |
| **Actor** | Khách vãng lai, Khách hàng | **Trigger** | Người dùng truy cập trang Shop hoặc nhập từ khóa vào ô tìm kiếm |
| **Pre-condition** | Hệ thống có sản phẩm đang hoạt động (IsActive = true) |
| **Post-condition** | Danh sách sản phẩm phù hợp được hiển thị |
| **Error situations** | 1. Không có sản phẩm nào khớp với từ khóa<br>2. Danh mục được chọn không có sản phẩm nào |
| **System state in error situations** | Hiển thị thông báo "Không tìm thấy sản phẩm phù hợp" |
| **Standard flow/process** | 1. Người dùng vào trang Shop<br>2. Hệ thống hiển thị danh sách sản phẩm (phân trang)<br>3. Người dùng nhập từ khóa hoặc chọn danh mục/lọc giá<br>4. Hệ thống truy vấn DB và trả về danh sách khớp<br>5. Hiển thị kết quả kèm ảnh, tên, giá |
| **Alternative Flow 1** | Không có kết quả: hiển thị thông báo và gợi ý xem sản phẩm khác |

---

## UC03 — Xem Chi Tiết Sản Phẩm

| | |
|---|---|
| **Name** | Xem chi tiết sản phẩm | **Code** | UC03 |
| **Description** | Người dùng xem thông tin đầy đủ của một sản phẩm bao gồm mô tả, giá, hình ảnh và đánh giá |
| **Actor** | Khách vãng lai, Khách hàng | **Trigger** | Người dùng nhấn vào một sản phẩm trong danh sách |
| **Pre-condition** | Sản phẩm tồn tại và đang hoạt động |
| **Post-condition** | Trang chi tiết sản phẩm được hiển thị đầy đủ thông tin |
| **Error situations** | 1. Sản phẩm không tồn tại hoặc đã bị xóa<br>2. Lỗi tải hình ảnh |
| **System state in error situations** | Trả về trang 404 nếu sản phẩm không tồn tại |
| **Standard flow/process** | 1. Người dùng nhấn vào sản phẩm<br>2. Hệ thống truy vấn DB theo ID/Slug<br>3. Hiển thị tên, mô tả, giá, hình ảnh, biến thể, số tồn kho<br>4. Hiển thị đánh giá và điểm trung bình<br>5. Hiển thị sản phẩm liên quan |
| **Alternative Flow 1** | Sản phẩm hết hàng: hiển thị trạng thái "Hết hàng", ẩn nút thêm vào giỏ |

---

## UC04 — Quản Lý Giỏ Hàng

| | |
|---|---|
| **Name** | Quản lý giỏ hàng | **Code** | UC04 |
| **Description** | Khách hàng thêm, cập nhật số lượng hoặc xóa sản phẩm khỏi giỏ hàng |
| **Actor** | Khách hàng | **Trigger** | Người dùng nhấn "Thêm vào giỏ" từ trang sản phẩm hoặc mở trang giỏ hàng |
| **Pre-condition** | Người dùng đã đăng nhập; sản phẩm còn hàng |
| **Post-condition** | Giỏ hàng được cập nhật, tổng tiền tính lại |
| **Error situations** | 1. Sản phẩm hết hàng hoặc số lượng yêu cầu vượt tồn kho<br>2. Sản phẩm đã bị xóa khỏi hệ thống |
| **System state in error situations** | Hiển thị thông báo lỗi, không cập nhật giỏ hàng |
| **Standard flow/process** | 1. Người dùng chọn sản phẩm và nhấn "Thêm vào giỏ"<br>2. Hệ thống kiểm tra tồn kho<br>3. Thêm/cập nhật CartItem trong DB<br>4. Cập nhật badge số lượng trên icon giỏ hàng<br>5. Hiển thị thông báo thêm thành công |
| **Alternative Flow 1** | Sản phẩm đã có trong giỏ: tăng số lượng thay vì thêm mới |
| **Alternative Flow 2** | Người dùng xóa sản phẩm: hệ thống xóa CartItem, tính lại tổng |

---

## UC05 — Đặt Hàng

| | |
|---|---|
| **Name** | Đặt hàng | **Code** | UC05 |
| **Description** | Khách hàng xác nhận đơn hàng từ giỏ hàng, chọn địa chỉ giao hàng và phương thức thanh toán |
| **Actor** | Khách hàng | **Trigger** | Người dùng nhấn "Tiến hành thanh toán" từ trang giỏ hàng |
| **Pre-condition** | Giỏ hàng không rỗng; người dùng đã đăng nhập và có địa chỉ giao hàng |
| **Post-condition** | Đơn hàng mới được tạo với trạng thái Pending; tồn kho giảm tương ứng |
| **Error situations** | 1. Sản phẩm trong giỏ hết hàng tại thời điểm đặt<br>2. Địa chỉ giao hàng chưa được chọn<br>3. Lỗi kết nối DB khi lưu đơn hàng |
| **System state in error situations** | Hiển thị thông báo lỗi, đơn hàng không được tạo, giỏ hàng giữ nguyên |
| **Standard flow/process** | 1. Người dùng vào trang Checkout<br>2. Hệ thống hiển thị danh sách sản phẩm trong giỏ<br>3. Người dùng chọn địa chỉ giao hàng<br>4. Hệ thống tính phí vận chuyển theo vùng<br>5. Người dùng chọn phương thức thanh toán<br>6. Người dùng nhấn "Đặt hàng"<br>7. Hệ thống tạo Order, lưu snapshot địa chỉ, trừ tồn kho<br>8. Xóa giỏ hàng, chuyển đến trang xác nhận đơn hàng |
| **Alternative Flow 1** | Người dùng áp mã giảm giá: hệ thống kiểm tra hợp lệ và trừ tiền giảm vào tổng |
| **Alternative Flow 2** | Sản phẩm hết hàng: thông báo và yêu cầu cập nhật giỏ hàng |

---

## UC06 — Quản Lý Đơn Hàng (Khách Hàng)

| | |
|---|---|
| **Name** | Quản lý đơn hàng | **Code** | UC06 |
| **Description** | Khách hàng xem lịch sử, chi tiết đơn hàng và có thể hủy đơn khi còn ở trạng thái Pending |
| **Actor** | Khách hàng | **Trigger** | Người dùng vào mục "Đơn hàng của tôi" trong tài khoản |
| **Pre-condition** | Người dùng đã đăng nhập và có ít nhất một đơn hàng |
| **Post-condition** | Hiển thị danh sách đơn hàng; nếu hủy thì trạng thái chuyển sang Cancelled |
| **Error situations** | 1. Hủy đơn hàng đã được xử lý (trạng thái Shipped/Delivered)<br>2. Lỗi kết nối DB |
| **System state in error situations** | Hiển thị thông báo "Không thể hủy đơn hàng này" |
| **Standard flow/process** | 1. Người dùng vào trang lịch sử đơn hàng<br>2. Hệ thống truy vấn và hiển thị danh sách đơn hàng<br>3. Người dùng chọn xem chi tiết một đơn<br>4. Hệ thống hiển thị sản phẩm, giá, trạng thái, địa chỉ<br>5. Nếu đơn đang Pending: hiển thị nút "Hủy đơn"<br>6. Người dùng nhấn hủy, nhập lý do<br>7. Hệ thống cập nhật trạng thái = Cancelled |
| **Alternative Flow 1** | Đơn đã Shipped/Delivered: ẩn nút hủy, chỉ cho xem |

---

## UC07 — Đánh Giá Sản Phẩm

| | |
|---|---|
| **Name** | Đánh giá sản phẩm | **Code** | UC07 |
| **Description** | Khách hàng viết đánh giá, chấm điểm sản phẩm đã mua; có thể chỉnh sửa trong 24h hoặc xóa đánh giá của mình |
| **Actor** | Khách hàng | **Trigger** | Người dùng nhấn "Viết đánh giá" trên trang chi tiết sản phẩm |
| **Pre-condition** | Người dùng đã đăng nhập và đã mua sản phẩm; chưa vượt giới hạn 5 đánh giá/ngày |
| **Post-condition** | Đánh giá được lưu; điểm trung bình và số lượng đánh giá của sản phẩm được cập nhật |
| **Error situations** | 1. Người dùng chưa mua sản phẩm<br>2. Vượt giới hạn 5 đánh giá/ngày<br>3. Nội dung chứa từ ngữ không phù hợp |
| **System state in error situations** | Hiển thị thông báo lỗi tương ứng, đánh giá không được lưu |
| **Standard flow/process** | 1. Người dùng nhấn "Viết đánh giá"<br>2. Hệ thống kiểm tra điều kiện (đã mua, rate limit)<br>3. Người dùng chọn số sao (1–5) và nhập nhận xét<br>4. Hệ thống lọc từ ngữ không phù hợp<br>5. Lưu Review, cập nhật AverageRating và ReviewCount của sản phẩm<br>6. Hiển thị đánh giá mới trên trang sản phẩm |
| **Alternative Flow 1** | Chỉnh sửa đánh giá: chỉ cho phép trong vòng 24h sau khi tạo |
| **Alternative Flow 2** | Xóa đánh giá: soft delete, không hiển thị nhưng vẫn lưu trong DB |

---

## UC08 — Quản Lý Hồ Sơ Cá Nhân

| | |
|---|---|
| **Name** | Quản lý hồ sơ cá nhân | **Code** | UC08 |
| **Description** | Khách hàng xem và cập nhật thông tin cá nhân, ảnh đại diện và địa chỉ giao hàng |
| **Actor** | Khách hàng | **Trigger** | Người dùng vào trang "Tài khoản của tôi" |
| **Pre-condition** | Người dùng đã đăng nhập |
| **Post-condition** | Thông tin cá nhân được cập nhật trong DB |
| **Error situations** | 1. Định dạng ảnh không hợp lệ hoặc kích thước quá lớn<br>2. Số điện thoại sai định dạng |
| **System state in error situations** | Hiển thị thông báo lỗi, dữ liệu không được lưu |
| **Standard flow/process** | 1. Người dùng vào trang hồ sơ<br>2. Hệ thống hiển thị thông tin hiện tại<br>3. Người dùng chỉnh sửa tên, SĐT hoặc upload ảnh đại diện<br>4. Hệ thống validate dữ liệu<br>5. Lưu thay đổi vào DB, hiển thị thông báo thành công |
| **Alternative Flow 1** | Upload ảnh đại diện: hệ thống resize và lưu vào thư mục uploads |

---

## UC09 — Quản Lý Sản Phẩm (Admin)

| | |
|---|---|
| **Name** | Quản lý sản phẩm | **Code** | UC09 |
| **Description** | Admin thêm mới, chỉnh sửa, xóa tạm thời hoặc khôi phục sản phẩm trong hệ thống |
| **Actor** | Quản trị viên | **Trigger** | Admin truy cập trang Quản lý sản phẩm trong Admin Panel |
| **Pre-condition** | Admin đã đăng nhập và có quyền products.create / products.update / products.delete |
| **Post-condition** | Sản phẩm được thêm/cập nhật/xóa trong DB; thay đổi phản ánh ngay trên giao diện khách hàng |
| **Error situations** | 1. Tên sản phẩm hoặc slug bị trùng<br>2. Hình ảnh upload sai định dạng<br>3. Giá nhập không hợp lệ (âm hoặc để trống) |
| **System state in error situations** | Hiển thị lỗi validation, sản phẩm không được lưu |
| **Standard flow/process** | 1. Admin vào trang danh sách sản phẩm<br>2. Chọn thêm mới hoặc chỉnh sửa sản phẩm<br>3. Nhập thông tin: tên, mô tả, giá, danh mục, tồn kho<br>4. Upload hình ảnh sản phẩm<br>5. Hệ thống validate và lưu vào DB<br>6. Ghi ProductLog với hành động tương ứng |
| **Alternative Flow 1** | Xóa tạm thời: IsDeleted = true, sản phẩm ẩn khỏi giao diện khách nhưng có thể khôi phục |
| **Alternative Flow 2** | Khôi phục: IsDeleted = false, sản phẩm hiển thị lại |

---

## UC10 — Quản Lý Đơn Hàng (Admin)

| | |
|---|---|
| **Name** | Quản lý đơn hàng | **Code** | UC10 |
| **Description** | Admin xem, cập nhật trạng thái xử lý và thanh toán của đơn hàng, hoặc hủy đơn với lý do |
| **Actor** | Quản trị viên | **Trigger** | Admin vào trang Quản lý đơn hàng trong Admin Panel |
| **Pre-condition** | Admin đã đăng nhập và có quyền orders.view_all / orders.update_status |
| **Post-condition** | Trạng thái đơn hàng được cập nhật; lịch sử thay đổi được ghi vào OrderStatusHistory |
| **Error situations** | 1. Cập nhật trạng thái không hợp lệ (lùi trạng thái)<br>2. Xung đột dữ liệu (RowVersion conflict) khi nhiều admin cùng sửa |
| **System state in error situations** | Hiển thị thông báo lỗi, trạng thái đơn hàng giữ nguyên |
| **Standard flow/process** | 1. Admin xem danh sách đơn hàng, lọc theo trạng thái/ngày<br>2. Chọn xem chi tiết một đơn hàng<br>3. Cập nhật trạng thái: Pending → Processing → Shipped → Delivered<br>4. Hệ thống lưu lịch sử thay đổi vào OrderStatusHistory<br>5. Có thể thêm ghi chú nội bộ vào đơn hàng |
| **Alternative Flow 1** | Hủy đơn hàng: Admin nhập lý do, hệ thống cập nhật Cancelled và tự động hoàn kho |

---

## UC11 — Quản Lý Người Dùng

| | |
|---|---|
| **Name** | Quản lý người dùng | **Code** | UC11 |
| **Description** | Admin xem danh sách khách hàng, lịch sử mua hàng và thực hiện khóa/mở khóa tài khoản vi phạm |
| **Actor** | Quản trị viên | **Trigger** | Admin vào trang Quản lý người dùng trong Admin Panel |
| **Pre-condition** | Admin đã đăng nhập và có quyền users.view / users.lock |
| **Post-condition** | Trạng thái tài khoản người dùng được cập nhật; hành động được ghi vào UserAccountLog |
| **Error situations** | 1. Cố khóa tài khoản Admin/SuperAdmin<br>2. Lý do khóa bị bỏ trống |
| **System state in error situations** | Hiển thị thông báo lỗi, tài khoản không bị khóa |
| **Standard flow/process** | 1. Admin xem danh sách người dùng (phân trang)<br>2. Tìm kiếm hoặc xem chi tiết người dùng<br>3. Chọn Khóa tài khoản: chọn loại khóa (tạm thời/vĩnh viễn), nhập lý do<br>4. Hệ thống cập nhật IsActive, lưu thông tin khóa và ghi UserAccountLog<br>5. Gửi email thông báo cho người dùng |
| **Alternative Flow 1** | Mở khóa: Admin nhập lý do, hệ thống xóa thông tin khóa, IsActive = true |

---

## UC12 — Kiểm Duyệt Đánh Giá

| | |
|---|---|
| **Name** | Kiểm duyệt đánh giá | **Code** | UC12 |
| **Description** | Admin xem, ẩn/hiện hoặc xóa các đánh giá vi phạm; xử lý các báo cáo từ người dùng |
| **Actor** | Quản trị viên | **Trigger** | Admin vào trang Kiểm duyệt đánh giá hoặc có báo cáo vi phạm mới |
| **Pre-condition** | Admin đã đăng nhập và có quyền reviews.moderate |
| **Post-condition** | Đánh giá được ẩn/xóa; báo cáo được đánh dấu đã xử lý |
| **Error situations** | 1. Đánh giá đã bị xóa trước đó<br>2. Lý do xử lý bị bỏ trống |
| **System state in error situations** | Hiển thị thông báo lỗi, trạng thái đánh giá không thay đổi |
| **Standard flow/process** | 1. Admin xem danh sách đánh giá, lọc theo trạng thái<br>2. Xem nội dung đánh giá và các báo cáo liên quan<br>3. Chọn Ẩn đánh giá: nhập lý do, IsHidden = true<br>4. Hệ thống ghi audit log cho hành động<br>5. Admin xem tab Báo cáo, xử lý từng báo cáo (Resolved/Dismissed) |
| **Alternative Flow 1** | Xóa đánh giá: soft delete (IsDeleted = true), không hiển thị với bất kỳ ai |
| **Alternative Flow 2** | Hiện lại đánh giá đã ẩn: IsHidden = false |

---

## UC13 — Xem Thống Kê Doanh Thu

| | |
|---|---|
| **Name** | Xem thống kê doanh thu | **Code** | UC13 |
| **Description** | Admin xem báo cáo doanh thu theo thời gian, danh mục, sản phẩm bán chạy và xuất file Excel |
| **Actor** | Quản trị viên | **Trigger** | Admin vào trang Thống kê doanh thu trong Admin Panel |
| **Pre-condition** | Admin đã đăng nhập và có quyền dashboard.view_statistics |
| **Post-condition** | Báo cáo doanh thu được hiển thị theo bộ lọc đã chọn |
| **Error situations** | 1. Khoảng thời gian chọn không hợp lệ (ngày bắt đầu > ngày kết thúc)<br>2. Không có dữ liệu trong khoảng thời gian |
| **System state in error situations** | Hiển thị thông báo "Không có dữ liệu" hoặc lỗi bộ lọc |
| **Standard flow/process** | 1. Admin vào trang Doanh thu<br>2. Chọn khoảng thời gian (hoặc dùng preset: tuần/tháng/quý)<br>3. Hệ thống truy vấn DB, tính doanh thu thuần từ các đơn Delivered<br>4. Hiển thị biểu đồ xu hướng, doanh thu theo danh mục, top sản phẩm<br>5. Admin có thể xuất báo cáo ra file Excel |
| **Alternative Flow 1** | Xuất Excel: hệ thống tạo file .xlsx qua ClosedXML và trả về cho trình duyệt tải xuống |

---

## UC14 — Quản Lý Role & Phân Quyền (RBAC)

| | |
|---|---|
| **Name** | Quản lý Role & Phân quyền | **Code** | UC14 |
| **Description** | SuperAdmin tạo, sửa, xóa các Role; gán/thu hồi Permission cho Role và gán Role cho người dùng |
| **Actor** | Quản trị cấp cao (SuperAdmin) | **Trigger** | SuperAdmin vào trang Quản lý Role trong Admin Panel |
| **Pre-condition** | SuperAdmin đã đăng nhập và có quyền system.manage_rbac |
| **Post-condition** | Thay đổi quyền có hiệu lực ngay; mọi thao tác được ghi vào RbacAuditLog |
| **Error situations** | 1. Xóa Role đang được gán cho người dùng<br>2. Tên Role bị trùng<br>3. Gán Permission không tồn tại |
| **System state in error situations** | Hiển thị thông báo lỗi, thao tác không được thực hiện |
| **Standard flow/process** | 1. SuperAdmin xem danh sách Role<br>2. Tạo Role mới: nhập tên, mô tả<br>3. Gán các Permission vào Role<br>4. Gán Role cho người dùng cụ thể<br>5. Hệ thống lưu UserRoleMapping và ghi RbacAuditLog |
| **Alternative Flow 1** | Thu hồi Role: xóa UserRoleMapping, quyền của người dùng đó thay đổi ngay lập tức |
| **Alternative Flow 2** | Xem Audit Log: hiển thị toàn bộ lịch sử thay đổi quyền với người thực hiện và thời gian |

---

## UC15 — Cấu Hình Hệ Thống

| | |
|---|---|
| **Name** | Cấu hình hệ thống | **Code** | UC15 |
| **Description** | SuperAdmin cấu hình các thông số vận hành của hệ thống: thông tin shop, email SMTP, phí vận chuyển, bảo mật, OAuth |
| **Actor** | Quản trị cấp cao (SuperAdmin) | **Trigger** | SuperAdmin vào trang Cài đặt hệ thống |
| **Pre-condition** | SuperAdmin đã đăng nhập và có quyền settings.update |
| **Post-condition** | Cài đặt mới được lưu vào bảng Settings và có hiệu lực ngay |
| **Error situations** | 1. Cấu hình SMTP sai → không gửi được email<br>2. Google OAuth Client ID/Secret không hợp lệ<br>3. Phí vận chuyển nhập âm hoặc không phải số |
| **System state in error situations** | Hiển thị thông báo lỗi validate, cài đặt không được lưu |
| **Standard flow/process** | 1. SuperAdmin vào trang Settings<br>2. Chọn nhóm cấu hình cần sửa (chung/email/vận chuyển/bảo mật)<br>3. Cập nhật giá trị tương ứng<br>4. Hệ thống validate dữ liệu<br>5. Lưu cặp key-value vào bảng Settings<br>6. Hiển thị thông báo "Lưu thành công" |
| **Alternative Flow 1** | Cấu hình phí vận chuyển: nhập phí theo từng vùng (nội thành/ngoại thành/vùng xa) và ngưỡng miễn phí ship |
| **Alternative Flow 2** | Upload logo: hệ thống lưu đường dẫn file vào Settings với key = "site_logo" |
