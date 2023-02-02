using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VacationRental.Api.Extensions;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly IRentalService rentalService;
        private readonly IDictionary<int, BookingViewModel> bookingsData;
        private readonly IDictionary<int, RentalViewModel> rentalsData;

        public CalendarService(
            IRentalService rentalService,
            IDictionary<int, BookingViewModel> bookingsData,
            IDictionary<int, RentalViewModel> rentalsData)
        {
            this.rentalService = rentalService;
            this.bookingsData = bookingsData;
            this.rentalsData = rentalsData;
        }

        public CalendarViewModel GetCalendar(
            int rentalId,
            DateTime start,
            int nights)
        {
            this.ValidateData(
                rentalId: rentalId,
                nights: nights);

            var rental = this.rentalsData[rentalId];

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
                    dateCalendar: dateCalendar,
                    rental: rental);
            }

            return calendar;
        }

        private void AddAvailability(
            CalendarViewModel calendar,
            CalendarDateViewModel dateCalendar,
            RentalViewModel rental)
        {
            IEnumerable<BookingViewModel> rentalBookings = bookingsData.Values.Where(x => x.RentalId == calendar.RentalId);
            int blockingDays = rental.PreparationTimeInDays;

            foreach (var booking in rentalBookings)
            {
                if (booking.Start <= dateCalendar.Date && booking.Start.AddDays(booking.Nights + blockingDays) > dateCalendar.Date)
                {
                    dateCalendar.Bookings.Add(new CalendarBookingViewModel()
                    { 
                        Id = booking.Id,
                        Unit = booking.UnitId
                    });
                }
            }

            dateCalendar.PreparationTimes = this.GetAvailableUnits(
                totalUnits: rental.Units,
                dateCalendar: dateCalendar);

            calendar.Dates.Add(dateCalendar);
        }

        private List<PreparationTimeCalendarViewModel> GetAvailableUnits(
            int totalUnits, 
            CalendarDateViewModel dateCalendar)
        {
            var result = new List<PreparationTimeCalendarViewModel>();

            for (int unit = 1; unit <= totalUnits; unit++)
            {
                if (dateCalendar.Bookings.Where(x=> x.Unit == unit).FirstOrDefault() == null)
                {
                    result.Add(new PreparationTimeCalendarViewModel()
                    {
                        UnitId = unit
                    });
                }
            }

            return result;
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
