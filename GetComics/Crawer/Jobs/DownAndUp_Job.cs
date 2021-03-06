﻿using Chloe;
using Chloe.Infrastructure.Interception;
using Chloe.SqlServer;
using CrawerEnum;
using Entity;
using Framework.Common.Extension;
using Lib.Helper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebPWrapper;

namespace Crawer.Jobs
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
    /// <summary>
    /// 下载并且上传
    /// </summary>
    [DisallowConcurrentExecution]
    public class DownAndUpPage_Job : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DownAndUpPage_Job));
        MsSqlContext dbcontext;
        //private static int pageIndex = 1;
        //private static int pageSize = 100;
        private static int m_DownTotal = 0;
        private static int m_DownIndex = 0;

        public DownAndUpPage_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
            string strDownTotal = "DownTotal".ValueOfAppSetting();
            string strDownIndex = "DownIndex".ValueOfAppSetting();
            if (strDownTotal != null && strDownIndex != null && int.TryParse(strDownTotal, out m_DownTotal) && int.TryParse(strDownIndex, out m_DownIndex))
            {
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            //IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            //dbcontext.Session.AddInterceptor(interceptor);
      
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");           
            IQuery<Page> cpq = dbcontext.Query<Page>();
            IQuery<Chapter> cq = dbcontext.Query<Chapter>();

            List<Page> plst;

            if (m_DownTotal > 0 )
            {
                plst = cpq.Where(a => a.pagestate == PageState.None && a.Id % m_DownTotal == m_DownIndex).Take(100).ToList();
            }
            else
                plst = cpq.Where(a => a.pagestate== PageState.None).Take(100).ToList();

            //if (plst.Count < pageSize)
            //    pageIndex = 1;
            //else
            //    pageIndex++;

            HttpWebHelper web = new HttpWebHelper();
            
            foreach (var p in plst)
            {
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "DownLoadImgs/" + p.Id + ".jpg";
                try
                {
                   
                    string refer = "";
                    if (p.source == Source.dongmanmanhua || p.source==Source.dmzj)
                    {
                        var chapter = cq.Where(x => x.chapterid == p.chapterid).FirstOrDefault();
                        refer = chapter.chapterurl;
                        Stream stream = web.GetStream(p.pagesource, 3000, "", null, refer, null);
                        Image img = Image.FromStream(stream);

                        stream.Close();

                        img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        WebClient myclient = new WebClient();
                        myclient.DownloadFile(p.pagesource, filePath);
                    }

                    string localimg = UcHelper.uploadFile("Page/"+p.Id + ".jpg", filePath);
                    p.pagelocal = localimg;
                    p.pagestate = PageState.成功;
                    p.modify = dt;
                    dbcontext.Update(p);
                  
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    try
                    {
                        WebClient myclient = new WebClient();
                        myclient.DownloadFile(p.pagesource, filePath);
                        Bitmap bytes;
                        using (WebP webp = new WebP())
                            bytes = webp.Load(filePath);
                        File.Delete(filePath);
                        bytes.Save(filePath);
                        string localimg = UcHelper.uploadFile("Page/" + p.Id + ".jpg", filePath);
                        p.pagelocal = localimg;
                        p.modify = dt;
                        p.pagestate = PageState.成功;
                        dbcontext.Update(p);
                      
                        File.Delete(filePath);
                        continue;
                    }
                    catch (Exception)
                    {

                        logger.Error(ex.Message);
                        Chapter chapter = cq.Where(x => x.chapterid == p.chapterid).FirstOrDefault();
                        chapter.retry = chapter.retry + 1;
                        chapter.modify = dt;
                        if (chapter.retry > 30)
                        {
                            p.pagestate = PageState.失败;
                            p.modify = dt;
                            dbcontext.Update(p);
                        }
                        dbcontext.Update(chapter);

                        //dbcontext.Update<Chapter>(a => a.chapterid == p.chapterid, a => new Chapter()
                        //{
                        //    retry = a.retry + 1,
                        //    modify = dt
                        //});
                        Err_PageJob err = new Err_PageJob();
                        err.imgurl = p.pagesource;
                        err.source = p.source;
                        err.errtype = ErrPage.限制访问;
                        err.modify = dt;
                        err.shortdate = shortdate;
                        err.message = ex.Message;
                        err = dbcontext.Insert(err);
                        continue;
                    }
                   
                    
                }
              
               
            }
        }
    }
    /// <summary>
    /// 下载并且上传
    /// </summary>
    [DisallowConcurrentExecution]
    public class DownAndUpComic_Job : IJob
    {
        MsSqlContext dbcontext;
        public DownAndUpComic_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            //dbcontext.Session.AddInterceptor(interceptor);
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Comic> cpq = dbcontext.Query<Comic>();
            List<Comic> plst = cpq.Where(a =>a.comiccoversource.Length!=0 && a.comiccoverlocal.Length == 0).TakePage(1, 20).ToList();
            HttpWebHelper web = new HttpWebHelper();
            foreach (var p in plst)
            {

                try
                {
                    string refer = "";
                    if (p.source == Source.dongmanmanhua || p.source == Source.dmzj)
                    {
                        
                        refer = p.bookurl;
                    }

                    Stream stream = web.GetStream(p.comiccoversource,300,"",null,refer,null);
                    Image img = Image.FromStream(stream);
                    stream.Close();
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "DownLoadImgs/" + p.Id + ".jpg";
                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    string localimg = UcHelper.uploadFile("Comic/"+p.Id + ".jpg", filePath);
                    p.comiccoverlocal = localimg;
                    p.modify = dt;
                    dbcontext.Update(p);
                }
                catch (Exception ex)
                {
                    Err_ComicJob err = new Err_ComicJob();
                    err.bookurl = p.bookurl;                   
                    err.errtype = ErrComic.图片出错;
                    err.modify = dt;
                    err.shortdate = shortdate;
                    err.message = ex.Message;
                    err = dbcontext.Insert(err);
                    continue;
                }


            }
        }
    }
    /// <summary>
    /// 下载并且上传
    /// </summary>
    [DisallowConcurrentExecution]
    public class DownAndUpChapter_Job : IJob
    {
        MsSqlContext dbcontext;
        public DownAndUpChapter_Job()
        {
            dbcontext = new MsSqlContext("Mssql".ValueOfAppSetting());
        }

        public void Execute(IJobExecutionContext context)
        {
            //IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            //dbcontext.Session.AddInterceptor(interceptor);
            DateTime dt = DateTime.Now;
            string shortdate = dt.ToString("yyyy-MM-dd");
            IQuery<Comic> comicq = dbcontext.Query<Comic>();
            IQuery<Chapter> cpq = dbcontext.Query<Chapter>();
            List<Chapter> plst = cpq.Where(a =>a.chaptersource.Length!=0 &&  a.chapterlocal.Length == 0).TakePage(1, 20).ToList();
            HttpWebHelper web = new HttpWebHelper();
            foreach (var p in plst)
            {

                try
                {
                    string refer = "";
                    if (p.source == Source.dongmanmanhua)
                    {
                        var comic = comicq.Where(x => x.comicid == p.comicid).FirstOrDefault();
                        refer = comic.bookurl;
                    }

                    Stream stream = web.GetStream(p.chaptersource,300,"",null,refer,null);
                    Image img = Image.FromStream(stream);
                    stream.Close();
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "DownLoadImgs/" + p.Id + ".jpg";
                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    string localimg = UcHelper.uploadFile("Chapter/" + p.Id + ".jpg", filePath);
                    p.chapterlocal = localimg;
                    p.modify = dt;
                    dbcontext.Update(p);
                }
                catch (Exception ex)
                {
                    Err_ChapterJob err = new Err_ChapterJob();
                    err.bookurl = p.chapterurl;
                    err.errtype = ErrChapter.图片出错;
                    err.modify = dt;
                    err.shortdate = shortdate;
                    err.message = ex.Message;
                    err = dbcontext.Insert(err);
                    continue;
                }


            }
        }
    }
}
