using System.Collections.Generic;
using VacationRental.Api.Models;
using VacationRental.Api.Services;
using Xunit;

namespace VacationRental.Api.Tests
{
    [CollectionDefinition("UnitTests")]
    public sealed class UnitTestFixture : ICollectionFixture<UnitTestFixture>
    {
        public IRentalService RentalService { get; }
        public IBookingService BookingService { get; }

        public UnitTestFixture()
        {
            var bookingData = new Dictionary<int, BookingViewModel>();
            var rentalData = new Dictionary<int, RentalViewModel>();

            var bookingService = new BookingService(bookingData, rentalData);
            var rentalService = new RentalService(rentalData, bookingService);

            RentalService = rentalService;
            BookingService = bookingService;
        }
    }
}
