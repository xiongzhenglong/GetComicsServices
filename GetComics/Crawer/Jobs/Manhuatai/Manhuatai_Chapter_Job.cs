﻿using Chloe;
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
    public class Manhuatai_Chapter_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Manhuatai_Chapter_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://www.manhuatai.com");
        public Manhuatai_Chapter_Job()
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
            List<Comic> comiclst = q.Where(a => a.source == Source.Manhuatai && a.shortdate == shortdate).Take(200).ToList();
            List<int> ids = comiclst.Select(x => x.Id).ToList();
            dbcontext.Update<Comic>(a => ids.Contains(a.Id), a => new Comic()
            {
                shortdate = yesterday,
                modify = dt
            });
            List<Chapter> chapterlst = new List<Chapter>();

            foreach (var comic in comiclst)
            {
                List<Chapter> cplst = cpq.Where(a => a.comicid == comic.comicid && a.source == Source.Manhuatai).ToList();
                if (cplst.Count == 0)
                {
                    try
                    {
                        
                        string bookurl = comic.bookurl.Replace("http://www.manhuatai.com", "");
                        var bookdata = _helper.Get(null, bookurl);
                        string pattern = "<li><a title=\"(?<key1>.*?)\" href=\"(?<key2>.*?)\"><span>(?<key3>.*?)</span></a></li>";
                        MatchCollection matches = Regex.Matches(bookdata, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        for (int i = 0; i < matches.Count; i++)
                        {
                            string chapterid = comic.comicid + "_" + matches[i].Groups["key2"].Value.Replace(".html","");
                            string chaptername = matches[i].Groups["key1"].Value;
                            string chapterurl = comic.bookurl + matches[i].Groups["key2"].Value;
                            int sort = matches.Count - i;
                            chapterlst.Add(new Chapter()
                            {
                                chapterid = chapterid,
                                chaptername = chaptername,
                                chapterurl = chapterurl,
                                sort = sort,

                                comicid = comic.comicid,
                                retry = 0,
                                source = comic.source,
                                downstatus = DownChapter.待处理链接,
                                isvip = "0",
                                chaptersource = "",
                                chapterlocal = "",
                                modify = dt,
                                shortdate = shortdate,
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
