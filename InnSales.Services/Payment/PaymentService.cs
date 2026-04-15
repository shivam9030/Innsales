using Stripe;
using InnSales.Common.DTO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InnSales.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IPaymentTokenService _paymentTokenService;
        private readonly IPaymentTransactionPublisher _publisher;

        public PaymentService(
            IOptions<StripeSettings> stripeOptions,
            IPaymentTokenService paymentTokenService,
            IPaymentTransactionPublisher publisher)
        {
            _stripeSettings = stripeOptions.Value;
            _paymentTokenService = paymentTokenService;
            _publisher = publisher;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            Console.WriteLine("[PAYMENT] PaymentService initialized");
        }

        public async Task<StripePaymentResponseDto> ProcessPaymentAsync(
            StripePaymentRequestDto request)
        {
            Console.WriteLine($"[PAYMENT] ProcessPaymentAsync started for OrderId: {request?.OrderId}");
            
            if (request == null || string.IsNullOrEmpty(request.Token))
            {
    
                return new StripePaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid payment request."
                };
            }

            if (string.IsNullOrWhiteSpace(request.PaymentToken))
            {
                return new StripePaymentResponseDto
                {
                    Success = false,
                    Message = "Missing payment token."
                };
            }

            IDictionary<string, string> payload;
            try
            {
                Console.WriteLine($"[PAYMENT] Validating payment token for OrderId: {request.OrderId}");
                
                payload = _paymentTokenService.Validate(request.PaymentToken);
                Console.WriteLine("[PAYMENT] Payment token validated successfully");
            }
            catch (Exception ex)
            {
                return new StripePaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid payment token."
                };
            }

            var amount = decimal.Parse(payload["amount"]);
            var currency = payload["currency"];
            var orderId = payload["orderId"];

            var chargeOptions = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency,
                Description = $"Payment for Order {orderId}",
                Source = request.Token
            };

            var chargeService = new ChargeService();
            StripePaymentResponseDto response;
            Charge? charge = null;

            try
            {
                Console.WriteLine($"[PAYMENT] Creating Stripe charge for OrderId: {orderId}, Amount: {chargeOptions.Amount}, Currency: {chargeOptions.Currency}");
                charge = await chargeService.CreateAsync(chargeOptions);
                Console.WriteLine($"[PAYMENT] Stripe charge created: {charge.Id}, Status: {charge.Status}");

                response = charge.Status == "succeeded"
                    ? new StripePaymentResponseDto
                    {
                        Success = true,
                        TransactionId = charge.Id,
                        MaskedCardNumber =
                            "**** **** **** " + charge.PaymentMethodDetails?.Card?.Last4,
                        CardExpiryDate =
                            $"{charge.PaymentMethodDetails?.Card?.ExpMonth}/{charge.PaymentMethodDetails?.Card?.ExpYear}",
                        Message = "Payment successful."
                    }
                    : new StripePaymentResponseDto
                    {
                        Success = false,
                        Message = "Payment failed."
                    };
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"[PAYMENT] StripeException: {ex.StripeError?.Message}");
                response = new StripePaymentResponseDto
                {
                    Success = false,
                    Message = ex.StripeError?.Message ?? "Stripe payment failed."
                };
            }

            //  ALWAYS publish transaction (success or failure)
            Console.WriteLine($"[PAYMENT] Publishing payment transaction message for OrderId: {orderId}, Success: {response.Success}");

            try
            {
                await _publisher.PublishAsync(new PaymentTransactionMessage
                {
                    OrderId = Guid.Parse(orderId),
                    Success = response.Success,
                    TransactionId = charge?.Id,
                    CardHolderName = request.CardHolderName,
                    MaskedCardNumber = response.MaskedCardNumber,
                    CardExpiryDate = response.CardExpiryDate,
                    FailureReason = response.Success ? null : response.Message,
                    OccurredAtUtc = DateTime.UtcNow
                });
                Console.WriteLine($"[PAYMENT] Payment transaction message published for OrderId: {orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAYMENT] ERROR publishing payment transaction message for OrderId: {orderId}: {ex.Message}");
                throw;
            }
            return response;
        }
    }
}
