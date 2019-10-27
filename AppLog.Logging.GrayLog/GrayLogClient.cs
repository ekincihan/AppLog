using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using AppLog.Core.Model;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace AppLog.Logging.GrayLog
{
    /// <summary>
    /// Base for GrayLog client implementations.
    /// </summary>
    internal abstract class GrayLogClient : IDisposable
    {
        public int NextChunckedMessageId { get; set; }
        public int MaxPacketSize { get; set; }
        public bool UdpClientIsOwned { get; set; }
        protected UdpClient UdpClient { get; set; }
        protected GrayLogClient(string facility)
        {
            this.Facility = facility;
            this.CompressionTreshold = 0;
        
        }
        public string Facility { get; protected set; }
        public int CompressionTreshold { get; set; }
        public void Send(string shortMessage, string fullMessage = null, object data = null)
        {
            // Construct log record:
            var logRecord = new Dictionary<string, object>();
            {
                logRecord["version"] = "1.1";
                logRecord["host"] = ((LogEntry)data).Environment.MachineName;
                logRecord["_facility"] = ((LogEntry)data).ApplicationId;
                logRecord["short_message"] = shortMessage;
                if (!String.IsNullOrWhiteSpace(fullMessage)) logRecord["full_message"] = fullMessage;
                logRecord["timestamp"] = EpochOf(DateTime.UtcNow);
                if (data is string) logRecord["_data"] = data;
                else if (data is System.Collections.IEnumerable) logRecord["_values"] = data;
                else if (data is System.Collections.IDictionary) MergeDictionary(logRecord, (System.Collections.IDictionary)data, "_");
                else if (data != null) MergeObject(logRecord, data, "_");

                // Serialize object:
                string logRecordString = JsonConvert.SerializeObject(logRecord);
                var logRecordBytes = Encoding.UTF8.GetBytes(logRecordString);

                // Dispatch message:
                InternallySendMessage(logRecordBytes);
            }
        }
        public async Task SendAsync(string shortMessage, string fullMessage = null, object data = null)
        {
            // Construct log record:
            var logRecord = new Dictionary<string, object>();
            {
                logRecord["version"] = "1.1";
                logRecord["host"] = ((LogEntry)data).Environment.MachineName;
                logRecord["_facility"] = ((LogEntry)data).ApplicationId;
                logRecord["short_message"] = shortMessage;
                if (!String.IsNullOrWhiteSpace(fullMessage)) logRecord["full_message"] = fullMessage;
                logRecord["timestamp"] = EpochOf(DateTime.UtcNow);
                if (data is string) logRecord["_data"] = data;
                else if (data is System.Collections.IEnumerable) logRecord["_values"] = data;
                else if (data is System.Collections.IDictionary) MergeDictionary(logRecord, (System.Collections.IDictionary)data, "_");
                else if (data != null) MergeObject(logRecord, data, "_");

                // Serialize object:
                string logRecordString = JsonConvert.SerializeObject(logRecord);
                var logRecordBytes = Encoding.UTF8.GetBytes(logRecordString);

                // Dispatch message:
                await this.InternallySendMessageAsync(logRecordBytes);
            }
        }
        public void Send(Exception ex)
        {
            // Collect exception data:
            var data = new System.Collections.Hashtable();
            for (var iex = ex; iex != null; iex = iex.InnerException)
            {
                foreach (var key in iex.Data.Keys)
                {
                    data.Add(key, iex.Data[key]);
                }
            }

            // Send exception:
            this.Send(ex.Message, ex.ToString(), data);
        }
        protected void InternallySendMessage(byte[] messageBody)
        {
            NextChunckedMessageId++;

            if (this.CompressionTreshold != -1 && messageBody.Length > this.CompressionTreshold)
                messageBody = this.Compress(messageBody, CompressionLevel.Optimal);

            using (var objectStream = new MemoryStream(messageBody))
            {
                var chunkCount = PartsNeeded(messageBody.Length, MaxPacketSize - 12);
                if (chunkCount > 128)
                {
                    throw new GrayLoggingException("Maximum number of GrayLog GELF UDP chuncks exceeded.");
                }

                for (byte chunkNumber = 0; chunkNumber < chunkCount; chunkNumber++)
                {
                    var chunkBuffer = new byte[MaxPacketSize];
                    chunkBuffer[0x00] = (byte)0x1e;
                    chunkBuffer[0x01] = (byte)0x0f;
                    BitConverter.GetBytes(NextChunckedMessageId).CopyTo(chunkBuffer, 0x02);
                    chunkBuffer[0x0a] = chunkNumber;
                    chunkBuffer[0x0b] = (byte)chunkCount;

                    var chunkSize = 12 + objectStream.Read(chunkBuffer, 12, MaxPacketSize - 12);

                    this.UdpClient.Send(chunkBuffer, chunkSize);
                }
            }
        }

        private int PartsNeeded(int totalSize, int partSize)
        {
            return (totalSize + partSize - 1) / partSize;
        }

        protected async Task InternallySendMessageAsync(byte[] messageBody)
        {
            NextChunckedMessageId++;

            if (this.CompressionTreshold != -1 && messageBody.Length > this.CompressionTreshold)
                messageBody = this.Compress(messageBody, CompressionLevel.Optimal);

            using (var objectStream = new MemoryStream(messageBody))
            {
                var chunkCount = PartsNeeded(messageBody.Length, MaxPacketSize - 12);
                if (chunkCount > 128)
                {
                    throw new GrayLoggingException("Maximum number of GrayLog GELF UDP chuncks exceeded.");
                }

                for (byte chunkNumber = 0; chunkNumber < chunkCount; chunkNumber++)
                {
                    var chunkBuffer = new byte[MaxPacketSize];
                    chunkBuffer[0x00] = (byte)0x1e;
                    chunkBuffer[0x01] = (byte)0x0f;
                    BitConverter.GetBytes(NextChunckedMessageId).CopyTo(chunkBuffer, 0x02);
                    chunkBuffer[0x0a] = chunkNumber;
                    chunkBuffer[0x0b] = (byte)chunkCount;

                    var chunkSize = 12 + objectStream.Read(chunkBuffer, 12, MaxPacketSize - 12);

                    await this.UdpClient.SendAsync(chunkBuffer, chunkSize);
                }
            }
        }
        public abstract void Dispose();
        protected byte[] Compress(byte[] raw, CompressionLevel compressionLevel)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, compressionLevel, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        private void MergeDictionary(Dictionary<string, object> target, System.Collections.IDictionary source, string prefix)
        {
            foreach (var key in source.Keys)
            {
                target[prefix + key] = source[key];
            }
        }

        private static void MergeObject(IDictionary<string, object> target, dynamic source, string prefix = "")
        {
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                target[prefix + property.Name] = property.GetValue(source);
            }
        }

        private static long EpochOf(DateTime dt)
        {
            TimeSpan t = dt.ToUniversalTime() - new DateTime(1970, 1, 1);
            return (long)t.TotalSeconds;
        }
    }
}
