# Thiết Kế Cơ Sở Dữ Liệu — Fruitables

---

## Nhóm 1: Người Dùng & Phân Quyền

**Users** (Id, Name, Email, Password, Phone, Avatar, Role, IsActive, GoogleId, LastLoginAt,
ResetPasswordToken, ResetPasswordTokenExpiresAt, CurrentLockType, LockReason,
LockViolationType, LockedAt, LockExpiresAt, LockedByAdminId, CreatedAt, UpdatedAt)

**Roles** (Id, Name, Description, IsActive, CreatedAt, UpdatedAt)

**Permissions** (Id, Name, Description, Module, CreatedAt)

**UserRoleMappings** (Id, UserId*, RoleId*, AssignedAt, AssignedByAdminId*)

**RolePermissions** (Id, RoleId*, PermissionId*, AssignedAt, AssignedByAdminId*)

**RbacAuditLogs** (Id, Action, EntityType, EntityId, ChangedByAdminId*, ChangedAt, OldValue, NewValue)

**UserAccountLogs** (Id, UserId*, AdminId*, Action, LockType, ViolationType, Reason, ExpiresAt, IpAddress, UserAgent, CreatedAt)

---

## Nhóm 2: Sản Phẩm & Danh Mục

**Categories** (Id, Name, Slug, Description, Image, ParentId*, SortOrder, IsActive, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)

**Products** (Id, CategoryId*, Name, Slug, Description, ShortDescription, Price, SalePrice, Unit, Weight, CountryOrigin, Quality, StockQuantity, MinOrderQuantity, IsFeatured, IsActive, IsDeleted, DeletedAt, AverageRating, ReviewCount, CreatedAt, UpdatedAt)

**ProductImages** (Id, ProductId*, ImageUrl, IsPrimary, SortOrder)

**ProductVariants** (Id, ProductId*, SKU, Name, Price, SalePrice, StockQuantity, IsActive, CreatedAt)

**ProductTags** (Id, Name, Slug)

> Bảng nối nhiều-nhiều: **ProductProductTag** (ProductId*, TagId*)

**ProductLogs** (Id, ProductId*, AdminId*, Action, Details, CreatedAt)

---

## Nhóm 3: Giỏ Hàng

**Carts** (Id, UserId*, SessionId, CreatedAt, UpdatedAt)

**CartItems** (Id, CartId*, ProductId*, Quantity, Price)

---

## Nhóm 4: Đơn Hàng

**Orders** (Id, UserId*, AddressId*, OrderNumber, Status, Subtotal, ShippingFee, Discount, Total, PaymentMethod, PaymentStatus, ShippingMethod, ShippingSnapshot, Notes, CancelReason, RowVersion, CreatedAt)

**OrderItems** (Id, OrderId*, ProductId*, ProductName, Quantity, Price, Total)

**OrderStatusHistories** (Id, OrderId*, OldStatus, NewStatus, AdminId*, Notes, CreatedAt)

**OrderNotes** (Id, OrderId*, AdminId*, AdminName, Content, CreatedAt)

---

## Nhóm 5: Đánh Giá

**Reviews** (Id, ProductId*, UserId*, Rating, Comment, Status, IsHidden, HiddenReason, HiddenByAdminId*, HiddenAt, IsDeleted, DeletedByAdminId*, DeletedAt, IsVerifiedPurchase, HelpfulCount, ReportCount, CreatedAt, UpdatedAt)

**ReviewReports** (Id, ReviewId*, ReportedByUserId*, Reason, Description, Status, HandledByAdminId*, HandledAt, CreatedAt)

**ReviewHelpfuls** (Id, ReviewId*, UserId*, CreatedAt)

---

## Nhóm 6: Địa Chỉ & Vận Chuyển

**Addresses** (Id, UserId*, FullName, Phone, ProvinceCode, ProvinceName, DistrictCode, DistrictName, WardCode, WardName, StreetAddress, IsDefault, CreatedAt, UpdatedAt)

---

## Nhóm 7: Cấu Hình & Tiện Ích

**Settings** (Id, Key, Value, Group)

**Coupons** (Id, Code, Type, Value, MinOrderAmount, MaxUses, UsedCount, StartDate, EndDate, IsActive)

**ContactMessages** (Id, Name, Email, Message, IsRead, CreatedAt)

**Testimonials** (Id, UserId*, Name, Profession, Avatar, Content, Rating, IsActive, CreatedAt)

**Wishlists** (Id, UserId*, ProductId*, CreatedAt)

---

## Ghi chú

- (*) là khóa ngoại (Foreign Key)
- Gạch dưới `Id` là khóa chính (Primary Key)
- `ShippingConfig` và `ShippingInfo` là các class cấu hình, không lưu trực tiếp thành bảng riêng mà được serialize vào bảng **Settings**
