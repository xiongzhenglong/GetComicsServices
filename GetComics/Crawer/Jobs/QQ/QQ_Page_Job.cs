using ApiModel.QQ;
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
    /// <summary>
    /// 爬取章节图片
    /// </summary>
    [DisallowConcurrentExecution]
    public class QQ_Page_Job: IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Page_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://ac.qq.com");
        public QQ_Page_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");         
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Chapter> cplst = cpq.Where(x => x.source == Source.QQ && x.downstatus == DownChapter.待处理链接).Take(200).ToList();
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
                    string chapterpage = cp.chapterurl.Replace("http://ac.qq.com", "");
                    var imgdata = _helper.Get(null, chapterpage);
                    Regex rex = new Regex("var DATA        = '(?<key1>.*?)',");
                    Match match = rex.Match(imgdata);
                    string key = match.Groups["key1"].Value;
                    if (string.IsNullOrEmpty(key))
                    {
                        cp.downstatus = DownChapter.待处理链接;
                        cp.modify = dt;
                        dbcontext.Update(dt);
                        Err_ChapterJob err = new Err_ChapterJob();
                        err.bookurl = cp.chapterurl;
                        err.source = cp.source;
                        err.errtype = ErrChapter.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = "DATA解析失败";
                        err = dbcontext.Insert(err);
                        continue;
                    }
                    var s = DecodeHelper.QQPageDecode(key.Substring(1));
                    var t = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(s);
                    if (t.chapter.vipStatus ==1 )
                    {
                        List<Page> pglst = new List<Page>();
                        for (int i = 0; i < t.picture.Count; i++)
                        {
                            pglst.Add(new Page()
                            {
                                chapterid = cp.Id,
                                modify = dt,
                                shortdate = shortdate,
                                sort = i+1,
                                source = cp.source,
                                pagelocal="",
                                pagesource = t.picture[i].url
                            });
                        }
                       
                        dbcontext.Update(cp);
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
