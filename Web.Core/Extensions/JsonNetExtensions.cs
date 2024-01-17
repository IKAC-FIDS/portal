namespace TES.Web.Core.Extensions
{
    public static class JsonNetExtensions
    {
        public static string SerializeToJson(this object value)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };

            return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        }
    }
}