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
    /// 爬取章节列表
    /// </summary>
    [DisallowConcurrentExecution]
    public class dongman_Chapter_Job: IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(dongman_Chapter_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("https://www.dongmanmanhua.cn");
        public dongman_Chapter_Job()
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
            List<Comic> comiclst = q.Where(a => a.source == Source.dongmanmanhua && a.shortdate == shortdate).Take(200).ToList();
            List<int> ids = comiclst.Select(x => x.Id).ToList();
            dbcontext.Update<Comic>(a => ids.Contains(a.Id), a => new Comic()
            {
                shortdate = yesterday,
                modify = dt
            });
            List<Chapter> chapterlst = new List<Chapter>();
            foreach (var comic in comiclst)
            {
                List<Chapter> cplst = cpq.Where(a => a.comicid == comic.Id && a.source == Source.dongmanmanhua).ToList();
                if (cplst.Count == 0)
                {
                    try
                    {
                        string bookurl = comic.bookurl.Replace("https://www.dongmanmanhua.cn/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        string pattern = "<li id=\"episode_(?<key1>.*?)\" data-episode-no=\"(?<key2>.*?)\">(?<key3>.*?)</li>";
                        MatchCollection matches = Regex.Matches(bookdata, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                       
                        int pagecount = (int)Math.Ceiling(int.Parse(matches[0].Groups[1].Value)/10.0);
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var lihtml  = matches[i].Groups["key3"].Value;
                            Regex reg1 = new Regex("href=\"(?<key1>.*?)\"");
                            Match match1 = reg1.Match(lihtml);
                            string chapterurl = match1.Groups["key1"].Value;
                            Regex reg2 = new Regex("<span class=\"tx\">#(?<key1>.*?)</span>");
                            Match match2 = reg2.Match(lihtml);
                            int sort =int.Parse(match2.Groups["key1"].Value);
                            Regex reg3 = new Regex("<span class=\"subj\"><span>(?<key1>.*?)</span></span>");
                            Match match3 = reg3.Match(lihtml);
                            string chaptername = match3.Groups["key1"].Value;

                            Regex reg4 = new Regex("src=\"(?<key1>.*?)\"");
                            Match match4 = reg4.Match(lihtml);
                            string chaptersource = match4.Groups["key1"].Value;

                            chapterlst.Add(new Chapter()
                            {
                                chapterid = comic.comicid + "_" + sort,
                                chaptername = chaptername,
                                chapterurl = "https:" + chapterurl,
                                sort = sort,

                                comicid = comic.Id,
                                retry = 0,
                                source = comic.source,
                                downstatus = DownChapter.待处理链接,
                                isvip = "0",
                                chaptersource = chaptersource,
                                chapterlocal = "",
                                modify = dt,
                                shortdate = shortdate,
                            });
                        }
                      
                        for (int i = 2; i <= pagecount ; i++)
                        {
                            var bookdata2 = _helper.Get(null, bookurl+"&page="+i);
                            string pattern2 = "<li id=\"episode_(?<key1>.*?)\" data-episode-no=\"(?<key2>.*?)\">(?<key3>.*?)</li>";
                            MatchCollection matches2 = Regex.Matches(bookdata2, pattern2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            
                            for (int j = 0; j < matches2.Count; j++)
                            {
                                var lihtml = matches2[j].Groups["key3"].Value;
                                Regex reg1 = new Regex("href=\"(?<key1>.*?)\"");
                                Match match1 = reg1.Match(lihtml);
                                string chapterurl = match1.Groups["key1"].Value;
                                Regex reg2 = new Regex("<span class=\"tx\">#(?<key1>.*?)</span>");
                                Match match2 = reg2.Match(lihtml);
                                int sort = int.Parse(match2.Groups["key1"].Value);
                                Regex reg3 = new Regex("<span class=\"subj\"><span>(?<key1>.*?)</span></span>");
                                Match match3 = reg3.Match(lihtml);
                                string chaptername = match3.Groups["key1"].Value;

                                Regex reg4 = new Regex("src=\"(?<key1>.*?)\"");
                                Match match4 = reg4.Match(lihtml);
                                string chaptersource = match4.Groups["key1"].Value;

                                chapterlst.Add(new Chapter()
                                {
                                    chapterid = comic.comicid + "_" + sort,
                                    chaptername = chaptername,
                                    chapterurl = "https:" + chapterurl,
                                    sort = sort,

                                    comicid = comic.Id,
                                    retry = 0,
                                    source = comic.source,
                                    downstatus = DownChapter.待处理链接,
                                    isvip = "0",
                                    chaptersource = chaptersource,
                                    chapterlocal = "",
                                    modify = dt,
                                    shortdate = shortdate,
                                });
                            }
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

            if (chapterlst.Count>0)
            {
                dbcontext.BulkInsert(chapterlst);
            }
        }

    }
}
