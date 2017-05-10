using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneCard.Models
{
    public class RoomBookingDataViewModel
    {

        [Display(Name = "房间号")]
        public int? RoomNumber { get; set; }

        [Display(Name = "客人姓名")]
        public String GuestName { get; set; }

        [Display(Name = "中文名")]
        public String ChineseName { get; set; }

        [Display(Name = "入住时间")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ArriveTime { get; set; }

        [Display(Name = "退房时间")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DepartTime { get; set; }

        
        [Display(Name = "Adults")]
        public int? Adults { get; set; }

        [Display(Name = "VIP")]
        public int? VIP { get; set; }

        [Display(Name = "Pax")]
        public int? Pax { get; set; }

        [Display(Name = "Package")]
        public String Package { get; set; }


        [Display(Name = "录入时间")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? InTime { get; set; }

    }
}