using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VacationRental.Api.Extensions;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public class BookingService : IBookingService
    {
        private readonly IDictionary<int, BookingViewModel> bookingsData;
        private readonly IDictionary<int, RentalViewModel> rentalsData;
        private readonly IConfiguration configuration;

        public BookingService(
            IDictionary<int, BookingViewModel> bookingsData,
            IDictionary<int, RentalViewModel> rentalsData,
            IConfiguration configuration)
        {
            this.bookingsData = bookingsData;
            this.rentalsData = rentalsData;
            this.configuration = configuration;
        }

        public ResourceIdViewModel AddBooking(BookingBindingModel bookingModel)
        {
            this.ValidateData(bookingModel);

            if (!IsReantalAvaileble(newBooking: bookingModel))
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.NotFound,
                    message: "Not available");
            }

            var key = new ResourceIdViewModel 
            { 
                Id = this.bookingsData.Keys.Count + 1 
            };

            this.bookingsData.Add(key.Id, new BookingViewModel
            {
                Id = key.Id,
                Nights = bookingModel.Nights,
                RentalId = bookingModel.RentalId,
                Start = bookingModel.Start.Date
            });

            return key;
        }

        public BookingViewModel GetBooking(int bookingId)
        {
            if (!bookingsData.ContainsKey(bookingId))
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.NotFound,
                    message: "Booking not found");
            }            

            return bookingsData[bookingId];
        }

        private bool IsReantalAvaileble(BookingBindingModel newBooking)
        {
            if (this.bookingsData.Values.Count == 0)
            {
                return true;
            }

            var ocupied = 0;
            RentalViewModel rental = this.rentalsData[newBooking.RentalId];
            int blockingDays = int.Parse(configuration["Preferences:BlokingDays"]);

            var bookingsForRental = this.bookingsData.Values.Where(x => x.RentalId == newBooking.RentalId);

            foreach(var booking in bookingsForRental)
            {
                bool isOcupied =
                    (booking.Start <= newBooking.Start.Date && booking.Start.AddDays(booking.Nights + blockingDays) > 
                    newBooking.Start.Date) || (booking.Start < newBooking.Start.AddDays(newBooking.Nights + blockingDays) && 
                    booking.Start.AddDays(booking.Nights) >= newBooking.Start.AddDays(newBooking.Nights + blockingDays)) || 
                    (booking.Start > newBooking.Start && booking.Start.AddDays(booking.Nights + blockingDays) < 
                    newBooking.Start.AddDays(newBooking.Nights + blockingDays));

                if (isOcupied)
                {
                    ocupied++;
                }
            }

            return ocupied < rental.Units;
        }

        private void ValidateData(BookingBindingModel bookingModel)
        {
            if (bookingModel.Nights <= 0)
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.BadRequest,
                    message: "Nights must be positive");
            }

            if (!this.rentalsData.ContainsKey(bookingModel.RentalId))
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.NotFound, 
                    message: "Rental not found");
            }
        }
    }
}
