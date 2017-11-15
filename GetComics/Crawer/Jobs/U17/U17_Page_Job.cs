using ApiModel.U17;
using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public class U17_Page_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(U17_Page_Job));
        private MsSqlContext dbcontext;

        private const string pageurl = "/comic/ajax.php?mod=chapter&act=get_chapter_v5&chapter_id={0}";
        private static HttpHelper _helper = new HttpHelper("http://www.u17.com");

        public U17_Page_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Chapter> cplst = cpq.Where(x => x.source == Source.U17 && x.downstatus == DownChapter.待处理链接 && x.isvip == "0").Take(200).ToList();


            List<int> ids = cplst.Select(x => x.Id).ToList();
            dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
            {
                downstatus = DownChapter.处理完链接,
                modify = dt
            });
           
            foreach (var cp in cplst)
            {
                try
                {
                    var imgdata = _helper.Get<U17_Page_Api>(null, string.Format(pageurl, cp.chapterid.Replace(cp.comicid+"_","")));

                    List<Page> pglst = new List<Page>();
                    int sort = 0;
                    imgdata.image_list.ForEach(x =>
                    {
                        sort = sort + 1;
                        pglst.Add(new Page()
                        {
                            pagesource = x.src,
                            chapterid = cp.chapterid,
                            modify = dt,
                            shortdate = shortdate,
                            sort = sort,
                            source = cp.source,
                            pagelocal = "",
                        });
                    });
                    if (pglst.Count>0)
                    {
                        dbcontext.BulkInsert(pglst);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    cp.downstatus = DownChapter.待处理链接;
                    cp.modify = dt;
                    dbcontext.Update(cp);
                    Err_ChapterJob err = new Err_ChapterJob();
                    err.bookurl = cp.chapterurl;
                    err.source = cp.source;
                    err.errtype = ErrChapter.解析出错;
                    err.modify = dt;
                    err.shortdate = shortdate;
                    err.message = ex.Message;
                    err = dbcontext.Insert(err);
                    continue;
                }
            }
        }
    }
}