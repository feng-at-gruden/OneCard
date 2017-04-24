using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OneCard.Models;

namespace OneCard.Controllers
{
    public class ReportController : BaseController
    {
        //
        // GET: /Report/

        public ActionResult DailyConsumption()
        {
            IEnumerable<RoomCosumptionDataViewModel> model = from row in db.CardRecord
                                                   group row by new { row.Room } into b
                                                   orderby b.Key.Room
                                                   select new RoomCosumptionDataViewModel
                                                   {
                                                       RoomNumber = b.Key.Room,
                                                       Count1 = b.Sum(c=>c.time1),
                                                       Count2 = b.Sum(c => c.time2),
                                                       Count3 = b.Sum(c => c.time3),
                                                       Count4 = b.Sum(c => c.time4),
                                                   };

            ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            return View(model);
        }


        public ActionResult ConsumptionHistory()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsumptionHistory(string StartTime, string EndTime, int? room)
        {
            if (string.IsNullOrWhiteSpace(StartTime) || string.IsNullOrWhiteSpace(EndTime))
            {
                ModelState.AddModelError("", "请输入查询起止时间");
                return View();
            }
            
            DateTime stTime = DateTime.Parse(StartTime + " 00:00:00");
            DateTime edTime = DateTime.Parse(EndTime + " 23:59:59");
            IEnumerable<RoomCosumptionDataViewModel> model = from row in db.CardRecord_His
                                                   where row.ChkTime >= stTime && row.ChkTime <= edTime
                                                   group row by new { row.Room } into b
                                                   orderby b.Key.Room
                                                   select new RoomCosumptionDataViewModel
                                                   {
                                                       RoomNumber = b.Key.Room,
                                                       Count1 = b.Sum(c => c.time1),
                                                       Count2 = b.Sum(c => c.time2),
                                                       Count3 = b.Sum(c => c.time3),
                                                       Count4 = b.Sum(c => c.time4),
                                                   };
            if (room.HasValue)
                model = model.Where(m => m.RoomNumber == room);
            ViewBag.Date = stTime.ToString("yyyy-M-d") + "至" + edTime.ToString("yyyy-M-d");
            return View(model);
        }


        public ActionResult DailyRoomBooking()
        {
            IEnumerable<RoomBookingDataViewModel> model = from row in db.ZaoCanIn24                                                             
                                                             orderby row.Room
                                                             select new RoomBookingDataViewModel
                                                             {
                                                                RoomNumber = row.Room,
                                                                Adults = row.Num,
                                                                ArriveTime = row.StartTime,
                                                                DepartTime = row.EndTime,
                                                                InTime = row.InTime,
                                                                VIP = row.Vip, 
                                                                ChineseName = row.ChineseName, 
                                                                GuestName = row.FullName, 
                                                                Package = row.Package, 
                                                                Pax = row.Pax,
                                                             };

            ViewBag.Date = db.ZaoCanIn24.FirstOrDefault().InTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            return View(model);
        }


        public ActionResult RoomBookingHistory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoomBookingHistory(string StartTime, string EndTime, int? room)
        {
            DateTime edTime = DateTime.Now.AddDays(-1);
            DateTime stTime = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(StartTime))
            {
                stTime = DateTime.Parse(StartTime + " 00:00:00");
            }
            if (!string.IsNullOrWhiteSpace(EndTime))
            {
                edTime = DateTime.Parse(EndTime + " 23:59:59");
            }
            

            IEnumerable<RoomBookingDataViewModel> model = from row in db.ZaoCanIn
                                                          orderby row.Room
                                                          where row.StartTime >= stTime && row.StartTime<= edTime
                                                          select new RoomBookingDataViewModel
                                                          {
                                                              RoomNumber = row.Room,
                                                              Adults = row.Num,
                                                              ArriveTime = row.StartTime,
                                                              DepartTime = row.EndTime,
                                                              InTime = row.InTime,
                                                              VIP = row.Vip,
                                                              ChineseName = row.ChineseName,
                                                              GuestName = row.FullName,
                                                              Package = row.Package,
                                                              Pax = row.Pax,
                                                          };

            if (room.HasValue)
                model = model.Where(m => m.RoomNumber == room);

            if (!string.IsNullOrWhiteSpace(StartTime))
            {
                ViewBag.Date = stTime.ToString("yyyy-MM-dd") + "至" + edTime.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Date = "截至" + edTime.ToString("yyyy-MM-dd");
            }
            
            return View(model);
        }




    }
}
