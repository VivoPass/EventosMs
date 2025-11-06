using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public class GridDto
    {
        public int StartRow { get; set; }
        public int StartCol { get; set; }
        public int RowSpan { get; set; }
        public int ColSpan { get; set; }
        public string? Color { get; set; }
        public int? ZIndex { get; set; }
        public bool Visible { get; set; } = true;
    }
}
