using System;
using System.Net;
using Skybrud.Essentials.Common;
using Skybrud.Essentials.Strings;
using Skybrud.Umbraco.Domains.Caching;
using Skybrud.Umbraco.Domains.Exceptions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Web.Cache;

namespace Skybrud.Umbraco.Domains.Models {

    public class DomainsService {

        #region Properties

        /// <summary>
        /// Gets a reference to the Umbraco database.
        /// </summary>
        protected UmbracoDatabase Database {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
        }

        protected ISqlSyntaxProvider SqlSyntax {
            get { return ApplicationContext.Current.DatabaseContext.SqlSyntax; }
        }

        protected readonly DatabaseSchemaHelper SchemaHelper = new DatabaseSchemaHelper(
            ApplicationContext.Current.DatabaseContext.Database,
            ApplicationContext.Current.ProfilingLogger.Logger,
            ApplicationContext.Current.DatabaseContext.SqlSyntax
        );

        public static DomainsService Current {
            get { return (DomainsService) ApplicationContext.Current.ApplicationCache.StaticCache.GetCacheItem("Skybrud:DomainsService", () => new DomainsService()); }
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Gets an array of all domains from the database.
        /// </summary>
        /// <returns>An array of <see cref="RedirectDomain"/>.</returns>
        public RedirectDomain[] GetAllDomains() {

            // Just return an empty array if the table doesn't exist (since there aren't any domains anyway)
            if (!SchemaHelper.TableExist(RedirectDomain.TableName)) return new RedirectDomain[0];

            // Generate the SQL for the query
            Sql sql = new Sql().Select("*").From(RedirectDomain.TableName);

            // Make the call to the database
            RedirectDomain[] all = Database.Fetch<RedirectDomain>(sql).ToArray();

            return all;

        }

        public RedirectDomain AddDomain(RedirectProtocol inboundProtocol, string inboundDomain, RedirectProtocol outboundProtocol, string outboundDomain, User user) {
            if (user == null) throw new ArgumentNullException("user");
            return AddDomain(inboundProtocol, inboundDomain, outboundProtocol, outboundDomain);
        }
        
        public RedirectDomain AddDomain(RedirectProtocol inboundProtocol, string inboundDomain, RedirectProtocol outboundProtocol, string outboundDomain, int userId = 0) {

            if (String.IsNullOrWhiteSpace(inboundDomain)) throw new ArgumentNullException("inboundDomain");
            if (String.IsNullOrWhiteSpace(outboundDomain)) throw new ArgumentNullException("outboundDomain");

            return AddDomain(
                inboundProtocol, inboundDomain, GetDefaultPort(inboundProtocol),
                outboundProtocol, outboundDomain, GetDefaultPort(outboundProtocol),
                userId
            );

        }

        public RedirectDomain AddDomain(RedirectProtocol inboundProtocol, string inboundDomain, int inboundPort, RedirectProtocol outboundProtocol, string outboundDomain, int outboundPort, IUser user) {
            if (user == null) throw new ArgumentNullException("user");
            return AddDomain(inboundProtocol, inboundDomain, inboundPort, outboundProtocol, outboundDomain, outboundPort, user.Id);
        }

        public RedirectDomain AddDomain(RedirectProtocol inboundProtocol, string inboundDomain, int inboundPort, RedirectProtocol outboundProtocol, string outboundDomain, int outboundPort, int userId = 0) {

            if (String.IsNullOrWhiteSpace(inboundDomain)) throw new ArgumentNullException("inboundDomain");
            if (String.IsNullOrWhiteSpace(outboundDomain)) throw new ArgumentNullException("outboundDomain");

            if (!SchemaHelper.TableExist(RedirectDomain.TableName)) {
                SchemaHelper.CreateTable<RedirectDomain>(false);
            }

            if (GetDomainByInbound(inboundProtocol, inboundDomain, inboundPort) != null) {
                throw new DomainsException("A domain with the specified inbound parameters already exists.");
            }

            // Initialize the new domain and populate the properties
            RedirectDomain row = new RedirectDomain {
                UniqueId = Guid.NewGuid(),
                InboundProtocol = inboundProtocol,
                InboundDomain = inboundDomain.Trim().ToLower(),
                InboundPort = inboundPort,
                OutboundProtocol = outboundProtocol,
                OutboundDomain = outboundDomain.Trim().ToLower(),
                OutboundPort = outboundPort,
                StatusCode = HttpStatusCode.MovedPermanently,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            // Attempt to add the redirect to the database
            try {
                Database.Insert(row);
            } catch (Exception ex) {
                LogHelper.Error<DomainsRepository>("Unable to insert domain into the database", ex);
                throw new Exception("Unable to insert domain into the database", ex);
            }

            // Get an updated reference to the created domain
            RedirectDomain domain = GetDomainById(row.UniqueId);

            // Update the domain in the caches across all domains
            DistributedCache.Instance.Refresh(DomainsCacheRefresher.CacheRefresherId, domain.Id);

            return domain;

        }

        /// <summary>
        /// Gets the domain mathing the specified <paramref name="domainId"/>.
        /// </summary>
        /// <param name="domainId">The ID of the domain.</param>
        /// <returns>An instance of <see cref="RedirectDomain"/>, or <code>null</code> if not found.</returns>
        public RedirectDomain GetDomainById(int domainId) {

            // Just return "null" if the table doesn't exist (since there aren't any domains anyway)
            if (!SchemaHelper.TableExist(RedirectDomain.TableName)) return null;

            // Generate the SQL for the query
            Sql sql = new Sql().Select("*").From(RedirectDomain.TableName).Where<RedirectDomain>(x => x.Id == domainId);

            // Make the call to the database
            return Database.FirstOrDefault<RedirectDomain>(sql);

        }

        /// <summary>
        /// Gets the domain mathing the specified <paramref name="domainId"/>.
        /// </summary>
        /// <param name="domainId">The ID of the domain.</param>
        /// <returns>An instance of <see cref="RedirectDomain"/>, or <code>null</code> if not found.</returns>
        public RedirectDomain GetDomainById(Guid domainId) {

            // Validate the input
            if (domainId == Guid.Empty) throw new ArgumentNullException("domainId");

            // Just return "null" if the table doesn't exist (since there aren't any domains anyway)
            if (!SchemaHelper.TableExist(RedirectDomain.TableName)) return null;

            // Generate the SQL for the query
            Sql sql = new Sql().Select("*").From(RedirectDomain.TableName).Where<RedirectDomain>(x => x.UniqueId == domainId);

            // Make the call to the database
            return Database.FirstOrDefault<RedirectDomain>(sql);

        }

        private RedirectDomain GetDomainByInbound(RedirectProtocol protocol, string domainName, int portNumber) {

            // Validate the input
            if (String.IsNullOrWhiteSpace(domainName)) throw new ArgumentNullException("domainName");

            // Just return "null" if the table doesn't exist (since there aren't any domains anyway)
            if (!SchemaHelper.TableExist(RedirectDomain.TableName)) return null;

            // Work a bit on the input
            string protocolStr = StringUtils.ToPascalCase(protocol);
            domainName = domainName.Trim().ToLower();

            // Generate the SQL for the query
            Sql sql = new Sql().Select("*").From(RedirectDomain.TableName).Where<RedirectDomain>(
                x => x.InboundProtocolString == protocolStr && x.InboundDomain == domainName && x.InboundPort == portNumber
            );

            // Make the call to the database
            return Database.FirstOrDefault<RedirectDomain>(sql);
            
        }

        public void SaveDomain(RedirectDomain domain, IUser user) {
            if (user == null) throw new ArgumentNullException("user");
            SaveDomain(domain, user.Id);
        }

        public void SaveDomain(RedirectDomain domain, int userId) {

            // Some input validation
            if (domain == null) throw new ArgumentNullException("domain");
            if (String.IsNullOrWhiteSpace(domain.InboundDomain)) throw new PropertyNotSetException("domain.InboundDomain");
            if (String.IsNullOrWhiteSpace(domain.OutboundDomain)) throw new PropertyNotSetException("domain.OutboundDomain");

            // Check whether another domain matches the inbound parameters
            RedirectDomain existing = GetDomainByInbound(domain.InboundProtocol, domain.InboundDomain, domain.InboundPort);
            if (existing != null && existing.Id != domain.Id) {
                throw new DomainsException("A domain with the same inbound parameters already exists.");
            }

            // Update the timestamp for when the domain was updated
            domain.Updated = DateTime.UtcNow;

            // Update the domain in the database
            Database.Update(domain);

            // Update the domain in the caches across all domains
            DistributedCache.Instance.Refresh(DomainsCacheRefresher.CacheRefresherId, domain.Id);
        
        }

        /// <summary>
        /// Deletes the specified <paramref name="domain"/>.
        /// </summary>
        /// <param name="domain">The domain to be deleted.</param>
        /// <param name="user">The user responsible for the action.</param>
        public void DeleteDomain(RedirectDomain domain, IUser user) {
            if (user == null) throw new ArgumentNullException("user");
            DeleteDomain(domain, user.Id);
        }

        /// <summary>
        /// Deletes the specified <paramref name="domain"/>.
        /// </summary>
        /// <param name="domain">The domain to be deleted.</param>
        /// <param name="userId">The ID of the user responsible for the action.</param>
        public void DeleteDomain(RedirectDomain domain, int userId = 0) {

            // Some input validation
            if (domain == null) throw new ArgumentNullException("domain");

            // Remove the domain from the database
            Database.Delete(domain);

            // Remove the domain from the distributed cache (removing doesn't support GUIDs, so we use the numeric ID instead)
            DistributedCache.Instance.Remove(DomainsCacheRefresher.CacheRefresherId, domain.Id);

        }

        #endregion

        #region Private helper methods

        private int GetDefaultPort(RedirectProtocol protocol) {
            switch (protocol) {
                case RedirectProtocol.Http: return 80;
                case RedirectProtocol.Https: return 443;
                default: return 0;
            }
        }

        #endregion

    }

}