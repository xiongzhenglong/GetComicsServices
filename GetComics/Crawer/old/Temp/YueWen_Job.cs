using ApiModel.YueWen;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public class YueWen_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(YueWen_Job));
        private MsSqlContext dbcontext;
        private static HttpHelper _helper = new HttpHelper("http://ubook.3g.qq.com");

        public YueWen_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting()); ;
        }

        public async void Execute(IJobExecutionContext context)
        {
            //List<YueWenIm> bklst = ExcelHelper.Import3(@"C:\Users\Administrator\Desktop\阅文版权商下-未上架追书图书-2017-10-26.xlsx").ToList();
            //http://ubook.3g.qq.com/8/search?key=[民调局异闻录]迷 香
            //http://ubook.3g.qq.com/8/intro?bid=679523
            //List<YueWenIm> bklst = File.ReadAllLines(@"C:\Users\Administrator\Desktop\111111.csv",Encoding.GetEncoding("GB2312")).Select(y => y.Split(',')).Select(x => new { bookid = x[0].Replace("\"",""), bookname = x[1].Replace("\"", ""), authorname = x[2].Replace("\"", "") }).Select(x=> new YueWenIm(x.bookid,x.bookname,x.authorname)).ToList();
            IQuery<YueWen> yq = dbcontext.Query<YueWen>();

            List<YueWen> ywlst = new List<YueWen>();
            //bklst.ForEach(x =>
            //{
            //    ywlst.Add(new YueWen()
            //    {
            //        agreedid = x.agreedid,
            //        agreedment = x.agreedment,
            //        authorname = x.authorname,
            //        bookid = x.bookid,
            //        bookname = x.bookname,
            //        channelname = x.channelname,
            //        merchantname = x.merchantname,
            //        status = "0",
            //        maxfreecount = ""
            //    });
            //});

            //dbcontext.BulkInsert(ywlst, null, 360000);
            ywlst = yq.Where(x => x.status == "0").Take(200).ToList();
            List<int> idlst = ywlst.Select(x=>x.Id).ToList();

            dbcontext.Update<YueWen>(a => idlst.Contains(a.Id), a => new YueWen()
            {
                status = "1"
            });
          

            foreach (var bk in ywlst)
            {
                try
                {
                    Dictionary<string, string> paras = new Dictionary<string, string>();
                    paras.Add("key", bk.bookname);
                    var t = _helper.Get<YueWenApi>(paras, "8/search");
                    if (t.booklist.Count > 0)
                    {
                        Dictionary<string, string> paras2 = new Dictionary<string, string>();
                        paras2.Add("bid", t.booklist[0].id);
                        var t2 = _helper.Get<YueWenDetailApi>(paras2, "8/intro");
                        if (t2.book ==null )
                        {
                            continue;
                        }
                       
                        if (t2.book.author.Trim() == bk.authorname.Trim() && t2.book.title.Trim() == bk.bookname.Trim())
                        {
                            bk.bid = t.booklist[0].id;
                            bk.maxfreecount = t2.book.maxfreechapter;
                        }
                       
                        bk.status = "1";
                        dbcontext.Update(bk);
                    }
                }
                catch (Exception ex)
                {
                    bk.status = "0";
                    bk.bid = ex.Message;
                    dbcontext.Update(bk);
                    continue;
                }

                //logger.Error(bklst.IndexOf(bk));
            }

            //ExcelHelper.ToExcel(bklst);
        }

        
    }
}