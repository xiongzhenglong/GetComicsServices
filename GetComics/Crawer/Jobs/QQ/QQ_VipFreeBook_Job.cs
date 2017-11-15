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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public class QQ_VipFreeBook_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_VipFreeBook_Job));
        MsSqlContext dbcontext;
        static HttpHelper _helper = new HttpHelper("http://ac.qq.com");
        public QQ_VipFreeBook_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string shortdate = dt.ToString("yyyy-MM-dd");
                var bookdata = _helper.Get(null, "http://ac.qq.com/Comic/all/search/time/vip/3/page/1");
                IQuery<VIPFreeComic> vfcq = dbcontext.Query<VIPFreeComic>();
                string pattern_page = "共<em>(?<key1>.*?)</em>个结果";
                MatchCollection matches_page = Regex.Matches(bookdata, pattern_page, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                int pagecount = (int)Math.Ceiling(int.Parse(matches_page[0].Groups[1].Value) / 12.0);
                List<VIPFreeComic> vfclst = new List<VIPFreeComic>();
                for (int i = 1; i <= pagecount; i++)
                {
                    var bookdata2 = _helper.Get(null, "http://ac.qq.com/Comic/all/search/time/vip/3/page/" + i);
                    bookdata2 = StringHelper.MergeSpace(bookdata2);
                    string pattern2 = "<h3 class=\"ret-works-title clearfix\"> <a href=\"(?<key1>.*?)\" target=\"_blank\" title=\"(?<key2>.*?)\">(?<key3>.*?)</a>";
                    MatchCollection matches2 = Regex.Matches(bookdata2, pattern2, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    for (int j = 0; j < matches2.Count; j++)
                    {
                        string bookurl = "http://ac.qq.com" + matches2[j].Groups["key1"].Value;
                        string comicname = matches2[j].Groups["key2"].Value;
                        string comicid = (int)Source.QQ+"_"+ bookurl.Split('/').LastOrDefault();


                        vfclst.Add(new VIPFreeComic()
                        {
                            bookurl = bookurl,
                            comicid = comicid,
                            comicname = comicname,
                            modify = dt,
                            shortdate = shortdate,
                            source = Source.QQ      
                        });
                    }
                }
                List<VIPFreeComic> vfcdblst = vfcq.Where(x => x.source == Source.QQ).ToList();
                if (vfclst.Count>0)
                {
                    List<VIPFreeComic> delete = vfcdblst.Except(vfclst, new VIPFreeComic_Comparer()).ToList(); 
                    List<VIPFreeComic> add = vfclst.Except(vfcdblst, new VIPFreeComic_Comparer()).ToList();  
                    if (delete.Count>0)
                    {
                        List<string> deleteidlst = delete.Select(x => x.comicid).ToList();
                        dbcontext.Delete<VIPFreeComic>(x => deleteidlst.Contains(x.comicid));
                    }
                    if (add.Count>0)
                    {
                        dbcontext.BulkInsert(add);
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }
    }
}
