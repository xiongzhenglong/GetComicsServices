//using Chloe;
//using Chloe.SqlServer;
//using CrawerEnum;
//using Entity;
//using Framework.Common.Extension;
//using Lib.Helper;
//using log4net;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Web;

//namespace Crawer.Jobs
//{
//    [DisallowConcurrentExecution]
//    public class Mh160_Page_Job:IJob
//    {
//        private static readonly ILog logger = LogManager.GetLogger(typeof(Mh160_Page_Job));
//        MsSqlContext dbcontext;
//        static HttpWebHelper _helper = new HttpWebHelper();
//        public Mh160_Page_Job()
//        {
//            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
//        }

//        public void Execute(IJobExecutionContext context)
//        {
//            string keykey = "L21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDEuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDIuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDMuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDQuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDUuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDYuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDcuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDguanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMDkuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTAuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTEuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTIuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTMuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTQuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTUuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTYuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTcuanBnJHFpbmd0aWFuZHkkL21oMTYwdHVrdS9qL+ebkeeLseWtpuWbrV8xMTIzMi/nrKwyNzHor51fNTY5NjEzLzAwMTguanBn";
            
//            DateTime dt = DateTime.Now;
//            string shortdate = dt.ToString("yyyy-MM-dd");
//            string yesterday = dt.AddDays(-1).ToString("yyyy-MM-dd");

//            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
//            List<Chapter> cplst = cpq.Where(x => x.source == Source.mh160 && x.downstatus == DownChapter.待处理链接).Take(200).ToList();

//            List<int> ids = cplst.Select(x => x.Id).ToList();
//            dbcontext.Update<Chapter>(a => ids.Contains(a.Id), a => new Chapter()
//            {
//                downstatus = DownChapter.处理完链接,
//                modify = dt
//            });
//            List<Chapter> chapterlst = new List<Chapter>();
//            foreach (var cp in cplst)
//            {
//                try
//                {

                    
//                    var imgdata = _helper.Get(cp.chapterurl, Encoding.GetEncoding("gb2312"));
//                    Regex picTree = new Regex("var picTree =\"(?<key1>.*?)\"");
//                    var picTreekey = picTree.Match(imgdata).Groups["key1"].Value;
//                    string outstr = DecodeHelper.DecodeBase64(Encoding.UTF8, keykey);
//                    List<string> piclst = outstr.Replace("$qingtiandy$", ",").Split(',').ToList();
//                    List<Page> pglst = new List<Page>();
//                    int imgcount = piclst.Count;
                  
                  
//                    for (int i = 0; i < imgcount; i++)
//                    {
//                        //string pgsource = "http://mhpic." + _domain + "/comic/" + imgdecodepath + (i + 1) + ".jpg" + _comic_size;
//                        pglst.Add(new Page()
//                        {
//                            pagesource = pgsource,
//                            chapterid = cp.chapterid,
//                            modify = dt,
//                            shortdate = shortdate,
//                            sort = i + 1,
//                            source = cp.source,
//                            pagelocal = "",
//                        });
//                    }

//                    dbcontext.BulkInsert(pglst);


//                }
//                catch (Exception ex)
//                {
//                    logger.Error(ex.Message);
//                    cp.downstatus = DownChapter.待处理链接;
//                    cp.modify = dt;
//                    dbcontext.Update(cp);
//                    Err_ChapterJob err = new Err_ChapterJob();
//                    err.bookurl = cp.chapterurl;
//                    err.source = cp.source;
//                    err.errtype = ErrChapter.解析出错;
//                    err.modify = dt;
//                    err.shortdate = shortdate;
//                    err.message = ex.Message;
//                    err = dbcontext.Insert(err);
//                    continue;
//                }
//            }
//        }
//    }
//}
