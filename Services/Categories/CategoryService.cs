using Bizkit_backend.Data;
using Bizkit_backend.DTOs.Categories;
using Bizkit_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bizkit_backend.Services.Categories;

public sealed class CategoryService(
    ApplicationDbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryResponseDto>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category =>
                category.Id == id &&
                category.IsActive)
            .Select(category => new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryServiceResult> CreateAsync(
        CreateCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return CategoryServiceResult.Failure(
                "Category name is required.");
        }

        if (await NameExistsAsync(
                name,
                null,
                cancellationToken))
        {
            return CategoryServiceResult.Failure(
                "A category with this name already exists.");
        }

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        dbContext.Categories.Add(category);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CategoryServiceResult.Success(
            ToResponse(category));
    }

    public async Task<CategoryServiceResult> UpdateAsync(
        int id,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (category is null)
        {
            return CategoryServiceResult.Failure(
                "Category not found.");
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return CategoryServiceResult.Failure(
                "Category name is required.");
        }

        if (await NameExistsAsync(
                name,
                id,
                cancellationToken))
        {
            return CategoryServiceResult.Failure(
                "A category with this name already exists.");
        }

        category.Name = name;
        category.Description = request.Description?.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return CategoryServiceResult.Success(
            ToResponse(category));
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        category.IsActive = false;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> NameExistsAsync(
        string name,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToLowerInvariant();

        return await dbContext.Categories
            .AnyAsync(
                item =>
                    item.Name.ToLower() == normalizedName &&
                    (!excludedId.HasValue ||
                     item.Id != excludedId.Value),
                cancellationToken);
    }

    private static CategoryResponseDto ToResponse(
        Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }
}