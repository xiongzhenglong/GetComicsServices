using CrawerEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Page:BaseEntity
    {
        #region 辅助
        /// <summary>
        /// 来源
        /// </summary>
        public Source source { get; set; }       

        public string shortdate { get; set; }

        public DateTime modify { get; set; }
        #endregion

        #region 基本字段
        public string chapterid { get; set; }

        public int sort { get; set; }
        /// <summary>
        /// 源图片
        /// </summary>
        public string pagesource { get; set; }
        /// <summary>
        /// 本地图片
        /// </summary>
        public string pagelocal { get; set; }
        #endregion
    }
}
