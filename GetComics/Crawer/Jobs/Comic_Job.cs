using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using ExcelModel;
using Framework.Common.Extension;
using Lib.Helper;
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
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://ac.qq.com");
        public Comic_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());

        }

        public void Execute(IJobExecutionContext context)
        {
            List<bkIm> bklst = ExcelHelper.Import(@"C:\ComicCrawer\GetComics\Excel\漫画抓取信息.xlsx").ToList();
            List<Comic> comiclst = new List<Comic>();
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            foreach (var bk in bklst)
            {
                if (bk.bookurl.StartsWith("http://ac.qq.com"))
                {
                    try
                    {
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
                            comiccover = comiccover,
                            comicdesc = bookdesc,
                            comicid = bk.bookurl.Split('/').LastOrDefault(),
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
            }
            if (comiclst.Count>0)
            {
               
                dbcontext.BulkInsert(comiclst);
            }
        }
    }
}
