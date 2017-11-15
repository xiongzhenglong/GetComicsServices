using ApiModel.QQ;
using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Framework.Redis;
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
    public class QQ_Page_Job_Free:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Page_Job_Free));
        MsSqlContext dbcontext;
        static HttpWebHelper _helper = new HttpWebHelper();
        public QQ_Page_Job_Free()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //string keystr = "meyJjb21pYyI6eyJpZCI6NTM4NzQzLCJ0aXRsZSI6Ilx1NGUwN1x1OTFjY1x1NjY3NFx1NWRkZCIsImNvbGxlY3QiOiI0NzA0MSIsImlzSmFwYW5Db21pYyI6ZmFsc2UsImlzTGlnaHROb3ZlbCI6ZmFsc2UsImlzTGlnaHRDb21pYyI6ZmFsc2UsImlzRmluaXNoIjpmYWxzZSwiaXNSb2FzdGFibGUiOnRydWUsImVJZCI6IktsQlBTVU5BVmxWV0Jnc2ZBUVlPQUF3S0hFUldOQT09In0sImNoYXB0ZXIiOnsiY2lkIjo0OSwiY1RpdGxlIjoiXHU3YjJjMjVcdTU2ZGUgXHU2NDk1XHU4OGMyMlx1ZmYwOFx1NGUwYVx1ZmYwOSIsImNTZXEiOiI0OSIsInZpcFN0YXR1cyI6MiwicHJldkNpZCI6NDgsIm5leHRDaWQiOjUwLCJibGFua0ZpcnN0IjoxLCJjYW5SZWFkIjp0cnVlfSwicGljdHVyZSI6W3sicGlkIjoiMjY1MCIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2NTgmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF8wY2E3NjllMDk1YTU0NTYwYzQ5Nzk3ZmEwNGY1NzQ5N18yNjUwLmpwZyJ9LHsicGlkIjoiMjY1MSIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2NzImZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF8yZmVmYmQ0Mzk3ODY5YmU5MGI4OTQxOTdmYTEwMGZmNF8yNjUxLmpwZyJ9LHsicGlkIjoiMjY1MiIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2NzImZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF9jODBiMTE0ZDIwOGIzZWMyZDkxNWM3MmRhODIwZDRhYl8yNjUyLmpwZyJ9LHsicGlkIjoiMjY1MyIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2NzImZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF9kYjI5MjI5MmM4NmVkN2U0MDIwN2Q2MDg4YjU2ODliYl8yNjUzLmpwZyJ9LHsicGlkIjoiMjY1NCIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2NzImZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF81YjEyOWVmMWJjY2NlZDQ3OWU1NDE3NTliMTIzZWJkNF8yNjU0LmpwZyJ9LHsicGlkIjoiMjY1NSIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2NzImZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF85MGY2MTg2MjMyNzcxYTE5NzVmOTVlM2RiYTQ3ZGIxZV8yNjU1LmpwZyJ9LHsicGlkIjoiMjY1NiIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODYmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF9mZGVmMjExZmIxZjRlYWIxNGQzNDE1MjM1ZDNjOWIxNV8yNjU2LmpwZyJ9LHsicGlkIjoiMjY1NyIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODYmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF9lNmQxZjAwNWQ1OTkxNzRkMjJlODI1MDE3MGVhMTM4N18yNjU3LmpwZyJ9LHsicGlkIjoiMjY1OCIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODYmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF81NmI3YjI1YjVkZDM5YzdhNmU1OGNhYjY1Njc1MGY3MV8yNjU4LmpwZyJ9LHsicGlkIjoiMjY1OSIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODYmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF9jODEyNGJiYzk2NTVlODVhMTk0ZTRiNDQwMzA2YTFjYV8yNjU5LmpwZyJ9LHsicGlkIjoiMjY2MCIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODYmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF85ZDE0ZDYzOTY1Mjg0ZDUzY2MwMDkwN2QxZGIzY2YzNV8yNjYwLmpwZyJ9LHsicGlkIjoiMjY2MSIsIndpZHRoIjo4MDAsImhlaWdodCI6MTIwMCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODYmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF9lZmU5OThmYjU1ZDJlZjYyMGFlMmIzYmM3ZjYyY2IzOV8yNjYxLmpwZyJ9LHsicGlkIjoiMjY2MiIsIndpZHRoIjo4NjAsImhlaWdodCI6MTQ1MywidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MTA3MDk2ODcmZGlyX3BhdGg9XC8mbmFtZT0xNV8wOV8zNF84NmYzY2QzMzgzYjA2N2JhOTRjYWQ4YjZlOThkMGY2NV8yNjYyLmpwZyJ9XSwiYWRzIjp7InRvcCI6eyJ0aXRsZSI6Ilx1NjVmNlx1N2E3YVx1NjA0Ylx1NGViYSIsInBpYyI6Imh0dHBzOlwvXC9tYW5odWEucXBpYy5jblwvb3BlcmF0aW9uXC8wXC8xM18wOV81NF9kOTU4YWYyOGU4NmYzNjhmYjhiN2EwMGMzMDZmODQ4OV8xNTEwNTM4MDQxNTMwLmpwZ1wvMCIsInVybCI6Imh0dHA6XC9cL2FjLnFxLmNvbVwvQ29taWNWaWV3XC9pbmRleFwvaWRcLzU1MDkyMVwvY2lkXC80Iiwid2lkdGgiOiI2NTAiLCJoZWlnaHQiOiIxMTAifSwibGVmdCI6W10sImJvdHRvbSI6eyJ0aXRsZSI6Ilx1NzNiMFx1NGUxNlx1NjBjNVx1NGViYVx1NjYyZlx1NWMzZVx1NzJkMCIsInBpYyI6Imh0dHBzOlwvXC9tYW5odWEucXBpYy5jblwvb3BlcmF0aW9uXC8wXC8xM18wOV81Ml84ZWJjZTJkZmJhMzZhZDhlYTA2YjEyODRkNzY1Mzc0Nl8xNTEwNTM3OTM3NDY2LmpwZ1wvMCIsInVybCI6Imh0dHA6XC9cL2FjLnFxLmNvbVwvQ29taWNWaWV3XC9pbmRleFwvaWRcLzYyNjg1MFwvY2lkXC8yIiwid2lkdGgiOiI2NTAiLCJoZWlnaHQiOiIxMTAifX0sImFydGlzdCI6eyJhdmF0YXIiOiJodHRwOlwvXC9xNC5xbG9nby5jblwvZz9iPXFxJms9d2g1c0ZOazAzUDV6RmlhOGljdGQ4ZW13JnM9NjQwJnQ9MTQ4MzMzOTY3NSIsIm5pY2siOiJcdTc3ZTVcdTk3ZjNcdTc5ZmJcdTUyYThcdTY1NzBcdTViNTciLCJ1aW5DcnlwdCI6IlVUSk1kVkpCZDAxTlFrSkVibGhzWmpOWlJqRjBaejA5In19";
            //var ss = DecodeHelper.QQPageDecode(keystr.Substring(1));
            //var tt = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(ss);
            DateTime dt = DateTime.Now;
            string ticks = dt.Ticks.ToString();
            string shortdate = dt.ToString("yyyy-MM-dd");
            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();//x.comicid == "1_622585"
            IQuery<VIPFreeComic> vfcq = dbcontext.Query<VIPFreeComic>();
            List<Chapter> cplst = cpq.Where(x => x.source == Source.QQ && x.downstatus == DownChapter.待处理链接 && x.isvip == "0").Take(200).ToList();

            var redis = new RedisProxy();

            var login_key = redis.Get<QQ_Login_Key>("QQ_1434299101");
            List<int> ids = cplst.Select(x => x.Id).ToList();
            dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
            {
                ticks = ticks,
                downstatus = DownChapter.处理完链接,
                modify = dt
            });
            List<Chapter> chapterlst = new List<Chapter>();

            foreach (var cp in cplst)
            {
               
                try
                {

                    Dictionary<string, string> headers = new Dictionary<string, string>()
                    {
                        {"Cookie","uin=o1434299101; skey="+login_key.skey+";" }
                    };


                    var imgdata = _helper.Get(cp.chapterurl, Encoding.GetEncoding("UTF-8"),  null, "", null, "", headers, "application/x-www-form-urlencoded; charset=UTF-8");
                    
                    Regex rex = new Regex("var DATA        = '(?<key1>.*?)',");
                    Match match = rex.Match(imgdata);
                    string key = match.Groups["key1"].Value;
                    if (string.IsNullOrEmpty(key))
                    {
                        cp.downstatus = DownChapter.待处理链接;
                        cp.modify = dt;
                        cp.ticks = ticks;
                        dbcontext.Update(cp);
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
                    if (t.chapter.vipStatus == 1)
                    {
                        List<Page> pglst = new List<Page>();
                        for (int i = 0; i < t.picture.Count; i++)
                        {
                            pglst.Add(new Page()
                            {
                                chapterid = cp.chapterid,
                                modify = dt,
                                shortdate = shortdate,
                                sort = i + 1,
                                source = cp.source,
                                pagelocal = "",   
                                ticks = ticks,                             
                                pagesource = t.picture[i].url
                            });
                        }
                    
                        cp.isvip = "0";
                        cp.ticks = ticks;
                        cp.downstatus = DownChapter.处理完链接;
                        cp.modify = dt;
                        dbcontext.Update(cp);
                        dbcontext.BulkInsert(pglst);
                    }
                    else
                    {
                        List<VIPFreeComic> vfclst = vfcq.Where(x =>x.source==Source.QQ &&  x.comicid == cp.comicid).ToList();
                        
                        cp.isvip =vfclst.Count==0? "1":"0";
                        cp.ticks = ticks;
                        cp.downstatus = DownChapter.待处理链接;
                        cp.modify = dt;
                        dbcontext.Update(cp);
                    }
                }
                catch (Exception ex)
                {
                    
                    cp.downstatus = DownChapter.待处理链接;
                    cp.modify = dt;
                    cp.ticks = ticks;
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
