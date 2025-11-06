using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.DTOs.Zonas
{
    public record GridRefBlock(int StartRow, int StartCol, int RowSpan, int ColSpan);
}
