﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text;
using System.IO;

namespace OneCard.Helpers
{
    public class CSVHelper
    {


        public static MemoryStream ExportCSV(IEnumerable<object> data, string[] columns)
        {
            System.IO.MemoryStream output = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(output, System.Text.Encoding.UTF8);

            var d = data.First();
            Type t = d.GetType();
            PropertyInfo[] pis = t.GetProperties();

            string header = "";
            for (int i = 0; i < pis.Length; i++ )
            {
                PropertyInfo pi = pis[i];
                string name = pi.Name;
                if (i != pis.Length - 1)
                    header += name + ",";
                else
                    header += name + "\r\n";
            }

            string header2 = "";
            for (int i = 0; i < columns.Length; i++)
            {
                if (i != columns.Length - 1)
                    header2 += columns[i] + ",";
                else
                    header2 += columns[i] + "\r\n";
            }
            StringBuilder sb = new StringBuilder();

            sb.Append(header2);
            foreach(var item in data)
            {
                for (int i = 0; i < pis.Length; i++)
                {
                    PropertyInfo pi = pis[i];
                    object v = pi.GetValue(item, null);
                    string v1 = v==null? "" : v.ToString().Replace(","," ");
                    if (i != pis.Length - 1)
                        sb.Append(v1 + ",");
                    else
                        sb.Append(v1 + "\r\n");
                }
            }

            var content = sb.ToString();
            writer.Write(sb.ToString());
            writer.Flush();
            output.Position = 0;
            sb = null;

            return output;
        }

    }
}