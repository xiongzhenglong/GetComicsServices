using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using ExcelModel;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawer.Jobs
{
    /// <summary>
    /// 漫画书单入库
    /// </summary>
    [DisallowConcurrentExecution]
    public class Comic_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Comic_Job));
        private MsSqlContext dbcontext;

        public Comic_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //List<bkIm> bklst = ExcelHelper.Import(@"C:\Users\Administrator\Desktop\7家书单抓取.xlsx").ToList();
            //List<bkIm> bklst = ExcelHelper.Import2(@"C:\Users\Administrator\Desktop\漫画抓取信息.xlsx").ToList();
            List<bkIm> bklst = new List<bkIm>();

            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628520" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627844" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627229" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626970" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626968" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626920" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626916" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626652" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626455" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624495" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624347" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624308" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624012" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623715" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623595" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623115" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622968" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622674" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622498" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/621673" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/621253" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/620523" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/620493" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553632" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553579" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553055" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/552920" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/552918" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/552839" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/552104" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/547900" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/547343" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/545168" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/544521" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/543172" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/542724" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/539105" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/536658" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628956" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628566" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628481" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628198" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628180" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628109" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627912" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627383" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627300" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627019" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626907" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626527" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626362" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624871" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624601" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623795" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623201" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623200" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623199" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623196" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623195" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622985" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622973" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622971" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622754" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622694" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622561" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622160" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/621727" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/621051" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/620725" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553972" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628458" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553644" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553026" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/552065" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/551693" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/551405" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/551386" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549847" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549277" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/548731" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/547243" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/545320" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/542861" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/538969" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/537982" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/534826" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/534796" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/534422" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/533555" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/533395" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/529810" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/518008" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/512742" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626289" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626269" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622660" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553518" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553204" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549673" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549599" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/545131" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/537899" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628565" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628464" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628428" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628305" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/628269" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627823" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/627456" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626955" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626819" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626604" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626465" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626268" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/626267" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624341" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624152" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624135" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/624009" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623833" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623720" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623494" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623492" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623459" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623283" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623125" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/623009" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622887" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622856" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622855" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622854" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622565" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622365" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622216" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/622108" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/621020" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/620928" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/553939" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/550484" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549278" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549274" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/549125" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/547358" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/545404" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/541345" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/540523" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/539443" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/536042" });
            bklst.Add(new bkIm() { bookurl = "http://ac.qq.com/Comic/comicInfo/id/535934" });








            //IQuery<VIPFreeComic> vfcq = dbcontext.Query<VIPFreeComic>();
            //List<VIPFreeComic> vfclst = vfcq.Where(x => true).ToList();
            //vfclst.ForEach(x =>
            //{
            //    bklst.Add(new bkIm()
            //    {
            //        bookurl = x.bookurl
            //    });
            //});



            List<Comic> comiclst = new List<Comic>();
            IQuery<Comic> cq = dbcontext.Query<Comic>();

            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            foreach (var bk in bklst)
            {
                if (!bk.bookurl.StartsWith("http://ac.qq.com"))
                {
                    continue;
                }
                var comic = cq.Where(x => x.bookurl == bk.bookurl).FirstOrDefault();
                if (comic != null)
                {
                    continue;
                }
                if (bk.bookurl.StartsWith("http://ac.qq.com"))
                {
                    try
                    {
                        HttpHelper _helper = new HttpHelper("http://ac.qq.com");
                        string bookurl = bk.bookurl.Replace("http://ac.qq.com/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        Regex reg1 = new Regex("id=\"input_artistName\" value=\"(?<key1>.*?)\"");
                        Match match1 = reg1.Match(bookdata);
                        string authorname = match1.Groups["key1"].Value;
                        Regex reg2 = new Regex("img src=\"(?<key1>.*?)\" alt=\"(?<key2>.*?)\" height=\"280\" width=\"210\"");
                        Match match2 = reg2.Match(bookdata);
                        string comiccover = match2.Groups["key1"].Value;
                        string comicname = match2.Groups["key2"].Value;
                        Regex reg3 = new Regex("<label class=\"works-intro-status\">(?<key1>.*?)</label>");
                        Match match3 = reg3.Match(bookdata);
                        string isfinished = match3.Groups["key1"].Value;
                        Regex reg4 = new Regex("<meta name=\"Description\" content=\"(?<key1>.*?)，简介：(?<key2>.*?)《(?<key3>.*?)》的标签：(?<key4>.*?)\" />");
                        Match match4 = reg4.Match(bookdata);
                        string bookdesc = match4.Groups["key2"].Value;
                        string theme = match4.Groups["key4"].Value;
                        comiclst.Add(new Comic()
                        {
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source.QQ + "_" + bk.bookurl.Split('/').LastOrDefault(),
                            comicname = comicname,
                            isfinished = isfinished,
                            theme = theme,
                            isvip = "0",
                            source = Source.QQ,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
                else if (bk.bookurl.StartsWith("https://www.dongmanmanhua.cn"))
                {
                    try
                    {
                        HttpHelper _helper = new HttpHelper("https://www.dongmanmanhua.cn");
                        string bookurl = bk.bookurl.Replace("https://www.dongmanmanhua.cn/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        Regex reg1 = new Regex("<meta property=\"og:image\" content=\"(?<key1>.*?)\" />");
                        Match match1 = reg1.Match(bookdata);
                        string comiccover = match1.Groups["key1"].Value;
                        Regex reg2 = new Regex("<meta property=\"com-dongman:webtoon:author\" content=\"(?<key1>.*?)\" />");
                        Match match2 = reg2.Match(bookdata);
                        string authorname = match2.Groups["key1"].Value;
                        Regex reg3 = new Regex("<p class=\"summary\">(?<key1>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match3 = reg3.Match(bookdata);
                        string bookdesc = match3.Groups["key1"].Value;
                        Regex reg4 = new Regex("<h1 class=\"subj\">(?<key1>.*?)</h1>");
                        Match match4 = reg4.Match(bookdata);
                        string comicname = match4.Groups["key1"].Value;
                        Regex reg5 = new Regex("txt_ico_up");
                        Match match5 = reg5.Match(bookdata);
                        string isfinished = match5.Length > 0 ? "连载中" : "已完结";
                        Regex reg6 = new Regex("<h2 class=\"genre(?<key1>.*?)\">(?<key2>.*?)</h2>");
                        Match match6 = reg6.Match(bookdata);
                        string theme = match6.Groups["key2"].Value;
                        comiclst.Add(new Comic()
                        {
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source.dongmanmanhua + "_" + bk.bookurl.Split('=').LastOrDefault(),
                            comicname = comicname,
                            isfinished = isfinished,
                            theme = theme,
                            isvip = "0",
                            source = Source.dongmanmanhua,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
                else if (bk.bookurl.StartsWith("http://www.u17.com"))
                {
                    try
                    {
                        HttpHelper _helper = new HttpHelper("http://www.u17.com");
                        string bookurl = bk.bookurl.Replace("http://www.u17.com/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        Regex reg1 = new Regex("<title>(?<key1>.*?)</title>");
                        Match match1 = reg1.Match(bookdata);
                        string comicname = match1.Groups["key1"].Value.Split('_')[0];
                        string authorname = match1.Groups["key1"].Value.Split('_')[1];

                        Regex reg2 = new Regex("cover_url = \"(?<key1>.*?)\"");
                        Match match2 = reg2.Match(bookdata);
                        string comiccover = match2.Groups["key1"].Value;
                        Regex reg3 = new Regex("id=\"words\">(?<key1>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match3 = reg3.Match(bookdata);
                        string bookdesc = match3.Groups["key1"].Value.Trim();
                        Regex reg4 = new Regex("状态：(?<key1>.*?)<span(?<key2>.*?)>(?<key3>.*?)</span>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match4 = reg4.Match(bookdata);
                        string isfinished = match4.Groups["key3"].Value.Trim();
                        Regex reg5 = new Regex("txt_ico_up");
                        Match match5 = reg5.Match(bookdata);

                        string theme = "";
                        string pattern = "class=\"label(?<key1>.*?)\" title=\"(?<key3>.*?)\">(?<key2>.*?)</a>";
                        MatchCollection match6 = Regex.Matches(bookdata, pattern);
                        if (match6.Count == 0)
                        {
                            pattern = "class=\"(?<key1>.*?)_tag \" id=\"tag_(?<key3>.*?)\">(?<key2>.*?)</a>";
                            match6 = Regex.Matches(bookdata, pattern);
                        }

                        for (int j = 0; j < match6.Count; j++)
                        {
                            if (j == 0)
                            {
                                theme = match6[j].Groups["key2"].Value.Trim();
                            }
                            else
                            {
                                theme = theme + "," + match6[j].Groups["key2"].Value.Trim();
                            }
                        }

                        comiclst.Add(new Comic()
                        {
                            comicname = comicname,
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source.U17 + "_" + bk.bookurl.Split('/').LastOrDefault().Replace(".html", ""),

                            isfinished = isfinished,
                            theme = theme,
                            isvip = "0",
                            source = Source.U17,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
                else if (bk.bookurl.StartsWith("http://www.zymk.cn"))
                {
                    try
                    {
                        HttpHelper _helper = new HttpHelper("http://www.zymk.cn");
                        string bookurl = bk.bookurl.Replace("http://www.zymk.cn/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        Regex reg1 = new Regex("<meta property=\"og:novel:book_name\" content=\"(?<key1>.*?)\">");
                        Match match1 = reg1.Match(bookdata);
                        string comicname = match1.Groups["key1"].Value;

                        Regex reg2 = new Regex("<meta property=\"og:novel:author\" content=\"(?<key1>.*?)\">");
                        Match match2 = reg2.Match(bookdata);
                        string authorname = match2.Groups["key1"].Value;
                        Regex reg3 = new Regex("<meta property=\"og:image\" content=\"(?<key1>.*?)\">");
                        Match match3 = reg3.Match(bookdata);
                        string comiccover = match3.Groups["key1"].Value;

                        Regex reg4 = new Regex("<div class=\"desc-con\">(?<key1>.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match4 = reg4.Match(bookdata);
                        string bookdesc = match4.Groups["key1"].Value.Trim();

                        Regex reg5 = new Regex("<meta property=\"og:novel:status\" content=\"(?<key1>.*?)\">");
                        Match match5 = reg5.Match(bookdata);
                        string isfinished = match5.Groups["key1"].Value == "连载" ? "连载中" : "已完结";

                        string theme = "";
                        Regex reg6 = new Regex("<meta property=\"og:novel:category\" content=\"(?<key1>.*?)\">");
                        Match match6 = reg6.Match(bookdata);
                        theme = string.Join(",", match6.Groups["key1"].Value.Trim().Split(' '));

                        comiclst.Add(new Comic()
                        {
                            comicname = comicname,
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source.Zymk + "_" + bk.bookurl.Split('/')[3],

                            isfinished = isfinished,
                            theme = theme,
                            isvip = "0",
                            source = Source.Zymk,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
                else if (bk.bookurl.StartsWith("http://www.manhuatai.com"))
                {
                    try
                    {
                        HttpHelper _helper = new HttpHelper("http://www.manhuatai.com");
                        string bookurl = bk.bookurl.Replace("http://www.manhuatai.com/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        Regex reg1 = new Regex("<meta property=\"og:novel:book_name\" content=\"(?<key1>.*?)\">");
                        Match match1 = reg1.Match(bookdata);
                        string comicname = match1.Groups["key1"].Value;

                        Regex reg2 = new Regex("<meta property=\"og:novel:author\" content=\"(?<key1>.*?)\">");
                        Match match2 = reg2.Match(bookdata);
                        string authorname = match2.Groups["key1"].Value;

                        Regex reg3 = new Regex("<meta property=\"og:image\" content=\"(?<key1>.*?)\">");
                        Match match3 = reg3.Match(bookdata);
                        string comiccover = match3.Groups["key1"].Value;

                        Regex reg4 = new Regex("<div class=\"wz clearfix t1\"><div>(?<key1>.*?)<a href=\"javascript:void(?<key2>.*?)\" target=\"_self\" class=\"wzrtitle\" onclick=\"openOrCloseSummary", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match4 = reg4.Match(bookdata);
                        string bookdesc = match4.Groups["key1"].Value.Trim();

                        Regex reg5 = new Regex("<meta property=\"og:novel:status\" content=\"(?<key1>.*?)\">");
                        Match match5 = reg5.Match(bookdata);
                        string isfinished = match5.Groups["key1"].Value == "连载中" ? "连载中" : "已完结";

                        string theme = "";
                        Regex reg6 = new Regex("<meta property=\"og:novel:category\" content=\"(?<key1>.*?)\">");
                        Match match6 = reg6.Match(bookdata);
                        theme = string.Join(",", match6.Groups["key1"].Value.Trim().Split(' '));

                        comiclst.Add(new Comic()
                        {
                            comicname = comicname,
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source.Manhuatai + "_" + bk.bookurl.Split('/')[3],

                            isfinished = isfinished,
                            theme = theme,
                            isvip = "0",
                            source = Source.Manhuatai,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
                else if (bk.bookurl.StartsWith("https://manhua.163.com"))
                {
                    try
                    {
                        HttpHelper _helper = new HttpHelper("https://manhua.163.com");
                        string bookurl = bk.bookurl.Replace("https://manhua.163.com/", "");
                        var bookdata = _helper.Get(null, bookurl);
                        Regex reg1 = new Regex("<meta name=\"keywords\" content=\"(?<key1>.*?)\">");
                        Match match1 = reg1.Match(bookdata);
                        var arry = match1.Groups["key1"].Value.Split(',').ToList();
                        string comicname = arry[0];
                        string authorname = arry[arry.Count - 2];
                        arry.Remove(arry[0]);
                        arry.Remove(arry[0]);
                        arry.Remove(arry[arry.Count - 1]);
                        arry.Remove(arry[arry.Count - 1]);
                        string theme = string.Join(",", arry.Where(x => x.Length == 2).ToArray());

                        Regex reg3 = new Regex("<img class=\"sr-bcover\" src=\"(?<key1>.*?)\"/>");
                        Match match3 = reg3.Match(bookdata);
                        string comiccover = match3.Groups["key1"].Value;

                        Regex reg4 = new Regex("<dt>简介</dt>(?<key2>.*?)<dd>(?<key1>.*?)</dd>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match4 = reg4.Match(bookdata);
                        string bookdesc = match4.Groups["key1"].Value.Trim();

                        Regex reg5 = new Regex("<dt>状态</dt>(?<key2>.*?)<dd><a href=\"(?<key3>.*?)\" target=\"_blank\">(?<key1>.*?)</a></dd>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match5 = reg5.Match(bookdata);
                        string isfinished = match5.Groups["key1"].Value == "连载中" ? "连载中" : "已完结";

                        comiclst.Add(new Comic()
                        {
                            comicname = comicname,
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source._163 + "_" + bk.bookurl.Split('/')[4],

                            isfinished = isfinished,
                            theme = theme,
                            isvip = "0",
                            source = Source._163,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
                else if (bk.bookurl.StartsWith("http://www.mh160.com"))
                {
                    try
                    {
                        HttpWebHelper _helper = new HttpWebHelper();

                        var bookdata = _helper.Get(bk.bookurl, Encoding.GetEncoding("gb2312"));
                        Regex reg1 = new Regex("<meta property=\"og:novel:book_name\" content=\"(?<key1>.*?)\">");
                        Match match1 = reg1.Match(bookdata);
                        string comicname = match1.Groups["key1"].Value;

                        Regex reg2 = new Regex("<meta property=\"og:novel:author\" content=\"(?<key1>.*?)\">");
                        Match match2 = reg2.Match(bookdata);
                        string authorname = match2.Groups["key1"].Value;

                        Regex reg3 = new Regex("<meta property=\"og:image\" content=\"(?<key1>.*?)\">");
                        Match match3 = reg3.Match(bookdata);
                        string comiccover = match3.Groups["key1"].Value;

                        Regex reg4 = new Regex("<div class=\"introduction\" id=\"intro1\"><p>(?<key1>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match4 = reg4.Match(bookdata);
                        string bookdesc = match4.Groups["key1"].Value.Trim();

                        Regex reg5 = new Regex("<meta property=\"og:novel:status\" content=\"(?<key1>.*?)\">");
                        Match match5 = reg5.Match(bookdata);

                        string isfinished = match5.Groups["key1"].Value == "连载中" ? "连载中" : "已完结";

                        string theme = "";
                        Regex reg6 = new Regex("<meta property=\"og:novel:category\" content=\"(?<key1>.*?)\">");
                        Match match6 = reg6.Match(bookdata);
                        theme = string.Join(",", match6.Groups["key1"].Value.Trim().Split(' '));

                        comiclst.Add(new Comic()
                        {
                            comicname = comicname,
                            authorname = authorname,
                            bookurl = bk.bookurl,
                            comiccoversource = comiccover,
                            comiccoverlocal = "",
                            comicdesc = bookdesc,
                            comicid = (int)Source.mh160 + "_" + bk.bookurl.Split('/')[4],
                            isfinished = isfinished,
                            theme = theme,

                            isvip = "0",
                            source = Source.mh160,
                            stopcrawer = false,
                            isoffline = false,
                            recrawer = false,
                            shortdate = shortdate,
                            modify = dt,
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        Err_ComicJob err = new Err_ComicJob();
                        err.bookurl = bk.bookurl;
                        err.errtype = ErrComic.解析出错;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                }
            }
            if (comiclst.Count > 0)
            {
                //foreach (var item in comiclst)
                //{
                //    var comic = cq.Where(x => x.bookurl == item.bookurl).FirstOrDefault();
                //    comic.comicdesc = item.comicdesc;
                //    dbcontext.Update(comic);
                //}
                dbcontext.BulkInsert(comiclst);
            }
        }
    }
}