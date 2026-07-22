using Fruitables.Models;
using Fruitables.ViewModels;

namespace Fruitables.Services.Interfaces;

public interface IOrderHistoryService
{
    /// <summary>
    /// Lấy danh sách lịch sử đơn hàng của khách hàng với phân trang và lọc
    /// </summary>
    /// <param name="userId">ID của khách hàng</param>
    /// <param name="filter">Bộ lọc và tham số phân trang</param>
    /// <returns>Kết quả phân trang chứa danh sách đơn hàng</returns>
    Task<PagedResult<OrderSummaryViewModel>> GetOrderHistoryAsync(int userId, OrderHistoryFilterViewModel filter);

    /// <summary>
    /// Lấy chi tiết đơn hàng của khách hàng
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="userId">ID khách hàng (để kiểm tra quyền truy cập)</param>
    /// <returns>Chi tiết đơn hàng hoặc null nếu không tìm thấy/không có quyền</returns>
    Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId, int userId);

    /// <summary>
    /// Kiểm tra xem khách hàng có thể hủy đơn hàng không
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="userId">ID khách hàng</param>
    /// <returns>True nếu có thể hủy, False nếu không thể</returns>
    Task<bool> CanCancelOrderAsync(int orderId, int userId);

    /// <summary>
    /// Hủy đơn hàng của khách hàng
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="userId">ID khách hàng</param>
    /// <param name="reason">Lý do hủy đơn hàng</param>
    /// <returns>True nếu hủy thành công, False nếu thất bại</returns>
    Task<bool> CancelOrderAsync(int orderId, int userId, string reason);

    /// <summary>
    /// Lấy lịch sử thay đổi trạng thái của đơn hàng
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Danh sách lịch sử thay đổi trạng thái</returns>
    Task<List<OrderStatusHistoryViewModel>> GetOrderStatusHistoryAsync(int orderId);

    /// <summary>
    /// Tra cứu đơn hàng theo số điện thoại (không cần đăng nhập)
    /// </summary>
    /// <param name="phone">Số điện thoại khách hàng</param>
    /// <returns>Danh sách đơn hàng tìm được</returns>
    Task<List<OrderSummaryViewModel>> GetOrdersByPhoneAsync(string phone);

    /// <summary>
    /// Lấy chi tiết đơn hàng theo orderId + phone (xác minh quyền cho guest)
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="phone">Số điện thoại để xác minh quyền sở hữu</param>
    /// <returns>Chi tiết đơn hàng hoặc null nếu không tìm thấy / không khớp phone</returns>
    Task<OrderDetailViewModel?> GetOrderDetailByPhoneAsync(int orderId, string phone);
}