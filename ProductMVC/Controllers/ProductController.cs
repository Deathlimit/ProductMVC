using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductMVC.BLL.DTO;
using ProductMVC.BLL.Interfaces;
using ProductMVC.Controllers.Requests;
using ProductMVC.ViewModels;

namespace ProductMVC.Controllers;

[Route("products")]
public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetProductsRequest request)
    {
        var result = await _productService.GetProductsAsync(request.Page, request.PageSize);
        var viewModel = new ProductsPageViewModel
        {
            Products = new PagedViewModel<ProductDTO>
            {
                Items = result.Items,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            }
        };
        return View(viewModel);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new ProductDTO());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] ProductDTO productDto)
    {
        if (!ModelState.IsValid)
        {
            return View(productDto);
        }

        try
        {
            await _productService.CreateProductAsync(productDto);
            return RedirectToAction("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }
            return View(productDto);
        }
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromForm] ProductDTO productDto)
    {
        productDto.Id = id;

        if (!ModelState.IsValid)
        {
            return View(productDto);
        }

        try
        {
            await _productService.UpdateProductAsync(productDto);
            return RedirectToAction("Index");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }
            return View(productDto);
        }
    }

    [HttpGet("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _productService.DeleteProductAsync(id);
        return RedirectToAction("Index");
    }

    [HttpPost("batch-insert")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> BatchInsertProducts([FromBody] BatchInsertProductsRequest request)
    {
        if (request?.Products == null || request.Products.Count == 0)
        {
            return BadRequest(new { message = "Products are required" });
        }

        try
        {
            await _productService.BulkInsertProducts(request.Products);
            return Ok(new { message = "Products inserted", count = request.Products.Count });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    [HttpPost("batch-update")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> BatchUpdateProducts([FromBody] UpdateProductsRequest request)
    {
        if (request?.Products == null || request.Products.Count == 0)
        {
            return BadRequest(new { message = "Products are required" });
        }

        try
        {
            await _productService.BulkUpdateProducts(request.Products);
            return Ok(new { message = "Products updated", count = request.Products.Count });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    [HttpPost("batch-delete")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> BatchDeleteProducts([FromBody] DeleteProductsRequest request)
    {
        if (request?.ProductIds == null || request.ProductIds.Count == 0)
        {
            return BadRequest(new { message = "Product IDs are required" });
        }

        try
        {
            await _productService.BulkDeleteProducts(request.ProductIds);
            return Ok(new { message = "Products deleted", count = request.ProductIds.Count });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = "Validation failed", errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("api/{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Json(product);
    }
}
