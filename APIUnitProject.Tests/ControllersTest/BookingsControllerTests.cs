using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyWarehouse.Application.Bookings.Commands;
using MyWarehouse.Application.Common.Bookings;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.WebApi.API.V1;

namespace APIUnitProject.Tests.ControllersTest
{
    public class BookingsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly BookingsController _controller;

        public BookingsControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new BookingsController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetBookings_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var query = new GetBookingsListQuery();
            var expected = new Mock<IListResponseModel<GetBookingsListDTO>>().Object;
            _mockMediator.Setup(m => m.Send(query, default)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetBookings(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task MoveBookingToRoom_Success_ReturnsOk()
        {
            // Arrange
            var request = new MoveBookingRequest { NewRoomNumber = "101", UserId = 1 };
            MyWarehouse.Domain.Bookings.Bookings? bookingResult = null;
            _mockMediator.Setup(m => m.Send(It.IsAny<MoveBookingCommand>(), default)).ReturnsAsync(bookingResult!);

            // Act
            var result = await _controller.MoveBookingToRoom("booking1", request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MoveBookingToRoom_InvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var request = new MoveBookingRequest { NewRoomNumber = "101", UserId = 1 };
            _mockMediator.Setup(m => m.Send(It.IsAny<MoveBookingCommand>(), default))
                .ThrowsAsync(new InvalidOperationException("Room not available"));

            // Act
            var result = await _controller.MoveBookingToRoom("booking1", request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task MoveBookingToRoom_UnexpectedException_ReturnsServerError()
        {
            // Arrange
            var request = new MoveBookingRequest { NewRoomNumber = "101", UserId = 1 };
            _mockMediator.Setup(m => m.Send(It.IsAny<MoveBookingCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected"));

            // Act
            var result = await _controller.MoveBookingToRoom("booking1", request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task UpdateBookingDates_Success_ReturnsOk()
        {
            // Arrange
            var request = new UpdateBookingDatesRequest
            {
                NewCheckInDate = DateTime.UtcNow,
                NewCheckOutDate = DateTime.UtcNow.AddDays(1),
                UserId = 1
            };
            MyWarehouse.Domain.Bookings.Bookings? bookingResult = null;
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateBookingDatesCommand>(), default)).ReturnsAsync(bookingResult!);

            // Act
            var result = await _controller.UpdateBookingDates("booking1", request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CancelBooking_Success_ReturnsOk()
        {
            // Arrange
            var request = new CancelBookingRequest { UserId = 1, CancellationReason = "Change of plans" };
            MyWarehouse.Domain.Bookings.Bookings? bookingResult = null;
            _mockMediator.Setup(m => m.Send(It.IsAny<CancelBookingCommand>(), default)).ReturnsAsync(bookingResult!);

            // Act
            var result = await _controller.CancelBooking("booking1", request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CancelBooking_InvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var request = new CancelBookingRequest { UserId = 1 };
            _mockMediator.Setup(m => m.Send(It.IsAny<CancelBookingCommand>(), default))
                .ThrowsAsync(new InvalidOperationException("Already cancelled"));

            // Act
            var result = await _controller.CancelBooking("booking1", request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
