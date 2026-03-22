
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Application.Common.Bookings;
using MyWarehouse.Application.Bookings.Commands;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Partners.CreatePartner;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.WebApi.API.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiversion}/Bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetBookings")]
        [LogList("Bookings")]
        public async Task<ActionResult<IListResponseModel<GetBookingsListDTO>>> GetBookings([FromQuery] GetBookingsListQuery query)
            => Ok(await _mediator.Send(query));

        /// <summary>
        /// Move a booking to a different room
        /// </summary>
        /// <param name="bookingId">The booking ID to move</param>
        /// <param name="request">Request containing new room number and user ID</param>
        /// <returns>Updated booking details</returns>
        [HttpPut("{bookingId}/move-room")]
        [LogUpdate("Bookings", "Move Room")]
        public async Task<ActionResult> MoveBookingToRoom(
            [FromRoute] string bookingId,
            [FromBody] MoveBookingRequest request)
        {
            try
            {
                var command = new MoveBookingCommand(bookingId, request.NewRoomNumber, request.UserId);
                var result = await _mediator.Send(command);
                return Ok(new { success = true, message = "Booking moved successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while moving the booking", error = ex.Message });
            }
        }

        /// <summary>
        /// Update booking check-in and check-out dates
        /// </summary>
        /// <param name="bookingId">The booking ID to update</param>
        /// <param name="request">Request containing new dates and user ID</param>
        /// <returns>Updated booking details</returns>
        [HttpPut("{bookingId}/update-dates")]
        [LogUpdate("Bookings", "Update Dates")]
        public async Task<ActionResult> UpdateBookingDates(
            [FromRoute] string bookingId,
            [FromBody] UpdateBookingDatesRequest request)
        {
            try
            {
                var command = new UpdateBookingDatesCommand(
                    bookingId, 
                    request.NewCheckInDate, 
                    request.NewCheckOutDate, 
                    request.UserId,
                    request.Reason
                );
                var result = await _mediator.Send(command);
                return Ok(new { success = true, message = "Booking dates updated successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating booking dates", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        /// <param name="bookingId">The booking ID to cancel</param>
        /// <param name="request">Request containing user ID and optional cancellation reason</param>
        /// <returns>Cancelled booking details</returns>
        [HttpPut("{bookingId}/cancel")]
        [LogUpdate("Bookings", "Cancel")]
        public async Task<ActionResult> CancelBooking(
            [FromRoute] string bookingId,
            [FromBody] CancelBookingRequest request)
        {
            try
            {
                var command = new CancelBookingCommand(bookingId, request.UserId, request.CancellationReason);
                var result = await _mediator.Send(command);
                return Ok(new { success = true, message = "Booking cancelled successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while cancelling the booking", error = ex.Message });
            }
        }
    }
}

