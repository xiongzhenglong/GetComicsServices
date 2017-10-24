using Chloe;
using Chloe.Infrastructure.Interception;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    /// <summary>
    /// 章节状态更新
    /// </summary>
    [DisallowConcurrentExecution]
    public class ChapterStatusUp_Job: IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ChapterStatusUp_Job));
        MsSqlContext dbcontext;
     
        public ChapterStatusUp_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Page> cpq = dbcontext.Query<Page>();
            IQuery<Chapter> cq = dbcontext.Query<Chapter>();
            List<Chapter> cqlst = cq.Where(x => x.downstatus != DownChapter.待处理链接).Take(500).ToList();
            foreach (var c in cqlst)
            {
                List<Page> pagelst = cpq.Where(x => x.chapterid == c.chapterid).ToList();
                List<Page> pagenulllst = pagelst.Where(x => x.pagelocal == "").ToList();
                c.downstatus = pagenulllst.Count > 0 ? DownChapter.处理完链接 : DownChapter.上传完图片;
                c.modify = dt;
                dbcontext.Update(c);
            }
        }
    }
}
