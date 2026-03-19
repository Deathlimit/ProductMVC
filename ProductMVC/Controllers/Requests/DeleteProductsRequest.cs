namespace ProductMVC.Controllers.Requests;

public class DeleteProductsRequest
{
    public List<int> ProductIds { get; set; } = new();
}
