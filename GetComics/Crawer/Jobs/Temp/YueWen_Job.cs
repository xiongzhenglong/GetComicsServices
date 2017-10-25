using ApiModel.YueWen;
using Chloe;
using Chloe.SqlServer;
using Entity;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;

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

        public void Execute(IJobExecutionContext context)
        {
            //List<YueWenIm> bklst = ExcelHelper.Import3(@"C:\Users\Administrator\Desktop\阅文版权商下-未上架追书图书-2017-10-19 - 副本.xlsx").ToList();
            //http://ubook.3g.qq.com/8/search?key=[民调局异闻录]迷 香
            //http://ubook.3g.qq.com/8/intro?bid=679523
            IQuery<YueWen> yq = dbcontext.Query<YueWen>();
            List<YueWen> ywlst = yq.Where(x => x.status == "0").Take(200).ToList();

            //bklst.ForEach(x =>
            //{
            //    ywlst.Add(new YueWen()
            //    {
            //        authorname = x.authorname,
            //        bookid = x.bookid,
            //        bookname = x.bookname,
            //        channelname = x.channelname,
            //        hetongid = x.合同id,
            //        hetongname = x.合同名称,
            //        remark = "",
            //        status = "0"
            //    });
            //});

            //dbcontext.BulkInsert(ywlst);

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
                        if (t2.book == null)
                        {
                            if (t.booklist[0].author.Trim() == bk.authorname.Trim() && t.booklist[0].title.Trim().Replace("<b>", "").Replace("</b>", "") == bk.bookname.Trim())
                            {
                                bk.remark = t.booklist[0].id;
                            }
                        }
                        else
                        {
                            if (t2.book.author.Trim() == bk.authorname.Trim() && t2.book.title.Trim() == bk.bookname.Trim())
                            {
                                bk.remark = t.booklist[0].id;
                            }
                        }
                        bk.status = "1";
                        dbcontext.Update(bk);
                    }
                }
                catch (Exception ex)
                {
                    bk.remark = ex.Message;
                    dbcontext.Update(bk);
                    continue;
                }
               
                //logger.Error(bklst.IndexOf(bk));
            }

            //ExcelHelper.ToExcel(bklst);
        }
    }
}