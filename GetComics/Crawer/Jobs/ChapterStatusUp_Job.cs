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
        private static int pageIndex = 1;
        private static int pageSize = 500;
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
            List<Chapter> cqlst = cq.Where(x =>x.source==Source.QQ &&  x.downstatus == DownChapter.处理完链接).OrderBy(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            if (cqlst.Count < pageSize)
                pageIndex = 1;
            else
                pageIndex++;
            List<string> cidlst = cqlst.Select(x => x.chapterid).ToList();
            List<Page> pagelst = cpq.Where(x => x.source == Source.QQ && cidlst.Contains(x.chapterid)).ToList();
            List<int> waitidlst = new List<int>();
            foreach (var c in cqlst)
            {             
                List<Page> pagenulllst = pagelst.Where(x =>x.chapterid==c.chapterid &&  x.pagelocal == "").ToList();
                if (pagenulllst.Count==0)
                {
                    waitidlst.Add(c.Id);
                }
                //c.downstatus = pagenulllst.Count > 0 ? DownChapter.处理完链接 : DownChapter.上传完图片;
                //c.modify = dt;
                //dbcontext.Update(c);
            }
            dbcontext.Update<Chapter>(a => waitidlst.Contains(a.Id), a => new Chapter()
            {
                downstatus = DownChapter.上传完图片,
                modify = dt
            });
        }
    }
}
