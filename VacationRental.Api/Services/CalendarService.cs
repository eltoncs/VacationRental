using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using VacationRental.Api.Extensions;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly IRentalService rentalService;
        private readonly IDictionary<int, BookingViewModel> bookingsData;
        private readonly IConfiguration configuration;

        public CalendarService(
            IRentalService rentalService,
            IDictionary<int, BookingViewModel> bookingsData,
            IConfiguration configuration)
        {
            this.rentalService = rentalService;
            this.bookingsData = bookingsData;
            this.configuration = configuration;
        }

        public CalendarViewModel GetCalendar(
            int rentalId,
            DateTime start,
            int nights)
        {
            this.ValidateData(
                rentalId: rentalId,
                nights: nights);

            var calendar = new CalendarViewModel
            {
                RentalId = rentalId,
                Dates = new List<CalendarDateViewModel>()
            };

            for (var i = 0; i < nights; i++)
            {
                var dateCalendar = new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>()
                };

                this.AddAvailability(
                    calendar: calendar,
                    dateCalendar: dateCalendar);
            }

            return calendar;
        }

        private void AddAvailability(
            CalendarViewModel calendar,
            CalendarDateViewModel dateCalendar)
        {
            int blockingDays = int.Parse(configuration["Preferences:BlokingDays"]);

            foreach (var booking in bookingsData.Values)
            {
                if (booking.RentalId == calendar.RentalId
                    && booking.Start <= dateCalendar.Date && booking.Start.AddDays(booking.Nights + blockingDays) > dateCalendar.Date)
                {
                    dateCalendar.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id });
                }
            }

            calendar.Dates.Add(dateCalendar);
        }

        private void ValidateData(
            int rentalId,
            int nights)
        {
            if (nights <= 0)
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.BadRequest,
                    message: "Nights must be greater than zero");
            }

            if (!this.rentalService.RentalExist(rentalId))
            {
                throw new HttpException(
                   httpStatusCode: HttpStatusCode.NotFound,
                   message: "Rental not found");
            }
        }
    }
}
