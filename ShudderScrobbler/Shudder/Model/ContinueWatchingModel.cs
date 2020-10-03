using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ShudderScrobbler.Shudder.Model
{
    public partial class ContinueWatchingModel
    {
        [JsonProperty("continuewatching")]
        public List<ContinueWatching> ContinueWatching { get; set; }
    }

    public partial class ContinueWatching
    {
        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("vpos", NullValueHandling = NullValueHandling.Ignore)]
        public long? Vpos { get; set; }

        [JsonProperty("episodes", NullValueHandling = NullValueHandling.Ignore)]
        public ContinueWatching[] Episodes { get; set; }
    }

    public enum TypeEnum { Episode, Movie, Series };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "EPISODE":
                    return TypeEnum.Episode;
                case "MOVIE":
                    return TypeEnum.Movie;
                case "SERIES":
                    return TypeEnum.Series;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.Episode:
                    serializer.Serialize(writer, "EPISODE");
                    return;
                case TypeEnum.Movie:
                    serializer.Serialize(writer, "MOVIE");
                    return;
                case TypeEnum.Series:
                    serializer.Serialize(writer, "SERIES");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }


}
