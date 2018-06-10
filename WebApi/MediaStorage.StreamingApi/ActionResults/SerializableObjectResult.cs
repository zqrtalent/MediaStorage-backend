using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using MediaStorage.Common.Serialization.MfcSerialize;

namespace MediaStorage.StreamingApi.ActionResults
{
    public class SerializableObjectResult : ActionResult
    {
        public SerializableObjectResult(object objSerialize, bool useCompression = false) : base()
        {
            SerializeObject = objSerialize;
        }

        private object SerializeObject { get; set; }
        private bool UseCompression { get; set; }

        public override void ExecuteResult(ActionContext context)
        {
            var resp = context.HttpContext.Response;
            resp.ContentType = "application/octet-stream";

            using (var mem = new MemoryStream())
            {
                var bw = new BinaryWriter(mem);
                if (SerializeObject.Serialize(ref bw))
                {
                    byte[] data = mem.ToArray();
                    // byte[] data = new byte[mem.Length];
                    // mem.Position = 0;
                    // mem.Read(data, 0, data.Length);
                    resp.ContentLength = data.Length;
                    resp.Body.WriteAsync(data, 0, data.Length);
                }
                else
                {
                    bw.Dispose();
                    mem.Dispose();
                    throw new InvalidOperationException(@"Unable to serialize object !");
                }

                bw.Dispose();
                mem.Dispose();
            }
        }
    }
}