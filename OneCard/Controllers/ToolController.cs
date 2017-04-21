using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Data;

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
        public ActionResult Import(String em)
        {
            HttpRequestBase request = this.Request;
            if (request.Files.Count !=1 )
            {
                ModelState.AddModelError("", "请选择正确的数据文件");
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
                var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/" + UploadFolder), fileSavedName);
                request.Files[0].SaveAs(path);
                //TODO,
            }
            return View();
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


        private static DataTable GetdataFromCVS(string fileload)
        {
            DataTable dt = new DataTable();
            StreamReader sr = new StreamReader(fileload.PostedFile.InputStream);
            string strTitle = sr.ReadLine();
            string[] strColumTitle = strTitle.Split(',');   //CVS 文件默认以逗号隔开
            for (int i = 0; i < strColumTitle.Length; i++)
            {
                dt.Columns.Add(strColumTitle[i]);
            }
            while (!sr.EndOfStream)
            {
                string strTest = sr.ReadLine();
                string[] strTestAttribute = strTest.Split(',');
                DataRow dr = dt.NewRow();
                for (int i = 0; i < strColumTitle.Length; i++)
                {
                    dr[strColumTitle[i]] = strTestAttribute[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }


    }
}
