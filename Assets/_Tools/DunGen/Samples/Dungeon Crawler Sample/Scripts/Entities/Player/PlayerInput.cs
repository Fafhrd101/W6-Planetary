using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DunGen.DungeonCrawler
{
	sealed class PlayerInput : MonoBehaviour
	{
		[SerializeField]
		private float clickRepeatInterval = 0.5f;

		[SerializeField]
		private ClickToMove movement = null;
		[SerializeField]
		private ClickableObjectHandler clickableObjectHandler = null;
		[SerializeField]
		private Camera playerCamera = null;
		private float _lastClickTime;
		private PrTopDownCharController _controller;
		private CanvasHitDetector[] _canvas;
		private void Awake()
		{
			_controller = GetComponent<PrTopDownCharController>();
			_canvas = FindObjectsOfType<CanvasHitDetector>();
		}
		private void Update()
		{
			if (_controller.m_isDead) return;
			if (Player.Instance.inputDisabled) return;
			if (_canvas.Any(canvas => canvas.ui)) return;
			if (EventSystem.current.IsPointerOverGameObject())
			{
				//print("Over UI");
				return;
			}
			if (Input.GetMouseButton(0) && !Input.GetMouseButton(1))
			{
				bool newlyPressed = Input.GetMouseButtonDown(0);
	
				if(newlyPressed)
					clickableObjectHandler.Click();

				if(	clickableObjectHandler.HoverClickable == null &&
					Time.time >= _lastClickTime + clickRepeatInterval)
					MoveToCursor();
			}

			// if (Input.GetMouseButtonUp(0))
			// 	movement.StopManualMovement();
		}

		private void MoveToCursor()
		{
			bool newlyPressed = Input.GetMouseButtonDown(0);
			var cursorRay = playerCamera.ScreenPointToRay(Input.mousePosition);
			movement.Click(cursorRay, newlyPressed);

			_lastClickTime = Time.time;
		}
	}
}
