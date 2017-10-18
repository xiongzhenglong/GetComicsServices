using Chloe.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class BaseEntity
    {
        //public BaseEntity()
        //{
        //    Id = Guid.NewGuid().ToString("N");
        //}
        [AutoIncrement]
        public int Id { get; set; }
    }
}
