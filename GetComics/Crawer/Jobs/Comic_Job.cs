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
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    /// <summary>
    /// 漫画书单入库
    /// </summary>
    [DisallowConcurrentExecution]
    public class Comic_Job:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Comic_Job));
        MsSqlContext dbcontext;
        
        public Comic_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

        }

        public void Execute(IJobExecutionContext context)
        {
            List<bkIm> bklst = ExcelHelper.Import(@"C:\Users\Administrator\Desktop\7家书单抓取.xlsx").ToList();
            List<Comic> comiclst = new List<Comic>();
            IQuery<Comic> cq = dbcontext.Query<Comic>();
            
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            foreach (var bk in bklst)
            {
                //var comic = cq.Where(x => x.bookurl == bk.bookurl).FirstOrDefault();
                //if (comic != null)
                //{
                //    continue;
                //}
                if (bk.bookurl.StartsWith("1http://ac.qq.com"))
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
                            comiccoverlocal="",
                            comicdesc = bookdesc,
                            comicid = Source.QQ+"_"+bk.bookurl.Split('/').LastOrDefault(),
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
                else if (bk.bookurl.StartsWith("1https://www.dongmanmanhua.cn"))
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
                        string isfinished =match5.Length>0?"连载中": "已完结";
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
                            comicid = bk.bookurl.Split('=').LastOrDefault(),
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
                else if (bk.bookurl.StartsWith("1http://www.u17.com"))
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
                        Regex reg3 = new Regex("id=\"words\">(?<key1>.*?)<a");
                        Match match3 = reg3.Match(bookdata);
                        string bookdesc = match3.Groups["key1"].Value;
                        Regex reg4 = new Regex("状态：(?<key1>.*?)<span(?<key2>.*?)>(?<key3>.*?)</span>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match match4 = reg4.Match(bookdata);
                        string isfinished = match4.Groups["key3"].Value.Trim();
                        Regex reg5 = new Regex("txt_ico_up");
                        Match match5 = reg5.Match(bookdata);

                        string theme = "";
                        string pattern = "class=\"label(?<key1>.*?)\" title=\"(?<key3>.*?)\">(?<key2>.*?)</a>";
                        MatchCollection match6 = Regex.Matches(bookdata, pattern);
                        if (match6.Count==0)
                        {

                            pattern = "class=\"(?<key1>.*?)_tag \" id=\"tag_(?<key3>.*?)\">(?<key2>.*?)</a>";
                            match6 = Regex.Matches(bookdata, pattern);
                        }
                        
                        for (int j = 0; j < match6.Count; j++)
                        {
                            if (j==0)
                            {
                                theme = match6[j].Groups["key2"].Value.Trim();
                            }
                            else
                            {
                                theme = theme +","+ match6[j].Groups["key2"].Value.Trim();
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
                            comicid =(int)Source.U17+"_"+ bk.bookurl.Split('/').LastOrDefault().Replace(".html",""),
                            
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
                        Regex reg1 = new Regex("<meta property=\"og: novel:book_name\" content=\"(?<key1>.*?)\">");
                        Match match1 = reg1.Match(bookdata);
                        string comicname = match1.Groups["key1"].Value;
                       

                        Regex reg2 = new Regex("<meta property=\"og: novel:author\" content=\"(?<key1>.*?)\">");
                        Match match2 = reg2.Match(bookdata);
                        string authorname = match2.Groups["key1"].Value;
                        Regex reg3 = new Regex("id=\"words\">(?<key1>.*?)<a");
                        Match match3 = reg3.Match(bookdata);
                        string bookdesc = match3.Groups["key1"].Value;
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
            }
            if (comiclst.Count>0)
            {
                
                dbcontext.BulkInsert(comiclst);
            }
        }
    }
}
