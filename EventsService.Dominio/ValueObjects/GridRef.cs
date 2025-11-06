using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.ValueObjects
{
    public class GridRef
    {
        public int StartRow { get; set; }   // 1-based
        public int StartCol { get; set; }   // 1-based
        public int RowSpan { get; set; }   // >=1
        public int ColSpan { get; set; }   // >=1
    }
}
