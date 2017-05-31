using System;
using System.Text;
using Skybrud.Umbraco.Domains.Caching;
using Skybrud.Umbraco.Domains.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Skybrud.Umbraco.Domains.EventHandlers {
    
    internal class DistributedCacheEventHandlers : ApplicationEventHandler {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) {
            CacheRefresherBase<DomainsCacheRefresher>.CacheUpdated += OnCacheUpdated;
        }

        private void OnCacheUpdated(DomainsCacheRefresher sender, CacheRefresherEventArgs args) {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Skybrud Domains cache cleared by ICacheRefresher Event");
            sb.AppendLine("Type: " + args.MessageType);
            sb.AppendLine("Object: " + (args.MessageObject == null ? "NULL" : args.MessageObject + " (" + args.MessageObject.GetType() + ")") + "");
            sb.AppendLine();

            LogHelper.Info<DistributedCacheEventHandlers>(sb + "");

            switch (args.MessageType) {

                case MessageType.RefreshAll:
                    DomainsRepository.Current.RebuildCache();
                    break;

                case MessageType.RefreshById:
                    if (args.MessageObject is Guid) {
                        DomainsRepository.Current.RefreshDomainInCache((Guid)args.MessageObject);
                    } else if (args.MessageObject is Int32) {
                        DomainsRepository.Current.RefreshDomainInCache((int)args.MessageObject);
                    }
                    break;

                case MessageType.RemoveById:
                    if (args.MessageObject is Guid) {
                        DomainsRepository.Current.RemoveDomainFromCache((Guid)args.MessageObject);
                    } else if (args.MessageObject is Int32) {
                        DomainsRepository.Current.RemoveDomainFromCache((int)args.MessageObject);
                    }
                    break;

            }
        
        }
    
    }

}