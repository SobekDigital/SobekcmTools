﻿#region Using directives

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;

#endregion

namespace SobekCM.Core.MicroservicesClient
{
    /// <summary> Enumeration indicates the type of protocol utilized by this endpoint</summary>
    public enum Microservice_Endpoint_Protocol_Enum : byte
    {
        /// <summary> Output of this endpoint is JSON </summary>
        JSON = 1,

        /// <summary> Output of this endpoint is Protocol Buffer octet-stream </summary>
        PROTOBUF = 2,

        /// <summary> Output of this endpoint is JSON-P </summary>
        JSON_P,

        /// <summary> Output of this endpoint is XML </summary>
        XML,

        /// <summary> If the engine and web are in the same solution and loaded together,
        /// you can also choose to use DIRECT method, which does not use serialization </summary>
        /// <remarks> This is only valid for a number of endpoints, but these are actually 
        /// the most used endpoints, such as item aggregations and digital resources.  This is
        /// particularly useful for testing latency caused by serialization during development. </remarks>
        DIRECT
    }

    /// <summary> Defines a single endpoint for a microservices client </summary>
    [Serializable, DataContract, ProtoContract]
    [XmlRoot("MicroserviceEndpoint")]
    public class MicroservicesClient_Endpoint
    {
        /// <summary> Complete URL for this microservices endpoint </summary>
        [DataMember(Name = "url")]
        [XmlAttribute("url")]
        [ProtoMember(1)]
        public string URL { get; set; }

        /// <summary> Protocol to use when connecting to this endpoint, via the URL </summary>
        [DataMember(Name = "protocol")]
        [XmlAttribute("protocol")]
        [ProtoMember(2)]
        public Microservice_Endpoint_Protocol_Enum Protocol { get; set; }

        /// <summary> Lookup key associated with this endpoint </summary>
        [DataMember(Name = "key")]
        [XmlAttribute("key")]
        [ProtoMember(3)]
        public string Key { get; set; }

        /// <summary> Constructor for a new instance of the MicroservicesClient_Endpoint class </summary>
        /// <remarks> Empty constructor, primarily for serialization purposes </remarks>
        public MicroservicesClient_Endpoint()
        {
            // Empty constructor
        }
        /// <summary> Constructor for a new instance of the MicroservicesClient_Endpoint class </summary>
        /// <param name="URL"> Complete URL for this microservices endpoint </param>
        /// <param name="Protocol"> Protocol to use when connecting to this endpoint, via the URL </param>
        /// <param name="Key"> Lookup key associated with this endpoint </param>
        public MicroservicesClient_Endpoint(string URL, Microservice_Endpoint_Protocol_Enum Protocol, string Key)
        {
            this.URL = URL;
            this.Protocol = Protocol;
            this.Key = Key;
        }

        /// <summary> Constructor for a new instance of the MicroservicesClient_Endpoint class </summary>
        /// <param name="URL"> Complete URL for this microservices endpoint </param>
        /// <param name="Protocol"> Protocol to use when connecting to this endpoint, via the URL </param>
        /// <param name="Key"> Lookup key associated with this endpoint </param>
        public MicroservicesClient_Endpoint(string URL, string Protocol, string Key)
        {
            this.URL = URL;
            this.Key = Key;
            switch (Protocol.ToLower())
            {
                case "protobuf":
                    this.Protocol = Microservice_Endpoint_Protocol_Enum.PROTOBUF;
                    break;

                case "direct":
                    this.Protocol = Microservice_Endpoint_Protocol_Enum.DIRECT;
                    break;

                case "xml":
                    this.Protocol = Microservice_Endpoint_Protocol_Enum.XML;
                    break;

                case "json-p":
                    this.Protocol = Microservice_Endpoint_Protocol_Enum.JSON_P;
                    break;

                default:
                    this.Protocol = Microservice_Endpoint_Protocol_Enum.JSON;
                    break;
            }
            this.Protocol = String.Compare(Protocol, "protobuf", StringComparison.InvariantCultureIgnoreCase ) == 0 ? Microservice_Endpoint_Protocol_Enum.PROTOBUF : Microservice_Endpoint_Protocol_Enum.JSON;
        }
    }
}
