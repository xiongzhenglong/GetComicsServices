using CrawerEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class VIPFreeComic : BaseEntity
    {
    
        /// <summary>
        /// 来源
        /// </summary>
        public Source source { get; set; }

        /// <summary>
        /// 书的链接
        /// </summary>
        public string bookurl { get; set; }

        public string shortdate { get; set; }

        public DateTime modify { get; set; }
    
        /// <summary>
        /// 书ID
        /// </summary>
        public string comicid { get; set; }
        /// <summary>
        /// 书名
        /// </summary>
        public string comicname { get; set; }

        

    }
}
