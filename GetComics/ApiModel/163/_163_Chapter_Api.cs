using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModel._163
{


    public class _163_Chapter_Api
    {
        public _163_Catalog catalog { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
    }

    public class _163_Catalog
    {
        
        public List<_163_Section> sections { get; set; }
        public string bookId { get; set; }
    }

    public class _163_Section
    {
        public string bookId { get; set; }
        public string sectionId { get; set; }      
        public string fullTitle { get; set; }
        public int needPay { get; set; }   
        public bool paied { get; set; } 
        public List<_163_Section> sections { get; set; }
        public bool _new { get; set; }
    }

   


}
