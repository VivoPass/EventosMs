using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Infrastructura.Settings
{
    public sealed class MongoSettings
    {
        public string ConnectionString { get; init; } = "mongodb://localhost:27017";
        public string Database { get; init; } = "Events";
    }
}
