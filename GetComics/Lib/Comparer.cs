using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Chapter_Comparer : IEqualityComparer<Chapter>
    {
        public bool Equals(Chapter x, Chapter y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            return x != null && y != null && x.chapterid == y.chapterid;

        }

        public int GetHashCode(Chapter obj)
        {
            int hashchapterid = obj.chapterid.GetHashCode();
            return hashchapterid;
        }
    }
}
