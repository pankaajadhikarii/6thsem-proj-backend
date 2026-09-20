using System.Security.Claims;
using Bizkit_backend.DTOs.Orders;
using Bizkit_backend.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bizkit_backend.Controllers.Orders;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(
    IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(OrderResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await orderService.CreateAsync(
            userId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Order could not be created.",
                errors = result.Errors
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = result.Response!.Id
            },
            result.Response);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<OrderResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var orders = await orderService.GetMyOrdersAsync(
            userId,
            cancellationToken);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(OrderResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await orderService.GetByIdAsync(
            id,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        if (order is null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        return Ok(order);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<OrderResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var orders = await orderService.GetAllAsync(
            cancellationToken);

        return Ok(orders);
    }

    [HttpPatch("admin/{id:int}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(OrderResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.UpdateStatusAsync(
            id,
            request,
            cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        if (result.Errors.Contains(
                "Order not found."))
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        return BadRequest(new
        {
            message = "Order status could not be updated.",
            errors = result.Errors
        });
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }
}