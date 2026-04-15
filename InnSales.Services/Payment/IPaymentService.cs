
using System.Threading.Tasks;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public interface IPaymentService
    {
        Task<StripePaymentResponseDto> ProcessPaymentAsync(StripePaymentRequestDto request);
    }
}
