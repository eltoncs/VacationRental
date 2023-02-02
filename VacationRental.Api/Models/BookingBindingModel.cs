using System;

namespace VacationRental.Api.Models
{
    public class BookingBindingModel
    {
        private DateTime _startIgnoreTime;

        public int RentalId { get; set; }

        public DateTime Start
        {
            get => _startIgnoreTime;
            set => _startIgnoreTime = value.Date;
        }
        
        public int Nights { get; set; }
    }
}
