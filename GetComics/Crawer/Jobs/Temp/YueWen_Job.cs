using ApiModel.YueWen;
using ExcelModel;
using Lib.Helper;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public class YueWen_Job : IJob
    {
        private static HttpHelper _helper = new HttpHelper("http://ubook.3g.qq.com");
        public void Execute(IJobExecutionContext context)
        {
            List<YueWenIm> bklst = ExcelHelper.Import3(@"C:\Users\Administrator\Desktop\阅文版权商下-未上架追书图书-2017-10-19 - 副本.xlsx").ToList();
            //http://ubook.3g.qq.com/8/search?key=[民调局异闻录]迷 香
            //http://ubook.3g.qq.com/8/intro?bid=679523
            foreach (var bk in bklst)
            {
                Dictionary<string, string> paras = new Dictionary<string, string>();
                paras.Add("key", bk.bookname);
                var t  = _helper.Get<YueWenApi>(paras, "8/search");
                if (t.booklist.Count>0)
                {
                    Dictionary<string, string> paras2 = new Dictionary<string, string>();
                    paras2.Add("bid", t.booklist[0].id);
                    var t2 = _helper.Get<YueWenDetailApi>(paras2, "8/intro");
                    if (t2.book==null)
                    {
                        if (t.booklist[0].author.Trim() == bk.authorname.Trim() && t.booklist[0].title.Trim().Replace("<b>","").Replace("</b>","") == bk.bookname.Trim())
                        {
                            bk.备注信息 = t.booklist[0].id;
                        }
                    }
                    else
                    {
                        if (t2.book.author.Trim() == bk.authorname.Trim() && t2.book.title.Trim() == bk.bookname.Trim())
                        {
                            bk.备注信息 = t.booklist[0].id;
                        }
                    }
                    
                }
            }

            ExcelHelper.ToExcel(bklst);


           
        }
    }


}
