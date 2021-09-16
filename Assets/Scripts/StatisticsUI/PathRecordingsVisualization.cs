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
            // instantiate visualizer
            var v = Visualizer.Instance;
            _addPathButton = transform.Find("Panel/Canvas/PathSelector/AddPathButton").GetComponent<Button>();
            _addPathButton.onClick.AddListener(AddStatisticsInformation);
            _pathsDropdown = GetComponentInChildren<Dropdown>();
            _pathRecordings = GetRecordings();
            _pathsDropdown.AddOptions(_pathRecordings);
            _activePaths = new List<StatisticsEntry>();

            _scrollView = GetComponentInChildren<ScrollRect>();
        }

        void Update()
        {
            if (_pathRecordings == null)
            {
                return;
            }

            if (_pathRecordings.Count == 0)
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

        private GameObject _pointColorPicker;
        private GameObject _lineColorPicker;
        private Slider _lineSlider;
        private Slider _pointSizeSlider;
        private Button _removeButton;
        private Button _confirmColorButton;
        private Button _pointColorButton;
        private Button _lineColorButton;
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

        void Start()
        {
            _pointColorPicker = transform.Find("ColorObjects/PointColorPicker").gameObject;
            _pointColorPicker.SetActive(false);
            _lineColorPicker = transform.Find("ColorObjects/LineColorPicker").gameObject;
            _lineColorPicker.SetActive(false);

            _pointColorRepr = transform.Find("ColorObjects/PointColorMarker").gameObject.GetComponent<Image>();
            _lineColorRepr = transform.Find("ColorObjects/LineColorMarker").gameObject.GetComponent<Image>();
            _pointColorButton = transform.Find("ColorObjects/SetPointColorButton").gameObject.GetComponent<Button>();
            _pointColorButton.onClick.AddListener(ChoosePointColor);
            _lineColorButton = transform.Find("ColorObjects/SetLineColorButton").gameObject.GetComponent<Button>();
            _lineColorButton.onClick.AddListener(ChooseLineColor);

            _confirmColorButton = transform.Find("ColorObjects/ConfirmColorButton").gameObject.GetComponent<Button>();
            _confirmColorButton.gameObject.SetActive(false);
            _confirmColorButton.onClick.AddListener(CloseColorPickers);

            pathPositions = new List<Vector3>();
            pathTimestamps = new List<double>();

            _labels = transform.Find("Labels");

            _lineSlider = transform.Find("LineWidthSlider").GetComponent<Slider>();
            _lineSlider.minValue = 0.01f;
            _lineSlider.maxValue = 2f;
            _lineSlider.value = 0.55f;
            _lineSlider.onValueChanged.AddListener(delegate {ChangeLineSize();});

            _pointSizeSlider = transform.Find("PointSizeSlider").GetComponent<Slider>();
            _pointSizeSlider.minValue = 0.01f;
            _pointSizeSlider.maxValue = 4f;
            _pointSizeSlider.value = 0.8f;
            _pointSizeSlider.onValueChanged.AddListener(ChangeSize);

            _removeButton = transform.Find("DisableButton").gameObject.GetComponent<Button>();
            _removeButton.onClick.AddListener(DestroyPath);

            _name = transform.Find("PathNameLabel").gameObject.GetComponent<Text>();
            _name.text = FileName;

            DrawPath();
        }

        void Update()
        {
            if (visualController == null)
            {
                return;
            }

            if (_pointColorPicker != null && _pointColorPicker.activeSelf)
            {
                activePointColor = _pointColorPicker.GetComponent<FlexibleColorPicker>().color;
                visualController.PointColor = activePointColor;
                _pointColorRepr.color = activePointColor;
            }

            if (_lineColorPicker != null && _lineColorPicker.activeSelf)
            {
                activeLineColor = _lineColorPicker.GetComponent<FlexibleColorPicker>().color;
                visualController.LineColor = activeLineColor;
                _lineColorRepr.color = activeLineColor;
            }

            if (_lineSlider.value != visualController.LineThickness)
            {
                _lineSlider.value = visualController.LineThickness;
            }

            if (_pointSizeSlider.value != visualController.PointSize)
            {
                _pointSizeSlider.value = visualController.PointSize;
            }

            if (activePointColor != visualController.PointColor)
            {
                activePointColor = visualController.PointColor;
                _pointColorRepr.color = activePointColor;
            }

            if (activeLineColor != visualController.LineColor)
            {
                activeLineColor = visualController.LineColor;
                _lineColorRepr.color = activeLineColor;
            }
        }

        void CloseColorPickers()
        {
            _pointColorPicker.SetActive(false);
            _lineColorPicker.SetActive(false);
            transform.Find("ColorObjects/ConfirmColorButton").gameObject.SetActive(false);
            ParentController.ToggleScroll();
        }

        void ChoosePointColor()
        {
            if (_pointColorPicker.activeSelf)
            {
                _pointColorPicker.SetActive(false);
                ParentController.ToggleScroll();
                transform.Find("ColorObjects/ConfirmColorButton").gameObject.SetActive(false);
            }
            else
            {
                _pointColorPicker.SetActive(true);
                ParentController.ToggleScroll();
                transform.Find("ColorObjects/ConfirmColorButton").gameObject.SetActive(true);
            }
        }

        void ChooseLineColor()
        {
            if (_lineColorPicker.activeSelf)
            {
                _lineColorPicker.SetActive(false);
                ParentController.ToggleScroll();
                transform.Find("ColorObjects/ConfirmColorButton").gameObject.SetActive(false);
            }
            else
            {
                _lineColorPicker.SetActive(true);
                ParentController.ToggleScroll();
                transform.Find("ColorObjects/ConfirmColorButton").gameObject.SetActive(true);
            }
        }

        public void ChangeSize(float value)
        {
            if (visualController != null && path != null)
            {
                visualController.PointSize = value;
            }
        }

        void ChangeLineSize()
        {
            if (visualController != null && path != null)
            {
                visualController.LineThickness = _lineSlider.value;
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

        void DrawPath()
        {
            /*try
            {*/
                var completeFilename = System.IO.Path.Combine(Application.dataPath, "PathRecordings", FileName);

                List<LogRecord<Vector3>> records = DataLoggerUtilities.GetLogRecordsFromFile<Vector3>(completeFilename);
                foreach (var record in records)
                {
                    pathPositions.Add(record.Value);
                    pathTimestamps.Add(record.TimeStamp.Subtract(new DateTime(1970,1,1,0,0,0)).TotalSeconds);
                }
                path = Visualizer.Instance.AddPath(pathPositions, "Path-" + FileName);
                path.OnDestroyPath.AddListener(RemovePath);
                CalculateStats();
                visualController = path.GetPathGameObject().GetComponent<PathVisualController>();
                _pointColorRepr.color = visualController.PointColor;
                _pointColorPicker.GetComponent<FlexibleColorPicker>().color = visualController.PointColor;
                _lineColorRepr.color = visualController.LineColor;
                _lineColorPicker.GetComponent<FlexibleColorPicker>().color = visualController.LineColor;
                Debug.Log("Path recording loaded: " + FileName);
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
