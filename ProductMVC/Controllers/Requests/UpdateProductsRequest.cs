using ProductMVC.BLL.DTO;

namespace ProductMVC.Controllers.Requests;

public class UpdateProductsRequest
{
    public List<ProductDTO> Products { get; set; } = new();
}
