
namespace OrderMicroservice.Common.DTO
{
     public sealed record OrderEvent
    (
         Guid VendorId,
         Guid OrderId,
         string CustomerId, 
         string OrderStatus,
         string PaymentStatus
    );
}
