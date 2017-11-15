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


    public class VIPFreeComic_Comparer : IEqualityComparer<VIPFreeComic>
    {
        public bool Equals(VIPFreeComic x,VIPFreeComic y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            return x != null && y != null && x.comicid == y.comicid;
        }

        public int GetHashCode(VIPFreeComic obj)
        {
            int hashchapterid = obj.comicid.GetHashCode();
            return hashchapterid;
        }
    }
}
