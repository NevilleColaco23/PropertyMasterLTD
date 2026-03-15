using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Products.CreateProduct;
using MyWarehouse.Application.Products.DeleteProduct;
using MyWarehouse.Application.Products.GetProduct;
using MyWarehouse.Application.Products.GetProductsList;
using MyWarehouse.Application.Products.GetProductsSummary;
using MyWarehouse.Application.Products.ProductStockMass;
using MyWarehouse.Application.Products.ProductStockValue;
using MyWarehouse.Application.Products.UpdateProduct;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.Infrastructure.API.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [LogCreate("Product")]
    public async Task<ActionResult<int>> Create(CreateProductCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet]
    [LogList("Products")]
    public async Task<ActionResult<IListResponseModel<ProductDto>>> GetList([FromQuery] GetProductsListQuery query)
        => Ok(await _mediator.Send(query));

    [HttpGet("{id}")]
    [LogView("Product")]
    public async Task<ActionResult<ProductDetailsDto>> Get(int id)
        => Ok(await _mediator.Send(new GetProductDetailsQuery() { Id = id }));

    [HttpDelete("{id}")]
    [LogDelete("Product")]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProductCommand() { Id = id });

        return NoContent();
    }

    [HttpPut("{id}")]
    [LogUpdate("Product")]
    public async Task<ActionResult> Update(int id, UpdateProductCommand command)
    {
        if (id != command.Id) return BadRequest();

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpGet("totalMass")]
    [LogView("Product Stock Analytics", Description = "Viewed product stock mass")]
    public async Task<ActionResult<StockMassDto>> ProductStockMass()
        => Ok(await _mediator.Send(new ProductStockMassQuery()));

    [HttpGet("totalValue")]
    [LogView("Product Stock Analytics", Description = "Viewed product stock value")]
    public async Task<ActionResult<StockValueDto>> ProductStockValue()
        => Ok(await _mediator.Send(new ProductStockValueQuery()));

    [HttpGet("stockCount")]
    [LogView("Product Stock Analytics", Description = "Viewed product stock count")]
    public async Task<ActionResult<ProductStockCountDto>> ProductStockCount()
        => Ok(await _mediator.Send(new ProductStockCountQuery()));
}
