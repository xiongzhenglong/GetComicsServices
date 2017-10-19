using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawerEnum;

namespace Entity
{
    public class Notice : BaseEntity
    {
        public Source source { get; set; }
        public NoticeStatus noticestatus { get; set; }
        public NoticeType noticetype { get; set; }
        public string noticeid { get; set; }
        public string shortdate { get; set; }
        public DateTime modify { get; set; }
    }
}
