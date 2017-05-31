using System;
using System.Net;
using System.Text;
using System.Web;
using Skybrud.Essentials.Strings;
using Skybrud.Umbraco.Domains.Models;

namespace Skybrud.Umbraco.Domains.Routing {

    public class DomainsHttpModule : IHttpModule {

        public DomainsRepository Repository {
            get { return DomainsRepository.Current; }
        }

        public HttpRequest Request {
            get { return HttpContext.Current.Request; }
        }

        public HttpResponse Response {
            get { return HttpContext.Current.Response; }
        }

        public void Init(HttpApplication context) {
            context.BeginRequest += ContextOnBeginRequest;
        }

        private void ContextOnBeginRequest(object sender, EventArgs eventArgs) {

            // Attempt to get a domain matching the current URL
            RedirectDomain domain;
            if (!Repository.TryGetDomain(Request.Url, out domain)) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("=== INBOUND ===");
            sb.AppendLine("Protocol: " + domain.InboundProtocol);
            sb.AppendLine("Domain: " + domain.InboundDomain);
            sb.AppendLine("Port: " + domain.InboundPort);
            sb.AppendLine();

            sb.AppendLine("=== OUTBOUND ===");
            sb.AppendLine("Protocol: " + domain.OutboundProtocol);
            sb.AppendLine("Domain: " + domain.OutboundDomain);
            sb.AppendLine("Port: " + domain.OutboundPort);
            sb.AppendLine("Path: " + domain.OutboundPath);
            sb.AppendLine();

            UriBuilder builder = new UriBuilder();

            builder.Scheme = StringUtils.ToCamelCase(domain.OutboundProtocol);
            builder.Host = domain.OutboundDomain;

            if (!String.IsNullOrWhiteSpace(domain.OutboundPath)) {
                builder.Path = domain.OutboundPath;
            } else if (domain.KeepPath) {
                builder.Path = Request.Url.LocalPath;
            }

            // Only append the port number if it differs from the default port number of the protocol
            if (domain.OutboundProtocol == RedirectProtocol.Http && domain.OutboundPort != 80) {
                builder.Port = domain.OutboundPort;
            } else if (domain.OutboundProtocol == RedirectProtocol.Https && domain.OutboundPort != 443) {
                builder.Port = domain.OutboundPort;
            }

            // throw new Exception("This is just a test. If this wasn't a test, you would totally be redirected to: " + builder + sb);

            if (domain.StatusCode == HttpStatusCode.MovedPermanently) {
                Response.RedirectPermanent(builder + "");
            } else {
                Response.Redirect(builder + "");
            }

        }

        public void Dispose() { }
    
    }

}