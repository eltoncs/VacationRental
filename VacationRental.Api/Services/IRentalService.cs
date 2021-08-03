using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public interface IRentalService
    {
        ResourceIdViewModel AddRental(RentalBindingModel rentalModel);
        RentalViewModel GetRental(int rentalId);
        RentalViewModel UpdateRental(RentalViewModel rentalModel);
        bool RentalExist(int rentalId);
    }
}