using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Skybrud.Umbraco.Domains.Models {
    
    public class DomainsRepository {
        
        private Dictionary<string, RedirectDomain> _lookup;

        #region Properties

        protected DomainsService Service { get; private set; }

        public bool HasCache {
            get { return _lookup != null; }
        }

        public static DomainsRepository Current {
            get { return (DomainsRepository) ApplicationContext.Current.ApplicationCache.StaticCache.GetCacheItem("Skybrud:DomainsRepository", () => new DomainsRepository(DomainsService.Current)); }
        }

        #endregion

        #region Constructors

        public DomainsRepository(DomainsService service) {
            Service = service;
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Invalidates the domain cache, forcing it to be updated the next time it is accessed.
        /// </summary>
        public void InvalidateCache() {
            _lookup = null;
        }

        /// <summary>
        /// Rebuilds the internal domain cache.
        /// </summary>
        public void RebuildCache() {
            _lookup = Service.GetAllDomains().ToDictionary(GetDomainKey);
        }

        public bool TryGetDomain(RedirectProtocol protocol, string domainName, out RedirectDomain domain) {
            int portNumber = protocol == RedirectProtocol.Http ? 80 : 443;
            return TryGetDomain(protocol, domainName, portNumber, out domain);
        }

        public bool TryGetDomain(RedirectProtocol protocol, string domainName, int portNumber, out RedirectDomain domain) {
            if (!HasCache) RebuildCache();
            return _lookup.TryGetValue(GetDomainKey(protocol, domainName, portNumber), out domain);
        }

        public bool TryGetDomain(Uri url, out RedirectDomain domain) {

            // Parse the inbound protocol
            RedirectProtocol protocol;
            switch (url.Scheme) {
                case "http":
                    protocol = RedirectProtocol.Http;
                    break;
                case "https":
                    protocol = RedirectProtocol.Https;
                    break;
                default:
                    domain = null;
                    return false;
            }

            // Use the method overload for looking up the domain in the cache
            return TryGetDomain(protocol, url.Host, url.Port, out domain);
        
        }

        /// <summary>
        /// Refreshes the domain with the specified <paramref name="domainId"/> in the database.
        /// </summary>
        /// <param name="domainId">The ID of the domain.</param>
        public void RefreshDomainInCache(int domainId) {

            LogHelper.Info<DomainsRepository>("RefreshDomainInCache - > " + domainId);

            // Remove the domain from the cache (eg. if the inbound values have changed)
            RemoveDomainFromCache(domainId);

            // Get the domain from the service
            RedirectDomain domain = Service.GetDomainById(domainId);

            // Return now if the domain wasn't found in the database
            if (domain == null) return;

            // Rebuild the entire cache if not already loaded
            if (!HasCache) {
                RebuildCache();
                return;
            }

            // Add/set the domain in the dictionary
            _lookup[GetDomainKey(domain)] = domain;

        }

        /// <summary>
        /// Refreshes the domain with the specified <paramref name="domainId"/> in the database.
        /// </summary>
        /// <param name="domainId">The unique ID of the domain.</param>
        public void RefreshDomainInCache(Guid domainId) {

            LogHelper.Info<DomainsRepository>("RefreshDomainInCache - > " + domainId);

            // Remove the domain from the cache (eg. if the inbound values have changed)
            RemoveDomainFromCache(domainId);

            // Get the domain from the service
            RedirectDomain domain = Service.GetDomainById(domainId);

            // Return now if the domain wasn't found in the database
            if (domain == null) return;

            // Rebuild the entire cache if not already loaded
            if (!HasCache) {
                RebuildCache();
                return;
            }

            // Add/set the domain in the dictionary
            _lookup[GetDomainKey(domain)] = domain;

        }

        /// <summary>
        /// Removes the domain with the specified <paramref name="domainId"/> from the database.
        /// </summary>
        /// <param name="domainId">The ID of the domain.</param>
        public void RemoveDomainFromCache(int domainId) {

            LogHelper.Info<DomainsRepository>("RemoveDomainFromCache - > " + domainId);

            // Just return if the cache isn't already loaded
            if (_lookup == null) return;

            // Loop through the items of the cache to delete matching domains
            foreach (KeyValuePair<string, RedirectDomain> pair in _lookup.ToArray()) {
                if (pair.Value.Id == domainId) {
                    _lookup.Remove(pair.Key);
                }
            }

        }

        /// <summary>
        /// Removes the domain with the specified <paramref name="domainId"/> from the database.
        /// </summary>
        /// <param name="domainId">The unique ID of the domain.</param>
        public void RemoveDomainFromCache(Guid domainId) {

            LogHelper.Info<DomainsRepository>("RemoveDomainFromCache - > " + domainId);

            // Just return if the cache isn't already loaded
            if (_lookup == null) return;

            // Loop through the items of the cache to delete matching domains
            foreach (KeyValuePair<string, RedirectDomain> pair in _lookup.ToArray()) {
                if (pair.Value.UniqueId == domainId) {
                    _lookup.Remove(pair.Key);
                }
            }

        }

        private string GetDomainKey(RedirectDomain domain) {
            return (domain.InboundProtocol + "__" + domain.InboundDomain + "__" + domain.InboundPort).ToLower();
        }

        private string GetDomainKey(RedirectProtocol protocol, string domainName, int portNumber) {
            return (protocol + "__" + domainName + "__" + portNumber).ToLower();
        }

        #endregion

    }

}