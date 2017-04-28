using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneCard.Models
{
    public class RoomCosumptionDataViewModel
    {

        [Display(Name = "房间号")]
        public int RoomNumber { get; set; }

        [Display(Name = "时段1")]
        public int? Count1 { get; set; }


        [Display(Name = "时段2")]
        public int? Count2 { get; set; }


        [Display(Name = "时段3")]
        public int? Count3 { get; set; }


        [Display(Name = "时段4")]
        public int? Count4 { get; set; }

        [Display(Name = "用餐次数")]
        public int Count { get { return (Count1.HasValue ? Count1.Value : 0) + (Count2.HasValue ? Count2.Value : 0) + (Count3.HasValue ? Count3.Value : 0) + (Count4.HasValue ? Count4.Value : 0); } }
    }
}