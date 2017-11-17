using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KiotlogDB
{
    class JsonSettings {       
        static public JsonSerializerSettings snakeSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = new DefaultContractResolver {
                NamingStrategy = new SnakeCaseNamingStrategy(true, false, false)
            }
        };
    }
}