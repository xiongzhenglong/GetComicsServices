using CrawerEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class MailMessage:BaseEntity
    {
        public Source source { set; get; }
        public string payurl { set; get; }

    

        public string chapterid { set; get; }

        public decimal chaptermoney { set; get; }

        public string shortdate { get; set; }

        public DateTime modify { get; set; }
    }
}
