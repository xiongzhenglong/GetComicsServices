using CrawerEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Comic:BaseEntity
    {
        #region 辅助
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
        #endregion

        #region 基础字段
        /// <summary>
        /// 书ID
        /// </summary>
        public string comicid { get; set; }
        /// <summary>
        /// 书名
        /// </summary>
        public string comicname { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public string comiccoversource { get; set; }
        public string comiccoverlocal { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string authorname { get; set; }

        /// <summary>
        /// 是否VIP 0 免费 1 收费
        /// </summary>
        public string isvip { get; set; }


        /// <summary>
        /// 分类 多个分类用 , 分隔
        /// </summary>
        public string theme { get; set; }

        /// <summary>
        /// 连载中 已完结
        /// </summary>
        public string isfinished { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string comicdesc { get; set; }
        #endregion

        #region 状态字段 影响服务
        /// <summary>
        /// 是否下架
        /// </summary>
        public bool isoffline { get; set; }

        /// <summary>
        /// 是否重抓
        /// </summary>
        public bool recrawer { get; set; }

        /// <summary>
        /// 是否停抓
        /// </summary>
        public bool stopcrawer { get; set; }
        #endregion

    }
}
