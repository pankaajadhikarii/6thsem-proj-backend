using Bizkit_backend.Data;
using Bizkit_backend.DTOs.Orders;
using Bizkit_backend.DTOs.Resale;
using Bizkit_backend.Models.Entities;
using Bizkit_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bizkit_backend.Services.Resale;

public sealed class ResaleService(
    ApplicationDbContext dbContext) : IResaleService
{
    public async Task<IReadOnlyCollection<ResaleListingResponseDto>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var listings = await dbContext.ResaleListings
            .AsNoTracking()
            .Include(listing => listing.Product)
            .Include(listing => listing.Seller)
            .Where(listing =>
                listing.Status == ResaleListingStatus.Active)
            .OrderByDescending(listing => listing.CreatedAt)
            .ToListAsync(cancellationToken);

        return listings
            .Select(ToResponse)
            .ToList();
    }

    public async Task<ResaleListingResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var listing = await dbContext.ResaleListings
            .AsNoTracking()
            .Include(listing => listing.Product)
            .Include(listing => listing.Seller)
            .FirstOrDefaultAsync(
                listing =>
                    listing.Id == id &&
                    listing.Status ==
                        ResaleListingStatus.Active,
                cancellationToken);

        return listing is null
            ? null
            : ToResponse(listing);
    }

    public async Task<ResaleServiceResult> CreateAsync(
        string sellerId,
        CreateResaleListingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Price <= 0)
        {
            return ResaleServiceResult.Failure(
                "Resale price must be greater than zero.");
        }

        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                product => product.Id == request.ProductId,
                cancellationToken);

        if (product is null)
        {
            return ResaleServiceResult.Failure(
                "Product not found.");
        }

        var ownsProduct = await dbContext.OrderItems
            .AnyAsync(
                item =>
                    item.ProductId == request.ProductId &&
                    item.Order.CustomerId == sellerId &&
                    item.Order.Status ==
                        OrderStatus.Delivered,
                cancellationToken);

        if (!ownsProduct)
        {
            return ResaleServiceResult.Failure(
                "You can only resell products that you purchased through BizKit and received.");
        }

        var alreadyListed = await dbContext.ResaleListings
            .AnyAsync(
                listing =>
                    listing.SellerId == sellerId &&
                    listing.ProductId == request.ProductId &&
                    listing.Status ==
                        ResaleListingStatus.Active,
                cancellationToken);

        if (alreadyListed)
        {
            return ResaleServiceResult.Failure(
                "You already have an active listing for this product.");
        }

        var seller = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Id == sellerId,
                cancellationToken);

        if (seller is null)
        {
            return ResaleServiceResult.Failure(
                "User not found.");
        }

        var now = DateTime.UtcNow;

        var listing = new ResaleListing
        {
            ProductId = product.Id,
            SellerId = sellerId,
            Price = request.Price,
            Condition = request.Condition,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            Status = ResaleListingStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ResaleListings.Add(listing);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        listing.Product = product;
        listing.Seller = seller;

        return ResaleServiceResult.Success(
            ToResponse(listing));
    }

    public async Task<ResaleServiceResult> UpdateAsync(
        string sellerId,
        int id,
        UpdateResaleListingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Price <= 0)
        {
            return ResaleServiceResult.Failure(
                "Resale price must be greater than zero.");
        }

        var listing = await dbContext.ResaleListings
            .Include(item => item.Product)
            .Include(item => item.Seller)
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (listing is null)
        {
            return ResaleServiceResult.Failure(
                "Resale listing not found.");
        }

        if (listing.SellerId != sellerId)
        {
            return ResaleServiceResult.Failure(
                "You can only modify your own listing.");
        }

        if (listing.Status !=
            ResaleListingStatus.Active)
        {
            return ResaleServiceResult.Failure(
                "Only active listings can be updated.");
        }

        listing.Price = request.Price;
        listing.Condition = request.Condition;
        listing.Description =
            request.Description?.Trim();
        listing.ImageUrl =
            request.ImageUrl?.Trim();
        listing.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return ResaleServiceResult.Success(
            ToResponse(listing));
    }

    public async Task<bool> RemoveAsync(
        string sellerId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var listing = await dbContext.ResaleListings
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (listing is null ||
            listing.SellerId != sellerId ||
            listing.Status !=
                ResaleListingStatus.Active)
        {
            return false;
        }

        listing.Status = ResaleListingStatus.Removed;
        listing.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<ResaleServiceResult> PurchaseAsync(
        string buyerId,
        int id,
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
                request.ShippingAddress))
        {
            return ResaleServiceResult.Failure(
                "Shipping address is required.");
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

        var listing = await dbContext.ResaleListings
            .Include(item => item.Product)
            .FirstOrDefaultAsync(
                item =>
                    item.Id == id &&
                    item.Status ==
                        ResaleListingStatus.Active,
                cancellationToken);

        if (listing is null)
        {
            return ResaleServiceResult.Failure(
                "Active resale listing not found.");
        }

        if (listing.SellerId == buyerId)
        {
            return ResaleServiceResult.Failure(
                "You cannot purchase your own listing.");
        }

        var buyerExists = await dbContext.Users
            .AnyAsync(
                user => user.Id == buyerId,
                cancellationToken);

        if (!buyerExists)
        {
            return ResaleServiceResult.Failure(
                "Buyer account not found.");
        }

        var now = DateTime.UtcNow;

        var order = new Order
        {
            OrderNumber =
                GenerateOrderNumber(),
            CustomerId = buyerId,
            TotalAmount = listing.Price,
            ShippingAddress =
                request.ShippingAddress.Trim(),
            PaymentMethod =
                request.PaymentMethod,
            PaymentStatus =
                request.PaymentMethod ==
                    PaymentMethod.CashOnDelivery
                    ? OrderPaymentStatus.Unpaid
                    : OrderPaymentStatus.Pending,
            Status = OrderStatus.Pending,
            OrderDate = now,
            UpdatedAt = now
        };

        order.OrderItems.Add(
            new OrderItem
            {
                ProductId = listing.ProductId,
                ResaleListingId = listing.Id,
                ProductName = listing.Product.Name,
                UnitPrice = listing.Price,
                Quantity = 1,
                Subtotal = listing.Price
            });

        order.Payments.Add(
            new Payment
            {
                PaymentMethod =
                    request.PaymentMethod,
                Amount = listing.Price,
                Status = PaymentStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            });

        listing.Status =
            ResaleListingStatus.Sold;

        listing.UpdatedAt = now;

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return ResaleServiceResult.PurchaseSuccess(
            order.Id,
            order.OrderNumber);
    }

    private static string GenerateOrderNumber()
    {
        return $"BZ-RESALE-{DateTime.UtcNow:yyyyMMddHHmmss}-" +
               Guid.NewGuid().ToString("N");
    }

    private static ResaleListingResponseDto ToResponse(
        ResaleListing listing)
    {
        return new ResaleListingResponseDto
        {
            Id = listing.Id,
            ProductId = listing.ProductId,
            ProductName = listing.Product.Name,
            SellerId = listing.SellerId,
            SellerName = listing.Seller.FullName,
            Price = listing.Price,
            Condition = listing.Condition,
            Description = listing.Description,
            ImageUrl = listing.ImageUrl,
            Status = listing.Status,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt
        };
    }
}