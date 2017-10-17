using CrawerEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Err_ChapterJob:BaseEntity
    {
        public string bookurl { get; set; }

        public Source source { get; set; }

        public ErrChapter errtype { get; set; }

        public string message { get; set; }

        public string shortdate { get; set; }

        public DateTime modify { get; set; }
    }
}
