using System;
using Umbraco.Core.Cache;

namespace Skybrud.Umbraco.Domains.Caching {
    
    public class DomainsCacheRefresher : JsonCacheRefresherBase<DomainsCacheRefresher> {

        public static readonly Guid CacheRefresherId = new Guid("4fb60fdf-db99-4222-ae1d-346268d9a7d4");

        protected override DomainsCacheRefresher Instance {
            get { return this; }
        }

        public override Guid UniqueIdentifier {
            get { return CacheRefresherId; }
        }

        public override string Name {
            get { return "Skybrud Domains Cache"; }
        }
    
    }

}