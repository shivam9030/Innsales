using InnSales.Common.DTO;

public class CreateOrderDto
{


    public string CustomerId { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; }

}
