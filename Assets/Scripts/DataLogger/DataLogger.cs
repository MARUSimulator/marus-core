using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Labust.Logger
{
    /// <summary>
    /// Singleton class used for data logging
    ///
    /// Resets on scene change
    /// </summary>
    public class DataLogger : Singleton<DataLogger>
    {

        private static Dictionary<string, GameObjectLogger> _loggers;


        protected override void Initialize()
        {
            _loggers = new Dictionary<string, GameObjectLogger>();
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            _loggers.Clear();
        }

        /// <summary>
        /// Get logger for topic with given name
        /// </summary>
        /// <param name="topic"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public GameObjectLogger<T> GetLogger<T>(string topic)
        {
            if (_loggers.ContainsKey(topic))
            {
                GameObjectLogger<T> logger;
                try
                {
                    logger = _loggers[topic] as GameObjectLogger<T>;
                }
                catch
                {
                    Debug.Log($"Logger for topic {topic} is not of type {typeof(GameObjectLogger<T>).FullName}.");
                    return null;
                }
                return logger;
            }
            var newLogger = new GameObjectLogger<T>(topic);
            _loggers.Add(topic, newLogger);
            return newLogger;
        }

        /// <summary>
        /// Export logs from all loggers 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IReadOnlyList<LogRecord>> ExportAllLogs()
        {
            return _loggers.ToDictionary(
                x => x.Key, // key
                x => x.Value.AbstractRecords // value
            );
        }

        /// <summary>
        /// Export logs from logger with given topic name
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IReadOnlyList<LogRecord> ExportLogsForTopic(string topic)
        {
            if (_loggers.TryGetValue(topic, out var logger))
            {
                return logger.AbstractRecords;
            }
            return new List<LogRecord>();
        }
    }


    /// <summary>
    /// Logger for game object
    ///
    /// Generic type determines the message data class that is saved
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class GameObjectLogger<T> : GameObjectLogger
    {
        List<LogRecord<T>> _records;
        public IReadOnlyList<LogRecord<T>> Records => _records;

        public override IReadOnlyList<LogRecord> AbstractRecords => _records;

        public GameObjectLogger(string topic) : base(topic)
        {
            _records = new List<LogRecord<T>>();
        }

        public void Log(T value)
        {
            _records.Add(new LogRecord<T>(DateTime.Now, Time.timeSinceLevelLoadAsDouble, value));
        }

    }

    /// <summary>
    /// Abstract game object logger
    /// </summary>
    public abstract class GameObjectLogger
    {
        protected string _topic;
        public abstract IReadOnlyList<LogRecord> AbstractRecords { get; }

        public GameObjectLogger(string topic)
        {
            _topic = topic;
        }

    }

    /// <summary>
    /// Abstract data class that defines log record header information
    /// </summary>
    public abstract class LogRecord
    {
        public DateTime TimeStamp { get; private set; }
        public double SimulationTime { get; private set; }
        public LogRecord(DateTime timeStamp, double simulationTime)
        {
            TimeStamp = timeStamp;
            SimulationTime = simulationTime;
        }

    }

    /// <summary>
    /// Log record with generic type as data value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LogRecord<T> : LogRecord
    {
        public T Value { get; private set; }

        public LogRecord(DateTime timeStamp, double simulationTime, T value) : base(timeStamp, simulationTime)
        {
            Value = value;
        }
    }
}
