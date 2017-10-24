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
    public class NoticeStatusUp_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(NoticeStatusUp_Job));
        MsSqlContext dbcontext;
     
        public NoticeStatusUp_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            IQuery<Comic> cq = dbcontext.Query<Comic>();
            IQuery<Notice> nq = dbcontext.Query<Notice>();
            List<Notice> nqlst = nq.Where(x =>x.noticestatus == NoticeStatus.等待处理).Take(500).ToList();
            foreach (var n in nqlst)
            {
                if (n.noticetype== NoticeType.目录变更)
                {
                    List<Chapter> cplst = cpq.Where(x => x.comicid == n.noticeid && x.downstatus != DownChapter.上传完图片).ToList();
                    if (cplst.Count==0)
                    {
                        n.noticestatus = NoticeStatus.等待发送;
                        n.modify = dt;
                        dbcontext.Update(n);
                    }
                }
                else if (n.noticetype == NoticeType.章节更新)
                {
                    Chapter chapter = cpq.Where(x => x.chapterid == n.noticeid).FirstOrDefault();
                    Notice notice = nq.Where(x => x.noticeid == chapter.comicid && x.noticetype == NoticeType.目录变更 && x.noticestatus != NoticeStatus.已发送).FirstOrDefault();
                    if (notice ==null)
                    {
                        n.noticestatus = NoticeStatus.等待发送;
                        n.modify = dt;
                        dbcontext.Update(n);
                    }
                }
               
               
            }
        }
    }
}
