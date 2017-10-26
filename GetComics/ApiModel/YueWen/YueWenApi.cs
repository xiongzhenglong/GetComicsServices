using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModel.YueWen
{

    public class YueWenApi
    {
        
        public List<YueWenApi_Book> booklist { get; set; }
        
    }

    public class YueWenApi_Book
    {
       
        public string id { get; set; }
  
        public string author { get; set; }
        public string title { get; set; }
       
    }


    public class YueWenDetailApi
    {
      
       
        public Book book { get; set; }
 
      
    }

   

    public class Book
    {
        public string author { get; set; }
     


        public string id { get; set; }
    
      
        public string title { get; set; }

        public string maxfreechapter { get; set; }


    }

   


}
