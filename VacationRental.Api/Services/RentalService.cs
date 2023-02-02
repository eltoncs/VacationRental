using System.Collections.Generic;
using System.Net;
using VacationRental.Api.Extensions;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public class RentalService : IRentalService
    {
        private readonly IDictionary<int, RentalViewModel> rentalsData;
        private readonly IBookingService bookingService;

        public RentalService(
            IDictionary<int, RentalViewModel> rentalsData,
            IBookingService bookingService)
        {
            this.rentalsData = rentalsData;
            this.bookingService = bookingService;
        }

        public RentalViewModel GetRental(int rentalId)
        {
            if (!this.RentalExist(rentalId))
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.NotFound,
                    message: "Rental not found");
            }

            return this.rentalsData[rentalId];
        }

        public ResourceIdViewModel AddRental(RentalBindingModel rentalModel)
        {
            this.ValidateData(rentalModel);

            var key = new ResourceIdViewModel { 
                Id = this.rentalsData.Keys.Count + 1 
            };

            this.rentalsData.Add(key.Id, new RentalViewModel
            {
                Id = key.Id,
                Units = rentalModel.Units,
                PreparationTimeInDays = rentalModel.PreparationTimeInDays
            });

            return key;
        }

        public RentalViewModel UpdateRental(RentalViewModel rentalModel)
        {
            this.ValidateData(rentalModel: rentalModel);
            this.ValidateRentalId(rentalModel.Id);

            var tempRental = new RentalViewModel()
            {
                Id = rentalModel.Id,
                Units = rentalModel.Units,
                PreparationTimeInDays = rentalModel.PreparationTimeInDays
            };

            this.bookingService.CheckBookingBackwardConsistency(tempRental);

            var rental = this.rentalsData[rentalModel.Id];
            rental.Units = rentalModel.Units;
            rental.PreparationTimeInDays = rentalModel.PreparationTimeInDays;            

            return rental;
        }

        public bool RentalExist(int rentalId)
        {
            return this.rentalsData.ContainsKey(rentalId);
        }

        private void ValidateRentalId(int rentalId)
        {
            if (!this.rentalsData.ContainsKey(rentalId))
            {
                throw new HttpException(
                    httpStatusCode: HttpStatusCode.NotFound,
                    message: "Rental not found");
            }
        }

        private void ValidateData(RentalBindingModel rentalModel)
        {
            if (rentalModel.Units <= 0)
            {
                throw new HttpException(
                   httpStatusCode: HttpStatusCode.BadRequest,
                   message: "Unit number must be positive");
            }
        }
    }
}
