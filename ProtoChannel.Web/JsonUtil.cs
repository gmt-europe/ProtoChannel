using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ProtoChannel.Web
{
    internal static class JsonUtil
    {
        public static object DeserializeMessage(JsonTextReader reader, ServiceType type)
        {
            return DeserializeMessage(reader, type, false);
        }

        private static object DeserializeMessage(JsonTextReader reader, ServiceType type, bool skipRead)
        {
            Require.NotNull(reader, "reader");
            Require.NotNull(type, "type");

            if (!skipRead && !reader.Read())
                throw new HttpException("Invalid request");

            if (reader.TokenType == JsonToken.Null)
                return null;

            object instance = Activator.CreateInstance(type.Type);

            if (reader.TokenType != JsonToken.StartObject)
                throw new HttpException("Invalid request");

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                else if (reader.TokenType != JsonToken.PropertyName)
                    throw new HttpException("Invalid request");

                int tag;

                if (!int.TryParse((string)reader.Value, NumberStyles.None, CultureInfo.InvariantCulture, out tag))
                    throw new HttpException("Invalid request");

                ServiceTypeField field;

                if (!type.Fields.TryGetValue(tag, out field))
                    throw new HttpException("Unknown tag");

                if (field.CollectionType != null)
                {
                    var collection = new ArrayList();

                    if (!reader.Read())
                        throw new HttpException("Invalid request");

                    if (reader.TokenType == JsonToken.Null)
                        continue;
                    if (reader.TokenType != JsonToken.StartArray)
                        throw new HttpException("Invalid request");

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndArray)
                            break;

                        if (field.ServiceType != null && !field.ServiceType.Type.IsEnum)
                            collection.Add(DeserializeMessage(reader, field.ServiceType, true));
                        else
                            collection.Add(Convert.ChangeType(reader.Value, field.Type));
                    }

                    if (reader.TokenType != JsonToken.EndArray)
                        throw new HttpException("Invalid request");

                    var targetCollection = (IList)Activator.CreateInstance(field.CollectionType, new object[] { collection.Count });

                    for (int i = 0; i < collection.Count; i++)
                    {
                        if (field.CollectionType.IsArray)
                            targetCollection[i] = collection[i];
                        else
                            targetCollection.Add(collection[i]);
                    }

                    field.Setter(instance, targetCollection);
                }
                else if (field.ServiceType != null && !field.ServiceType.Type.IsEnum)
                {
                    if (!reader.Read())
                        throw new HttpException("Invalid request");

                    field.Setter(instance, DeserializeMessage(reader, field.ServiceType, true));
                }
                else
                {
                    if (!reader.Read())
                        throw new HttpException("Invalid request");

                    field.Setter(instance, reader.Value);
                }
            }

            if (reader.TokenType != JsonToken.EndObject)
                throw new HttpException("Invalid request");

            return instance;
        }

        public static void SerializeMessage(JsonTextWriter writer, ServiceType type, object message)
        {
            Require.NotNull(writer, "writer");
            Require.NotNull(type, "type");
            Require.NotNull(message, "message");

            writer.WriteStartObject();

            foreach (var field in type.Fields.Values)
            {
                if (
                    field.ShouldSerializeMethod != null &&
                    !field.ShouldSerializeMethod(message)
                )
                    continue;

                writer.WritePropertyName(field.Tag.ToString(CultureInfo.InvariantCulture));

                object value = field.Getter(message);

                if (value == null)
                {
                    writer.WriteNull();
                }
                else if (field.CollectionType != null)
                {
                    writer.WriteStartArray();

                    foreach (object item in (IEnumerable)value)
                    {
                        if (field.ServiceType != null && !field.ServiceType.Type.IsEnum)
                            SerializeMessage(writer, field.ServiceType, item);
                        else
                            writer.WriteValue(item);
                    }

                    writer.WriteEndArray();
                }
                else 
                {
                    if (field.ServiceType != null && !field.ServiceType.Type.IsEnum)
                        SerializeMessage(writer, field.ServiceType, value);
                    else
                        writer.WriteValue(value);
                }
            }

            writer.WriteEndObject();
        }
    }
}
