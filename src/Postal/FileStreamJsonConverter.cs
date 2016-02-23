using System;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace Postal
{
    public class FileStreamJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(FileStream).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var fileName = serializer.Deserialize<string>(reader);
            return fileName != null ? new Attachment(fileName, MediaTypeNames.Application.Octet) : null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fileName = ((FileStream)value).Name;
            serializer.Serialize(writer, fileName);
        }
    }


    public class AttachmentReadConverter : JsonConverter
    {
        private readonly bool _compress;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentReadConverter"/> class.
        /// </summary>
        /// <param name="compress">if set to <c>true</c> [compress].</param>
        public AttachmentReadConverter(bool compress = false)
        {
            _compress = compress;
        }

        // http://stackoverflow.com/questions/27828350/argumentnullexception-in-json-net-6-0-7-when-deserializing-into-namevaluecollect
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Attachment);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<AttachmentInfo>(reader);

            var attachment = info != null
                ? new Attachment(new MemoryStream(Decompress(Convert.FromBase64String(info.ContentBase64))),
                    MediaTypeNames.Application.Octet)
                {
                    ContentId = info.ContentId,
                    ContentDisposition =
                    {
                        FileName = info.FileName
                    }
                }
                : null;
            return attachment;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var attachment = value as Attachment;
            var info = new AttachmentInfo
            {
                FileName = attachment.Name,
                ContentBase64 = Convert.ToBase64String(Compress(StreamToBytes(attachment.ContentStream))),
                ContentId = attachment.ContentId
            };

            serializer.Serialize(writer, info);
        }

        private byte[] StreamToBytes(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public byte[] Compress(byte[] data)
        {
            if (!_compress) return data;
            using (var outStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var srcStream = new MemoryStream(data))
                    srcStream.CopyTo(gzipStream);
                return outStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            if (!_compress) return data;
            using (var inStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                gzipStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }

        private class AttachmentInfo
        {
            [JsonProperty(Required = Required.Always)]
            public string FileName { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string ContentBase64 { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string ContentId { get; set; }
        }
    }

    public class EncodingReadConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Encoding).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var encodingName = serializer.Deserialize<string>(reader);
            return Encoding.GetEncoding(encodingName);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}