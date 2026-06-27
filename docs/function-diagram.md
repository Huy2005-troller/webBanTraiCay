# Sơ Đồ Chức Năng Hệ Thống Fruitables

---

## 1. Khách Vãng Lai (Guest)

```
KHÁCH VÃNG LAI
├── Xem trang chủ
├── Xem danh sách sản phẩm
│   ├── Tìm kiếm sản phẩm
│   └── Lọc theo danh mục / giá
├── Xem chi tiết sản phẩm
│   └── Xem đánh giá của sản phẩm
├── Liên hệ (gửi form)
└── Xác thực tài khoản
    ├── Đăng ký tài khoản
    ├── Đăng nhập (email/password)
    ├── Đăng nhập bằng Google
    └── Quên mật khẩu / Reset mật khẩu
```

---

## 2. Khách Hàng (Customer)

```
KHÁCH HÀNG
├── [Kế thừa toàn bộ chức năng Khách Vãng Lai]
│
├── Giỏ hàng
│   ├── Thêm sản phẩm vào giỏ
│   ├── Cập nhật số lượng
│   └── Xóa sản phẩm khỏi giỏ
│
├── Thanh toán & Đặt hàng
│   ├── Chọn địa chỉ giao hàng
│   ├── Xem phí vận chuyển
│   ├── Áp dụng mã giảm giá
│   └── Xác nhận đặt hàng
│
├── Quản lý đơn hàng
│   ├── Xem lịch sử đơn hàng
│   ├── Xem chi tiết đơn hàng
│   └── Hủy đơn hàng
│
├── Đánh giá sản phẩm
│   ├── Viết đánh giá (rating 1-5 sao + comment)
│   ├── Chỉnh sửa đánh giá (trong 24h)
│   ├── Xóa đánh giá của mình
│   ├── Đánh dấu đánh giá hữu ích
│   └── Báo cáo đánh giá vi phạm
│
├── Quản lý hồ sơ cá nhân
│   ├── Xem & chỉnh sửa thông tin cá nhân
│   └── Upload ảnh đại diện
│
└── Quản lý địa chỉ
    ├── Thêm địa chỉ mới
    ├── Chỉnh sửa địa chỉ
    └── Xóa địa chỉ
```

---

## 3. Quản Trị Viên (Admin)

```
QUẢN TRỊ VIÊN
├── Dashboard
│   └── Xem tổng quan hệ thống (đơn hàng, doanh thu, người dùng)
│
├── Quản lý sản phẩm
│   ├── Xem danh sách sản phẩm
│   ├── Thêm sản phẩm mới
│   │   ├── Upload nhiều hình ảnh
│   │   └── Thêm biến thể (variants/SKU)
│   ├── Chỉnh sửa sản phẩm
│   ├── Xóa tạm thời (soft delete) & khôi phục
│   └── Quản lý danh mục (cây danh mục)
│       ├── Thêm / sửa / xóa danh mục
│       └── Quản lý danh mục cha-con
│
├── Quản lý đơn hàng
│   ├── Xem danh sách đơn hàng
│   ├── Xem chi tiết đơn hàng
│   ├── Cập nhật trạng thái đơn hàng
│   │   └── Pending → Processing → Shipped → Delivered
│   ├── Cập nhật trạng thái thanh toán
│   │   └── Unpaid → Paid → Refunded
│   └── Hủy đơn hàng (có lý do, hoàn kho tự động)
│
├── Quản lý người dùng
│   ├── Xem danh sách khách hàng
│   ├── Xem chi tiết & lịch sử mua hàng
│   ├── Khóa tài khoản (tạm thời / vĩnh viễn)
│   └── Mở khóa tài khoản
│
├── Quản lý đánh giá
│   ├── Xem tất cả đánh giá
│   ├── Ẩn / hiện đánh giá
│   ├── Xóa đánh giá vi phạm
│   ├── Xem & xử lý báo cáo vi phạm
│   └── Xem thống kê đánh giá
│
└── Thống kê doanh thu
    ├── Xem doanh thu theo khoảng thời gian
    ├── Doanh thu theo danh mục sản phẩm
    ├── Top sản phẩm bán chạy
    ├── Biểu đồ xu hướng (ngày/tuần/tháng)
    ├── Thống kê đơn hàng bị hủy
    └── Xuất báo cáo Excel
```

---

## 4. Quản Trị Cấp Cao (SuperAdmin)

```
QUẢN TRỊ CẤP CAO
├── [Kế thừa toàn bộ chức năng Admin]
│
├── Quản lý phân quyền (RBAC)
│   ├── Quản lý Role (vai trò)
│   │   ├── Tạo / sửa / xóa role
│   │   └── Gán permission cho role
│   ├── Quản lý Permission (quyền)
│   │   ├── Xem danh sách permission
│   │   └── Tạo permission mới
│   ├── Gán role cho người dùng
│   └── Xem RBAC Audit Log
│
├── Cấu hình hệ thống
│   ├── Thông tin chung (tên, logo, liên hệ)
│   ├── Cấu hình email / SMTP
│   ├── Cấu hình Google OAuth
│   ├── Cấu hình mạng xã hội
│   ├── Cấu hình banner
│   ├── Cấu hình SEO
│   ├── Cấu hình phí vận chuyển theo vùng
│   └── Cấu hình bảo mật (token expiry, rate limit)
│
└── Công cụ hệ thống (Diagnostics)
    ├── Xem trạng thái hệ thống
    └── Chạy RBAC Migration
```

---

## Tổng hợp quan hệ Actor — Chức năng

```
┌─────────────────────────────────────────────────────────────┐
│                     HỆ THỐNG FRUITABLES                     │
├──────────────┬──────────────┬──────────────┬────────────────┤
│   GUEST      │  CUSTOMER    │    ADMIN     │  SUPER ADMIN   │
│              │              │              │                │
│ Xem SP       │ + Giỏ hàng  │ + Dashboard  │ + RBAC         │
│ Tìm kiếm     │ + Đặt hàng  │ + Quản lý SP │ + Cấu hình     │
│ Liên hệ      │ + Đơn hàng  │ + Quản lý ĐH │   hệ thống     │
│ Đăng ký      │ + Đánh giá  │ + Quản lý ND │ + Diagnostics  │
│ Đăng nhập    │ + Hồ sơ     │ + Kiểm duyệt │                │
│ Reset PW     │ + Địa chỉ   │ + Doanh thu  │                │
└──────────────┴──────────────┴──────────────┴────────────────┘
     (mở rộng dần theo cấp độ phân quyền →)
```
