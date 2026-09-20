using System.Security.Claims;
using Bizkit_backend.DTOs.Cart;
using Bizkit_backend.Services.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bizkit_backend.Controllers.Cart;

[ApiController]
[Route("api/cart")]
[Authorize]
public sealed class CartController(
    ICartService cartService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(CartResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await cartService.GetAsync(
            userId,
            cancellationToken);

        return Ok(cart);
    }

    [HttpPost("items")]
    [ProducesResponseType(
        typeof(CartResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await cartService.AddItemAsync(
            userId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Cart item could not be added.",
                errors = result.Errors
            });
        }

        return Ok(result.Response);
    }

    [HttpPut("items/{itemId:int}")]
    [ProducesResponseType(
        typeof(CartResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateItem(
        int itemId,
        [FromBody] UpdateCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await cartService.UpdateItemAsync(
            userId,
            itemId,
            request,
            cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Response);
        }

        if (result.Errors.Contains(
                "Cart item not found."))
        {
            return NotFound(new
            {
                message = "Cart item not found."
            });
        }

        return BadRequest(new
        {
            message = "Cart item could not be updated.",
            errors = result.Errors
        });
    }

    [HttpDelete("items/{itemId:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveItem(
        int itemId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var removed = await cartService.RemoveItemAsync(
            userId,
            itemId,
            cancellationToken);

        if (!removed)
        {
            return NotFound(new
            {
                message = "Cart item not found."
            });
        }

        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Clear(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        await cartService.ClearAsync(
            userId,
            cancellationToken);

        return NoContent();
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }
}