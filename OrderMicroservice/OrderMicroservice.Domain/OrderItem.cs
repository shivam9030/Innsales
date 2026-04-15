using OrderMicroservice.Common.Enums;
namespace OrderMicroservice.Domain

{
public class OrderItem
    {
        public int OrderId { get; set; }     
        public required string ItemId { get; set; } 
        public ItemType ItemType { get; set; }           // PRODUCT | SHIPPING | FEE
        public string? Sku { get; set; }
        public required string Name { get; set; }
        public int Quantity { get; set; }
        public Order Order {get; set;}
    }
}