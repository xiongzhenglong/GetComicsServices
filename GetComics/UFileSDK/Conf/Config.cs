using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFileSDK
{
    public class Config
    {
        public static string VERSION = @"1.0.2";

        /// <summary>
        /// UCloud管理服务器地址后缀
        /// </summary>
        public static string UCLOUD_PROXY_SUFFIX = @".cn-bj.ufileos.com";

        /// <summary>
        /// UCloud提供的公钥
        /// </summary>
        public static string UCLOUD_PUBLIC_KEY = @"7QBRSRQAKySMJNNGWuWfJTDNtzXhDkOePNA+qgY2VAJEXlfp";

        /// <summary>
        /// UCloud提供的密钥
        /// </summary>
        public static string UCLOUD_PRIVATE_KEY = @"0aabf7602d46092ed501107415234e09eeca4d18";

        public static string UCLOUD_BUCKET = @"skmh";


        public static string GetUserAgent()
        {
            return @"UCloudCSharp/" + VERSION;
        }

        public static void Init()
        {
            //if (System.Configuration.ConfigurationManager.AppSettings["PUBLIC_KEY"] != null)
            //{
            //    UCLOUD_PUBLIC_KEY = System.Configuration.ConfigurationManager.AppSettings["PUBLIC_KEY"];
            //}
            //if (System.Configuration.ConfigurationManager.AppSettings["PRIVATE_KEY"] != null)
            //{
            //    UCLOUD_PRIVATE_KEY = System.Configuration.ConfigurationManager.AppSettings["PRIVATE_KEY"];
            //}
            //if (System.Configuration.ConfigurationManager.AppSettings["PUBLIC_KEY"] != null)
            //{
            //    UCLOUD_PROXY_SUFFIX = System.Configuration.ConfigurationManager.AppSettings["PROXY_SUFFIX"];
            //}
        }

    }
}
