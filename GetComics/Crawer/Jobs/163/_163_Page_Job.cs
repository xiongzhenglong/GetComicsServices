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
    public class _163_Page_Job:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(_163_Page_Job));
        MsSqlContext dbcontext;
     
        static HttpHelper _helper = new HttpHelper("https://manhua.163.com");
        public _163_Page_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Chapter> cplst = cpq.Where(x => x.source == Source._163 && x.downstatus == DownChapter.待处理链接 && x.isvip=="0").Take(200).ToList();

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

                    string chapterpage = cp.chapterurl.Replace("https://manhua.163.com/", "");
                    var imgdata = _helper.Get(null, chapterpage);
                    Regex rex1 = new Regex("imageId:");
                    var match1 = rex1.Match(imgdata);

                    if (match1.Value.Trim()!="")
                    {
                        Regex rex2 = new Regex("url: window.IS_SUPPORT_WEBP (?<key1>.*?),");
                        var match2 = rex2.Matches(imgdata);
                        List<Page> pglst = new List<Page>();
                        int sort = 0;
                        foreach (var item in match2)
                        {
                            sort = sort + 1;
                            var tt = ((Match)item).Groups["key1"].Value;
                            var imgurl = tt.Split('"')[3].Substring(0, tt.Split('"')[3].IndexOf("%3D") + 3);
                            pglst.Add(new Page()
                            {
                                pagesource = imgurl,
                                chapterid = cp.chapterid,
                                modify = dt,
                                shortdate = shortdate,
                                sort = sort,
                                source = cp.source,
                                pagelocal = "",
                            });

                           

                        }
                        if (pglst.Count>0)
                        {
                            dbcontext.BulkInsert(pglst);
                        }
                       

                    }
                    else
                    {
                        cp.downstatus = DownChapter.待处理链接;
                        cp.modify = dt;
                        dbcontext.Update(cp);
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
