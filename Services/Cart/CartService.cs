using Bizkit_backend.Data;
using Bizkit_backend.DTOs.Cart;
using Microsoft.EntityFrameworkCore;

namespace Bizkit_backend.Services.Cart;

public sealed class CartService(
    ApplicationDbContext dbContext) : ICartService
{
    public async Task<CartResponseDto> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(
            userId,
            cancellationToken);

        return ToResponse(cart);
    }

    public async Task<CartServiceResult> AddItemAsync(
        string userId,
        AddCartItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            return CartServiceResult.Failure(
                "Quantity must be greater than zero.");
        }

        var product = await dbContext.Products
            .Include(item => item.Category)
            .FirstOrDefaultAsync(
                item =>
                    item.Id == request.ProductId &&
                    item.IsActive &&
                    item.Category.IsActive,
                cancellationToken);

        if (product is null)
        {
            return CartServiceResult.Failure(
                "Active product not found.");
        }

        if (product.StockQuantity <= 0)
        {
            return CartServiceResult.Failure(
                "Product is out of stock.");
        }

        var cart = await GetCartAsync(
            userId,
            cancellationToken);

        var cartItem = cart.CartItems.FirstOrDefault(
            item => item.ProductId == request.ProductId);

        var requestedQuantity =
            request.Quantity +
            (cartItem?.Quantity ?? 0);

        if (requestedQuantity > product.StockQuantity)
        {
            return CartServiceResult.Failure(
                $"Only {product.StockQuantity} item(s) are available.");
        }

        if (cartItem is null)
        {
            cartItem = new Models.Entities.CartItem
            {
                Cart = cart,
                Product = product,
                ProductId = product.Id,
                Quantity = request.Quantity
            };

            cart.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity = requestedQuantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return CartServiceResult.Success(
            ToResponse(cart));
    }

    public async Task<CartServiceResult> UpdateItemAsync(
        string userId,
        int itemId,
        UpdateCartItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            return CartServiceResult.Failure(
                "Quantity must be greater than zero.");
        }

        var cart = await GetCartAsync(
            userId,
            cancellationToken);

        var cartItem = cart.CartItems.FirstOrDefault(
            item => item.Id == itemId);

        if (cartItem is null)
        {
            return CartServiceResult.Failure(
                "Cart item not found.");
        }

        var product = cartItem.Product;

        if (!product.IsActive ||
            !product.Category.IsActive)
        {
            return CartServiceResult.Failure(
                "The product is no longer available.");
        }

        if (product.StockQuantity <= 0)
        {
            return CartServiceResult.Failure(
                "Product is out of stock.");
        }

        if (request.Quantity > product.StockQuantity)
        {
            return CartServiceResult.Failure(
                $"Only {product.StockQuantity} item(s) are available.");
        }

        cartItem.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return CartServiceResult.Success(
            ToResponse(cart));
    }

    public async Task<bool> RemoveItemAsync(
        string userId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(
            userId,
            cancellationToken);

        var cartItem = cart.CartItems.FirstOrDefault(
            item => item.Id == itemId);

        if (cartItem is null)
        {
            return false;
        }

        dbContext.CartItems.Remove(cartItem);

        cart.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task ClearAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(
            userId,
            cancellationToken);

        if (cart.CartItems.Count == 0)
        {
            return;
        }

        dbContext.CartItems.RemoveRange(
            cart.CartItems);

        cart.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Models.Entities.Cart> GetCartAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var cart = await dbContext.Carts
            .Include(item => item.CartItems)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product.Category)
            .FirstOrDefaultAsync(
                item => item.UserId == userId,
                cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Models.Entities.Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Carts.Add(cart);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return cart;
    }

    private static CartResponseDto ToResponse(
        Models.Entities.Cart cart)
    {
        var items = cart.CartItems
            .Where(item =>
                item.Product.IsActive &&
                item.Product.Category.IsActive)
            .OrderBy(item => item.Product.Name)
            .Select(item => new CartItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                Subtotal =
                    item.Product.Price * item.Quantity,
                ImageUrl = item.Product.ImageUrl
            })
            .ToList();

        return new CartResponseDto
        {
            Id = cart.Id,
            Items = items,
            TotalAmount = items.Sum(
                item => item.Subtotal),
            UpdatedAt = cart.UpdatedAt
        };
    }
}