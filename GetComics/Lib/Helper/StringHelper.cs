using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Helper
{
    public static class StringHelper
    {
        #region 字符串中多个连续空格转为一个空格  
        /// <summary>  
        /// 字符串中多个连续空格转为一个空格  
        /// </summary>  
        /// <param name="str">待处理的字符串</param>  
        /// <returns>合并空格后的字符串</returns>  
        public static string MergeSpace(string str)
        {
            if (str != string.Empty &&
                str != null &&
                str.Length > 0
                )
            {
                str = new System.Text.RegularExpressions.Regex("[\\s]+").Replace(str, " ");
            }
            return str;
        }


        #endregion


        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }
    }
}
