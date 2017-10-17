using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lib.Helper
{
    public class DecodeHelper
    {
      
        public static string QQPageDecode(string data)
        {
            string _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            var a = "";
            var e = 0;
            var b = 0;
            var d = 0;
            var f = 0;
            var g = 0;
            while (true)
            {
                if (data.Length > e)
                {
                    data = Regex.Replace(data, @"[^A-Za-z0-9\+\/\=]", "");
                    if (data.Length > e)
                    {
                        b = _keyStr.IndexOf(data[e++]);
                    }
                    else
                    {
                        b = 0;
                    }
                    if (data.Length > e)
                    {
                        d = _keyStr.IndexOf(data[e++]);
                    }
                    else
                    {
                        d = 0;
                    }
                    if (data.Length > e)
                    {
                        f = _keyStr.IndexOf(data[e++]);
                    }
                    else
                    {
                        f = 0;
                    }
                    if (data.Length > e)
                    {
                        g = _keyStr.IndexOf(data[e++]);
                    }
                    else
                    {
                        g = 0;
                    }

                    b = b << 2 | d >> 4;
                    d = (d & 15) << 4 | f >> 2;
                    var h = (f & 3) << 6 | g;
                    a += (char)b;
                    if (f != 64)
                    {
                        a += (char)d;

                    }
                    if (g != 64)
                    {
                        a += (char)h;
                    }
                }
                else
                {
                    break;
                }

            }





            return QQ_UTF8(a);
        }

        public static string QQ_UTF8(string c)
        {
            var a = "";
            var b = 0;
            var d = 0;
            var c1 = 0;
            var c2 = 0;
            while (true)
            {
                if (b < c.Length)
                {
                    d = c[b];
                    if (128 > d)
                    {
                        a += (char)d;
                        b++;
                    }
                    else
                    {
                        if (191 < d && 224 > d)
                        {
                            c2 = c[b + 1];
                            a += (char)((d & 31) << 6 | c2 & 63);
                            b += 2;
                        }
                        else
                        {
                            if (b + 1 < c.Length && b + 2 < c.Length)
                            {
                                c2 = c[b + 1];
                                int c3 = c[b + 2];
                                a += (char)((d & 15) << 12 | (c2 & 63) << 6 | c3 & 63);
                                b += 3;
                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return a;
        }

        public static string Decode(string data, int pageid)
        {
            string a = "";
            int length = data.Length;
            string data2 = data;
            for (int i = 0; i < length; i++)
            {
                data = data2.Substring(i);
                a += (char)(data[0] - pageid % 10);
            }
            return a;
        }
    }
}
