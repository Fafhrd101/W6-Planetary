using UnityEngine;

namespace DunGen.Demo
{
	public class AutoDoor : MonoBehaviour
	{
		public enum DoorState
		{
			Open,
			Closed,
			Opening,
			Closing,
		}
		[Tooltip("Disable this object, so navmesh can properly generate")]
		public GameObject door;
		public Vector3 openOffset = new Vector3(0, -7f, 0);
		public float speed = 3.0f;
		public string doorStateString;
		private Vector3 _closedPosition;
		private DoorState _currentState = DoorState.Open;
		private float _currentFramePosition = 0.0f;
		private Door _doorComponent;


		private void Start()
		{
			// Navmesh done, activate door
			door.SetActive(true);
			_doorComponent = door.GetComponent<Door>();
			_closedPosition = door.transform.localPosition;
		}

		private void Update()
		{
			if (_currentState == DoorState.Opening || _currentState == DoorState.Closing)
			{
				Vector3 openPosition = _closedPosition + openOffset;

				float frameOffset = speed * Time.deltaTime;

				if (_currentState == DoorState.Closing)
					frameOffset *= -1;

				_currentFramePosition += frameOffset;
				_currentFramePosition = Mathf.Clamp(_currentFramePosition, 0, 1);

				door.transform.localPosition = Vector3.Lerp(_closedPosition, openPosition, _currentFramePosition);

				// Finished
				if (_currentFramePosition == 1.0f)
					_currentState = DoorState.Open;
				else if (_currentFramePosition == 0.0f)
				{
					_currentState = DoorState.Closed;
					if (_doorComponent)
						_doorComponent.IsOpen = false;
				}
			}

			doorStateString = _currentState.ToString();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.GetComponent<PrTopDownCharController>()/* && !other.GetComponent<PrNPCAI>()*/) return;
			_currentState = DoorState.Opening;
			if (_doorComponent)
				_doorComponent.IsOpen = true;
		}
		
		private void OnTriggerExit(Collider other)
		{
			if (!other.GetComponent<PrTopDownCharController>()/* && !other.GetComponent<PrNPCAI>()*/) return;
			_currentState = DoorState.Closing;
		}
	}
}