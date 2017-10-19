using CrawerEnum;
using System;

namespace Entity
{
    public class Chapter:BaseEntity
    {
        #region 辅助
        /// <summary>
        /// 来源
        /// </summary>
        public Source source { get; set; }
        /// <summary>
        /// 章节url
        /// </summary>
        public string chapterurl { get; set; }

        public string shortdate { get; set; }

        public DateTime modify { get; set; }
        #endregion

        #region 基础字段
        /// <summary>
        /// 漫画id
        /// </summary>
        public string comicid { get; set; }
        /// <summary>
        /// 章节id
        /// </summary>
        public string chapterid { get; set; }
        /// <summary>
        /// 章节名
        /// </summary>
        public string chaptername { get; set; }
        /// <summary>
        /// 章节封面
        /// </summary>
        public string chaptersource { get; set; }
        public string chapterlocal { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int sort { get; set; }
        /// <summary>
        /// 是否收费 0 免费 1 收费
        /// </summary>
        public string isvip { get; set; }

        /// <summary>
        /// 图片处理进度
        /// </summary>
        public DownChapter downstatus { get; set; }

        #endregion

        #region 状态字段 影响服务
        /// <summary>
        /// 下载重试次数
        /// </summary>
        public int retry { get; set; }
        #endregion
    }
}
