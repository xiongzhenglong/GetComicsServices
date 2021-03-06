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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    /// <summary>
    /// 爬取章节图片
    /// </summary>
    [DisallowConcurrentExecution]
    public class QQ_Page_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Page_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://ac.qq.com");
        static IWebDriver selenium;
        static ChromeOptions chromeOptions;
        static bool isHasMoney = true;
        public QQ_Page_Job()
        {
            try {
                dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

                if (selenium == null)
                {
                    Process[] pList = Process.GetProcessesByName("chromedriver");
                    foreach (var p in pList)
                    {
                        p.Kill();
                    }
                    InitChromeDriver();
                }
            }
            catch(Exception e)
            {
                logger.Info("QQ_page vip init err:"+e.Message +"line:"+e.StackTrace);
                throw e;
            }
        }
        private static void InitChromeDriver()
        {
            chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("window-size=1280x800");
            chromeOptions.AddArguments("test-type");
            chromeOptions.AddArguments("ignore-certificate-errors");
            chromeOptions.AddArguments("disk-cache-size=1048576000");
            chromeOptions.AddArguments("media-cache-size=1048576000");
            chromeOptions.AddArguments("disable-gpu");
            chromeOptions.AddArguments("disable-extensions");
            chromeOptions.AddArguments("disable-translate");
            chromeOptions.AddArguments("start-maximized", "no-sandbox", "user-data-dir=C:/UserDataFolder");

            chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.stylesheet", 2);
            chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);

            selenium = new ChromeDriver(chromeOptions);
            selenium.Navigate().GoToUrl("http://ac.qq.com/");
        }

        public void Execute(IJobExecutionContext context)
        {
            //logger.Info("QQ_page vip begin");
            //string isStart = "IsStartBuyQQ".ValueOfAppSetting();
            //if (isStart != null && isStart.Equals("1"))
            {
                DateTime dt = DateTime.Now;
                string shortdate = dt.ToString("yyyy-MM-dd");
                string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");
                IQuery<Chapter> cpq = dbcontext.Query<Chapter>();//x.comicid == "1_524356" 
                IQuery<PageHis> phisq = dbcontext.Query<PageHis>();
                //List<Chapter> cplst = cpq.Where(x => x.source == Source.QQ && x.downstatus == DownChapter.待处理链接 && x.isvip.Equals("1")).Take(200).ToList();
                List<Chapter> cplst = cpq.Where(x => x.source == Source.QQ && x.downstatus == DownChapter.上传完图片 && x.isvip.Equals("1")).Take(200).ToList();
                List<int> ids = cplst.Select(x => x.Id).ToList();
                dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
                {
                    downstatus = DownChapter.处理中,
                    modify = dt
                });
                List<Chapter> chapterlst = new List<Chapter>();
                Regex rex = new Regex("var DATA        = '(?<key1>.*?)',");
                string errMsg;
                foreach (var cp in cplst)
                {
                    try
                    {
                        IQuery<PageHis> cpHis = phisq.Where(x => x.chapterid == cp.chapterid);
                        if(cpHis != null && cpHis.Count() > 0)
                        {
                            List<PageHis> cpHisList = cpHis.ToList();
                       
                            List<Page> pglst = new List<Page>();
                            foreach (var page in cpHisList)
                            {
                                pglst.Add(new Page()
                                {
                                    chapterid = page.chapterid,
                                    modify = dt,
                                    shortdate = shortdate,
                                    sort = page.sort,
                                    source = page.source,
                                    pagelocal = "",
                                    pagesource = page.pagesource
                                });
                            }

                            cp.downstatus = DownChapter.处理完链接;
                            cp.modify = dt;
                            dbcontext.Update(cp);
                            dbcontext.BulkInsert(pglst);
                            logger.Info("QQ_page vip syn history sucess:id=" + cp.Id);
                        }
                        else
                        {

                            errMsg = string.Empty;
                            selenium.Navigate().GoToUrl(cp.chapterurl);
                            IList<IWebElement> frames = selenium.FindElements(By.TagName("iframe"));
                            IWebElement controlPanelFrame = null;
                            foreach (var frame in frames)
                            {
                                if (frame.GetAttribute("id") == "iframeAll")
                                {
                                    controlPanelFrame = frame;
                                    break;
                                }
                            }
                            if (controlPanelFrame != null && controlPanelFrame.Displayed == true) //QQ登录 
                            {
                                try
                                {
                                    selenium.SwitchTo().Frame(controlPanelFrame);
                                    IReadOnlyCollection<IWebElement> switchtoElement = selenium.FindElements(By.Id("switcher_plogin"));

                                    if (switchtoElement != null && switchtoElement.Count > 0 && switchtoElement.First().Displayed == true)
                                    {
                                        switchtoElement.First().Click();

                                        selenium.FindElement(By.Id("u")).Clear();
                                        selenium.FindElement(By.Id("u")).SendKeys("3283360259");
                                        selenium.FindElement(By.Id("p")).Clear();
                                        selenium.FindElement(By.Id("p")).SendKeys("xxxttt5544");

                                        //selenium.FindElement(By.Id("u")).Clear();
                                        //selenium.FindElement(By.Id("u")).SendKeys("1434299101");
                                        //selenium.FindElement(By.Id("p")).Clear();
                                        //selenium.FindElement(By.Id("p")).SendKeys("zhangyin123");

                                        selenium.FindElement(By.Id("login_button")).Click();
                                    }
                                    selenium.SwitchTo().DefaultContent();
                                 
                                }
                                catch (Exception ex)
                                {
                                    errMsg = "QQ 登录失败：" + ex.Message;
                                    logger.Error(errMsg);
                                }
                            }
                            selenium.Navigate().GoToUrl("http://ac.qq.com/Home/buyList");
                            ICookieJar listCookie = selenium.Manage().Cookies;
                            // IList<Cookie> listCookie = selenuim.Manage( ).Cookies.AllCookies;//只是显示 可以用Ilist对象
                            //显示初始Cookie的内容
                            Console.WriteLine("--------------------");
                            Console.WriteLine($"当前Cookie集合的数量：\t{listCookie.AllCookies.Count}");
                            for (int i = 0; i < listCookie.AllCookies.Count; i++)
                            {

                                Console.WriteLine($"Cookie的名称:{listCookie.AllCookies[i].Name}");
                                Console.WriteLine($"Cookie的值:{listCookie.AllCookies[i].Value}");
                                Console.WriteLine($"Cookie的所在域:{listCookie.AllCookies[i].Domain}");
                                Console.WriteLine($"Cookie的路径:{listCookie.AllCookies[i].Path}");
                                Console.WriteLine($"Cookie的过期时间:{listCookie.AllCookies[i].Expiry}");
                                Console.WriteLine("-----");
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
                            if (checkVipFrame != null && checkVipFrame.Displayed == true)
                            {
                                try
                                {
                                    //自动购买
                                    selenium.SwitchTo().Frame(checkVipFrame);
                                    IReadOnlyCollection<IWebElement> checkAutoElement = selenium.FindElements(By.Id("check_auto_next"));
                                    IReadOnlyCollection<IWebElement> singlBbuyElement = selenium.FindElements(By.ClassName("single_buy"));
                                    if (checkAutoElement != null && singlBbuyElement != null && checkAutoElement.Count > 0 && singlBbuyElement.Count > 0 && checkAutoElement.First().Displayed == true)
                                    {
                                        if (singlBbuyElement.First().Text.IndexOf("点券不足") > -1)
                                        {
                                            //列表中未成功购买的数据还原成待处理
                                            dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
                                            {
                                                downstatus = DownChapter.待处理链接,
                                                modify = dt
                                            });
                                            ////关闭购买，等待修改配置
                                            //"IsStartBuyQQ".SetAppSettingValue("0");
                                            if (isHasMoney)
                                            {
                                                Err_ChapterJob err = new Err_ChapterJob();
                                                err.bookurl = cp.chapterurl;
                                                err.source = cp.source;
                                                err.errtype = ErrChapter.解析出错;
                                                err.modify = dt;
                                                err.shortdate = shortdate;
                                                err.message = "点券不足，请去充值！";
                                                err = dbcontext.Insert(err);
                                            }
                                            isHasMoney = false;
                                            //Thread.Sleep(3600000);
                                            continue;
                                        }
                                        else
                                            isHasMoney = true;

                                        checkAutoElement.First().Click();
                                        singlBbuyElement.First().Click();
                                    }
                                    selenium.SwitchTo().DefaultContent();
                                }
                                catch (Exception ex)
                                {
                                    errMsg = "自动购买失败：" + ex.Message;
                                    logger.Error(errMsg);
                                }
                            }
                            Match match1 = rex.Match(selenium.PageSource);
                            string key = match1.Groups["key1"].Value;
                            if (string.IsNullOrEmpty(key) || errMsg != string.Empty)
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
                                err.message = errMsg != string.Empty ? errMsg : "DATA解析失败";
                                err = dbcontext.Insert(err);
                                continue;
                            }

                            string s = DecodeHelper.QQPageDecode(key.Substring(1));
                            var t = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(s);
                            if (t.picture.Count < 1)
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
                                err.message = "访问付费章节内容时只存在一张图片";
                                err = dbcontext.Insert(err);
                                continue;
                            }
                            List<Page> pglst = new List<Page>();
                            for (int i = 0; i < t.picture.Count; i++)
                            {
                                if (pglst.Exists(x => x.pagesource == t.picture[i].url) == false)
                                {
                                    pglst.Add(new Page()
                                    {
                                        chapterid = cp.chapterid,
                                        modify = DateTime.Now,
                                        shortdate = shortdate,
                                        sort = i + 1,
                                        source = cp.source,
                                        pagelocal = "",
                                        pagesource = t.picture[i].url
                                    });
                                }
                                else
                                {
                                    Err_ChapterJob err = new Err_ChapterJob();
                                    err.bookurl = cp.chapterurl;
                                    err.source = cp.source;
                                    err.errtype = ErrChapter.解析出错;
                                    err.modify = DateTime.Now;
                                    err.shortdate = shortdate;
                                    err.message = "存在重复图片";
                                    err = dbcontext.Insert(err);
                                }
                            }

                            cp.downstatus = DownChapter.处理完链接;
                            cp.modify = dt;
                            dbcontext.Update(cp);
                            dbcontext.BulkInsert(pglst);
                            ids.Remove(cp.Id);

                            logger.Info("QQ_page vip buy sucess:id=" + cp.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("Unexpected error. System.Net.WebException:") > -1) { InitChromeDriver(); }
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
        //public void Execute(IJobExecutionContext context) //测试
        //{
        //    DateTime dt = DateTime.Now;
        //    string shortdate = dt.ToString("yyyy-MM-dd");
        //    string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");
        //    Regex rex = new Regex("var DATA        = '(?<key1>.*?)',");
        //    string key;
        //    try
        //    {
        //        for (int i = 150; i < 287; i++)
        //        {
        //            //DateTime dtNow = DateTime.Now;
        //            string chapterpage = "http://ac.qq.com/ComicView/index/id/526730/cid/" + i;
        //            //var imgdata = _helper.Get(null, chapterpage);
        //            //var count = (DateTime.Now - dtNow).TotalMilliseconds;
        //            //logger.Info("getPage" + i + "Api=" + count);

        //            DateTime dtNow = DateTime.Now;
        //            selenium.Navigate().GoToUrl(chapterpage);
        //            IList<IWebElement> frames = selenium.FindElements(By.TagName("iframe"));
        //            IWebElement controlPanelFrame = null;  //commentFrame  ptlogin_iframe
        //            foreach (var frame in frames)
        //            {
        //                if (frame.GetAttribute("id") == "iframeAll")
        //                {
        //                    controlPanelFrame = frame;
        //                    break;
        //                }
        //            }
        //            if (controlPanelFrame != null && controlPanelFrame.Displayed == true) //QQ登录 
        //            {
        //                selenium.SwitchTo().Frame(controlPanelFrame);

        //                IReadOnlyCollection<IWebElement> switchtoElement = selenium.FindElements(By.Id("switcher_plogin"));
        //                if (switchtoElement != null && switchtoElement.Count > 0)
        //                {
        //                    try {
        //                        switchtoElement.First().Click();

        //                        selenium.FindElement(By.Id("u")).Clear();
        //                        selenium.FindElement(By.Id("u")).SendKeys("3283360259");
        //                        selenium.FindElement(By.Id("p")).Clear();
        //                        selenium.FindElement(By.Id("p")).SendKeys("xxxttt5544");

        //                        selenium.FindElement(By.Id("login_button")).Click();
        //                    }
        //                    catch(Exception e)
        //                    {
        //                        string errMsg = "qq登录失败：" + e.Message;
        //                        logger.Error(errMsg);
        //                    }
        //                }
        //                selenium.SwitchTo().DefaultContent();
        //            }

        //            frames = selenium.FindElements(By.TagName("iframe"));
        //            IWebElement checkVipFrame = null;
        //            foreach (var frame in frames)
        //            {
        //                if (frame.GetAttribute("id") == "checkVipFrame")
        //                {
        //                    checkVipFrame = frame;
        //                    break;
        //                }
        //            }
        //            if (checkVipFrame != null && checkVipFrame.Displayed == true)//自动购买
        //            {
        //                try
        //                {
        //                    selenium.SwitchTo().Frame(checkVipFrame);
        //                    IReadOnlyCollection<IWebElement> checkAutoElement = selenium.FindElements(By.Id("check_auto_next"));
        //                    //div[@class='ui-dialog-buttonset']
        //                    IReadOnlyCollection<IWebElement> singlBbuyElement = selenium.FindElements(By.ClassName("single_buy"));
        //                    if (checkAutoElement != null && singlBbuyElement != null && checkAutoElement.Count > 0 && singlBbuyElement.Count > 0 && checkAutoElement.First().Displayed == true)
        //                    {
        //                        //if (checkAutoElement.First().Selected == false)
        //                        //{
        //                        //    IJavaScriptExecutor js = (IJavaScriptExecutor)selenium;
        //                        //    js.ExecuteScript("$('#check_auto_next').click();");
        //                        //}
        //                        checkAutoElement.First().Click();
        //                        singlBbuyElement.First().Click();
        //                    }
        //                    selenium.SwitchTo().DefaultContent();
        //                }
        //                catch(Exception e)
        //                {
        //                   string errMsg = "自动购买失败：" + e.Message;
        //                   logger.Error(errMsg);
        //                }
        //            }
        //            Match match1 = rex.Match(selenium.PageSource);
        //            key = match1.Groups["key1"].Value;
        //            //IReadOnlyCollection<IWebElement> webElement = selenium.FindElements(By.XPath("/html"));
        //            //if (webElement != null && webElement.Count > 0)
        //            //{
        //            //    Match match1 = rex.Match(webElement.First().GetAttribute("outerHTML"));
        //            //    key = match1.Groups["key1"].Value;
        //            //}
        //            //else
        //            //    key = string.Empty;
        //            if (string.IsNullOrEmpty(key))
        //            {
        //                logger.Info("getPage" + i + "ChromeDriver err key =" + key);
        //            }
        //            else
        //            {
        //                string s = DecodeHelper.QQPageDecode(key.Substring(1));
        //                var t = JsonHelper.DeserializeJsonToObject<QQ_Page_Api>(s);
        //                List<Page> pglst = new List<Page>();
        //                logger.Info("getPage" + i + "ChromeDriver succ=" + (DateTime.Now - dtNow).TotalMilliseconds + "pagenum=" + t.picture.Count);
        //            }
        //            //dt = DateTime.Now;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        if (e.Message.IndexOf("Unexpected error. System.Net.WebException:") > -1) { InitChromeDriver(); }
        //        logger.Info("getPage error=" + e.Message + e.StackTrace);
        //    }
        //}
    }
}
