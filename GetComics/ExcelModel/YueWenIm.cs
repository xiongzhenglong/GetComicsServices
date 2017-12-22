using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelModel
{
    public class YueWenIm
    {
        public string bookid { get; set; }
        public string bookname { get; set; }
        public string authorname { get; set; }
        public string merchantname { get; set; }
        public string channelname { get; set; }
        public string agreedid { get; set; }
        public string agreedment { get; set; }

        public YueWenIm()
        {

        }
        public YueWenIm(string bookid,string bookname,string authorname)
        {
            this.bookid = bookid;
            this.bookname = bookname;
            this.authorname = authorname;
        }
    }
}
