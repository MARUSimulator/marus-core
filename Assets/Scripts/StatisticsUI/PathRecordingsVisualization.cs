using System.Collections;
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
		Button m_Button;
		Dropdown m_Dropdown;

		Visualizer visualizer;
		
		private List<StatisticsEntry> activePaths;

		List<string> pathRecordings;
		
		private GameObject i1;
		private GameObject i2;
		
		void Start()
		{
			m_Button = GameObject.Find("AddPathButton").GetComponent<Button>();
			m_Button.onClick.AddListener(AddDiverPath);
			m_Dropdown = GetComponentInChildren<Dropdown>();
			pathRecordings = GetRecordings();
			m_Dropdown.AddOptions(pathRecordings);

			i1 = GameObject.Find("TripInfo1");
			i1.SetActive(false);
			i2 = GameObject.Find("TripInfo2");
			i2.SetActive(false);
			activePaths = new List<StatisticsEntry>();
		}

		public void RemovePath(StatisticsEntry path)
		{
			activePaths.Remove(path);
			Destroy(path.gameObject.GetComponent<StatisticsEntry>());
			pathRecordings.Add(path.FileName);
			m_Dropdown.ClearOptions();
			m_Dropdown.AddOptions(pathRecordings);
			path.gameObject.SetActive(false);
		}

		private void AddDiverPath()
		{
			string _filename = pathRecordings[m_Dropdown.value];
			StatisticsEntry se;
			if (!i1.activeSelf)
			{
				i1.SetActive(true);
				se = i1.AddComponent(typeof(StatisticsEntry)) as StatisticsEntry;
				
			}
			else
			{
				i2.SetActive(true);
				se = i2.AddComponent(typeof(StatisticsEntry)) as StatisticsEntry;
			}
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
	}

	public class StatisticsEntry : MonoBehaviour
	{
		public PathRecordingsVisualization ParentController;
		public string FileName;

		private Slider _slider;
		private Button _removeButton;
		private Dropdown _colorDropdown;
		private Image _colorRepr;
		private UnityEngine.Transform _labels;
		private Visualizer visualizer;

		private List<Color> colors = new List<Color> {Color.white, Color.yellow, Color.green, Color.red};
		private List<string> colorOptions = new List<string> {"White", "Yellow", "Green", "Red"};
		private LinearPath path;
		private Color activeColor;

		private List<Vector3> pathPositions;
		private List<double> pathTimestamps;
		
		
		void Start()
		{
			pathPositions = new List<Vector3>();
			pathTimestamps = new List<double>();

			visualizer = Visualizer.Instance;

			_labels = transform.Find("Labels");

			_slider = transform.Find("PointSizeSlider").gameObject.GetComponent<Slider>();
			_slider.minValue = 0.01f;
			_slider.maxValue = 2f;
			_slider.value = 0.5f;
			_slider.onValueChanged.AddListener(delegate {ChangeSize();});
			
			_removeButton = transform.Find("DisableButton").gameObject.GetComponent<Button>();
			_removeButton.onClick.AddListener(RemovePath);

			_colorDropdown = transform.Find("ColorDropdown").gameObject.GetComponent<Dropdown>();
			_colorDropdown.ClearOptions();
			_colorDropdown.AddOptions(colorOptions);
			_colorRepr = transform.Find("ColorMarker").gameObject.GetComponent<Image>();
			_colorDropdown.onValueChanged.AddListener(delegate {ChangeColor();});
			ChangeColor();

			DrawPath();
			
		}
		void ChangeColor()
		{
			activeColor = colors[_colorDropdown.value];
			_colorRepr.color = activeColor;

			if (path != null)
			{
				path.SetPointColor(activeColor);
			}
		}

		void ChangeSize()
		{
			if (path != null)
			{
				path.SetPointSize(_slider.value);
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
