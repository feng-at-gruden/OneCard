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

        public ActionResult DailyReport()
        {
            IEnumerable<RoomDataViewModel> model = from row in db.CardRecord
                                                   group row by new { row.Room } into b
                                                   orderby b.Key.Room
                                                   select new RoomDataViewModel
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





        public ActionResult History()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult History(string StartTime, string EndTime, int? room)
        {
            if (string.IsNullOrWhiteSpace(StartTime) || string.IsNullOrWhiteSpace(EndTime))
            {
                ModelState.AddModelError("", "请输入查询起止时间");
                return View();
            }
            
            DateTime stTime = DateTime.Parse(StartTime + " 00:00:00");
            DateTime edTime = DateTime.Parse(EndTime + " 23:59:59");
            IEnumerable<RoomDataViewModel> model = from row in db.CardRecord_His
                                                   where row.ChkTime >= stTime && row.ChkTime <= edTime
                                                   group row by new { row.Room } into b
                                                   orderby b.Key.Room
                                                   select new RoomDataViewModel
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

    }
}
