using Bizkit_backend.Data;
using Bizkit_backend.DTOs.Products;
using Bizkit_backend.Models.Entities;
using Bizkit_backend.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace Bizkit_backend.Services.Products;

public sealed class ProductService(
    ApplicationDbContext dbContext,
    IFileStorageService fileStorageService) : IProductService
{
    public async Task<IReadOnlyCollection<ProductResponseDto>>
        GetAllAsync(
            int? categoryId = null,
            string? search = null,
            CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.IsActive &&
                product.Category.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(product =>
                product.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();

            query = query.Where(product =>
                product.Name.ToLower().Contains(searchTerm) ||
                (product.Description != null &&
                 product.Description.ToLower().Contains(searchTerm)));
        }

        return await query
            .OrderBy(product => product.Name)
            .Select(product => new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SupersedesProductId = product.SupersedesProductId,
                SupersedesProductName =
                    product.SupersedesProduct != null
                        ? product.SupersedesProduct.Name
                        : null,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Id == id &&
                product.IsActive &&
                product.Category.IsActive)
            .Select(product => new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SupersedesProductId = product.SupersedesProductId,
                SupersedesProductName =
                    product.SupersedesProduct != null
                        ? product.SupersedesProduct.Name
                        : null,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductServiceResult> CreateAsync(
        CreateProductRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateRequestAsync(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            request.SupersedesProductId,
            null,
            cancellationToken);

        if (validationError is not null)
        {
            return ProductServiceResult.Failure(validationError);
        }

        if (request.Image is null || request.Image.Length == 0)
        {
            return ProductServiceResult.Failure("Product image is required.");
        }

        var (succeeded, filePath, errorMessage) = await fileStorageService.SaveFileAsync(
            request.Image,
            "products",
            cancellationToken);

        if (!succeeded)
        {
            return ProductServiceResult.Failure(errorMessage ?? "Product image upload failed.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = filePath,
            CategoryId = request.CategoryId,
            SupersedesProductId = request.SupersedesProductId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(product)
            .Reference(item => item.Category)
            .LoadAsync(cancellationToken);

        if (product.SupersedesProductId.HasValue)
        {
            await dbContext.Entry(product)
                .Reference(item => item.SupersedesProduct)
                .LoadAsync(cancellationToken);
        }

        return ProductServiceResult.Success(
            ToResponse(product));
    }

    public async Task<ProductServiceResult> UpdateAsync(
        int id,
        UpdateProductRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .Include(item => item.Category)
            .Include(item => item.SupersedesProduct)
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (product is null)
        {
            return ProductServiceResult.Failure(
                "Product not found.");
        }

        var validationError = await ValidateRequestAsync(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            request.SupersedesProductId,
            id,
            cancellationToken);

        if (validationError is not null)
        {
            return ProductServiceResult.Failure(validationError);
        }

        var imageUrl = product.ImageUrl;

        if (request.Image is not null && request.Image.Length > 0)
        {
            var (succeeded, newFilePath, errorMessage) = await fileStorageService.SaveFileAsync(
                request.Image,
                "products",
                cancellationToken);

            if (!succeeded)
            {
                return ProductServiceResult.Failure(errorMessage ?? "Product image upload failed.");
            }

            fileStorageService.DeleteFile(product.ImageUrl);
            imageUrl = newFilePath;
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = imageUrl;
        product.CategoryId = request.CategoryId;
        product.SupersedesProductId =
            request.SupersedesProductId;
        product.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(product)
            .Reference(item => item.Category)
            .LoadAsync(cancellationToken);

        await dbContext.Entry(product)
            .Reference(item => item.SupersedesProduct)
            .LoadAsync(cancellationToken);

        return ProductServiceResult.Success(
            ToResponse(product));
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (product is null)
        {
            return false;
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<string?> ValidateRequestAsync(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        int categoryId,
        int? supersedesProductId,
        int? currentProductId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Product name is required.";
        }

        name = name.Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            return "Product description is required.";
        }

        if (price < 0)
        {
            return "Product price cannot be negative.";
        }

        if (stockQuantity < 0)
        {
            return "Stock quantity cannot be negative.";
        }

        var categoryExists = await dbContext.Categories
            .AnyAsync(
                category =>
                    category.Id == categoryId &&
                    category.IsActive,
                cancellationToken);

        if (!categoryExists)
        {
            return "Active category not found.";
        }

        if (await NameExistsAsync(
                name,
                currentProductId,
                cancellationToken))
        {
            return "A product with this name already exists.";
        }

        if (supersedesProductId.HasValue)
        {
            if (currentProductId.HasValue &&
                supersedesProductId.Value ==
                currentProductId.Value)
            {
                return "A product cannot supersede itself.";
            }

            var supersededProductExists =
                await dbContext.Products.AnyAsync(
                    product =>
                        product.Id ==
                        supersedesProductId.Value &&
                        product.IsActive,
                    cancellationToken);

            if (!supersededProductExists)
            {
                return "Active superseded product not found.";
            }
        }

        return null;
    }

    private async Task<bool> NameExistsAsync(
        string name,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToLowerInvariant();

        return await dbContext.Products
            .AnyAsync(
                product =>
                    product.Name.ToLower() == normalizedName &&
                    (!excludedId.HasValue ||
                     product.Id != excludedId.Value),
                cancellationToken);
    }

    private static ProductResponseDto ToResponse(
        Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            SupersedesProductId =
                product.SupersedesProductId,
            SupersedesProductName =
                product.SupersedesProduct?.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}