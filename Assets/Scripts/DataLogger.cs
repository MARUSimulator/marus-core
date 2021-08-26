using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Labust.Logger
{
    public class DataLogger : Singleton<DataLogger>
    {

        private static Dictionary<string, GameObjectLogger> _loggers;


        public override void Initialize()
        {
            _loggers = new Dictionary<string, GameObjectLogger>();
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            _loggers.Clear();
        }

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

        public Dictionary<string, IReadOnlyList<LogRecord>> ExportAllLogs()
        {
            return _loggers.ToDictionary(
                x => x.Key, // key
                x => x.Value.AbstractRecords // value
            );
        }

        public IReadOnlyList<LogRecord> ExportLogsForTopic(string topic)
        {
            if (_loggers.TryGetValue(topic, out var logger))
            {
                return logger.AbstractRecords;
            }
            return new List<LogRecord>();
        }
    }


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

    public abstract class GameObjectLogger
    {
        protected string _topic;
        public abstract IReadOnlyList<LogRecord> AbstractRecords { get; }

        public GameObjectLogger(string topic)
        {
            _topic = topic;
        }



    }

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

    public class LogRecord<T> : LogRecord
    {
        public T Value { get; private set; }

        public LogRecord(DateTime timeStamp, double simulationTime, T value) : base(timeStamp, simulationTime)
        {
            Value = value;
        }
    }


    
}