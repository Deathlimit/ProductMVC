namespace ProductMVC.Controllers.Requests;

public class GetProductsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
