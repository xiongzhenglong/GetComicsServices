using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFileSDK;

namespace Lib.Helper
{
    public static class UcHelper
    {
        public static string uploadFile(string key, string fullFileName)
        {
            Proxy.PutFileV2(Config.UCLOUD_BUCKET, key, fullFileName);
            //return String.Format("http://{0}.ufile.ucloud.com.cn/{1}", Config.UCLOUD_BUCKET, key);
            return String.Format("http://{0}.cn-bj.ufileos.com/{1}", Config.UCLOUD_BUCKET, key);
        }
        public static void DeleteFileV2(string key)
        {
            Proxy.DeleteFileV2(Config.UCLOUD_BUCKET, key);
        }


    }
}
