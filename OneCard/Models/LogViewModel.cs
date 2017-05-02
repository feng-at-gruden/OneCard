using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneCard.Models
{
    public class LogViewModel
    {

        [Display(Name = "操作")]
        public string Action { get; set; }

        [Display(Name = "操作时间")]
        public DateTime ActionTime { get; set; }

        [Display(Name = "操作人")]
        public string User { get; set; }

        [Display(Name = "IP地址")]
        public string IP { get; set; }

        [Display(Name = "浏览器")]
        public string UserClient { get; set; }

    }

}