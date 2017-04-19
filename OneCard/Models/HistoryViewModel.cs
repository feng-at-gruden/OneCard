using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneCard.Models
{
    public class HistoryViewModel
    {
        [Display(Name = "房间号")]
        public string RoomNumber { get; set; }


        [Display(Name = "用餐次数")]
        public int Count { get; set; }
    }
}