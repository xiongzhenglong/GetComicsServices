﻿using ApiModel.QQ;
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
    /// 更新章节
    /// </summary>
    [DisallowConcurrentExecution]
    public class QQ_Page_Up_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Page_Up_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://ac.qq.com");
        static IWebDriver selenium;
        static ChromeOptions chromeOptions = new ChromeOptions();
        public QQ_Page_Up_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

            if (selenium == null)
            {
                chromeOptions.AddArguments("--headless");
                chromeOptions.AddArguments("window-size=1200x600");
                chromeOptions.AddArguments("start-maximized", "no-sandbox", "user-data-dir=D:/CacheFolder");
                selenium = new ChromeDriver(chromeOptions);//
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");
            IQuery<Page> pq = dbcontext.Query<Page>();
            IQuery<Notice> nq = dbcontext.Query<Notice>();
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();//x.comicid == "1_622585"
            List<Chapter> cplst = cpq.Where(x => x.source == Source.QQ && x.downstatus == DownChapter.上传完图片 && x.updatedate != shortdate).Take(200).ToList();
            List<int> ids = cplst.Select(x => x.Id).ToList();
            dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
            {
                updatedate = shortdate,
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
                                pagesource = t.picture[i].url
                            });
                        }

                        List<Page> pgdblst = pq.Where(x => x.chapterid == cp.chapterid).ToList();
                        if (pgdblst.Count != pglst.Count)
                        {
                            List<int> idlst = pgdblst.Select(x => x.Id).ToList();
                            dbcontext.Delete<Page>(x => idlst.Contains(x.Id));
                            dbcontext.BulkInsert(pglst);
                            cp.downstatus = DownChapter.处理完链接;
                            cp.modify = dt;
                            dbcontext.Update(cp);
                            Notice notice = new Notice();
                            notice.noticeid = cp.chapterid;
                            notice.noticestatus = NoticeStatus.等待处理;
                            notice.noticetype = NoticeType.章节更新;
                            notice.source = cp.source;
                            notice.shortdate = shortdate;
                            notice.modify = dt;
                            var nqwait = nq.Where(x => x.noticeid == cp.chapterid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.章节更新).FirstOrDefault();
                            if (nqwait == null)
                            {

                                dbcontext.Insert(notice);

                            }
                            continue;
                        }

                      

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
                                selenium.FindElement(By.Id("u")).SendKeys("2806126975");
                                selenium.FindElement(By.Id("p")).Clear();
                                selenium.FindElement(By.Id("p")).SendKeys("rby123456");

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
                                    continue;
                                }
                                else
                                {
                                    checkAutoElement.First().Click();
                                    singlBbuyElement.First().Click();
                                }


                            }
                        }
                        Match match1 = rex.Match(selenium.PageSource);
                        key = match1.Groups["key1"].Value;
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

                        List<Page> pgdblst = pq.Where(x => x.chapterid == cp.chapterid).ToList();
                        if (pgdblst.Count != pglst.Count)
                        {
                            List<int> idlst = pgdblst.Select(x => x.Id).ToList();
                            dbcontext.Delete<Page>(x => idlst.Contains(x.Id));
                            dbcontext.BulkInsert(pglst);
                            cp.downstatus = DownChapter.处理完链接;
                            cp.modify = dt;
                            dbcontext.Update(cp);
                            Notice notice = new Notice();
                            notice.noticeid = cp.chapterid;
                            notice.noticestatus = NoticeStatus.等待处理;
                            notice.noticetype = NoticeType.章节更新;
                            notice.source = cp.source;
                            notice.shortdate = shortdate;
                            notice.modify = dt;
                            var nqwait = nq.Where(x => x.noticeid == cp.chapterid && x.noticestatus == NoticeStatus.等待处理 && x.noticetype == NoticeType.章节更新).FirstOrDefault();
                            if (nqwait == null)
                            {

                                dbcontext.Insert(notice);

                            }
                            continue;
                        }

                      
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    cp.updatedate = yesterday;
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
