 
 namespace InnSales.Services
 {      
 public interface IPaymentTokenService
    {
        string Issue(Guid orderId, decimal amount, string currency, string userId, int expMinutes = 10);
        IDictionary<string, string> Validate(string token);
    }
    }