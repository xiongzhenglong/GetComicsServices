using ApiModel.QQ;
using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
        static IWebDriver selenium;
        static ChromeOptions chromeOptions = new ChromeOptions();
        public QQ_Page_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

            if (selenium == null)
            {
                chromeOptions.AddArguments("--headless");
                chromeOptions.AddArguments("window-size=1200x600");
                chromeOptions.AddArguments("start-maximized", "no-sandbox", "user-data-dir=C:/UserDataFolder");
                selenium = new ChromeDriver(chromeOptions);//
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            var ss = DecodeHelper.QQPageDecode("4eyJjb21pYyI6eyJpZCI6NDg1Mjg2LCJ0aXRsZSI6Ilx1OTYzZlUiLCJjb2xsZWN0IjoiNTY5NzIiLCJpc0phcGFuQ29taWMiOmZhbHNlLCJpc0xpZ2h0Tm92ZWwiOmZhbHNlLCJpc0xpZ2h0Q29taWMiOmZhbHNlLCJpc0ZpbmlzaCI6ZmFsc2UsImlzUm9hc3RhYmxlIjp0cnVlLCJlSWQiOiJLbEJQU0V0UFZsMVFBUVFmQUEwREJRQVBIRUZaV2lnPSJ9LCJjaGFwdGVyIjp7ImNpZCI6MTYzLCJjVGl0bGUiOiJcdTdiMmMyNFx1NTE4YyAwOFx1N2FlMCIsImNTZXEiOiIxNTEiLCJ2aXBTdGF0dXMiOjIsInByZXZDaWQiOjE2MiwibmV4dENpZCI6MTY0LCJibGFua0ZpcnN0IjoxLCJjYW5SZWFkIjpmYWxzZX0sInBpY3R1cmUiOlt7InBpZCI6IjY5ODkiLCJ3aWR0aCI6OTAwLCJoZWlnaHQiOjE1MjgsInVybCI6Imh0dHA6XC9cL2FjLnRjLnFxLmNvbVwvc3RvcmVfZmlsZV9kb3dubG9hZD9idWlkPTE1MDE3JnVpbj0xNTA2NDE1MjA5JmRpcl9wYXRoPVwvJm5hbWU9MjZfMTZfNDBfMmY5MzE2Zjk3NGVhOTFkMzQ5ZTlkODEzMGZhZTg0ZjVfNjk4OS5qcGcifV0sImFkcyI6eyJ0b3AiOiIiLCJsZWZ0IjpbXSwiYm90dG9tIjp7InRpdGxlIjoiSk1cdTcyNzlcdTZiOGFcdTViYTJcdTRlYmFcdTY3MGRcdTUyYTFcdTkwZTgiLCJwaWMiOiJodHRwczpcL1wvbWFuaHVhLnFwaWMuY25cL29wZXJhdGlvblwvMFwvMjRfMDlfNTVfOTkzMGZmZjViMGM4NGYxZjk0ZjE5MDQyY2NkOWQxZDdfMTUwODgxMDE1NTEwNi5qcGdcLzAiLCJ1cmwiOiJodHRwOlwvXC9hYy5xcS5jb21cL0NvbWljVmlld1wvaW5kZXhcL2lkXC82MjA3OTNcL2NpZFwvMSIsIndpZHRoIjoiNjUwIiwiaGVpZ2h0IjoiMTEwIn19LCJhcnRpc3QiOnsiYXZhdGFyIjoiaHR0cDpcL1wvcTMucWxvZ28uY25cL2c/Yj1xcSZrPWRMU0taU3FkT2RDWFBUNnVFSnV1TlEmcz02NDAmdD0xNDgzMzgxODgxIiwibmljayI6Ilx1OTYzZlUiLCJ1aW5DcnlwdCI6ImR6SmhkV2RSVkM4eFRHeHVlVEJaUzNwNVZrRlFVVDA5In19".Substring(1));
            var tt = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(ss);
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");         
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();//x.comicid == "1_622585" && x.downstatus == DownChapter.待处理链接
            List<Chapter> cplst = cpq.Where(x => x.source == Source.QQ && x.comicid == "1_622585").Take(200).ToList();
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
                    var s = DecodeHelper.QQPageDecode("geyJjb21pYyI6eyJpZCI6NDg1Mjg2LCJ0aXRsZSI6Ilx1OTYzZlUiLCJjb2xsZWN0IjoiNTY5NTMiLCJpc0phcGFuQ29taWMiOmZhbHNlLCJpc0xpZ2h0Tm92ZWwiOmZhbHNlLCJpc0xpZ2h0Q29taWMiOmZhbHNlLCJpc0ZpbmlzaCI6ZmFsc2UsImlzUm9hc3RhYmxlIjp0cnVlLCJlSWQiOiJLbEJQU0V0QVVGQmJBd0VmQUEwREJRQVBIRUZZV3lnPSJ9LCJjaGFwdGVyIjp7ImNpZCI6MTcyLCJjVGl0bGUiOiJcdTdiMmMyNVx1NTE4YyAwOFx1N2FlMCIsImNTZXEiOiIxNTkiLCJ2aXBTdGF0dXMiOjIsInByZXZDaWQiOjE3MSwibmV4dENpZCI6MTczLCJibGFua0ZpcnN0IjoxLCJjYW5SZWFkIjp0cnVlfSwicGljdHVyZSI6W3sicGlkIjoiNzEyMiIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MDImZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMV83OWUxZDI0OWMyMDVlYmVjMmExZWJiNjdjNmQ4OGNmMF83MTIyLmpwZyJ9LHsicGlkIjoiNzEyMyIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzImZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl9mODI5ZTEzNTI4MTdlM2IyZmIxNGUyMWQ2MjA3MGMxMF83MTIzLmpwZyJ9LHsicGlkIjoiNzEyNCIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl9iNzBlNmZkNjZlMTc5NDIyYTg5MDg1ZmJmM2NjMjcwM183MTI0LmpwZyJ9LHsicGlkIjoiNzEyNSIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl82NTEyYTA3YTY0YjZlYWIwMWUxNjMzMDZhMWEyYTQyNl83MTI1LmpwZyJ9LHsicGlkIjoiNzEyNiIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl82NmZkYjNiMDk0ODIxNzUwYTk1ZWE1NmYwMGU0NzgwN183MTI2LmpwZyJ9LHsicGlkIjoiNzEyNyIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzImZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl9lM2NhOGU5YmNmNzU2MjBlNmVjM2RiZTRlNjI1NzM0NV83MTI3LmpwZyJ9LHsicGlkIjoiNzEyOCIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl9mYmY1MGE3MTcyODkwY2RiNGVkZDdjNDhkMTUxYTRkOF83MTI4LmpwZyJ9LHsicGlkIjoiNzEyOSIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl8wZjc1NzcyNTU2ZjFhZmQwNWRlNjY5ZWIyZjcxY2FiOF83MTI5LmpwZyJ9LHsicGlkIjoiNzEzMCIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl80ZmMwY2JjZWRmMDBkNWY2NGU0ODk2OWNkMjU2MmQzN183MTMwLmpwZyJ9LHsicGlkIjoiNzEzMSIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5MzAmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl85ZWNkYTUzYmMyNTUzMWIwMjliMmZjMzAxMWM5ZWM3YV83MTMxLmpwZyJ9LHsicGlkIjoiNzEzMiIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5NDQmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl81NTQxN2UyOGE5YWRlNTNhMGUyOWJkNGJmYjZkY2VmNV83MTMyLmpwZyJ9LHsicGlkIjoiNzEzMyIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5NDQmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl9kNTIxZGIwNTEyOWE1ZWEyY2MyOWI5MTZkMjdiY2ZmY183MTMzLmpwZyJ9LHsicGlkIjoiNzEzNCIsIndpZHRoIjo5MDAsImhlaWdodCI6MTUyOCwidXJsIjoiaHR0cDpcL1wvYWMudGMucXEuY29tXC9zdG9yZV9maWxlX2Rvd25sb2FkP2J1aWQ9MTUwMTcmdWluPTE1MDgxMzE5NTgmZGlyX3BhdGg9XC8mbmFtZT0xNl8xM18zMl80NGNmNGEwYTg4OTYwMzE0NTI3NWQxYWRmM2UwNTJmYV83MTM0LmpwZyJ9XSwiYWRzIjp7InRvcCI6eyJ0aXRsZSI6Ilx1NGUwMFx1NGViYVx1NGU0Ylx1NGUwYlx1N2NmYlx1NTIxN1x1N2VhYVx1NWY1NVx1NzI0NyIsInBpYyI6Imh0dHBzOlwvXC9tYW5odWEucXBpYy5jblwvb3BlcmF0aW9uXC8wXC8yM18xMl8wMl9iYzk0NjhmNTJhYzlhN2ViMDk2MTdmYjNjMzNiMDRmZF8xNTA4NzMxMzMxOTkxLmpwZ1wvMCIsInVybCI6Imh0dHA6XC9cL2FjLnFxLmNvbVwvZXZlbnRcL3RyYW5zTGlua1wveWlyZW4yLmh0bWwiLCJ3aWR0aCI6IjY1MCIsImhlaWdodCI6IjExMCJ9LCJsZWZ0IjpbXSwiYm90dG9tIjp7InRpdGxlIjoiXHU4ZDg1XHU0ZWJhXHU3YzdiXHU2MjE4XHU0ZTg5IiwicGljIjoiaHR0cHM6XC9cL21hbmh1YS5xcGljLmNuXC9vcGVyYXRpb25cLzBcLzIzXzA5XzQxXzc3YTE3N2Q3OWQxYWMxYWRjZWU1Y2ZlYTZiNWExY2JlXzE1MDg3MjI5MDM3MDMuanBnXC8wIiwidXJsIjoiaHR0cDpcL1wvYWMucXEuY29tXC9Db21pY1ZpZXdcL2luZGV4XC9pZFwvNTUyOTAzXC9jaWRcLzciLCJ3aWR0aCI6IjY1MCIsImhlaWdodCI6IjExMCJ9fSwiYXJ0aXN0Ijp7ImF2YXRhciI6Imh0dHA6XC9cL3EzLnFsb2dvLmNuXC9nP2I9cXEmaz1kTFNLWlNxZE9kQ1hQVDZ1RUp1dU5RJnM9NjQwJnQ9MTQ4MzM4MTg4MSIsIm5pY2siOiJcdTk2M2ZVIiwidWluQ3J5cHQiOiJkekpoZFdkUlZDOHhUR3h1ZVRCWlMzcDVWa0ZRVVQwOSJ9fQ==".Substring(1));
                    var t = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(s);
                    if (t.chapter.vipStatus ==1 )
                    {
                        List<Page> pglst = new List<Page>();
                        for (int i = 0; i < t.picture.Count; i++)
                        {
                            pglst.Add(new Page()
                            {
                                chapterid = cp.chapterid,
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
                    else
                    {
                       
                        selenium.Navigate().GoToUrl(cp.chapterurl);
                        IList<IWebElement> frames = selenium.FindElements(By.TagName("iframe"));
                        IWebElement controlPanelFrame = null;  //commentFrame  ptlogin_iframe
                        foreach (var frame in frames)
                        {
                            if (frame.GetAttribute("id") == "iframeAll")
                            {
                                controlPanelFrame = frame;
                                break;
                            }
                        }
                        if (controlPanelFrame != null) //QQ登录 
                        {
                            selenium.SwitchTo().Frame(controlPanelFrame);

                            IReadOnlyCollection<IWebElement> switchtoElement = selenium.FindElements(By.Id("switcher_plogin"));
                            if (switchtoElement != null && switchtoElement.Count > 0)
                            {
                                switchtoElement.First().Click();

                                selenium.FindElement(By.Id("u")).Clear();
                                selenium.FindElement(By.Id("u")).SendKeys("3283360259");
                                selenium.FindElement(By.Id("p")).Clear();
                                selenium.FindElement(By.Id("p")).SendKeys("xxxttt5544");

                                selenium.FindElement(By.Id("login_button")).Click();
                            }
                            selenium.SwitchTo().DefaultContent();
                        }

                        frames = selenium.FindElements(By.TagName("iframe"));
                        IWebElement checkVipFrame = null;
                        foreach (var frame in frames)
                        {
                            if (frame.GetAttribute("id") == "checkVipFrame")
                            {
                                checkVipFrame = frame;
                                break;
                            }
                        }
                        if (checkVipFrame != null)
                        {
                            
                            selenium.SwitchTo().Frame(checkVipFrame);
                            //自动购买
                            IReadOnlyCollection<IWebElement> checkAutoElement = selenium.FindElements(By.Id("check_auto_next"));
                            IReadOnlyCollection<IWebElement> singlBbuyElement = selenium.FindElements(By.ClassName("single_buy"));
                            if (checkAutoElement != null && singlBbuyElement != null && checkAutoElement.Count > 0 && singlBbuyElement.Count > 0)
                            {
                                if (singlBbuyElement.First().Text.Trim().StartsWith("点券不足"))
                                {
                                    MailMessage msg = new MailMessage();
                                    msg.chapterid = cp.chapterid;
                                    msg.modify = dt;
                                    msg.shortdate = shortdate;
                                    msg.source = cp.source;
                                    msg.chaptermoney = 50;
                                    dbcontext.Insert(msg);
                                    continue;
                                }
                                else
                                {
                                    checkAutoElement.First().Click();
                                    singlBbuyElement.First().Click();
                                }
                               

                            }
                            selenium.SwitchTo().DefaultContent();
                        }
                        //Match match1 = rex.Match(selenium.PageSource);
                        IReadOnlyCollection<IWebElement> webElement = selenium.FindElements(By.XPath("/html"));
                        if (webElement != null && webElement.Count > 0)
                        {
                            Match match1 = rex.Match(webElement.First().GetAttribute("outerHTML"));
                            key = match1.Groups["key1"].Value;
                        }
                        else
                            key = string.Empty;
                        if (string.IsNullOrEmpty(key))
                        {
                            cp.downstatus = DownChapter.待处理链接;
                            cp.modify = dt;
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

                        s = DecodeHelper.QQPageDecode(key.Substring(1));
                        t = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(s);
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
