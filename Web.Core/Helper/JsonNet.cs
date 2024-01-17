using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace TES.Web.Core.Helper
{
    public static class JsonNet
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        static JsonNet()
        {
            SerializerSettings.Converters.Add(new IsoDateTimeConverter());
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public static dynamic Decode(string value) => JsonConvert.DeserializeObject(value, SerializerSettings);
        public static dynamic Decode(string value, Type targetType) => JsonConvert.DeserializeObject(value, targetType, SerializerSettings);
        public static T Decode<T>(string value) => JsonConvert.DeserializeObject<T>(value, SerializerSettings);
        public static string Encode(object value) => JsonConvert.SerializeObject(value, Formatting.None, SerializerSettings);
    }
}