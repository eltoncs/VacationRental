using System.Collections.Generic;
using System.Net;
using VacationRental.Api.Extensions;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public class RentalService : IRentalService
    {
        private readonly IDictionary<int, RentalViewModel> rentalsData;

        public RentalService(IDictionary<int, RentalViewModel> rentalsData)
        {
            this.rentalsData = rentalsData;
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
                Units = rentalModel.Units
            });

            return key;
        }

        public bool RentalExist(int rentalId)
        {
            return this.rentalsData.ContainsKey(rentalId);
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
