using System;
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
        private Slider _pointSizeSlider;
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

        void Start()
        {
            _addPathButton = transform.Find("Panel/Canvas/PathSelector/AddPathButton").GetComponent<Button>();
            _addPathButton.onClick.AddListener(AddDiverPath);
            _pathsDropdown = GetComponentInChildren<Dropdown>();
            _pathRecordings = GetRecordings();
            _pathsDropdown.AddOptions(_pathRecordings);
            _activePaths = new List<StatisticsEntry>();

            _pointSizeSlider = transform.Find("Panel/Canvas/PathSelector/PointSizeSlider").GetComponent<Slider>();
            _pointSizeSlider.minValue = 0.01f;
            _pointSizeSlider.maxValue = 4f;
            _pointSizeSlider.value = 1f;
            _pointSizeSlider.onValueChanged.AddListener(delegate {ChangePointSize();});

            _scrollView = GetComponentInChildren<ScrollRect>();
        }

        /// <summary>
        /// Removes path from scene and scrollview
        /// </summary>
        /// <param name="path"></param>
        public void RemovePath(StatisticsEntry path)
        {
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
        private void AddDiverPath()
        {
            GameObject t = Instantiate(TripInfoPrefab) as GameObject;
            t.transform.SetParent(_scrollViewContent, false);

            string _filename = _pathRecordings[_pathsDropdown.value];
            StatisticsEntry se;
            se = t.AddComponent(typeof(StatisticsEntry)) as StatisticsEntry;

            se.FileName = _filename;
            se.ParentController = this;
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
                var i = fileName.LastIndexOf('/');
                var f = fileName.Substring(i);
                if (f.EndsWith(".json"))
                {
                    recordings.Add(f);
                }
            }
            return recordings;
        }

        /// <summary>
        /// Change point size for all added paths
        /// </summary>
        public void ChangePointSize()
        {
            foreach (StatisticsEntry se in _activePaths)
            {
                se.ChangeSize(_pointSizeSlider.value);
            }
        }

        /// <summary>
        /// Helper function to disable/enable scroll while color picker is active/inactive
        /// </summary>
        public void ToggleScroll()
        {
            if (_scrollView.isActiveAndEnabled == true)
            {
                _scrollView.enabled = false;
            }
            else
            {
                _scrollView.enabled = true;
            }
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

        /// <summary>
        /// Recording filename
        /// </summary>
        public string FileName;

        private GameObject _colorPicker;
        private Slider _lineSlider;
        private Button _removeButton;
        private Button _colorButton;
        private Image _colorRepr;
        private Text _name;
        private UnityEngine.Transform _labels;
        private List<Color> colors = new List<Color> {Color.white, Color.yellow, Color.green, Color.red};
        private List<string> colorOptions = new List<string> {"White", "Yellow", "Green", "Red"};
        private Path3D path;
        private Color activeColor;
        private Color lastColor;
        private List<Vector3> pathPositions;
        private List<double> pathTimestamps;

        void Start()
        {
            _colorPicker = transform.Find("FlexibleColorPicker").gameObject;
            _colorPicker.SetActive(false);

            _colorRepr = transform.Find("ColorMarker").gameObject.GetComponent<Image>();
            _colorRepr.color = Color.white;

            pathPositions = new List<Vector3>();
            pathTimestamps = new List<double>();

            _labels = transform.Find("Labels");

            _lineSlider = transform.Find("LineWidthSlider").GetComponent<Slider>();
            _lineSlider.minValue = 0.01f;
            _lineSlider.maxValue = 5f;
            _lineSlider.value = 0.05f;
            _lineSlider.onValueChanged.AddListener(delegate {ChangeLineSize();});

            _removeButton = transform.Find("DisableButton").gameObject.GetComponent<Button>();
            _removeButton.onClick.AddListener(RemovePath);

            _name = transform.Find("PathNameLabel").gameObject.GetComponent<Text>();
            _name.text = FileName;

            _colorButton = transform.Find("ChangeColorButton").gameObject.GetComponent<Button>();
            _colorButton.onClick.AddListener(ChooseColor);
            activeColor = Color.white;
            lastColor = Color.white;
            ChangeLineSize();

            DrawPath();
            ParentController.ChangePointSize();
        }

        void ChooseColor()
        {
            if (_colorPicker.activeSelf)
            {
                _colorPicker.SetActive(false);
                ParentController.ToggleScroll();
                transform.Find("ChangeColorButton").gameObject.GetComponentInChildren<Text>().text = "Change color";
            }
            else
            {
                _colorPicker.SetActive(true);
                ParentController.ToggleScroll();
                transform.Find("ChangeColorButton").gameObject.GetComponentInChildren<Text>().text = "OK";
            }
        }

        void Update()
        {
            if (_colorPicker != null && _colorPicker.activeSelf)
            {
                activeColor = _colorPicker.GetComponent<FlexibleColorPicker>().color;
                ChangeColor();
            }
        }

        void ChangeColor()
        {
            _colorRepr.color = activeColor;
            if (path != null)
            {
                path.SetPointColor(activeColor);
            }
        }

        public void ChangeSize(float value)
        {
            if (path != null)
            {
                path.SetPointSize(value);
            }
        }

        void ChangeLineSize()
        {
            if (path != null)
            {
                path.SetLineThickness(_lineSlider.value);
            }
        }

        void RemovePath()
        {
            if (path != null)
            {
                path.Destroy();
                ParentController.RemovePath(this);
            }
        }

        void DrawPath()
        {
            /*try
            {*/
                var completeFilename = System.IO.Path.Combine(Application.dataPath, "PathRecordings") + FileName;

                List<LogRecord<Vector3>> records = DataLoggerUtilities.GetLogRecordsFromFile<Vector3>(completeFilename);
                foreach (var record in records)
                {
                    pathPositions.Add(record.Value);
                    pathTimestamps.Add(record.TimeStamp.Subtract(new DateTime(1970,1,1,0,0,0)).TotalSeconds);
                }

                path = new Path3D(pathPositions, 0.5f, activeColor, 0.05f, Color.red);

                path.Draw();
                CalculateStats();
            /*}
            catch
            {
                Debug.Log("Invalid recording file");
                RemovePath();
            }*/
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
    }
}
