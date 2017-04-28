using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneCard.Models
{
    public class FitnessDataViewModel
    {

        [Display(Name = "房间号")]
        public int RoomNumber { get; set; }

        [Display(Name = "打卡次数")]
        public int Count { get; set; }

        [Display(Name = "打卡时间")]
        public DateTime? CheckInTime { get; set; }
    }
}