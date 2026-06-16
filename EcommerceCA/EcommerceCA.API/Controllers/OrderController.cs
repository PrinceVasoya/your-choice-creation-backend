using EcommerceCA.Application.DTOs.Order;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Constants;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceCA.API.Controllers;

/// <summary>Order management — place orders, view history, admin status updates</summary>
[ApiController]
[Route("api/orders")]
[Authorize]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService) => _orderService = orderService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Place a new order from the current user's cart</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
    {
        var result = await _orderService.PlaceOrderAsync(CurrentUserId, dto);
        return StatusCode(201, ApiResponse<OrderResponseDto>.Created(result, "Order placed successfully."));
    }

    /// <summary>Get the current user's order history (paginated)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<OrderResponseDto>), 200)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetUserOrdersAsync(CurrentUserId, page, pageSize);
        return Ok(new PaginatedResponse<OrderResponseDto>
        {
            Data = result.Items,
            Meta = new PaginationMeta
            {
                TotalCount  = result.TotalCount,
                Page        = result.Page,
                PageSize    = result.PageSize,
                TotalPages  = result.TotalPages,
                HasPrevious = result.HasPrevious,
                HasNext     = result.HasNext
            }
        });
    }

    /// <summary>Get details of a specific order (must belong to the current user)</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(CurrentUserId, id);
        return Ok(ApiResponse<OrderResponseDto>.Ok(result));
    }

    /// <summary>Cancel a specific order (must belong to the current user and status must be Pending or Confirmed)</summary>
    [HttpPut("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(CurrentUserId, id);
        if (order.Status != "Pending" && order.Status != "Confirmed" && order.Status != "Processing")
        {
            return BadRequest(ApiResponse<object>.Fail("Cannot cancel order that is already shipped or delivered."));
        }
        
        var result = await _orderService.UpdateOrderStatusAsync(id, new UpdateOrderStatusDto { Status = "Cancelled" });
        return Ok(ApiResponse<OrderResponseDto>.Ok(result, "Order cancelled successfully."));
    }

    /// <summary>Return a specific order (must belong to the current user and status must be Delivered)</summary>
    [HttpPut("{id:int}/return")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ReturnOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(CurrentUserId, id);
        if (order.Status != "Delivered")
        {
            return BadRequest(ApiResponse<object>.Fail("Only delivered orders can be returned."));
        }
        
        var result = await _orderService.UpdateOrderStatusAsync(id, new UpdateOrderStatusDto { Status = "Refunded" });
        return Ok(ApiResponse<OrderResponseDto>.Ok(result, "Order return requested successfully."));
    }

    // ── Admin endpoints ────────────────────────────────────────────────────────

    /// <summary>Get all orders — Admin only. Filter by status: Pending | Confirmed | Shipped …</summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(PaginatedResponse<OrderResponseDto>), 200)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10,
        [FromQuery] string? status   = null)
    {
        var result = await _orderService.GetAllOrdersAsync(page, pageSize, status);
        return Ok(new PaginatedResponse<OrderResponseDto>
        {
            Data = result.Items,
            Meta = new PaginationMeta
            {
                TotalCount  = result.TotalCount,
                Page        = result.Page,
                PageSize    = result.PageSize,
                TotalPages  = result.TotalPages,
                HasPrevious = result.HasPrevious,
                HasNext     = result.HasNext
            }
        });
    }

    /// <summary>Get details of any order — Admin only</summary>
    [HttpGet("admin/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetAdminOrderById(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        return Ok(ApiResponse<OrderResponseDto>.Ok(result));
    }
 
    /// <summary>Update order status — Admin only</summary>
    [HttpPut("admin/{id:int}/status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, dto);
        return Ok(ApiResponse<OrderResponseDto>.Ok(result, $"Order status updated to '{dto.Status}'."));
    }
}
