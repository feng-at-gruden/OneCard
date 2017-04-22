﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Data;
using System.Globalization;

namespace OneCard.Controllers
{
    public class ToolController : BaseController
    {

        private const String UploadFolder = "upload";

        //
        // GET: /Tool

        public ActionResult Import()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(String path)
        {
            HttpRequestBase request = this.Request;
            if (request.Files.Count == 1 && string.IsNullOrWhiteSpace(request.Files[0].FileName))
            {
                ModelState.AddModelError("", "请选择正确的数据文件");
                return View();
            }
            else if(!string.IsNullOrWhiteSpace(path))
            {                
                //Move current data to history
                //Clean current table
                foreach(var d in db.ZaoCanIn24)
                {
                    db.ZaoCanIn.Add(new Models.ZaoCanIn
                    {
                        Room = d.Room,
                        ChineseName = d.ChineseName,
                        EndTime = d.EndTime,
                        StartTime = d.StartTime,
                        FullName = d.FullName,
                        InTime = d.InTime,
                        Num = d.Num,
                        Package = d.Package,
                        Pax = d.Pax,
                        Vip = d.Vip,
                    });
                    db.ZaoCanIn24.Remove(d);                    
                }
                db.SaveChanges();

                //Insert to current ZaoCanIn24 table
                var data = GetDataFromCVS(path);
                DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
　　            dtFormat.ShortDatePattern = "MM/dd/yy";                
                foreach(DataRow row in data.Rows)
                {
                    db.ZaoCanIn24.Add(new Models.ZaoCanIn24
                    {
                        Room = int.Parse(row["Room"].ToString()),
                        FullName = row["Full Name"].ToString(),
                        ChineseName = row["Chinese Name"].ToString(),
                        StartTime = Convert.ToDateTime(row["Arrive Date"].ToString(), dtFormat),
                        EndTime = Convert.ToDateTime(row["Depart Date"].ToString(), dtFormat),
                        InTime = DateTime.Now,
                        Num = int.Parse(row["Adults"].ToString()),
                        Package = row["Package"].ToString(),
                        Pax = string.IsNullOrWhiteSpace(row["Pax"].ToString()) ? 0 : int.Parse(row["Pax"].ToString()),
                        Vip = string.IsNullOrWhiteSpace(row["VIP"].ToString()) ? 0 : int.Parse(row["VIP"].ToString()),
                    }); 
                }
                db.SaveChanges();

                //Clean temp file
                FileInfo f = new FileInfo(path);
                f.Delete();

                ViewBag.SuccessMessage = "数据导入成功！";
                return View();
            }
            else
            {
                var fileName = (new FileInfo(request.Files[0].FileName)).Name;
                if (!IsValidaDataFile(fileName))
                {
                    ModelState.AddModelError("", "请选择正确的文件格式");
                    return View();
                }
                string fileSavedName = DateTime.Now.ToString("yyyyMMddHHmmss") + "" + fileName.Substring(fileName.LastIndexOf("."));
                var filePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/" + UploadFolder), fileSavedName);
                request.Files[0].SaveAs(filePath);
                var data = GetDataFromCVS(filePath);

                ViewBag.path = filePath;
                return View(data);
            }            
        }



        private bool IsValidaDataFile(string filename)
        {
            var ext = Path.GetExtension(filename);
            var imageExtensions = new string[] { "txt", "csv"};
            if (string.IsNullOrWhiteSpace(ext))
            {
                return false;
            }
            ext = ext.ToLower().Trim('.');
            foreach (var e in imageExtensions)
            {
                if (ext == e)
                    return true;
            }
            return false;
        }


        private static DataTable GetDataFromCVS(string filepath)
        {
            DataTable dt = new DataTable();
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                string strTitle = sr.ReadLine();
                string[] strColumTitle = strTitle.Split('\t');   //CVS 文件默认以逗号隔开
                
                /*
                //Read columns name from first line
                for (int i = 0; i < strColumTitle.Length; i++)
                {
                    dt.Columns.Add(strColumTitle[i]);
                }*/

                string[] columns = new string[]{"Room", "Full Name", "Chinese Name", "VIP", "Adults", "Arrive Date", "Depart Date", "Pax", "Package"};
                for (int i = 0; i < columns.Length; i++)
                {
                    dt.Columns.Add(columns[i]);
                }
                while (!sr.EndOfStream)
                {
                    string strTest = sr.ReadLine();
                    string[] strTestAttribute = strTest.Split('\t');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < columns.Length; i++)
                    {
                        dr[columns[i]] = strTestAttribute[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            
            return dt;
        }


    }
}
