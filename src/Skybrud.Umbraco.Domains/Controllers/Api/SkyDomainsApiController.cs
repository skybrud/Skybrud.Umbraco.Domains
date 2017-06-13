using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Skybrud.Essentials.Enums;
using Skybrud.Umbraco.Domains.Models;
using Umbraco.Web.WebApi;
using JsonMetaResponse = Skybrud.WebApi.Json.Meta.JsonMetaResponse;

namespace Skybrud.Umbraco.Domains.Controllers.Api
{
    public class SkyDomainsApiController : UmbracoApiController
    {
        private readonly DomainsService _ds = DomainsService.Current;

        [HttpGet]
        public object GetAllDomains()
        {
            try
            {
                var domains = _ds.GetAllDomains();

                return JsonMetaResponse.GetSuccess(domains);
            }
            catch (Exception e)
            {
                return JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpGet]
        public object GetDomnainById(int id)
        {
            try
            {
                var domain = _ds.GetDomainById(id);

                if (domain == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, JsonMetaResponse.GetError(HttpStatusCode.NotFound, "Domain not found :'("));

                return JsonMetaResponse.GetSuccess(domain);
            }
            catch (Exception e)
            {
                return JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpGet]
        public object AddDomain(string inboundProtocol, string inbound, string outboundProtocol, string outbound)
        {
            try
            {
                var ibProtocol = EnumUtils.ParseEnum<RedirectProtocol>(inboundProtocol);
                var obProtocol = EnumUtils.ParseEnum<RedirectProtocol>(outboundProtocol);

                var domain = _ds.AddDomain(ibProtocol, inbound, obProtocol, outbound);

                if (domain == null)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, "Why this happened, i dont know ¯\\_(ツ)_/¯"));

                return JsonMetaResponse.GetSuccess(domain);
            }
            catch (Exception e)
            {
                return JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        
        [HttpGet]
        public object RemoveDomain(int id)
        {
            try
            {
                RedirectDomain domain = _ds.GetDomainById(id);

                if (domain == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, JsonMetaResponse.GetError(HttpStatusCode.NotFound, "The one that got away: The domain suffers from an ExistentialCrisisException"));

                _ds.DeleteDomain(domain);

                return JsonMetaResponse.GetSuccess("The domain is sleeping with the fishes");
            }
            catch (Exception e)
            {
                return JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}
