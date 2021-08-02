using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService rentalService;

        public RentalsController(IRentalService rentalService)
        {
            this.rentalService = rentalService;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public RentalViewModel Get(int rentalId)
        {
            return this.rentalService.GetRental(rentalId: rentalId);
        }

        [HttpPost]
        public IActionResult Post(RentalBindingModel model)
        {
            return Ok(this.rentalService.AddRental(rentalModel: model));
        }
    }
}
