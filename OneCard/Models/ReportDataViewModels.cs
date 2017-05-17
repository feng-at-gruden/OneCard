using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneCard.Models
{
    


    //用餐数据
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


        [Display(Name = "Package")]
        public string Package { get; set; }

        [Display(Name = "含早")]
        public bool IncludeBreakfast { get; set; }

        [Display(Name = "打卡时间")]
        public DateTime? CheckInTime { get; set; }

        [Display(Name = "打卡设备")]
        public int? DeviceID { get; set; }
    }

    public class CosumptionHistoryViewModel
    {
        [Display(Name = "房间号")]
        public string RoomNumber { get; set; }


        [Display(Name = "用餐次数")]
        public int Count { get; set; }
    }

    public class MonthlyCosumptionViewModel
    {
        [Display(Name = "月份")]
        public string Month { get; set; }

        [Display(Name = "时段1")]
        public int? Count1 { get; set; }


        [Display(Name = "时段2")]
        public int? Count2 { get; set; }


        [Display(Name = "时段3")]
        public int? Count3 { get; set; }


        [Display(Name = "时段4")]
        public int? Count4 { get; set; }


        [Display(Name = "用餐次数")]
        public int Total { get { return (Count1.HasValue ? Count1.Value : 0) + (Count2.HasValue ? Count2.Value : 0) + (Count3.HasValue ? Count3.Value : 0) + (Count4.HasValue ? Count4.Value : 0); } }
    }

    public class DailyCosumptionViewModel
    {
        [Display(Name = "日期")]
        public string Day { get; set; }

        [Display(Name = "时段1")]
        public int? Count1 { get; set; }


        [Display(Name = "时段2")]
        public int? Count2 { get; set; }


        [Display(Name = "时段3")]
        public int? Count3 { get; set; }


        [Display(Name = "时段4")]
        public int? Count4 { get; set; }


        [Display(Name = "用餐次数")]
        public int Total { get { return (Count1.HasValue ? Count1.Value : 0) + (Count2.HasValue ? Count2.Value : 0) + (Count3.HasValue ? Count3.Value : 0) + (Count4.HasValue ? Count4.Value : 0); } }
    }




    //订房数据
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

    


    //健身中心数据
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