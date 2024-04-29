using Microsoft.AspNetCore.Mvc;

namespace APBD7.Models;

[Route("api/[controller]")]
[ApiController]
public class ProductWarehouseController : ControllerBase
{
    private IProductWarehouseService _productWarehouseService;

    public ProductWarehouseController(IProductWarehouseService productWarehouseService)
    {
        _productWarehouseService = productWarehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(ProductWarehouse productWarehouse)
    {
        var isProductAndWarehouseExist =
            await _productWarehouseService.IsProductWarehouseExist(productWarehouse.idProduct,
                productWarehouse.idWarehouse);
        if (!isProductAndWarehouseExist)
        {
            return NotFound("Product or warehouse does not exist");
        }

        if (productWarehouse.amount <= 0)
        {
            return BadRequest("Amount must ne greater then 0");
        }

        if (!await _productWarehouseService.IsOrderInOrderTable(productWarehouse))
        {
            return NotFound("Product is not in Order table");
        }

        if (await _productWarehouseService.IsOrderComplete(productWarehouse))
        {
            return BadRequest("Order already complete");
        }

        _productWarehouseService.UpdateFullfilledAt(productWarehouse);

        int key = await _productWarehouseService.InsertIntoProductWarehouse(productWarehouse);

        return Ok(key);
    }
}