using CrawerEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Err_ComicJob:BaseEntity
    {
        public string bookurl { get; set; }
    

        public ErrComic errtype { get; set; }

        public string message { get; set; }

        public string shortdate { get; set; }

        public DateTime modify { get; set; }
    }
}
