using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModel.QQ
{
    public class QQ_Page_Api
    {
        public QQ_Page_Comic comic { get; set; }
        public QQ_Page_Chapter chapter { get; set; }
        public List<QQ_Page_Picture> picture { get; set; }
 
      
    }
    public class QQ_Page_Comic
    {
        public int id { get; set; }
        public string title { get; set; }
        public string collect { get; set; }
        public bool isJapanComic { get; set; }
        public bool isLightNovel { get; set; }
        public bool isLightComic { get; set; }
        public bool isFinish { get; set; }
        public bool isRoastable { get; set; }
        public string eId { get; set; }
    }
    public class QQ_Page_Chapter
    {
        public int cid { get; set; }
        public string cTitle { get; set; }
        public string cSeq { get; set; }
        public int vipStatus { get; set; }
        public int prevCid { get; set; }
        public int nextCid { get; set; }
        public int blankFirst { get; set; }
        public bool canRead { get; set; }
    }
  
  

    public class QQ_Page_Picture
    {
        public string pid { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string url { get; set; }
    }

}
