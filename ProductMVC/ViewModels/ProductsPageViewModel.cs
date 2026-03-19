using ProductMVC.BLL.DTO;

namespace ProductMVC.ViewModels;

public class ProductsPageViewModel
{
    public PagedViewModel<ProductDTO> Products { get; set; } = new();
}
