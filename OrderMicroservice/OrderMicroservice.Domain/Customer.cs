namespace OrderMicroservice.Domain
{
public class Customer
    {
        public required string CustomerId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public required string FullName { get; set; } 
        public required Address BillingAddress { get; set; } 
        public required Address ShippingAddress { get; set; }
    }
}
 