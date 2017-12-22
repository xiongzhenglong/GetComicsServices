using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Lib;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    /// <summary>
    /// 更新章节目录
    /// </summary>
    [DisallowConcurrentExecution]
    public class dongman_Update_Job: IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(dongman_Update_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("https://www.dongmanmanhua.cn");
        public dongman_Update_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string ticks = dt.Ticks.ToString();
            string shortdate = dt.ToString("yyyy-MM-dd");
            string updatedatetime = shortdate + " " + ((dt.Hour / 0.5 + 1) * 0.5).ToString();
            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");
            IQuery<Comic> q = dbcontext.Query<Comic>();
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            IQuery<PageHis> phisq = dbcontext.Query<PageHis>();
            IQuery<Page> pq = dbcontext.Query<Page>();
            IQuery<Notice> nq = dbcontext.Query<Notice>();
            List<Comic> comiclst = q.Where(a => a.source == Source.dongmanmanhua && (a.updatedatetime == null || a.updatedatetime != updatedatetime)).Take(200).ToList();
            List<int> ids = comiclst.Select(x => x.Id).ToList();
            dbcontext.Update<Comic>(a => ids.Contains(a.Id), a => new Comic()
            {

                updatedatetime = updatedatetime,
                modify = dt
            });

            foreach (var comic in comiclst)
            {
                List<Chapter> cplst = cpq.Where(a => a.comicid == comic.comicid && a.source == Source.dongmanmanhua).ToList();
                List<Chapter> chapterlst = new List<Chapter>();
                if (cplst.Count > 0)
                {
                    try
                    {
                        string bookurl = comic.bookurl.Replace("https://www.dongmanmanhua.cn/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        string pattern = "<li id=\"episode_(?<key1>.*?)\" data-episode-no=\"(?<key2>.*?)\">(?<key3>.*?)</li>";
                        MatchCollection matches = Regex.Matches(bookdata, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        int pagecount = (int)Math.Ceiling(int.Parse(matches[0].Groups[1].Value) / 10.0);
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var lihtml = matches[i].Groups["key3"].Value;
                            Regex reg1 = new Regex("href=\"(?<key1>.*?)\"");
                            Match match1 = reg1.Match(lihtml);
                            string chapterurl = match1.Groups["key1"].Value;
                            Regex reg2 = new Regex("<span class=\"tx\">#(?<key1>.*?)</span>");
                            Match match2 = reg2.Match(lihtml);
                            int sort = int.Parse(match2.Groups["key1"].Value);
                            //Regex reg3 = new Regex("<span class=\"subj\"><span>(?<key1>.*?)</span></span>");
                            //Match match3 = reg3.Match(lihtml);
                            //string chaptername = match3.Groups["key1"].Value;
                            Regex reg3 = new Regex("alt=\"(?<key1>.*?)\"");
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

                                comicid = comic.comicid,
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

                        for (int i = 2; i <= pagecount; i++)
                        {
                            var bookdata2 = _helper.Get(null, bookurl + "&page=" + i);
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
                                //Regex reg3 = new Regex("<span class=\"subj\"><span>(?<key1>.*?)</span></span>");
                                //Match match3 = reg3.Match(lihtml);
                                //string chaptername = match3.Groups["key1"].Value;
                                Regex reg3 = new Regex("alt=\"(?<key1>.*?)\"");
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

                                    comicid = comic.comicid,
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
                        bool contextChange = false;

                        if (chapterlst.Count>0)
                        {
                            chapterlst = chapterlst.OrderBy(x => x.sort).ToList();
                            int cmpCount = cplst.Count >= chapterlst.Count ? chapterlst.Count : cplst.Count;
                            for (int i = 0; i < cmpCount; i++)
                            {
                                if (cplst[i].chapterid != chapterlst[i].chapterid)
                                {
                                    contextChange = true;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }

                        if (contextChange)
                        {
                            dbcontext.Delete<Chapter>(x=>x.comicid == comic.comicid);
                            if (chapterlst.Count>0)
                            {
                                dbcontext.BulkInsert(chapterlst);
                                Notice notice = new Notice();
                                notice.noticeid = comic.comicid;
                                notice.noticestatus = NoticeStatus.等待处理;
                                notice.noticetype = NoticeType.目录变更;
                                notice.source = comic.source;
                                notice.shortdate = shortdate;
                                notice.modify = dt;
                                //var nqwait = nq.Where(x => x.noticeid == comic.comicid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.目录变更).FirstOrDefault();
                                //if (nqwait == null)
                                //{
                                    dbcontext.Insert(notice);
                                //}
                            }
                        }
                        else
                        {
                            

                            if (cplst.Count == chapterlst.Count)
                            {
                                continue;
                            }

                            if (cplst.Count > chapterlst.Count)
                            {
                                List<Chapter> needrmvlst = cplst.Except(chapterlst, new Chapter_Comparer()).ToList();
                                List<int> rmvidlst = needrmvlst.Select(x => x.Id).ToList();
                                logger.Error(string.Join(",",needrmvlst.Select(x => x.chapterid).ToArray()));
                                dbcontext.Delete<Chapter>(x => rmvidlst.Contains(x.Id));
                            }

                            if (cplst.Count < chapterlst.Count)
                            {
                                List<Chapter> needaddlst = chapterlst.Except(cplst, new Chapter_Comparer()).ToList();
                                dbcontext.BulkInsert(needaddlst);
                            }
                        }

                        //int delete = cplst.Except(chapterlst, new Chapter_Comparer()).Count(); // 删章
                        //List<Chapter> add = chapterlst.Except(cplst, new Chapter_Comparer()).ToList();  // 新增

                        //Chapter maxChapter = cplst.OrderBy(x => x.sort).Max();
                       

                        //if (delete > 0)
                        //{
                        //    List<string> idlst = cplst.Select(x => x.chapterid).ToList();

                        //    dbcontext.Delete<Page>(x => idlst.Contains(x.chapterid));
                        //    dbcontext.Delete<Chapter>(x => idlst.Contains(x.chapterid));
                        //    if (chapterlst.Count > 0)
                        //    {
                        //        dbcontext.BulkInsert(chapterlst);
                        //    }
                        //    Notice notice = new Notice();
                        //    notice.noticeid = comic.comicid;
                        //    notice.noticestatus = NoticeStatus.等待处理;
                        //    notice.noticetype = NoticeType.目录变更;
                        //    notice.source = comic.source;
                        //    notice.shortdate = shortdate;
                        //    notice.modify = dt;
                        //    var nqwait = nq.Where(x => x.noticeid == comic.comicid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.目录变更).FirstOrDefault();
                        //    if (nqwait == null)
                        //    {
                        //        dbcontext.Insert(notice);
                        //    }

                        //    continue;
                        //}
                        //else
                        //{
                        //    List<Chapter> mvadd = chapterlst.Except(add, new Chapter_Comparer()).ToList();
                        //    string cplststr = string.Join(",", cplst.Select(x => x.chapterid).ToArray());
                        //    string chapterlststr = string.Join(",", mvadd.Select(x => x.chapterid).ToArray());
                        //    if (cplststr != chapterlststr) // 调序
                        //    {
                        //        List<string> idlst = cplst.Select(x => x.chapterid).ToList();

                        //        dbcontext.Delete<Page>(x => idlst.Contains(x.chapterid));
                        //        dbcontext.Delete<Chapter>(x => idlst.Contains(x.chapterid));
                        //        if (chapterlst.Count > 0)
                        //        {
                        //            dbcontext.BulkInsert(chapterlst);
                        //        }
                        //        Notice notice = new Notice();
                        //        notice.noticeid = comic.comicid;
                        //        notice.noticestatus = NoticeStatus.等待处理;
                        //        notice.noticetype = NoticeType.目录变更;
                        //        notice.source = comic.source;
                        //        notice.shortdate = shortdate;
                        //        notice.modify = dt;
                        //        var nqwait = nq.Where(x => x.noticeid == comic.comicid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.目录变更).FirstOrDefault();
                        //        if (nqwait == null)
                        //        {
                        //            dbcontext.Insert(notice);
                        //        }

                        //        continue;
                        //    }
                        //}
                        //if (add.Count > 0)
                        //{
                        //    //int addsort = cplst.Count();
                        //    //foreach (var item in add)
                        //    //{
                        //    //    item.sort = item.sort + addsort;
                        //    //}
                        //    dbcontext.BulkInsert(add);
                        //}
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        comic.updatedatetime = "";
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
        }
    }
}
