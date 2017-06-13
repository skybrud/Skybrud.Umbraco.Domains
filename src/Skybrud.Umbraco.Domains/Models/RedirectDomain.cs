using System;
using System.Net;
using Newtonsoft.Json;
using Skybrud.Essentials.Enums;
using Skybrud.Essentials.Json.Converters.Enums;
using Skybrud.Essentials.Strings;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Skybrud.Umbraco.Domains.Models {

    [TableName(TableName)]
    [PrimaryKey(PrimaryKey, autoIncrement = true)]
    [ExplicitColumns]
    public class RedirectDomain {

        #region Constants

        public const string TableName = "SkybrudDomainRedirects";

        public const string PrimaryKey = "Id";

        #endregion

        #region Properties

        [JsonProperty("id")]
        [Column(PrimaryKey)]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; protected set; }

        [JsonProperty("uniqueId")]
        [Column("UniqueId")]
        public Guid UniqueId { get; internal set; }

        [Ignore]
        [JsonProperty("inboundProtocol")]
        [JsonConverter(typeof(EnumCamelCaseConverter))]
        public RedirectProtocol InboundProtocol { get; set; }

        [JsonIgnore]
        [Column("InboundProtocol")]
        public string InboundProtocolString {
            get { return StringUtils.ToPascalCase(InboundProtocol); }
            set { InboundProtocol = EnumUtils.ParseEnum<RedirectProtocol>(value); }
        }

        [JsonProperty("inboundDomain")]
        [Column("InboundDomain")]
        public string InboundDomain { get; set; }

        [JsonProperty("inboundPort")]
        [Column("InboundPort")]
        public int InboundPort { get; set; }

        [Ignore]
        [JsonProperty("outboundProtocol")]
        [JsonConverter(typeof(EnumCamelCaseConverter))]
        public RedirectProtocol OutboundProtocol { get; set; }

        [JsonIgnore]
        [Column("OutboundProtocol")]
        public string OutboundProtocolString {
            get { return StringUtils.ToPascalCase(OutboundProtocol); }
            set { OutboundProtocol = EnumUtils.ParseEnum<RedirectProtocol>(value); }
        }

        [JsonProperty("outboundDomain")]
        [Column("OutboundDomain")]
        public string OutboundDomain { get; set; }

        [JsonProperty("outboundPort")]
        [Column("OutboundPort")]
        public int OutboundPort { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("outboundPath")]
        [Column("OutboundPath")]
        public string OutboundPath { get; set; }

        [Ignore]
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonIgnore]
        [Column("StatusCode")]
        public int StatusCodeInt32 {
            get { return (int) StatusCode; }
            set { StatusCode = (HttpStatusCode) value; }
        }

        [JsonProperty("keepPath")]
        [Column("KeepPath")]
        public bool KeepPath { get; set; }

        [JsonProperty("created")]
        [Column("Created")]
        public DateTime Created { get; internal set; }

        [JsonProperty("updated")]
        [Column("Updated")]
        public DateTime Updated { get; internal set; }

        #endregion

        #region Constructors

        public RedirectDomain() {
            StatusCode = HttpStatusCode.MovedPermanently;
        }

        #endregion

    }

}