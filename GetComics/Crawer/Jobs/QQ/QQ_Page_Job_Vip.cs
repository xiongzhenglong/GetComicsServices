using ApiModel.QQ;
using Chloe;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawer.Jobs
{
    [DisallowConcurrentExecution]
    public class QQ_Page_Job_Vip : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QQ_Page_Job_Vip));
        MsSqlContext dbcontext;
        static HttpWebHelper _helper = new HttpWebHelper();
        public QQ_Page_Job_Vip()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
          

            Dictionary<string, string> qbr = new Dictionary<string, string>()
            {
                {"tokenKey","6po9oJ4vSkwBlc5QjdZ58aGlhZIgJaW0D6Nf5BCTYHTDCwesNCedNa/Mw6lNeqILzFXoF46zIJu6tv5mEt+HgRPd/2mqaM0pQtEHGjQb/yg=" },
                {"comic_id","520392" },
                {"chapter_id","99" },
                {"buy_type","1" },
                {"pay_type","1" },
                //{"uin","o3283360259" },
                //{"skey","@Yb26CdH3W" }
            };
          

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                {"Cookie","uin=o3283360259; skey=@Yb26CdH3W;" }
            };


            var rs = _helper.Post("http://ac.qq.com/Buy/buyComic", qbr, Encoding.GetEncoding("UTF-8"), Encoding.GetEncoding("UTF-8"), null,  "", null, "", headers, "application/x-www-form-urlencoded; charset=UTF-8");
        }
    }


}
