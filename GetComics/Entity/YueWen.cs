using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class YueWen : BaseEntity
    {
        public string bookid { get; set; }
        public string bookname { get; set; }
        public string authorname { get; set; }
        public string channelname { get; set; }
        public string hetongid { get; set; }
        public string hetongname { get; set; }
        public string remark { get; set; }

        /// <summary>
        /// 0 未完成 1 完成
        /// </summary>
        public string status { get; set; }
    }
}
