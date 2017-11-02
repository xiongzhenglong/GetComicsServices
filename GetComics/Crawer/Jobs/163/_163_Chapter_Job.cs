using ApiModel._163;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public  class _163_Chapter_Job:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(_163_Chapter_Job));
        MsSqlContext dbcontext;
        private const string chapterurl = "/book/catalog/{0}.json";
        static HttpHelper _helper = new HttpHelper("https://manhua.163.com");
        public _163_Chapter_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");
            IQuery<Comic> q = dbcontext.Query<Comic>();
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Comic> comiclst = q.Where(a => a.source == Source._163 && a.shortdate == shortdate).Take(200).ToList();
            List<int> ids = comiclst.Select(x => x.Id).ToList();
            dbcontext.Update<Comic>(a => ids.Contains(a.Id), a => new Comic()
            {
                shortdate = yesterday,
                modify = dt
            });
            List<Chapter> chapterlst = new List<Chapter>();

            foreach (var comic in comiclst)
            {
                List<Chapter> cplst = cpq.Where(a => a.comicid == comic.comicid && a.source == Source._163).ToList();
                if (cplst.Count == 0)
                {
                    try
                    {

                        int sort = 0;

                        var bookdata = _helper.Get<_163_Chapter_Api>(null, string.Format(chapterurl, comic.comicid.Replace("6_","")));
                        if (bookdata.code==200)
                        {
                            bookdata.catalog.sections.ForEach(x =>
                            {
                               
                                foreach (var item in x.sections)
                                {
                                    sort = sort + 1;
                                    chapterlst.Add(new Chapter()
                                    {
                                        chapterid = comic.comicid+"_"+item.sectionId,
                                        chaptername = item.fullTitle,
                                        chapterurl = $"https://manhua.163.com/reader/{item.bookId}/{item.sectionId}#imgIndex=0",
                                        sort = sort,
                                        comicid = comic.comicid,
                                        retry = 0,
                                        source = comic.source,
                                        downstatus = DownChapter.待处理链接,
                                        isvip = item.needPay==0 ?"0":"1",
                                        chaptersource = "",
                                        chapterlocal = "",
                                        modify = dt,
                                        shortdate = shortdate,
                                    });
                                }
                            });
                        }
                       

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        comic.shortdate = shortdate;
                        comic.modify = dt;
                        dbcontext.Update(comic);
                        Err_ChapterJob err = new Err_ChapterJob();
                        err.bookurl = comic.bookurl;
                        err.source = comic.source;
                        err.errtype = ErrChapter.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
            }

            if (chapterlst.Count > 0)
            {
                dbcontext.BulkInsert(chapterlst);
            }
        }
    }
}
