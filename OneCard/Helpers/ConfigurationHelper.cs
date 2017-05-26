using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OneCard.Models;

namespace OneCard.Helpers
{
    public class ConfigurationHelper
    {


        public static decimal GetCorrectSettingPercentValue(string settingKey)
        {
            string strRateValue = GetSystemSettingString(settingKey);
            if (strRateValue == null)
                return 0m;
            if (strRateValue.IndexOf("%") >= 0)
            {
                //Setting like 60%
                return decimal.Parse(strRateValue.Replace("%", "")) / 100;
            }
            else
            {
                return GetSystemSettingDecimal(settingKey);        //Setting like  0.6
            }
        }

        public static String GetSystemSettingString(string key)
        {
            using (onecardEntities db = new onecardEntities())
            {
                var s = db.Configuration.SingleOrDefault(m => m.Key.Equals(key));
                if (s == null)
                    return null;
                else
                    return s.Value;
            }
        }

        public static int GetSystemSettingInt(string key)
        {
            string value = GetSystemSettingString(key);
            return int.Parse(value);
        }

        public static decimal GetSystemSettingDecimal(string key)
        {
            string value = GetSystemSettingString(key);
            return decimal.Parse(value);
        }

        public static bool GetSystemSettingBoolean(string key)
        {
            string value = GetSystemSettingString(key);
            return bool.Parse(value);
        }

    }


}