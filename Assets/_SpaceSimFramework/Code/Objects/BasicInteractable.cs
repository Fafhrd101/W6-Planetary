using System;
using DunGen.DungeonCrawler;
using UnityEngine;

public class BasicInteractable : MonoBehaviour, IClickableObject
{
	[Tooltip("The cursor to use when the player hovers over it with the mouse")]
	public Texture2D hoverCursor = null;
    [Tooltip("How close the player has to be to interact. If further away, the player will run to it first")]
    public float interactDistance = 2f;
    [Tooltip("Only good one time?")]
    public bool oneShot;
    private bool _depleted = false;
    [Header("Settings")]
    public bool IsEnabled = true;
    public string MessageToSend = "Action";
    public float UseDelay = 1.0f;
    private Transform lookAt;
    
    public void OnTriggerEnter(Collider other)
    {
	    //throw new NotImplementedException();
    }

    public void OnTriggerStay(Collider other)
    {
	    // if (other.CompareTag("Player"))
		   //  lookAt = other.transform;
    }

    public void OnTriggerExit(Collider other)
    {
	    if (other.CompareTag("Player"))
		    lookAt = null;
    }

    public void FixedUpdate()
    {
	    if (lookAt)
	    {
		    var direction = lookAt.position - transform.position;
		    var toRotation = Quaternion.FromToRotation(transform.forward, direction);
		    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 1f/*speed*/ * Time.time);
	    }  
    }

    public bool CanInteract()
		{
			return !_depleted;
		}

		public Texture2D GetHoverCursor()
		{
			return hoverCursor;
		}

		public void Interact()
		{
			var player = FindObjectOfType<ClickToMove>();
			float distanceToPlayer = (transform.position - player.transform.position).magnitude;

			// If we're in range to use the obelisk, just do it...
			if (distanceToPlayer <= interactDistance)
				Use(player);
			// ...otherwise, have the player run to the item instead
			else
				PathTo(player);
		}

		/// <summary>
		/// Have the player move to the obelisk's location
		/// </summary>
		/// <param name="player">The player's ClickToMove component</param>
		private void PathTo(ClickToMove player)
		{
			// Tell the player to move to a point in front of the object
			var transform1 = transform;
			var destination = transform1.position + transform1.forward * 0.5f;

			player.StopManualMovement();
			player.MoveTo(destination, true);
		}

		private void Use(ClickToMove player)
		{
			if (oneShot)
				_depleted = true;
			if (_depleted)
				return;
			
			//print("BasicInteractable used. Message "+MessageToSend);
			SendMessage(MessageToSend, player.transform, SendMessageOptions.RequireReceiver);
		}
}
