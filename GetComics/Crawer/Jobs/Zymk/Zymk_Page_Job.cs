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
    public class Zymk_Page_Job:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Zymk_Page_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://www.zymk.cn");
        public Zymk_Page_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Chapter> cplst = cpq.Where(x => x.source == Source.Zymk && x.downstatus == DownChapter.待处理链接).Take(200).ToList();

            List<int> ids = cplst.Select(x => x.Id).ToList();
            dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
            {
                downstatus = DownChapter.处理完链接,
                modify = dt
            });
            List<Chapter> chapterlst = new List<Chapter>();
            foreach (var cp in cplst)
            {
                try
                {
                    
                    string chapterpage = cp.chapterurl.Replace("http://www.zymk.cn/", "");
                    var imgdata = _helper.Get(null, chapterpage);
                    Regex imgpath = new Regex("chapter_addr:\"(?<key1>.*?)\",");
                    Regex starindex = new Regex("start_var:(?<key1>.*?),");
                    Regex totalimg = new Regex("end_var:(?<key1>.*?),");
                    Regex pageid = new Regex("chapter_id:(?<key1>.*?),");
                    Regex domain = new Regex("domain:\"(?<key1>.*?)\",");
                    Regex comic_size = new Regex("middle:\"(?<key1>.*?)\"");
                    var _imgpath = imgpath.Match(imgdata).Groups["key1"].Value.Replace("\\\\", "#").Replace("\\", "").Replace("#", "\\");
                    var _totalimg = totalimg.Match(imgdata).Groups["key1"].Value;
                    var _pageid = pageid.Match(imgdata).Groups["key1"].Value;
                    var _domain = domain.Match(imgdata).Groups["key1"].Value;
                    var _comic_size = comic_size.Match(imgdata).Groups["key1"].Value;
                    int start = int.Parse(starindex.Match(imgdata).Groups["key1"].Value.Trim());
                    List<Page> pglst = new List<Page>();
                    int imgcount = int.Parse(_totalimg);
                    
                    string imgdecodepath = DecodeHelper.Decode(_imgpath, int.Parse(_pageid));
                    for (int i = 0; i < imgcount; i++)
                    {
                        string pgsource = "http://mhpic." + _domain + "/comic/" + imgdecodepath + start + ".jpg" + _comic_size;
                        pglst.Add(new Page()
                        {
                            pagesource = pgsource,
                            chapterid = cp.chapterid,
                            modify = dt,
                            shortdate = shortdate,
                            sort = i + 1,
                            source = cp.source,
                            pagelocal = "",
                        });
                        start = start + 1;
                    }
                   
                    dbcontext.BulkInsert(pglst);


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
