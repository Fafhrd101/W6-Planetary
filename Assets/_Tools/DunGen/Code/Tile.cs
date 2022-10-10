using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DunGen.DungeonCrawler;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace DunGen
{
	[AddComponentMenu("DunGen/Tile")]
	public class Tile : MonoBehaviour, ISerializationCallbackReceiver
	{
		public const int CurrentFileVersion = 1;

		#region Legacy Properties

		// Legacy properties only exist to avoid breaking existing projects
		// Converting old data structures over to the new ones

		[SerializeField]
		[FormerlySerializedAs("AllowImmediateRepeats")]
		private bool allowImmediateRepeats = true;

		#endregion

		/// <summary>
		/// Should this tile be allowed to rotate to fit in place?
		/// </summary>
		public bool AllowRotation = true;

		/// <summary>
		/// Should this tile be allowed to be placed next to another instance of itself?
		/// </summary>
		public TileRepeatMode RepeatMode = TileRepeatMode.Allow;

		/// <summary>
		/// Should the automatically generated tile bounds be overriden with a user-defined value?
		/// </summary>
		public bool OverrideAutomaticTileBounds = false;

		/// <summary>
		/// Optional tile bounds to override the automatically calculated tile bounds
		/// </summary>
		public Bounds TileBoundsOverride = new Bounds(Vector3.zero, Vector3.one);

		/// <summary>
		/// An optional entrance doorway. DunGen will try to use this doorway as the entrance to the tile if possible
		/// </summary>
		public Doorway Entrance;

		/// <summary>
		/// An optional exit doorway. DunGen will try to use this doorway as the exit to the tile if possible
		/// </summary>
		public Doorway Exit;

		/// <summary>
		/// Should this tile override the connection chance globally defined in the DungeonFlow?
		/// </summary>
		public bool OverrideConnectionChance = false;

		/// <summary>
		/// The overriden connection chance value. Only used if <see cref="OverrideConnectionChance"/> is true.
		/// If both tiles have overriden the connection chance, the lowest value is used
		/// </summary>
		public float ConnectionChance = 0f;

		/// <summary>
		/// The calculated world-space bounds of this Tile
		/// </summary>
		[HideInInspector]
		public Bounds Bounds { get { return transform.TransformBounds(Placement.LocalBounds); } }

		public bool doNotTree;
		public float tileSize;
		/// <summary>
		/// Information about the tile's position in the generated dungeon
		/// </summary>
		public TilePlacementData Placement
		{
			get { return placement; }
			internal set { placement = value; }
		}
		/// <summary>
		/// The dungeon that this tile belongs to
		/// </summary>
		public Dungeon Dungeon { get; internal set; }

		public List<Doorway> AllDoorways = new List<Doorway>();
		public List<Doorway> UsedDoorways = new List<Doorway>();
		public List<Doorway> UnusedDoorways = new List<Doorway>();

		[SerializeField]
		private TilePlacementData placement;
		[SerializeField]
		private int fileVersion;
		[SerializeField]
		public List<GameObject> trees;
		public void Start()
		{
			var _setup = GameObject.FindObjectOfType<PlanetaryBaseSetup>();
			_setup.totalTiles.Add(this.transform);
			tileSize = Bounds.size.magnitude;
		}
		internal void AddTriggerVolume()
		{
			BoxCollider triggerVolume = gameObject.AddComponent<BoxCollider>();
			triggerVolume.center = Placement.LocalBounds.center;
			triggerVolume.size = Placement.LocalBounds.size;
			triggerVolume.isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other == null)
				return;

			DungenCharacter character = other.gameObject.GetComponent<DungenCharacter>();

			if (character != null)
				character.HandleTileChange(this);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Bounds? bounds = null;
			
			if (OverrideAutomaticTileBounds)
				bounds = transform.TransformBounds(TileBoundsOverride);
			else if (placement != null)
				bounds = Bounds;

			if (bounds.HasValue)
				Gizmos.DrawWireCube(bounds.Value.center, bounds.Value.size);
		}

		public IEnumerable<Tile> GetAdjactedTiles()
		{
			return UsedDoorways.Select(x => x.ConnectedDoorway.Tile).Distinct();
		}

		public bool IsAdjacentTo(Tile other)
		{
			foreach (var door in UsedDoorways)
				if (door.ConnectedDoorway.Tile == other)
					return true;

			return false;
		}

		public void CreateLandscape(GameObject tempTreePrefab)
		{
			if (doNotTree)
				return;
			var counter = 0;
			var maxAttempts = 30;

			while(counter < 40 && maxAttempts > 0)
			{
				Vector3 randomPoint = transform.position + Random.insideUnitSphere * 20;
				randomPoint.y = 0f;
				
				if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
				{
					// GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					// cube.transform.position = randomPoint;
					maxAttempts--;
					continue;
				}
				
				// GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				// sphere.transform.position = randomPoint;
					randomPoint.y = -.1f;
					var tree = GameObject.Instantiate(tempTreePrefab, this.transform);
					trees.Add(tree);
					tree.transform.position = randomPoint;
					tree.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.4f, 1f);
				counter++;
			}
			// if (counter < 5)
			// 	print("Bad tile..."+counter);
		}
		#region ISerializationCallbackReceiver Implementation

		public void OnBeforeSerialize()
		{
			fileVersion = CurrentFileVersion;
		}

		public void OnAfterDeserialize()
		{
			if (fileVersion < 1)
				RepeatMode = (allowImmediateRepeats) ? TileRepeatMode.Allow : TileRepeatMode.DisallowImmediate;
		}

		#endregion
	}
}
