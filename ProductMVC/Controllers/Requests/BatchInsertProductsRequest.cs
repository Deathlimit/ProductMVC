using ProductMVC.BLL.DTO;

namespace ProductMVC.Controllers.Requests;

public class BatchInsertProductsRequest
{
    public List<ProductDTO> Products { get; set; } = new();
}
