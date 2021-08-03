using System;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            this.calendarService = calendarService;
        }

        [HttpGet]
        public CalendarViewModel Get(
            int rentalId, 
            DateTime start, 
            int nights)
        {
            return this.calendarService.GetCalendar(
                rentalId: rentalId,
                start: start,
                nights: nights);
        }
    }
}
