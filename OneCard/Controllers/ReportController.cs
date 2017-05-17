using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;
using OneCard.Models;
using OneCard.Filters;
using OneCard.Helpers;

namespace OneCard.Controllers
{
    public class ReportController : BaseController
    {
        

        //餐饮信息
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        public ActionResult DailyConsumptionDetails(bool exportCSV = false)
        {
            IEnumerable<RoomCosumptionDataViewModel> model = from row in db.CardRecord
                                                             orderby row.ChkTime
                                                             select new RoomCosumptionDataViewModel
                                                             {
                                                                 RoomNumber = row.Room,
                                                                 Count1 = row.time1,
                                                                 Count2 = row.time2,
                                                                 Count3 = row.time3,
                                                                 Count4 = row.time4,
                                                                 Package = row.Package1==1 ? "ABF" : "FBF",
                                                                 DeviceID = row.StationID,
                                                                 CheckInTime = row.ChkTime,
                                                                 IncludeBreakfast = row.yes==1 ? "Yes" : "No",
                                                             };

            ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数", "Package", "含早", "打卡时间", "打卡设备" }), "text/comma-separated-values", ViewBag.Date + "用餐明细记录.csv");
            }
            return View(model);
        }

        //TODO
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        public ActionResult DailyConsumptionSummary(bool exportCSV = false)
        {

            var count1 = db.CardRecord.Count(m => m.time1==1);
            var count2 = db.CardRecord.Count(m => m.time2==1);
            var count3 = db.CardRecord.Count(m => m.time3==1);
            var count4 = db.CardRecord.Count(m => m.time4==1);
            var mABNCount = db.CardRecord.Count(m=>m.Package1==1);
            var mFBNCount = db.CardRecord.Count(m => m.package2 == 1);
            var mYesCount = db.CardRecord.Count(m => m.yes == 1);
            var mNoCount = db.CardRecord.Count(m => m.yes != 1);

            var model = new DailyCosumptionSummaryViewModel
            {
                Count1 = count1,
                Count2 = count2,
                Count3 = count3,
                Count4 = count4,
                ABNCount = mABNCount,
                FBNCount = mFBNCount,
                YesCount = mYesCount,
                NoCount = mNoCount,
            };

            ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(new List<DailyCosumptionSummaryViewModel> { model }, new string[] { "房间号", "时段1", "时段2", "时段3", "时段4", "用餐总数" }), "text/comma-separated-values", ViewBag.Date + "用餐统计.csv");
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        public ActionResult DailyConsumption(bool exportCSV = false)
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
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数" }), "text/comma-separated-values", ViewBag.Date + "用餐按房间汇总.csv");
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        public ActionResult ConsumptionHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsumptionHistory(string StartTime, string EndTime, int? room, bool exportCSV = false)
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
            ViewBag.Date = stTime.ToString("yyyy-M-d") + "至" + edTime.ToString("yyyy-M-d");
            if (room.HasValue)
            {
                model = model.Where(m => m.RoomNumber == room);
                //ViewBag.Date = room + "房间" + ViewBag.Date;
            }
            
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数" }), "text/comma-separated-values", ViewBag.Date + "用餐记录.csv");
            }

            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.room = room;

            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        public ActionResult YearlyConsumption()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YearlyConsumption(int? Year, bool exportCSV = false)
        {
            int mYear = DateTime.Now.Year;

            if(Year.HasValue)
                mYear = Year.Value;
            DateTime st = new DateTime(mYear, 1, 1, 0, 0, 0);
            DateTime et = new DateTime(mYear, 12, 31, 23, 59, 59);
            IEnumerable<MonthlyCosumptionViewModel> model = db.CardRecord_His
                                             .Where(m => m.ChkTime >= st && m.ChkTime <= et)
                                             .GroupBy(m => m.ChkTime.Value.Month)
                                             .OrderBy(m=>m.Key)
                                             .Select(m => new MonthlyCosumptionViewModel {
                                                 Month = SqlFunctions.StringConvert((double)m.Key, 2) + "月",  //SqlFunctions.StringConvert((double)mYear, 4) + "-" +
                                                 Count1 = m.Sum(i => i.time1), 
                                                 Count2 = m.Sum(i => i.time2), 
                                                 Count3 = m.Sum(i => i.time3), 
                                                 Count4 = m.Sum(i => i.time4) 
                                             });

            ViewBag.Year = mYear;
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "月份", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "当月总计" }), "text/comma-separated-values", ViewBag.Year + "年度就餐统计.csv");
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        public ActionResult MonthlyConsumption()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_FINANCE + "," + Constants.Roles.ROLE_DIET)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MonthlyConsumption(int Year, int Month, bool exportCSV = false)
        {

            DateTime st = new DateTime(Year, Month, 1, 0, 0, 0);
            DateTime et = st.AddMonths(1);
            IEnumerable<DailyCosumptionViewModel> model = db.CardRecord_His
                                             .Where(m => m.ChkTime >= st && m.ChkTime < et)
                                             .GroupBy(m => m.ChkTime.Value.Day)
                                             .OrderBy(m => m.Key)
                                             .Select(m => new DailyCosumptionViewModel
                                             {
                                                 Day = SqlFunctions.StringConvert((double)m.Key, 2) + "",  //SqlFunctions.StringConvert((double)mYear, 4) + "-" +
                                                 Count1 = m.Sum(i => i.time1),
                                                 Count2 = m.Sum(i => i.time2),
                                                 Count3 = m.Sum(i => i.time3),
                                                 Count4 = m.Sum(i => i.time4)
                                             });

            ViewBag.Year = Year;
            ViewBag.Month = Month;

            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "日期", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "当天总计" }), "text/comma-separated-values", Year + "年" + Month + "月就餐统计.csv");
            }
            return View(model);
        }

        




        //客房信息
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_LOBBY)]
        public ActionResult DailyRoomBooking(bool exportCSV = false)
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

            ViewBag.Date = db.ZaoCanIn24.FirstOrDefault().InTime.Value.ToString("yyyy-MM-dd");

            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "客人姓名", "中文名", "入住时间", "退房时间", "Adults", "VIP", "Pax", "Package", "录入时间" }), "text/comma-separated-values", ViewBag.Date + "客房数据.csv");
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_LOBBY)]
        public ActionResult RoomBookingHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_LOBBY)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoomBookingHistory(string StartTime, string EndTime, int? room, bool exportCSV = false)
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

            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "客人姓名", "中文名", "入住时间", "退房时间", "Adults", "VIP", "Pax", "Package", "录入时间" }), "text/comma-separated-values", ViewBag.Date + "客房数据.csv");
            }

            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.room = room;

            return View(model);
        }






        //健身中心
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_LOBBY)]
        public ActionResult DailyFitness(bool exportCSV = false)
        {
            IEnumerable<FitnessDataViewModel> model = from row in db.Fitness24
                                                             orderby row.Room
                                                             select new FitnessDataViewModel
                                                             {
                                                                 RoomNumber = row.Room,
                                                                 Count = 1,
                                                                 CheckInTime = row.ChkTime,
                                                             };
            ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "健身中心数据.csv");
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_LOBBY)]
        public ActionResult FitnessHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT + "," + Constants.Roles.ROLE_LOBBY)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FitnessHistory(string StartTime, string EndTime, int? room, bool exportCSV = false)
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


            IEnumerable<FitnessDataViewModel> model = from row in db.Fitness24
                                                      where row.StartTime >= stTime && row.StartTime <= edTime
                                                        group row by new { row.Room } into b
                                                        orderby b.Key.Room
                                                      select new FitnessDataViewModel
                                                          {
                                                              RoomNumber = b.Key.Room,
                                                              Count = b.Count(),
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

            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数" }), "text/comma-separated-values", ViewBag.Date + "健身中心数据.csv");
            }

            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.room = room;

            return View(model);
        }


    }
}
