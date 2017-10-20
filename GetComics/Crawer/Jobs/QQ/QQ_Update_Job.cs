using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
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
    /// 更新章节
    /// </summary>
    [DisallowConcurrentExecution]
    public  class QQ_Update_Job:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Update_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://ac.qq.com");
        public QQ_Update_Job()
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
            IQuery<Notice> nq = dbcontext.Query<Notice>();
            List<Comic> comiclst = q.Where(a => a.source == Source.QQ && a.recrawer==false).Take(200).ToList();
            List<int> ids = comiclst.Select(x => x.Id).ToList();
            dbcontext.Update<Comic>(a => ids.Contains(a.Id), a => new Comic()
            {
                recrawer = true,
                modify = dt
            });
           
            foreach (var comic in comiclst)
            {
                List<Chapter> cplst = cpq.Where(a => a.comicid == comic.comicid && a.source == Source.QQ && a.shortdate !=shortdate).ToList();
                List<Chapter> chapterlst = new List<Chapter>();
                if (cplst.Count >0)
                {
                    try
                    {
                        string bookurl = comic.bookurl.Replace("http://ac.qq.com/", "");
                        var bookdata = _helper.Get(null, bookurl);

                        //class=\"chapter-page-all works-(?<key1>.*?)-list\"
                        Regex reg1 = new Regex("<ol class=\"chapter-page-all works-chapter-list\">(?<key1>.*?)</ol>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match1 = reg1.Match(bookdata);
                        string htmlSource = match1.Groups["key1"].Value;

                        string pattern = "<a target=\"_blank\" title=\"(?<key1>.*?)\" href=\"(?<key2>.*?)\">(?<key3>.*?)</a>";
                        MatchCollection matches = Regex.Matches(htmlSource, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        for (int i = 0; i < matches.Count; i++)
                        {
                            chapterlst.Add(new Chapter()
                            {
                                chapterid = comic.comicid + "_" + matches[i].Groups["key2"].Value.Split('/').LastOrDefault(),
                                chaptername = matches[i].Groups["key3"].Value.Trim(),
                                chapterurl = "http://ac.qq.com" + matches[i].Groups["key2"].Value,
                                sort = i + 1,
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
                        int delete = cplst.Except(chapterlst, new Chapter_Comparer()).Count(); // 删章
                        List<Chapter> add = chapterlst.Except(cplst, new Chapter_Comparer()).ToList();  // 新增

                     
                        if (delete > 0)
                        {
                            List<string> idlst = cplst.Select(x => x.chapterid).ToList();
                            dbcontext.Delete<Page>(x => idlst.Contains(x.chapterid));
                            dbcontext.Delete<Chapter>(x => idlst.Contains(x.chapterid));
                            if (chapterlst.Count > 0)
                            {
                                dbcontext.BulkInsert(chapterlst);
                            }
                            Notice notice = new Notice();
                            notice.noticeid = comic.comicid;
                            notice.noticestatus = NoticeStatus.等待处理;
                            notice.noticetype = NoticeType.目录变更;
                            notice.source = comic.source;
                            notice.shortdate = shortdate;
                            notice.modify = dt;
                            var nqwait= nq.Where(x => x.noticeid == comic.comicid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.目录变更).FirstOrDefault();
                            if (nqwait == null)
                            {
                                dbcontext.Insert(notice);
                            }
                            comic.recrawer = false;
                            dbcontext.Update(comic);
                            continue;
                            
                        }
                        else
                        {
                       
                            List<Chapter> mvadd = chapterlst.Except(add, new Chapter_Comparer()).ToList();
                            string cplststr = string.Join(",", cplst.Select(x => x.chapterid).ToArray());
                            string chapterlststr = string.Join(",", mvadd.Select(x => x.chapterid).ToArray());
                            if (cplststr != chapterlststr) // 调序
                            {
                                List<string> idlst = cplst.Select(x => x.chapterid).ToList();
                                dbcontext.Delete<Page>(x => idlst.Contains(x.chapterid));
                                dbcontext.Delete<Chapter>(x => idlst.Contains(x.chapterid));
                                if (chapterlst.Count > 0)
                                {
                                    dbcontext.BulkInsert(chapterlst);
                                }
                                Notice notice = new Notice();
                                notice.noticeid = comic.comicid;
                                notice.noticestatus = NoticeStatus.等待处理;
                                notice.noticetype = NoticeType.目录变更;
                                notice.source = comic.source;
                                notice.shortdate = shortdate;
                                notice.modify = dt;
                                var nqwait = nq.Where(x => x.noticeid == comic.comicid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.目录变更).FirstOrDefault();
                                if (nqwait == null)
                                {
                                    dbcontext.Insert(notice);
                                }
                                comic.recrawer = false;
                                dbcontext.Update(comic);
                                continue;
                            }
                            
                        }
                        if (add.Count > 0)
                        {
                            dbcontext.BulkInsert(add);
                        }
                        comic.recrawer = false;
                        dbcontext.Update(comic);




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

            
        }

    }

    public class Chapter_Comparer: IEqualityComparer<Chapter>
    {
        public bool Equals(Chapter x,Chapter y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            return x != null && y != null && x.chapterid == y.chapterid;
           
        }

        public int GetHashCode(Chapter obj)
        {
            int hashchapterid = obj.chapterid.GetHashCode();
            return hashchapterid;
        }
    }
}
