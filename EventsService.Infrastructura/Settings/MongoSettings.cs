using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Infrastructura.Settings
{
    public sealed class MongoSettings
    {
        public string ConnectionString { get; init; } = "mongodb+srv://maibarra21_db_user:DSW2025EduMari2@clusterdsw2025-2.hhle8dc.mongodb.net/?appName=ClusterDSW2025-2";
        public string Database { get; init; } = "BDEventos";
    }
}
