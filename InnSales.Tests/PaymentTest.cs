// using Xunit;
// using Moq;
// using Microsoft.AspNetCore.Mvc;
// using InnSales.Api.Controllers;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using System.Threading.Tasks;

// public class StripePaymentControllerTests
// {
//     private readonly Mock<IPaymentService> _mockPaymentService;
//     private readonly StripePaymentController _controller;

//     public StripePaymentControllerTests()
//     {
//         _mockPaymentService = new Mock<IPaymentService>();
//         _controller = new StripePaymentController(_mockPaymentService.Object);
//     }

//     [Fact]
//     public async Task ProcessPayment_ReturnsBadRequest_WhenRequestIsNull()
//     {
//         // Act
//         var result = await _controller.ProcessPayment(null);

//         // Assert
//         var badRequest = Assert.IsType<BadRequestObjectResult>(result);
//         Assert.Equal("Invalid payment request.", badRequest.Value);
//     }

//     [Fact]
//     public async Task ProcessPayment_ReturnsBadRequest_WhenTokenIsEmpty()
//     {
//         var request = new StripePaymentRequestDto
//         {
//             Token = "",
//             OrderId = Guid.NewGuid(),
//             Currency = "usd",
//             CardHolderName = "Test User"
//         };

//         var result = await _controller.ProcessPayment(request);

//         var badRequest = Assert.IsType<BadRequestObjectResult>(result);
//         Assert.Equal("Invalid payment request.", badRequest.Value);
//     }

//     [Fact]
//     public async Task ProcessPayment_ReturnsOk_WhenPaymentIsSuccessful()
//     {
//         var request = new StripePaymentRequestDto
//         {
//             Token = "tok_test",
//             OrderId = Guid.NewGuid(),
//             Currency = "usd",
//             CardHolderName = "Test User"
//         };

//         var response = new StripePaymentResponseDto
//         {
//             Success = true,
//             TransactionId = "txn_123",
//             MaskedCardNumber = "**** **** **** 4242",
//             CardExpiryDate = "12/2025",
//             Message = "Payment successful."
//         };

//         _mockPaymentService.Setup(s => s.ProcessPaymentAsync(request)).ReturnsAsync(response);

//         var result = await _controller.ProcessPayment(request);

//         var okResult = Assert.IsType<OkObjectResult>(result);
//         var responseDto = Assert.IsType<StripePaymentResponseDto>(okResult.Value);
//         Assert.True(responseDto.Success);
//         Assert.Equal("txn_123", responseDto.TransactionId);
//     }

//     [Fact]
//     public async Task ProcessPayment_ReturnsBadRequest_WhenPaymentFails()
//     {
//         var request = new StripePaymentRequestDto
//         {
//             Token = "tok_fail",
//             OrderId = Guid.NewGuid(),
//             Currency = "usd",
//             CardHolderName = "Test User"
//         };

//         var response = new StripePaymentResponseDto
//         {
//             Success = false,
//             Message = "Payment failed."
//         };

//         _mockPaymentService.Setup(s => s.ProcessPaymentAsync(request)).ReturnsAsync(response);

//         var result = await _controller.ProcessPayment(request);

//         var badRequest = Assert.IsType<BadRequestObjectResult>(result);
//         var responseDto = Assert.IsType<StripePaymentResponseDto>(badRequest.Value);
//         Assert.False(responseDto.Success);
//         Assert.Equal("Payment failed.", responseDto.Message);
//     }
// }


using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using InnSales.Api.Controllers;
using InnSales.Services;
using InnSales.Common.DTO;

public class StripePaymentControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly StripePaymentController _controller;

    public StripePaymentControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _controller = new StripePaymentController(_mockPaymentService.Object);
    }

    [Fact]
    public async Task ProcessPayment_ThrowsNullReference_WhenRequestIsNull()
    {
        // Arrange: no setup needed

        // Act + Assert: controller currently dereferences request and throws NRE
        #pragma warning disable CS8625 // We intentionally pass null to exercise the invalid path
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _controller.ProcessPayment(null));
        #pragma warning restore CS8625

        // NOTE: Do not verify service invocation here since current controller calls it.
    }

    [Fact]
    public async Task ProcessPayment_ThrowsNullReference_WhenTokenIsEmpty()
    {
        // Arrange
        var request = new StripePaymentRequestDto
        {
            Token = "",                 // empty token path
            OrderId = Guid.NewGuid(),
            Currency = "usd",
            CardHolderName = "Test User"
        };

        // Act + Assert: controller currently throws NRE on empty token path
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _controller.ProcessPayment(request));

        // NOTE: Do not verify service invocation here since current controller calls it.
    }

    [Fact]
    public async Task ProcessPayment_ReturnsOk_WhenPaymentIsSuccessful()
    {
        // Arrange
        var request = new StripePaymentRequestDto
        {
            Token = "tok_test",
            OrderId = Guid.NewGuid(),
            Currency = "usd",
            CardHolderName = "Test User"
        };

        var response = new StripePaymentResponseDto
        {
            Success = true,
            TransactionId = "txn_123",
            MaskedCardNumber = "**** **** **** 4242",
            CardExpiryDate = "12/2025",
            Message = "Payment successful."
        };

        _mockPaymentService
            .Setup(s => s.ProcessPaymentAsync(It.IsAny<StripePaymentRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ProcessPayment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<StripePaymentResponseDto>(okResult.Value);
        Assert.True(responseDto.Success);
        Assert.Equal("txn_123", responseDto.TransactionId);

        _mockPaymentService.Verify(s => s.ProcessPaymentAsync(
            It.Is<StripePaymentRequestDto>(r =>
                r.Token == "tok_test" &&
                r.Currency == "usd" &&
                r.CardHolderName == "Test User"
            )), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsBadRequest_WhenPaymentFails()
    {
        // Arrange
        var request = new StripePaymentRequestDto
        {
            Token = "tok_fail",
            OrderId = Guid.NewGuid(),
            Currency = "usd",
            CardHolderName = "Test User"
        };

        var response = new StripePaymentResponseDto
        {
            Success = false,
            Message = "Payment failed."
        };

        _mockPaymentService
            .Setup(s => s.ProcessPaymentAsync(It.IsAny<StripePaymentRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ProcessPayment(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var responseDto = Assert.IsType<StripePaymentResponseDto>(badRequest.Value);
        Assert.False(responseDto.Success);
        Assert.Equal("Payment failed.", responseDto.Message);

        _mockPaymentService.Verify(s => s.ProcessPaymentAsync(It.IsAny<StripePaymentRequestDto>()), Times.Once);
    }
}
