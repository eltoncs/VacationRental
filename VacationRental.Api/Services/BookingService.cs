using System;
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

        public BookingService(
            IDictionary<int, BookingViewModel> bookingsData,
            IDictionary<int, RentalViewModel> rentalsData)
        {
            this.bookingsData = bookingsData;
            this.rentalsData = rentalsData;
        }

        public ResourceIdViewModel AddBooking(BookingBindingModel bookingModel)
        {
            this.ValidateData(bookingModel);

            (bool isAvailable, int unitId) = this.CheckRentalAvailability(newBooking: bookingModel);

            if (!isAvailable)
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
                UnitId = unitId,
                Start = bookingModel.Start.Date
            });

            return key;
        }

        public BookingViewModel GetBooking(int bookingId)
        {
            if (!this.bookingsData.ContainsKey(bookingId))
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.NotFound,
                    message: "Booking not found");
            }            

            return bookingsData[bookingId];
        }

        public void CheckBookingBackwardConsistency(RentalViewModel rental)
        {
            var rentalBookings = this.bookingsData.Values.Where(x => x.RentalId == rental.Id).OrderBy(x => x.Start);
            var newBookings = new Dictionary<int, BookingViewModel>();

            foreach (var booking in rentalBookings)
            {
                (bool isAvailable, int unitId) = this.CheckRentalBackwardAvailability(
                    newBooking: booking,
                    tempBookingsData: newBookings,
                    tempRental: rental);

                if (!isAvailable)
                {
                    throw new HttpException(
                        httpStatusCode: HttpStatusCode.Forbidden,
                        message: "Unable to change rental");
                }

                newBookings.Add(booking.Id, booking);
            }
        }

        private Tuple<bool, int> CheckRentalBackwardAvailability(
            BookingViewModel newBooking,
            IDictionary<int, BookingViewModel> tempBookingsData,
            RentalViewModel tempRental)
        {
            if (tempBookingsData.Values.Count == 0)
            {
                return new Tuple<bool, int>(true, 1);
            }

            var ocupiedUnits = new List<int>();
            int blockingDays = tempRental.PreparationTimeInDays;

            var bookingsForRental = tempBookingsData.Values.Where(x => x.RentalId == newBooking.RentalId);

            foreach (var booking in bookingsForRental)
            {
                bool isOcupied =
                    (booking.Start <= newBooking.Start.Date && booking.Start.AddDays(booking.Nights + blockingDays) >
                    newBooking.Start.Date) || (booking.Start < newBooking.Start.AddDays(newBooking.Nights + blockingDays) &&
                    booking.Start.AddDays(booking.Nights) >= newBooking.Start.AddDays(newBooking.Nights + blockingDays)) ||
                    (booking.Start > newBooking.Start && booking.Start.AddDays(booking.Nights + blockingDays) <
                    newBooking.Start.AddDays(newBooking.Nights + blockingDays));

                if (isOcupied)
                {
                    ocupiedUnits.Add(booking.UnitId);
                }
            }

            bool isAvailable = ocupiedUnits.Count < tempRental.Units;

            if (isAvailable)
            {
                int nextAvailableUnit = this.GetNextAvailableUnit(
                    ocupiedUnits: ocupiedUnits,
                    rentalUnits: tempRental.Units);

                return new Tuple<bool, int>(true, nextAvailableUnit);
            }

            return new Tuple<bool, int>(false, 0);
        }

        private Tuple<bool, int> CheckRentalAvailability(BookingBindingModel newBooking)
        {
            if (this.bookingsData.Values.Count == 0)
            {
                return new Tuple<bool, int>(true, 1);
            }

            var ocupiedUnits = new List<int>();
            RentalViewModel rental = this.rentalsData[newBooking.RentalId];
            int blockingDays = rental.PreparationTimeInDays;

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
                    ocupiedUnits.Add(booking.UnitId);
                }
            }

            bool isAvailable = ocupiedUnits.Count < rental.Units;

            if (isAvailable)
            {
                int nextAvailableUnit = this.GetNextAvailableUnit(
                    ocupiedUnits: ocupiedUnits,
                    rentalUnits: rental.Units);

                return new Tuple<bool, int>(true, nextAvailableUnit);
            }

            return new Tuple<bool, int>(false, 0);
        }

        private int GetNextAvailableUnit(
            List<int> ocupiedUnits, 
            int rentalUnits)
        {
            for(var unit = 1; unit <= rentalUnits; unit++)
            {
                if (ocupiedUnits.Where(x => x == unit).FirstOrDefault() == 0)
                {
                    return unit;
                }
            }

            return 0;//never hit due to isAvailable check in CheckReantalability
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
