using Bizkit_backend.DTOs.Products;
using Bizkit_backend.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bizkit_backend.Controllers.Products;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductService productService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ProductResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(
            categoryId,
            search,
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(ProductResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(ProductResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await productService.CreateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Product creation failed.",
                errors = result.Errors
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Response!.Id },
            result.Response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(ProductResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] UpdateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await productService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.Errors.Contains("Product not found."))
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            return BadRequest(new
            {
                message = "Product update failed.",
                errors = result.Errors
            });
        }

        return Ok(result.Response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(new
        {
            message = "Product deleted successfully."
        });
    }
}