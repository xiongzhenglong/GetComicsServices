using Chloe;
using Chloe.Infrastructure.Interception;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Crawer.Api
{
    class DbCommandInterceptor : IDbCommandInterceptor
    {
        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result.FieldCount);
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }


        static void AmendParameter(IDbCommand command)
        {
            //foreach (var parameter in command.Parameters)
            //{
            //    if (parameter is OracleParameter)
            //    {
            //        OracleParameter oracleParameter = (OracleParameter)parameter;
            //        if (oracleParameter.Value is string)
            //        {
            //            /* 针对 oracle 长文本做处理 */
            //            string value = (string)oracleParameter.Value;
            //            if (value != null && value.Length > 4000)
            //            {
            //                oracleParameter.OracleDbType = OracleDbType.NClob;
            //            }
            //        }
            //    }
            //}
        }

        public static string AppendDbCommandInfo(IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IDbDataParameter param in command.Parameters)
            {
                if (param == null)
                    continue;

                object value = null;
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value;

                    if (param.DbType == DbType.String || param.DbType == DbType.AnsiString || param.DbType == DbType.DateTime)
                        value = "'" + value + "'";
                }

                sb.AppendFormat("{3} {0} {1} = {2};", Enum.GetName(typeof(DbType), param.DbType), param.ParameterName, value, Enum.GetName(typeof(ParameterDirection), param.Direction));
                sb.AppendLine();
            }

            sb.AppendLine(command.CommandText);

            return sb.ToString();
        }
    }
    public class BookStoreController : Controller
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BookStoreController));
        private const string bookstoretoken = "MYGEc6789oPTmXfuiKlpKUYE";
        MsSqlContext dbcontext;
        public BookStoreController()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        /// <summary>
        /// 获取分页漫画信息列表
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageNum"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET: BookStore
        [Route("bookstore/book/{source}/{pageNum}/{pageSize}")]
        public JsonResult book(int source ,int pageNum, int pageSize)
        {
            BookResult br = new BookResult();
            IQuery<Comic> q = dbcontext.Query<Comic>();
            var token = Request.Headers["X-AuthToken"];
            if (bookstoretoken != token)
            {
                br.code = 403;
                br.message = "无数据访问权限";
                return Json(br, JsonRequestBehavior.AllowGet);
            }
            try
            {
                List<Comic> lst = q.Where(x => x.source == (Source)source).TakePage(pageNum, pageSize).ToList();
                List<Book> bookList = new List<Book>();
                lst.ForEach(x =>
                {
                    bookList.Add(new Book()
                    {
                        bookid = x.comicid,
                        bookname = x.comicname,
                        authorname = x.authorname
                    });
                });

                br.code = 200;
                br.message = "漫画信息获取成功";
               
                br.result = bookList;
            }
            catch (Exception ex)
            {
                br.code = 500;
                br.message = "漫画分页信息获取失败";
            }


            return Json(br, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取指定漫画详情信息
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bookId"></param>
        /// <returns></returns>
        [Route("bookstore/bookinfo/{source}/{bookId}")]
        public JsonResult bookInfoResult(int source,string bookId)
        {
            
            BookInfoResult bir = new BookInfoResult();
            IQuery<Comic> q = dbcontext.Query<Comic>();
            IQuery<Chapter> cq = dbcontext.Query<Chapter>();
            var token = Request.Headers["X-AuthToken"];
            if (bookstoretoken != token)
            {
                bir.code = 403;
                bir.message = "无数据访问权限";
                return Json(bir, JsonRequestBehavior.AllowGet);
            }
            try
            {
                
                Comic comic = q.Where(x =>x.source==(Source)source &&  x.comicid == bookId).FirstOrDefault();
                List<Chapter> chapterlst = cq.Where(x =>x.source==(Source)source &&  x.comicid == bookId).ToList();
                var cplst2 = chapterlst.Where(x => x.downstatus != DownChapter.上传完图片).OrderBy(x => x.sort).FirstOrDefault();
                if (cplst2 != null)
                {
                    chapterlst = chapterlst.Where(x => x.sort < cplst2.sort).ToList();
                }
                bir.result = new BookInfo()
                {
                    bookname = comic.comicname,
                    alias = comic.comicname,
                    authorname = comic.authorname,
                    category = comic.theme,
                    intro = comic.comicdesc,
                    bookpic = comic.comiccoverlocal,
                    maxfreecount = 0,
                    cartoontype = 0,
                    fullflag = comic.isfinished == "连载中" ? 0 : 1,
                    chaptercount = chapterlst.Count,
                    region = 5,
                    iswhole = 0,
                    price = 0,
                    bookid = comic.comicid

                };
                bir.code = 200;
                bir.message = "漫画详情获取成功";
            }
            catch (Exception ex)
            {
                bir.code = 500;
                bir.message = "漫画详情获取失败";
            }
            return Json(bir, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取漫画的章节信息
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bookId"></param>
        /// <returns></returns>
        [Route("bookstore/bookchapter/{source}/{bookId}")]
        public JsonResult bookChapterResult(int source,string bookId)
        {
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            dbcontext.Session.AddInterceptor(interceptor);
            BookChapterResult bcr = new BookChapterResult();
            IQuery<Chapter> cq = dbcontext.Query<Chapter>();
            IQuery<Page> pq = dbcontext.Query<Page>();
            var token = Request.Headers["X-AuthToken"];
            if (bookstoretoken != token)
            {
                bcr.code = 403;
                bcr.message = "无数据访问权限";
                return Json(bcr, JsonRequestBehavior.AllowGet);
            }

            try
            {
                List<BookChapter> bookChapters = new List<BookChapter>();
                List<Chapter> chapterList = cq.Where(x => x.source == (Source)source && x.comicid == bookId).OrderBy(x=>x.sort).ToList();
                var cplst2 = chapterList.Where(x => x.downstatus != DownChapter.上传完图片).OrderBy(x => x.sort).FirstOrDefault();
                if (cplst2 != null)
                {
                    chapterList = chapterList.Where(x => x.sort < cplst2.sort).ToList();
                }
                List<string> chapterids = chapterList.Select(x => x.chapterid).ToList();
               
                chapterList.ForEach(x =>
                {
                    bookChapters.Add(new BookChapter()
                    {
                        chapterid = x.chapterid,
                        chaptername = x.chaptername,
                        chaptercover = x.chapterlocal,
                        chapterorder = x.sort,
                        vip = 0,
                        volumename ="",
                       

                    });
                });
                bcr.result = bookChapters;
            }
            catch (Exception)
            {

                bcr.code = 500;
                bcr.message = "获取指定漫画的漫画章节信息失败";
            }

            return Json(bcr, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取指定漫画章节详情信息
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bookId"></param>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        [Route("bookstore/chapterinfo/{source}/{bookId}/{chapterId}")]
        public JsonResult bookChapterInfoResult(int source,string bookId,string chapterId)
        {
            BookChapterInfoResult bcir = new BookChapterInfoResult();
            IQuery<Chapter> cq = dbcontext.Query<Chapter>();
            IQuery<Page> pq = dbcontext.Query<Page>();
            var token = Request.Headers["X-AuthToken"];
            if (bookstoretoken != token)
            {
                bcir.code = 403;
                bcir.message = "无数据访问权限";
                return Json(bcir, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Chapter chapter = cq.Where(x => x.source == (Source)source && x.chapterid == chapterId).FirstOrDefault();
                List<BookChapterInfo> chapterScenes = new List<BookChapterInfo>();
                List<Page> pglst = pq.Where(x => x.source == (Source)source && x.chapterid == chapter.chapterid && x.pagelocal != "").OrderBy(x=>x.sort).ToList();
                pglst.ForEach(x =>
                {
                    chapterScenes.Add(new BookChapterInfo()
                    {
                        pictureurl = x.pagelocal,                        
                        pictureorder = x.sort,
                        
                    });
                });
                bcir.code = 200;
                bcir.message = "指定漫画章节详情获取成功";
               
                bcir.result = chapterScenes;
            }
            catch (Exception ex)
            {

                bcir.code = 500;
                bcir.message = "指定漫画章节详情获取失败";
            }

            return Json(bcir, JsonRequestBehavior.AllowGet);
        }
    }

    #region 与书库对应的实体
    public class Book
    {
        /// <summary>
        /// 漫画id
        /// </summary>
        public string bookid { get; set; }
        /// <summary>
        /// 漫画名称
        /// </summary>

        public string bookname { get; set; }

        /// <summary>
        /// 作者名称
        /// </summary>
        public string authorname { get; set; }
    }

    public class BookResult
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 书单列表
        /// </summary>
        public List<Book> result { get; set; }
    }


    public class BookInfoResult
    {
        /// <summary>
        /// 书单详情
        /// </summary>
        public BookInfo result { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string message { get; set; }
    }

    public class BookInfo
    {
        public string bookid { get; set; }
        /// <summary>
        /// 书名
        /// </summary>
        public string bookname { get; set; }

        /// <summary>
        /// 书的别名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 作者名称
        /// </summary>
        public string authorname { get; set; }


        /// <summary>
        /// 完结状态 1完结 0连载
        /// </summary>
        public int fullflag { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string category { get; set; }

        /// <summary>
        /// 关键字、标签
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 漫画篇幅 0长篇 1短篇 2四个漫画 3单幅 4条漫
        /// </summary>
        public int cartoontype { get; set; }

        /// <summary>
        /// 地区 0日本 1韩国 2美国 3欧洲 4其他 5中国
        /// </summary>
        public int region { get; set; }

        /// <summary>
        /// 章节总数
        /// </summary>
        public int chaptercount { get; set; }

        /// <summary>
        /// 免费章节数
        /// </summary>
        public int maxfreecount { get; set; }


        /// <summary>
        /// 1 表示按本计费  0表示默认值、按章计费（没有默认按章计费）
        /// </summary>

        public int iswhole { get; set; }

        /// <summary>
        /// 按本收费价格，当按本计费时候有效
        /// </summary>
        public double price { get; set; }

     

        /// <summary>
        /// 图书简介
        /// </summary>
        public string intro { get; set; }

        /// <summary>
        /// 图片封面
        /// </summary>
        public string bookpic { get; set; }
       

       
    }

    /// <summary>
    /// 漫画章节结果
    /// </summary>
    public class BookChapterResult
    {
        /// <summary>
        /// 章节列表
        /// </summary>
        public List<BookChapter> result { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string message { get; set; }
    }


    public class BookChapter
    {

        /// <summary>
        /// 章卷名（注：没分卷，可用 "正文"代替)
        /// </summary>
        public string volumename { get; set; }

        /// <summary>
        /// 章节序号
        /// </summary>
        public int chapterorder { get; set; }

        /// <summary>
        /// 0免费 1收费
        /// </summary>
        public int vip { get; set; }

        /// <summary>
        /// 章节id
        /// </summary>
        public string chapterid { get; set; }

        /// <summary>
        /// 章节名称
        /// </summary>
        public string chaptername { get; set; }


        /// <summary>
        /// 章节封面URL
        /// </summary>
        public string chaptercover { get; set; }



       
 

    }

    /// <summary>
    /// 漫画章节具体信息结果实体
    /// </summary>
    public class BookChapterInfoResult
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 状态信息
        /// </summary>
        public string message { get; set; }

       

        /// <summary>
        /// 章节图片列表
        /// </summary>
        public List<BookChapterInfo> result { get; set; }
    }

    /// <summary>
    /// 漫画章节详情
    /// </summary>
    public class BookChapterInfo
    {
        /// <summary>
        /// 图片完整路径 http://cdn.icomico.com/77_3_1_cc24a991197a100e.jpg
        /// </summary>
        public string pictureurl { get; set; }

        /// <summary>
        /// 图片序号
        /// </summary>
        public int pictureorder { get; set; }
    }

    #endregion
}