public interface IOrderEventPublisher
{
    Task PublishOrderUpdatedAsync(Guid orderId, string customerId, string status);
}
