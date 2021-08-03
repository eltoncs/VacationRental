//Tests not covered by Integration collection, are covered at service level

using System;
using VacationRental.Api.Extensions;
using VacationRental.Api.Models;
using VacationRental.Api.Services;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("UnitTests")]
    public class ServiceTests
    {
        private readonly IRentalService rentalService;
        private readonly IBookingService bookingService;

        public ServiceTests(UnitTestFixture fixture)
        {
            this.rentalService = fixture.RentalService;
            this.bookingService = fixture.BookingService;
        }

        [Fact]
        public void GivenSuccessfulBookings_WhenChangingRental_ThenAPostReturnsErrorWhenPreparationTimeInDaysIsIncreased()
        {
            var newRental = new RentalBindingModel()
            {
                Units = 1,
                PreparationTimeInDays = 1
            };

            int rentalId = this.rentalService.AddRental(newRental).Id;

            var newBooking1 = new BookingBindingModel(){
                RentalId = rentalId,
                Nights = 3,
                Start = new DateTime(2021, 08, 15)
            };

            this.bookingService.AddBooking(newBooking1);

            var newBooking2 = new BookingBindingModel()
            {
                RentalId = rentalId,
                Nights = 3,
                Start = new DateTime(2021, 08, 19)
            };

            this.bookingService.AddBooking(newBooking2);

            var rentalToUpdate = this.rentalService.GetRental(rentalId);
            rentalToUpdate.PreparationTimeInDays = 2;

            Assert.Throws<HttpException>(() =>
            {
                var id = this.rentalService.UpdateRental(rentalToUpdate);
            });           
        }

        [Fact]
        public void GivenSuccessfulBookings_WhenChangingRental_ThenAPostReturnsErrorWhenUnitIsDecreased()
        {
            var newRental = new RentalBindingModel()
            {
                Units = 2,
                PreparationTimeInDays = 1
            };

            int rentalId = this.rentalService.AddRental(newRental).Id;

            var newBooking1 = new BookingBindingModel()
            {
                RentalId = rentalId,
                Nights = 3,
                Start = new DateTime(2021, 08, 15)
            };

            this.bookingService.AddBooking(newBooking1);

            var newBooking2 = new BookingBindingModel()
            {
                RentalId = rentalId,
                Nights = 1,
                Start = new DateTime(2021, 08, 15)
            };

            this.bookingService.AddBooking(newBooking2);

            var rentalToUpdate = this.rentalService.GetRental(rentalId);
            rentalToUpdate.Units = 1;

            Assert.Throws<HttpException>(() =>
            {
                var id = this.rentalService.UpdateRental(rentalToUpdate);
            });
        }

        [Fact]
        public void GivenRentalWith2Unit_WhenBookingForTheSameDay_ThenSecondUnitIsAssigned()
        {
            var newRental = new RentalBindingModel()
            {
                Units = 2,
                PreparationTimeInDays = 1
            };

            int rentalId = this.rentalService.AddRental(newRental).Id;

            var newBooking1 = new BookingBindingModel()
            {
                RentalId = rentalId,
                Nights = 3,
                Start = new DateTime(2021, 08, 15)
            };

            this.bookingService.AddBooking(newBooking1);

            var newBooking2 = new BookingBindingModel()
            {
                RentalId = rentalId,
                Nights = 1,
                Start = new DateTime(2021, 08, 15)
            };

            int bookingId = this.bookingService.AddBooking(newBooking2).Id;
            BookingViewModel booking = this.bookingService.GetBooking(bookingId);

            Assert.Equal(2, booking.UnitId);
        }
    }
}
