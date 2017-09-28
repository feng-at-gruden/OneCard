using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Data.Objects.SqlClient;
using OneCard.Models;
using OneCard.Filters;
using OneCard.Helpers;

namespace OneCard.Controllers
{

    public class ReportController : BaseController
    {


    #region 餐饮信息

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        public ActionResult DailyConsumptionDetails(bool exportCSV = false, bool mail = false)
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
                                                                 Package = row.Package1==1 ? Constants.PackageDisplay.Package1 : Constants.PackageDisplay.Package2,
                                                                 DeviceID = row.StationID,
                                                                 CheckInTime = row.ChkTime,
                                                                 IncludeBreakfast = row.yes==1 ? "Yes" : "No",
                                                             };
            if (db.CardRecord.Count() <= 0)
            {
                ViewBag.ErrorMessage = "对不起，当日没有就餐记录。";
                return View(model);
            }
            ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if(exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数", "Package", "含早", "打卡时间", "打卡设备" }), "text/comma-separated-values", ViewBag.Date + "用餐明细记录.csv");
            }
            if(mail)
            {
                if(string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "用餐明细记录", 
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数", "Package", "含早", "打卡时间", "打卡设备" }),
                        ViewBag.Date + "用餐明细记录.csv" );

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        public ActionResult DailyConsumptionSummary(int? year, int? month, int? day, bool exportCSV = false, bool mail = false)
        {
            var count1 = 0;
            var count2 = 0;
            var count3 = 0;
            var count4 = 0;
            var mABNCount = 0;
            var mFBNCount = 0;
            var mYesCount = 0;
            var mNoCount = 0;
            if(year.HasValue && month.HasValue && day.HasValue)
            {
                DateTime st = new DateTime(year.Value, month.Value, day.Value, 0, 0, 0);
                DateTime et = new DateTime(year.Value, month.Value, day.Value, 23, 59, 59);
                count1 = db.CardRecord_His.Count(m => m.time1 == 1 && m.ChkTime>=st && m.ChkTime<=et);
                count2 = db.CardRecord_His.Count(m => m.time2 == 1 && m.ChkTime >= st && m.ChkTime <= et);
                count3 = db.CardRecord_His.Count(m => m.time3 == 1 && m.ChkTime >= st && m.ChkTime <= et);
                count4 = db.CardRecord_His.Count(m => m.time4 == 1 && m.ChkTime >= st && m.ChkTime <= et);
                mABNCount = db.CardRecord_His.Count(m => m.package1 == 1 && m.ChkTime >= st && m.ChkTime <= et);
                //mFBNCount = db.CardRecord_His.Count(m => m.package2 == 1 && m.ChkTime >= st && m.ChkTime <= et);
                mYesCount = db.CardRecord_His.Count(m => m.yes == 1 && m.ChkTime >= st && m.ChkTime <= et);
                //mNoCount = db.CardRecord_His.Count(m => m.yes != 1 && m.ChkTime >= st && m.ChkTime <= et);
                ViewBag.Date = st.ToString("yyyy-M-d");
            }
            else
            {
                count1 = db.CardRecord.Count(m => m.time1 == 1);
                count2 = db.CardRecord.Count(m => m.time2 == 1);
                count3 = db.CardRecord.Count(m => m.time3 == 1);
                count4 = db.CardRecord.Count(m => m.time4 == 1);
                mABNCount = db.CardRecord.Count(m => m.Package1 == 1);
                //mFBNCount = db.CardRecord.Count(m => m.package2 == 1);
                mYesCount = db.CardRecord.Count(m => m.yes == 1);
                //mNoCount = db.CardRecord.Count(m => m.yes != 1);
                if (db.CardRecord.Count() <= 0)
                {
                    ViewBag.ErrorMessage = "对不起，当日没有就餐记录。";
                    ViewBag.Date = DateTime.Now.ToString("yyyy-M-d");
                    return View(new DailyCosumptionSummaryViewModel
                    {
                        Count1 = 0,
                        Count2 = 0,
                        Count3 = 0,
                        Count4 = 0,
                        ABNCount = 0,
                        YesCount = 0,
                    });
                }
                ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            }

            var model = new DailyCosumptionSummaryViewModel
            {
                Count1 = count1,
                Count2 = count2,
                Count3 = count3,
                Count4 = count4,
                ABNCount = mABNCount,
                //FBNCount = mFBNCount,
                YesCount = mYesCount,
                //NoCount = mNoCount,
            };
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(new List<DailyCosumptionSummaryViewModel> { model }, new string[] { "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", Constants.PackageDisplay.Package1, Constants.PackageDisplay.Package2, "含早", "不含早", "用餐总数" }), "text/comma-separated-values", ViewBag.Date + "用餐汇总.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "用餐汇总",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(new List<DailyCosumptionSummaryViewModel> { model }, new string[] { "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", Constants.PackageDisplay.Package1, Constants.PackageDisplay.Package2, "含早", "不含早", "用餐总数" }),
                        ViewBag.Date + "用餐汇总.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        public ActionResult DailyConsumption(bool exportCSV = false, bool mail = false)
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
            if (db.CardRecord.Count() <= 0)
            {
                ViewBag.ErrorMessage = "对不起，当日没有就餐记录。";
                return View(model);
            }
            ViewBag.Date = db.CardRecord.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数" }), "text/comma-separated-values", ViewBag.Date + "用餐按房间汇总.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "用餐按房间汇总",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数" }),
                        ViewBag.Date + "用餐按房间汇总.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        public ActionResult ConsumptionHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsumptionHistory(string StartDate, string EndDate, string StartTime, string EndTime, int? room, bool exportCSV = false, bool mail = false)
        {
            if (string.IsNullOrWhiteSpace(StartDate) || string.IsNullOrWhiteSpace(EndDate))
            {
                ModelState.AddModelError("", "请输入查询起止日期");
                return View();
            }
            
            DateTime stTime = DateTime.Parse(StartDate + " 00:00:00");
            DateTime edTime = DateTime.Parse(EndDate + " 23:59:59");
            IEnumerable<RoomCosumptionDataViewModel> model = from row in db.CardRecord_His
                                                   where row.ChkTime >= stTime && row.ChkTime <= edTime
                                                   orderby row.ChkTime
                                                   select new RoomCosumptionDataViewModel
                                                   {
                                                       RoomNumber = row.Room,
                                                       Count1 = row.time1,
                                                       Count2 = row.time2,
                                                       Count3 = row.time3,
                                                       Count4 = row.time4,
                                                       Package = row.package1 == 1 ? Constants.PackageDisplay.Package1 : Constants.PackageDisplay.Package2,
                                                       DeviceID = row.StationID,
                                                       CheckInTime = row.ChkTime,
                                                       IncludeBreakfast = row.yes == 1 ? "Yes" : "No",
                                                   };
            ViewBag.Date = stTime.ToString("yyyy-M-d") + "至" + edTime.ToString("yyyy-M-d");
            if (room.HasValue)
            {
                model = model.Where(m => m.RoomNumber == room);
                //ViewBag.Date = room + "房间" + ViewBag.Date;
            }
            List<RoomCosumptionDataViewModel> filteredModel = new List<RoomCosumptionDataViewModel>();
            if (!string.IsNullOrWhiteSpace(StartTime) && !string.IsNullOrWhiteSpace(EndTime))
            {
                try
                {
                    DateTime stt = DateTime.Parse("2017-05-23 " + StartTime + ":00");
                    DateTime edt = DateTime.Parse("2017-05-23 " + EndTime + ":00");
                    if (stt > edt)
                    { 
                        ModelState.AddModelError("", "截至时间不能小于起始时间");
                        return View();
                    }   

                    foreach (RoomCosumptionDataViewModel item in model)
                    {
                        string date = item.CheckInTime.Value.ToString("yyyy-MM-dd") + " ";
                        stt = DateTime.Parse(date + StartTime + ":00");
                        edt = DateTime.Parse(date + EndTime + ":00");
                        if(item.CheckInTime.Value>=stt && item.CheckInTime.Value <= edt)
                            filteredModel.Add(item);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "时间输入有误，请输入起始时间和截至时间.");
                    return View();
                }    
            }
            else
            {
                filteredModel = model.ToList();
            }

            var filename = ViewBag.Date + "用餐详细记录";
            if (room.HasValue)
                filename = ViewBag.Date + " " + room.Value + "房间用餐详细记录";
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(filteredModel, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数", "Package", "含早", "打卡时间", "打卡设备" }), "text/comma-separated-values", filename + ".csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        filename,
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(filteredModel, new string[] { "房间号", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "用餐总数", "Package", "含早", "打卡时间", "打卡设备" }),
                        filename + ".csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }

            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.room = room;

            return View(filteredModel);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        public ActionResult YearlyConsumption()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult YearlyConsumption(int? Year, bool exportCSV = false, bool mail = false)
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
                return File(CSVHelper.ExportCSV(model, new string[] { "月份", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "当月总计" }), "text/comma-separated-values", ViewBag.Year + "年度用餐统计.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Year + "年度用餐统计",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "月份", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "当月总计" }),
                        ViewBag.Year + "年度用餐统计.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        public ActionResult MonthlyConsumption()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + "," 
            + Constants.Roles.ROLE_FINANCE + "," 
            + Constants.Roles.ROLE_DIET)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MonthlyConsumption(int Year, int Month, bool exportCSV = false, bool mail = false)
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
                return File(CSVHelper.ExportCSV(model, new string[] { "日期", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "当天总计" }), "text/comma-separated-values", Year + "年" + Month + "月用餐统计.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        Year + "年" + Month + "月用餐统计",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "日期", "6:30 - 7:30", "7:30 - 9:00", "9:00 - 10:30", "其他时段", "当天总计" }),
                        Year + "年" + Month + "月用餐统计.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

    #endregion
        



    #region 客房信息
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult DailyRoomBooking(bool exportCSV = false, bool mail = false)
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
            if (db.ZaoCanIn24.Count() <= 0)
            {
                ViewBag.ErrorMessage = "对不起，暂无当日入住信息。";
                return View(model);
            }
            ViewBag.Date = db.ZaoCanIn24.FirstOrDefault().InTime.Value.ToString("yyyy-MM-dd");
            model = filterGuestName(model);
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "客人姓名", "中文名", "入住时间", "退房时间", "Adults", "VIP", "Pax", "Package", "录入时间" }), "text/comma-separated-values", ViewBag.Date + "客房数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "客房数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "客人姓名", "中文名", "入住时间", "退房时间", "Adults", "VIP", "Pax", "Package", "录入时间" }),
                        ViewBag.Date + "客房数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult RoomBookingHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_LOBBY)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoomBookingHistory(string StartTime, string EndTime, int? room, bool exportCSV = false, bool mail = false)
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
                                                          where row.InTime >= stTime && row.InTime<= edTime
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

            model = filterGuestName(model);
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
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "客房数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "客人姓名", "中文名", "入住时间", "退房时间", "Adults", "VIP", "Pax", "Package", "录入时间" }),
                        ViewBag.Date + "客房数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }

            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.room = room;

            return View(model);
        }

    #endregion




    #region 健身中心

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult DailyFitness(bool exportCSV = false, bool mail = false)
        {
            IEnumerable<FitnessDataViewModel> model = from row in db.Fitness24
                                                             orderby row.Room
                                                             select new FitnessDataViewModel
                                                             {
                                                                 RoomNumber = row.Room,
                                                                 Count = 1,
                                                                 CheckInTime = row.ChkTime,
                                                             };
            if(db.Fitness24.Count()<=0)
            {
                ViewBag.ErrorMessage = "对不起，没有当日健身记录。";
                return View(model);
            }
            ViewBag.Date = db.Fitness24.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "健身中心当日数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "健身中心当日数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }),
                        ViewBag.Date + "健身中心当日数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult FitnessHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_LOBBY)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FitnessHistory(string StartTime, string EndTime, int? room, bool exportCSV = false, bool mail = false)
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


            IEnumerable<FitnessDataViewModel> model = from row in db.Fitness
                                                      where row.StartTime >= stTime && row.StartTime <= edTime
                                                      orderby row.ChkTime
                                                      select new FitnessDataViewModel
                                                        {
                                                            RoomNumber = row.Room,
                                                            Count = 1,
                                                            CheckInTime = row.ChkTime,
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
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "健身中心数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "健身中心数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }),
                        ViewBag.Date + "健身中心数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.room = room;

            return View(model);
        }

    #endregion




    #region 行政酒廊

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult DailyBar(bool exportCSV = false, bool mail = false)
        {
            IEnumerable<FitnessDataViewModel> model = from row in db.ExFloor_Record24
                                                      orderby row.Room
                                                      select new FitnessDataViewModel
                                                      {
                                                          RoomNumber = row.Room,
                                                          Count = 1,
                                                          CheckInTime = row.ChkTime,
                                                      };
            if (db.ExFloor_Record24.Count() <= 0)
            {
                ViewBag.ErrorMessage = "对不起，没有当日消费记录。";
                return View(model);
            }
            ViewBag.Date = db.ExFloor_Record24.FirstOrDefault().ChkTime.Value.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "行政酒廊当日数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "行政酒廊当日数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }),
                        ViewBag.Date + "行政酒廊当日数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult BarHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," 
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BarHistory(string StartTime, string EndTime, int? room, bool exportCSV = false, bool mail = false)
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


            IEnumerable<FitnessDataViewModel> model = from row in db.ExFloor_Record
                                                      where row.StartTime >= stTime && row.StartTime <= edTime
                                                      orderby row.ChkTime
                                                      select new FitnessDataViewModel
                                                      {
                                                          RoomNumber = row.Room,
                                                          Count = 1,
                                                          CheckInTime = row.ChkTime,
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
                return File(CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "行政酒廊数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "行政酒廊数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "房间号", "打卡次数", "打卡时间" }),
                        ViewBag.Date + "行政酒廊数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.room = room;

            return View(model);
        }

    #endregion




    #region 游泳池

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + ","
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult SwimmingCards()
        {
            var model = from row in db.SwimmingCard
                        select new SwimmingCardViewModel
                        {
                            Id = row.Id,
                            CardID = row.CardId,
                            CardNumber = row.CardNumber,
                            Name = row.UserName,
                            Gender = row.Gender,
                            Count = row.Count,
                            IssueTime = row.CreatedAt,
                            ExpiredTime = row.ExpiredAt,
                            LastTime = row.LastTime,
                        };
            return View(model);
        }

       
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + ","
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult DailySwimming(bool exportCSV = false, bool mail = false)
        {
            IEnumerable<SwimmingHistoryViewModel> model = from row in db.SwimmingRecord24
                                                      orderby row.CardNumber
                                                      select new SwimmingHistoryViewModel
                                                      {
                                                          CardNumber = row.CardNumber,
                                                          Count = 1,
                                                          CheckInTime = row.ChkTime,
                                                      };
            if (db.SwimmingRecord24.Count() <= 0)
            {
                ViewBag.ErrorMessage = "对不起，没有当日游泳记录。";
                return View(model);
            }
            ViewBag.Date = db.SwimmingRecord24.FirstOrDefault().ChkTime.ToString("yyyy-M-d");
            if (exportCSV)
            {
                return File(CSVHelper.ExportCSV(model, new string[] { "游泳卡号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "游泳池当日数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "游泳池当日数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "游泳卡号", "打卡次数", "打卡时间" }),
                        ViewBag.Date + "游泳池当日数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            return View(model);            
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + ","
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        public ActionResult SwimmingHistory()
        {
            return View();
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + ","
            + Constants.Roles.ROLE_IT + ","
            + Constants.Roles.ROLE_FINANCE + ","
            + Constants.Roles.ROLE_DIET + ","
            + Constants.Roles.ROLE_LOBBY)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SwimmingHistory(string StartTime, string EndTime, String card, bool exportCSV = false, bool mail = false)
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


            IEnumerable<SwimmingHistoryViewModel> model = from row in db.SwimmingRecord
                                                          where row.ChkTime >= stTime && row.ChkTime <= edTime
                                                          orderby row.ChkTime
                                                        select new SwimmingHistoryViewModel
                                                          {
                                                              CardNumber = row.CardNumber,
                                                              Count = 1,
                                                              CheckInTime = row.ChkTime,
                                                          };

            if (!String.IsNullOrWhiteSpace(card))
                model = model.Where(m => m.CardNumber == card);

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
                return File(CSVHelper.ExportCSV(model, new string[] { "游泳卡号", "打卡次数", "打卡时间" }), "text/comma-separated-values", ViewBag.Date + "游泳池数据.csv");
            }
            if (mail)
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Email))
                {
                    ViewBag.ErrorMessage = "对不起，您还没有设置接收邮箱，请在个人设置中设置您的接收邮箱。";
                }
                else
                {
                    MailHelper.SendMail(
                        ViewBag.Date + "游泳池数据",
                        CurrentUser.Email,
                        CSVHelper.ExportCSV(model, new string[] { "游泳卡号", "打卡次数", "打卡时间" }),
                        ViewBag.Date + "游泳池数据.csv");

                    ViewBag.SuccessMessage = "邮件发送成功";
                }
            }
            //Store form values;
            ViewBag.StartTime = StartTime;
            ViewBag.EndTime = EndTime;
            ViewBag.card = card;

            return View(model);
        }

    #endregion




        private IEnumerable<RoomBookingDataViewModel> filterGuestName(IEnumerable<RoomBookingDataViewModel> model)
        {

            if(CurrentUser.UserRole.Role.Equals(Constants.Roles.ROLE_ADMIN) ||
                CurrentUser.UserRole.Role.Equals(Constants.Roles.ROLE_IT))
            {
                return model;
            }
            List<RoomBookingDataViewModel> filteredModel = new List<RoomBookingDataViewModel>();
            foreach(RoomBookingDataViewModel m in model)
            {
                m.ChineseName = addMaskToName(m.ChineseName, 1);
                m.GuestName = addMaskToName(m.GuestName, 3);
                filteredModel.Add(m);
            }
            return filteredModel;
        }

        private string addMaskToName(string name, int len)
        {
            if(!string.IsNullOrWhiteSpace(name) && name.Length > 1)
            {
                int st = Math.Min(2, name.Length - len - 1);
                st = Math.Max(st, 0);
                string k = name.Substring(st, len);
                return name.Replace(k, "*");
            }
            return name;
        }

    }

}
