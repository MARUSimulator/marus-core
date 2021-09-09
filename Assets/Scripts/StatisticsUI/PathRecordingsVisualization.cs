using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Labust.Visualization;
using Labust.Visualization.Primitives;
using Labust.Utils;

namespace Labust.StatisticsUI
{
	public class PathRecordingsVisualization : MonoBehaviour
	{
		private Button m_Button;
		private Dropdown m_Dropdown;
		private Slider _slider;
		private ScrollRect _scrollView;
		
		private List<StatisticsEntry> activePaths;

		private List<string> pathRecordings;

		public UnityEngine.Transform _scrollViewContent;
		
		public GameObject tripInfo;
		
		void Start()
		{
			m_Button = GameObject.Find("AddPathButton").GetComponent<Button>();
			m_Button.onClick.AddListener(AddDiverPath);
			m_Dropdown = GetComponentInChildren<Dropdown>();
			pathRecordings = GetRecordings();
			m_Dropdown.AddOptions(pathRecordings);
			activePaths = new List<StatisticsEntry>();

			_slider = GameObject.Find("PointSizeSlider").GetComponent<Slider>();
			_slider.minValue = 0.01f;
			_slider.maxValue = 4f;
			_slider.value = 1f;
			_slider.onValueChanged.AddListener(delegate {ChangePointSize();});

			_scrollView = GetComponentInChildren<ScrollRect>();
		}

		public void RemovePath(StatisticsEntry path)
		{
			activePaths.Remove(path);
			Destroy(path.gameObject.GetComponent<StatisticsEntry>());
			pathRecordings.Add(path.FileName);
			m_Dropdown.ClearOptions();
			m_Dropdown.AddOptions(pathRecordings);
			Destroy(path.gameObject);
		}

		public void RefreshPaths()
		{
			pathRecordings = GetRecordings();
			foreach (StatisticsEntry entry in activePaths)
			{
				if (pathRecordings.Contains(entry.FileName))
				{
					pathRecordings.Remove(entry.FileName);
				}
			}
			m_Dropdown.ClearOptions();
			m_Dropdown.AddOptions(pathRecordings);
		}

		private void AddDiverPath()
		{
			GameObject t = Instantiate(tripInfo) as GameObject;
			t.transform.SetParent(_scrollViewContent, false);

			string _filename = pathRecordings[m_Dropdown.value];
			StatisticsEntry se;
			se = t.AddComponent(typeof(StatisticsEntry)) as StatisticsEntry;
			
			se.FileName = _filename;
			se.ParentController = this;
			activePaths.Add(se);
			pathRecordings.Remove(_filename);
			m_Dropdown.ClearOptions();
			m_Dropdown.AddOptions(pathRecordings);
		}

		private List<string> GetRecordings()
		{
			List<string> recordings = new List<string>();
			string PathRecordingsPath = Path.Combine(Application.dataPath, "PathRecordings");
			string [] fileEntries = Directory.GetFiles(PathRecordingsPath);
			
			foreach(string fileName in fileEntries)
			{
				var i = fileName.LastIndexOf('/');
				var f = fileName.Substring(i);
				if (f.EndsWith(".bin"))
				{
					recordings.Add(f);
				}
			}
			return recordings;
		}

		public void ChangePointSize()
		{
			foreach (StatisticsEntry se in activePaths)
			{
				se.ChangeSize(_slider.value);
			}
		}

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

	public class StatisticsEntry : MonoBehaviour
	{
		public PathRecordingsVisualization ParentController;
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
		private LinearPath path;
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
			if (_colorPicker != null &&_colorPicker.activeSelf)
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
			path.Destroy();
			ParentController.RemovePath(this);

		}

		void DrawPath()
		{
			var completeFilename = Path.Combine(Application.dataPath, "PathRecordings") + FileName;
			using (PathRecordingBinaryReader reader = new PathRecordingBinaryReader(File.Open(completeFilename, FileMode.Open))) 
			{
				while (reader.BaseStream.Position != reader.BaseStream.Length) {
					(Vector3 pos, double timestamp) = reader.ReadVector();
					pathPositions.Add(pos);
					pathTimestamps.Add(timestamp);
				}
			}
			path = new LinearPath(pathPositions, 0.5f, activeColor, 0.05f, Color.red);

			path.Draw();
			CalculateStats();
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
