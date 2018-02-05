namespace KiotlogDBF

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

module Utils =
    let snakeSettings =
        JsonSerializerSettings (
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = DefaultContractResolver (
                NamingStrategy = SnakeCaseNamingStrategy(true, false, false)
            )
        )
    let toJsonString entity =
        JsonConvert.SerializeObject(
            entity,
            Formatting.Indented,
            JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore))