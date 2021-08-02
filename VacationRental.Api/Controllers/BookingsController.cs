using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService bookingService;

        public BookingsController(
            IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        [HttpGet]
        [Route("{bookingId:int}")]
        public BookingViewModel Get(int bookingId)
        {
            return this.bookingService.GetBooking(bookingId);
        }

        [HttpPost]
        public IActionResult Post(BookingBindingModel model)
        {
            return Ok( this.bookingService.AddBooking(model));
        }
    }
}
