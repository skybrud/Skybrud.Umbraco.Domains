using System;
using System.Net;

namespace Skybrud.Umbraco.Domains.Exceptions {
    
    public class DomainsException : Exception {

        public HttpStatusCode StatusCode { get; private set; }

        public DomainsException(string message) : base(message) { }

        public DomainsException(HttpStatusCode statusCode, string message) : base(message) {
            StatusCode = statusCode;
        }

        public DomainsException(string message, HttpStatusCode statusCode) : base(message) {
            StatusCode = statusCode;
        }

    }

}