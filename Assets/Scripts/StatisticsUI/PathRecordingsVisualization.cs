using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Labust.Visualization;
using Labust.Visualization.Primitives;
using Labust.Utils;
using Labust.Logger;

namespace Labust.StatisticsUI
{
    /// <summary>
    /// This class controls Statistics UI view.
    /// </summary>
    public class PathRecordingsVisualization : MonoBehaviour
    {
        private Button _addPathButton;
        private Dropdown _pathsDropdown;
        private ScrollRect _scrollView;

        private List<StatisticsEntry> _activePaths;

        private List<string> _pathRecordings;

        /// <summary>
        /// Content child object of relevant ScrollView object
        /// </summary>
        public UnityEngine.Transform _scrollViewContent;

        /// <summary>
        /// Trip information prefab
        /// </summary>
        public GameObject TripInfoPrefab;

        public List<Tuple<Color, Color>> ColorPresets;

        void Start()
        {
            // instantiate visualizer
            var v = Visualizer.Instance;
            _addPathButton = transform.Find("Panel/StatisticsUI/PathSelector/AddPathButton").GetComponent<Button>();
            _addPathButton.onClick.AddListener(AddStatisticsInformation);
            _pathsDropdown = GetComponentInChildren<Dropdown>();
            _pathRecordings = GetRecordings();
            _pathsDropdown.AddOptions(_pathRecordings);
            _activePaths = new List<StatisticsEntry>();

            _scrollView = GetComponentInChildren<ScrollRect>();

            ColorPresets = new List<Tuple<Color, Color>>()
            {
                new Tuple<Color, Color>(Color.white, Color.red),
                new Tuple<Color, Color>(MakeColor(254, 127, 45), MakeColor(35, 61, 77)),
                new Tuple<Color, Color>(MakeColor(126, 47, 142), MakeColor(63, 142, 47)),
                new Tuple<Color, Color>(MakeColor(10, 161, 245), MakeColor(245, 94, 10)),
                new Tuple<Color, Color>(MakeColor(15, 240, 146), MakeColor(240, 15, 109)),
                new Tuple<Color, Color>(MakeColor(32, 194, 223), MakeColor(223, 61, 32))
            };
        }

        private Color MakeColor(int r, int g, int b)
        {
            return new Color(r/255f, g/255f, b/255f);
        }

        void Update()
        {
            if (_pathRecordings == null)
            {
                return;
            }

            if (_pathRecordings.Count == 0 || ColorPresets.Count == 0)
            {
                _addPathButton.enabled = false;
            }
            else
            {
                _addPathButton.enabled = true;
            }
        }

        /// <summary>
        /// Removes path from scene and scrollview
        /// </summary>
        /// <param name="path"></param>
        public void RemovePath(StatisticsEntry path)
        {
            ColorPresets.Insert(0, path.GetColors());
            _activePaths.Remove(path);
            Destroy(path.gameObject.GetComponent<StatisticsEntry>());
            _pathRecordings.Add(path.FileName);
            _pathsDropdown.ClearOptions();
            _pathsDropdown.AddOptions(_pathRecordings);
            Destroy(path.gameObject);
        }

        /// <summary>
        ///  Refreshes list of available path recordings
        /// </summary>
        public void RefreshPaths()
        {
            _pathRecordings = GetRecordings();
            foreach (StatisticsEntry entry in _activePaths)
            {
                if (_pathRecordings.Contains(entry.FileName))
                {
                    _pathRecordings.Remove(entry.FileName);
                }
            }
            _pathsDropdown.ClearOptions();
            _pathsDropdown.AddOptions(_pathRecordings);
        }

        /// <summary>
        /// Adds path to scene and scroll view
        /// </summary>
        private void AddStatisticsInformation()
        {
            GameObject t = Instantiate(TripInfoPrefab) as GameObject;
            t.transform.SetParent(_scrollViewContent, false);

            string _filename = _pathRecordings[_pathsDropdown.value];
            StatisticsEntry se;
            se = t.AddComponent(typeof(StatisticsEntry)) as StatisticsEntry;

            se.FileName = _filename;
            se.ParentController = this;
            se.cameraViewController = transform.Find("Panel/TopDownCamera/CameraView").gameObject.GetComponent<MousePointToImagePointController>();
            _activePaths.Add(se);
            _pathRecordings.Remove(_filename);
            _pathsDropdown.ClearOptions();
            _pathsDropdown.AddOptions(_pathRecordings);
        }

        /// <summary>
        /// Loads up list of all path recordings from PathRecordings subfolder
        /// </summary>
        /// <returns></returns>
        private List<string> GetRecordings()
        {
            List<string> recordings = new List<string>();
            string PathRecordingsPath = System.IO.Path.Combine(Application.dataPath, "PathRecordings");
            if (!System.IO.Directory.Exists(PathRecordingsPath))
            {
                System.IO.Directory.CreateDirectory(PathRecordingsPath);
            }
            string [] fileEntries = System.IO.Directory.GetFiles(PathRecordingsPath);

            foreach(string fileName in fileEntries)
            {
                var f = System.IO.Path.GetFileName(fileName);
                if (f.EndsWith(".json"))
                {
                    recordings.Add(f);
                }
            }
            return recordings;
        }

        public Tuple<Color, Color> GetAvailableColorPreset()
        {
            if (ColorPresets.Count == 0)
            {
                return null;
            }
            var colors = ColorPresets[0];
            ColorPresets.RemoveAt(0);
            return colors;
        }
    }

    /// <summary>
    /// Wrapper class for single path statistics and controller for UI representation
    /// </summary>
    public class StatisticsEntry : MonoBehaviour
    {
        /// <summary>
        /// Reference to parent script for communicating
        /// </summary>
        public PathRecordingsVisualization ParentController;
        public MousePointToImagePointController cameraViewController;

        /// <summary>
        /// Recording filename
        /// </summary>
        public string FileName;
        private Slider _sizeSlider;
        private Button _removeButton;
        private Button _focusButton;
        private Image _pointColorRepr;
        private Image _lineColorRepr;
        private Text _name;
        private UnityEngine.Transform _labels;
        private Path path;
        private Color activePointColor;
        private Color activeLineColor;
        private List<Vector3> pathPositions;
        private List<double> pathTimestamps;
        private PathVisualController visualController;

        private const float _ratio = 1.61f;
        private int index = 5;
        void Start()
        {
            _pointColorRepr = transform.Find("ColorObjects/PointColorMarker").gameObject.GetComponent<Image>();
            _lineColorRepr = transform.Find("ColorObjects/LineColorMarker").gameObject.GetComponent<Image>();

            pathPositions = new List<Vector3>();
            pathTimestamps = new List<double>();

            _labels = transform.Find("Labels");

            _sizeSlider = transform.Find("ColorObjects/SizeSlider").GetComponent<Slider>();
            _sizeSlider.minValue = 0f;
            _sizeSlider.maxValue = 2.0f;
            _sizeSlider.value = 0.8f;
            _sizeSlider.onValueChanged.AddListener(ChangeSize);

            _removeButton = transform.Find("Buttons/DisableButton").gameObject.GetComponent<Button>();
            _removeButton.onClick.AddListener(DestroyPath);

            _focusButton = transform.Find("Buttons/FocusButton").gameObject.GetComponent<Button>();
            _focusButton.onClick.AddListener(FocusPath);

            _name = transform.Find("PathNameLabel").gameObject.GetComponent<Text>();
            _name.text = FileName;

            DrawPath();
        }

        public void ChangeSize(float value)
        {
            if (visualController != null && path != null)
            {
                visualController.SetLineThickness(value);
                visualController.SetPointSize(_ratio * value);
            }
        }

        void RemovePath()
        {
            if (path != null)
            {
                ParentController.RemovePath(this);
                path = null;
            }
        }

        void DestroyPath()
        {
            if (path != null)
            {
                path.Destroy();
                path = null;
            }
        }

        void FocusPath()
        {
            if (path != null)
            {
                Vector3 pathDimension = path.GetPathDimension();
                cameraViewController.Focus(path.GetPathPosition(), Math.Max(pathDimension.x, pathDimension.z));
            }
        }

        void DrawPath()
        {
            try
            {
                var completeFilename = System.IO.Path.Combine(Application.dataPath, "PathRecordings", FileName);

                List<LogRecord<Vector3>> records = DataLoggerUtilities.GetLogRecordsFromFile<Vector3>(completeFilename);
                foreach (var record in records)
                {
                    pathPositions.Add(record.Value);
                    pathTimestamps.Add(record.TimeStamp.Subtract(new DateTime(1970,1,1,0,0,0)).TotalSeconds);
                }
                CalculateStats();
                var everyNth = (pathPositions.Where((x,i) => i % index == 0)).ToList();
                path = Visualizer.Instance.AddPath(everyNth, "Path-" + FileName);
                path.OnDestroyPath.AddListener(RemovePath);

                var colors = ParentController.GetAvailableColorPreset();

                visualController = path.GetPathGameObject().GetComponent<PathVisualController>();
                activeLineColor = colors.Item2;
                activePointColor = colors.Item1;
                visualController.SetPointColor(activePointColor);
                _pointColorRepr.color = activePointColor;
                visualController.SetLineColor(activeLineColor);
                _lineColorRepr.color = activeLineColor;
                ChangeSize(_sizeSlider.value);
                FocusPath();
                Debug.Log("Path recording loaded: " + FileName);
            }
            catch(Exception e)
            {
                Debug.Log(e);
                Debug.Log("Invalid recording file");
                RemovePath();
            }
        }

        void CalculateStats()
        {
            float len = 0f;
            float maxDepth = 0;
            double duration = pathTimestamps[pathTimestamps.Count - 1] - pathTimestamps[0];

            for (int i = 0; i < pathPositions.Count - 1; i++)
            {
                len += Vector3.Distance(pathPositions[i], pathPositions[i+1]);
                if (Mathf.Abs(pathPositions[i].y) > maxDepth)
                {
                    maxDepth = Mathf.Abs(pathPositions[i].y);
                }
            }

            GameObject o1 = _labels.Find("DistanceVariableLabel").gameObject;
            Text t1 = o1.GetComponent<Text>();
            t1.text = string.Format("{0:0.##} meters", len);

            GameObject o2 = _labels.Find("TripTimeVariableLabel").gameObject;
            Text t2 = o2.GetComponent<Text>();
            t2.text = string.Format("{0:0.##} seconds", duration);

            GameObject o3 = _labels.Find("MaxDepthVariableLabel").gameObject;
            Text t3 = o3.GetComponent<Text>();
            t3.text = string.Format("{0:0.##} meters", maxDepth);
        }

        public Tuple<Color, Color> GetColors()
        {
            return new Tuple<Color, Color>(_pointColorRepr.color, _lineColorRepr.color);
        }
    }
}
