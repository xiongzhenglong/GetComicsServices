using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class YueWen : BaseEntity
    {
        
        public string authorname { get; set; }
        public string bookid { get; set; }
        public string bookname { get; set; }
        public string merchantname { get; set; }
        public string channelname { get; set; }
        public string agreedid { get; set; }
        public string agreedment { get; set; }
        public string status { get; set; }
        public string maxfreecount { get; set; }

        public string bid { get; set; }

       
    }
}
