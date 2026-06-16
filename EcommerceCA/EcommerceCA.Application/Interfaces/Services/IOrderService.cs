using EcommerceCA.Application.DTOs.Order;

namespace EcommerceCA.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderResponseDto>       PlaceOrderAsync(string userId, PlaceOrderDto dto);
    Task<PaginatedOrderResult>   GetUserOrdersAsync(string userId, int page, int pageSize);
    Task<OrderResponseDto>       GetOrderByIdAsync(string userId, int orderId);
    Task<OrderResponseDto>       GetOrderByIdAsync(int orderId);
    Task<PaginatedOrderResult>   GetAllOrdersAsync(int page, int pageSize, string? status);
    Task<OrderResponseDto>       UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
}
