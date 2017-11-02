using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModel.U17
{

    public class U17_Page_Api
    {
        public int code { get; set; }
       
        public List<U17_Image_List> image_list { get; set; }
    }

   
    public class U17_Image_List
    {
        public string src { get; set; }
       
    }

}
