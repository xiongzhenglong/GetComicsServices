using ApiModel.QQ;
using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Framework.Redis;
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
using System.Web;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public class QQ_Key_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Key_Job));
        MsSqlContext dbcontext;
 
        static IWebDriver selenium;
        static ChromeOptions chromeOptions;
    
        public QQ_Key_Job()
        {
            try
            {
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
            catch (Exception e)
            {
                logger.Info("QQ_page vip init err:" + e.Message + "line:" + e.StackTrace);
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
            selenium.Navigate().GoToUrl("http://ac.qq.com/Home/buyList");
        }

        public void Execute(IJobExecutionContext context)
        {
            string errMsg = string.Empty;
            
            IList<IWebElement> frames = selenium.FindElements(By.TagName("iframe"));
            IWebElement controlPanelFrame = null;
            foreach (var frame in frames)
            {
                if (frame.GetAttribute("id") == "login_ifr")
                {
                    controlPanelFrame = frame;
                    break;
                }
            }
            if (controlPanelFrame != null && controlPanelFrame.Displayed == true)
            {
                try
                {
                    selenium.SwitchTo().Frame(controlPanelFrame);
                    IReadOnlyCollection<IWebElement> switchtoElement = selenium.FindElements(By.Id("switcher_plogin"));

                    if (switchtoElement != null && switchtoElement.Count > 0 && switchtoElement.First().Displayed == true)
                    {
                        switchtoElement.First().Click();

                        //selenium.FindElement(By.Id("u")).Clear();
                        //selenium.FindElement(By.Id("u")).SendKeys("3283360259");
                        //selenium.FindElement(By.Id("p")).Clear();
                        //selenium.FindElement(By.Id("p")).SendKeys("xxxttt5544");

                        selenium.FindElement(By.Id("u")).Clear();
                        selenium.FindElement(By.Id("u")).SendKeys("1434299101");
                        selenium.FindElement(By.Id("p")).Clear();
                        selenium.FindElement(By.Id("p")).SendKeys("zhangyin123");

                        selenium.FindElement(By.Id("login_button")).Click();
                    }
                    selenium.SwitchTo().DefaultContent();
                    ICookieJar listCookie = selenium.Manage().Cookies;
                    string uin = listCookie.AllCookies.Where(x => x.Name == "uin").FirstOrDefault().Value;
                    string skey = listCookie.AllCookies.Where(x => x.Name == "skey").FirstOrDefault().Value;
                    string pc_userinfo_cookie = HttpUtility.UrlDecode(listCookie.AllCookies.Where(x => x.Name == "pc_userinfo_cookie").FirstOrDefault().Value);

                    Regex reg1 = new Regex("token\":\"(?<key1>.*?)\"", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    Match match1 = reg1.Match(pc_userinfo_cookie);
                    string token = match1.Groups["key1"].Value;

                    var redis = new RedisProxy();
                    redis.Set("QQ_1434299101", new QQ_Login_Key()
                    {
                        skey = skey,
                        token = token
                    });
                    //redis.Add("QQ_1434299101", new QQ_Login_Key()
                    //{
                    //    skey = skey,
                    //    token = token
                    //});

                }
                catch (Exception ex)
                {
                    errMsg = "QQ 登录失败：" + ex.Message;
                    logger.Error(errMsg);
                }
            }
            
        }
    }

    public class QQ_Login_Key
    {
        public string skey { get; set; }
        public string token { get; set; }
    }
}
