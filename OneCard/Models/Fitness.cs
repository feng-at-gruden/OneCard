//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace OneCard.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Fitness
    {
        public int id { get; set; }
        public Nullable<int> StationID { get; set; }
        public Nullable<int> Room { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<System.DateTime> ChkTime { get; set; }
    }
}
