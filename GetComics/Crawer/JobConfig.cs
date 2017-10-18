using Autofac;
using Autofac.Extras.Quartz;
using Framework.Common.Extension;
using log4net;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Topshelf.Quartz;
using Topshelf.ServiceConfigurators;


namespace Crawer
{
    public interface IService
    {
    }
    public class JobConfig
    {
        private static ILog log = LogManager.GetLogger(typeof(JobConfig));
        public static readonly string JobFile = "jobs.xml";
        public static readonly string ServiceName;
        public static readonly string JobNamespceFormat;
        public static readonly List<Job> JobList;

        static JobConfig()
        {
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, JobFile);
            var xml = string.Empty;
            using (var reander = File.OpenText(xmlPath))
            {
                xml = reander.ReadToEnd();
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Windows 服务名称
            ServiceName = doc.SelectSingleNode("quartz/service").InnerText;
            // JOB 命名空间
            JobNamespceFormat = doc.SelectSingleNode("quartz/namespace").InnerText;
            // JOBS
            var nodes = doc.SelectNodes("quartz/jobs/job");
            JobList = new List<Job>();
            foreach (var node in nodes)
            {
                var el = (XmlElement)node;
                JobList.Add(new Job(el.GetAttribute("name"), el.InnerText));
            }

            log.Info("Jobs.xml 初始化完毕：\r\n" + JobList.ToJson());
        }

        internal static ContainerBuilder ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new QuartzAutofacFactoryModule());
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(JobConfig).Assembly));
            builder.RegisterType<JobService>().AsSelf();


            var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var files = Directory.GetFiles(execDir, "QD.*.dll", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                var assemblies = new Assembly[files.Length];
                for (int i = 0; i < files.Length; i++)
                    assemblies[i] = Assembly.LoadFile(files[i]);

                builder.RegisterAssemblyTypes(assemblies)
                  .Where(t => t.GetInterfaces().Contains(typeof(IService)))
                  .AsSelf()
                  .InstancePerLifetimeScope();
            }





            return builder;
        }

        public static void Schedule(IContainer container, ServiceConfigurator<JobService> svc)
        {
            try
            {
                svc.UsingQuartzJobFactory(() => container.Resolve<IJobFactory>());
            }
            catch (Exception)
            {

                throw;
            }

            foreach (var job in JobList)
            {
                svc.ScheduleQuartzJob(q =>
                {
                    q.WithJob(JobBuilder.Create(Type.GetType(JobNamespceFormat.Fmt(job.Name)))
                        .WithIdentity(job.Name, ServiceName)
                        .Build);

                    q.AddTrigger(() => TriggerBuilder.Create()
                        .WithCronSchedule(job.Cron)
                        .Build());

                    log.InfoFormat("任务 {0} 已完成调度设置", JobNamespceFormat.Fmt(job.Name));
                });
            }
        }
    }

    public class Job
    {
        public Job(string name, string cron)
        {
            this.Name = name;
            this.Cron = cron;
        }

        public string Name { get; set; }
        public string Cron { get; set; }
    }

    public class JobService
    {
        private static ILog log = LogManager.GetLogger(typeof(JobService));

        public bool Start()
        {
            log.Info("服务已启动");
            return true;
        }

        public bool Stop()
        {
            log.Info("服务已关闭");
            return false;
        }
    }
}
