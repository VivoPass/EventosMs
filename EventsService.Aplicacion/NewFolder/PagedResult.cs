using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Aplicacion.NewFolder
{
    // Application/Common/Models/PagedResult.cs
    public record PagedResult<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);

}
