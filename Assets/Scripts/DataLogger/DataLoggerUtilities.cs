using System.IO;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Labust.Logger
{
    /// <summary>
    /// Helper utility class for saving and loading JSON log records
    /// </summary>
    public static class DataLoggerUtilities
    {
        private static string _savesPath = Path.Combine(Application.dataPath, "Saves");
        private static JsonSerializerSettings _jsonConfig = new JsonSerializerSettings
        {
            Converters  = new List<JsonConverter>()
            { 
                new UnityVectorJsonConverter(), 
                new UnityQuaternionJsonConverter()
            },
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Save all logs to file
        /// </summary>
        /// <param name="savesPath">Optional save path</param>
        public static void SaveAllLogs(string savesPath = null)
        {
            if (savesPath == null)
            {
                savesPath = _savesPath;
            }

            var logs = DataLogger.Instance.ExportAllLogs();
            if (!Directory.Exists(savesPath))
            {
                Directory.CreateDirectory(savesPath);
            }
            var currentPath = Path.Combine(savesPath, $"Scenario{DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")}.json");
            var asJson = JsonConvert.SerializeObject(logs, _jsonConfig);
            using(var writer = new StreamWriter(currentPath))
            {
                writer.Write(asJson);
            }
        }

        /// <summary>
        /// Save only logs for single topic to file
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="savesPath">Optional save path</param>
        public static void SaveLogsForTopic(string topic, string savesPath = null)
        {
            if (savesPath == null)
            {
                savesPath = _savesPath;
            }

            IReadOnlyList<LogRecord> logs = DataLogger.Instance.ExportLogsForTopic(topic);

            if (logs.Count == 0)
            {
                Debug.Log($"No logs available for topic {topic}!\nNo file created.");
                return;

            }

            if (!Directory.Exists(savesPath))
            {
                Directory.CreateDirectory(savesPath);
            }
            var currentPath = Path.Combine(savesPath, $"{topic}-Scenario{DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")}.json");
            var asJson = JsonConvert.SerializeObject(logs, _jsonConfig);
            using(var writer = new StreamWriter(currentPath))
            {
                writer.Write(asJson);
            }
        }

        /// <summary>
        /// Load up records from file
        /// </summary>
        /// <param name="fileName">File to load from</param>
        /// <typeparam name="T">Type of record</typeparam>
        /// <returns></returns>
        public static List<LogRecord<T>> GetLogRecordsFromFile<T>(string fileName)
        {
            List<LogRecord<T>> recordsList = new List<LogRecord<T>>();
            if (!File.Exists(fileName))
            {
                Debug.Log("Error: File " + fileName + " does not exist!");
                return recordsList;
            }
            string jsonString = File.ReadAllText(fileName);
            recordsList = JsonConvert.DeserializeObject<List<LogRecord<T>>>(jsonString, _jsonConfig);
            return recordsList;
        }
    }

    /// <summary>
    /// Custom json converter for Unity Quaternion struct
    /// </summary>
    internal class UnityQuaternionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            var x = reader.ReadAsDouble();
            var y = reader.ReadAsDouble();
            var z = reader.ReadAsDouble();
            var w = reader.ReadAsDouble();
            reader.Read();
            return new Quaternion((float)x, (float)y, (float)z, (float)w);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = value as Quaternion?;
            writer.WriteStartArray();
            writer.WriteValue(v.Value.x);
            writer.WriteValue(v.Value.y);
            writer.WriteValue(v.Value.z);
            writer.WriteValue(v.Value.w);
            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Custom json converter for Unity Vector struct
    /// </summary>
    internal class UnityVectorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2) || objectType == typeof(Vector3) || objectType == typeof(Vector4);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var p1 = reader.ReadAsDouble();
            var p2 = reader.ReadAsDouble();
            if (objectType == typeof(Vector2))
            {
                reader.Read(); // end array token
                return new Vector2((float)p1, (float)p2);
            }
            var p3 = reader.ReadAsDouble();
            if (objectType == typeof(Vector3))
            {
                reader.Read(); // end array token
                return new Vector3((float)p1, (float)p2, (float)p3);
            }
            var p4 = reader.ReadAsDouble();
            if (objectType == typeof(Vector4))
            {
                reader.Read(); // end array token
                return new Vector4((float)p1, (float)p2, (float)p3, (float)p4);
            }
            throw new JsonException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value is Vector2 v2)
            {
                writer.WriteValue(v2[0]);
                writer.WriteValue(v2[1]);
            }
            else if (value is Vector3 v3)
            {
                writer.WriteValue(v3[0]);
                writer.WriteValue(v3[1]);
                writer.WriteValue(v3[2]);
            }
            else if (value is Vector4 v4)
            {
                writer.WriteValue(v4[0]);
                writer.WriteValue(v4[1]);
                writer.WriteValue(v4[2]);
                writer.WriteValue(v4[3]);
            }
            writer.WriteEndArray();
        }
    }
}