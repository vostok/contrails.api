using System;
using Newtonsoft.Json;

namespace Vostok.Contrails.Api
{
    public class JsonGuidConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid) || objectType == typeof(Guid?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // We declared above CanRead false so the default serialization will be used
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null || Guid.Empty.Equals(value))
            {
                writer.WriteValue(string.Empty);
            }
            else
            {
                writer.WriteValue(((Guid)value).ToString("N"));
            }
        }
    }
}