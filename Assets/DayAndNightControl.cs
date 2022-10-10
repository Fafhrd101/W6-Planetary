using TMPro;
using UnityEngine;

namespace Npc_AI
{
	[System.Serializable]
	public class DayColors
	{
		public Color skyColor;
		public Color equatorColor;
		public Color horizonColor;
	}

	public class DayAndNightControl : Singleton<DayAndNightControl>
	{
		public bool startDay = true; //start game as day time
		public DayColors dawnColors;
		public DayColors dayColors;
		public DayColors nightColors;
		public int currentDay = 2135;
		public Light directionalLight; //the directional light in the scene we're going to work with
		[Tooltip("60 seconds per day...")]
		public float secondsInAFullDay = 120f;
		[Range(0, 1)]
		public float currentTime; 
		public int hour;
		public int minutes;
		private int _prevHour;
		[HideInInspector]
		public float timeMultiplier = 1f; //how fast the day goes by regardless of the secondsInAFullDay var. lower values will make the days go by longer, while higher values make it go faster. This may be useful if you're simulating seasons where daylight and night times are altered.
		public bool showUI;
		private float _lightIntensity; //static variable to see what the current light's intensity is in the inspector

		public Camera targetCam;

		public delegate void OnMorningListener();
		public event OnMorningListener OnMorningHandler;
		public delegate void OnEveningListener();
		public event OnEveningListener OnEveningHandler;
		public delegate void OnDayPassedListener();
		public event OnDayPassedListener OnDayPassedHandler;
		public delegate void OnHourPassedListener();
		public event OnHourPassedListener OnHourPassedHandler;
		
		public Canvas canvas;
		public TMP_Text starDate;
		public TMP_Text dayState;
		public TMP_Text timeOfDay;

		public bool dayCall = true;
		public bool nightCall = true;

		private void Start()
		{
			_lightIntensity = directionalLight.intensity; //what's the current intensity of the light
			if (startDay)
			{
				currentTime = 0.3f; //start at morning
			} // else grab time from server
			var firstPart = currentTime * 24+1;
			hour = (int)Mathf.Floor(firstPart);
			_prevHour = hour;
		}
		
		private void Update()
		{
			UpdateGUI();
			
			if (targetCam == null)
				targetCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
			
			UpdateLight();
			currentTime += (Time.deltaTime / secondsInAFullDay) * timeMultiplier;
			if (currentTime >= 1)
			{
				currentTime = 0;
				currentDay++;
				OnDayPassedHandler?.Invoke();
				dayCall = true;
				nightCall = true;
			}

			if (hour > _prevHour)
			{
				OnHourPassedHandler?.Invoke();
				_prevHour = hour;
			}
			// if (currentTime is < 0.5f and > 0.3f && _dayCall)
			// {
			// 	OnMorningHandler?.Invoke();
			// 	_dayCall = false;
			// }
			// if (currentTime > 0.7f && _nightCall)
			// {
			// 	OnEveningHandler?.Invoke();
			// 	_nightCall = false;
			// }
		}

		private void UpdateLight()
		{
			directionalLight.transform.localRotation = Quaternion.Euler((currentTime * 360f) - 90, 170, 0);
			// ^^ We rotate the sun 360 degrees around the x axis, or one full rotation times the current time variable.
			// Subtract 90 from this to make it go up in increments of 0.25.
			// 170 is where the sun will sit on the horizon line.
			// if it were at 180, or completely flat, it would be hard to see.
			// Tweak this value to what you find comfortable.

			var intensityMultiplier = currentTime switch
			{
				<= 0.23f or >= 0.75f => 0,
				<= 0.25f => Mathf.Clamp01((currentTime - 0.23f) * (1 / 0.02f)),
				<= 0.73f => Mathf.Clamp01(1 - ((currentTime - 0.73f) * (1 / 0.02f))),
				_ => 1
			};
			directionalLight.intensity = _lightIntensity * intensityMultiplier;
		}

		private string TimeOfDay()
		{
			var stateOfDay = hour switch
			{
				>= 24 => "Midnight",
				>= 0 and < 6 => "Midnight",
				>= 6 and < 12 => "Morning",
				>= 12 and < 17 => "Mid Noon",
				>= 17 and < 20 => "Evening",
				>= 20 and <= 24 => "Night",
				_ => "Error"
			};
			return stateOfDay;
		}

		private void UpdateGUI()
		{
			if (showUI)
			{
				if (!canvas.gameObject.activeSelf)
					canvas.gameObject.SetActive(true);
				
				var firstPart = currentTime * 24+1;
				hour = (int)Mathf.Floor(firstPart);

				var secondPart = firstPart % 1;
				minutes = (int)Mathf.Floor(secondPart * 60);
				
				starDate.text = "StarDate: " + currentDay;
				timeOfDay.text = "Time: " + hour.ToString("F0") + ":"
				                 + (minutes > 9 ? minutes.ToString("F0") : "0" + minutes.ToString("F0"));
				dayState.text = TimeOfDay();
				
			} else if (canvas.gameObject.activeSelf)
				canvas.gameObject.SetActive(false);
		}
	}
}
