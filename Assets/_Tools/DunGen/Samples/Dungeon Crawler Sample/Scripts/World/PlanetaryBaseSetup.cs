using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Performs some game-specific logic after the dungeon has been generated.
	/// Must be attached to the same GameObject as the dungeon generator
	/// </summary>
	[RequireComponent(typeof(RuntimeDungeon))]
	public sealed class PlanetaryBaseSetup : MonoBehaviour
	{
		[SerializeField] [Tooltip("The player prefab to spawn once the dungeon is complete")]
		private GameObject playerPrefab;

		public PlanetaryFunction planetaryFunction;
		public GameObject[] landscapePrefabs;
		[TextArea] public string note = "All below, we simply keep track of everything.";
		private RuntimeDungeon _runtimeDungeon;
		private GameObject _spawnedPlayerInstance;
		public GameObject[] waypoints;

		// ReSharper disable once InconsistentNaming
		public List<POI> POIs;
		public List<Transform> totalShops;
		public List<Transform> totalHomes;
		public List<Transform> totalEmptyBuildings;
		public List<Transform> totalResourceZones;
		public List<Transform> totalJobs;
		[Space] 
		public List<GameObject> merchants;
		public List<GameObject> visitors;
		public List<GameObject> citizens;
		public List<GameObject> raiders;
		public List<GameObject> police;
		public List<GameObject> workers;
		[Space] 
		public List<GameObject> totalNpcs;
		public List<Transform> totalTiles;

		public bool landscapeBuilt = false;
		public List<GameObject> trees;

		private void OnEnable()
		{
			_runtimeDungeon = GetComponent<RuntimeDungeon>();
			_runtimeDungeon.Generator.OnGenerationStatusChanged += OnDungeonGenerationStatusChanged;
		}

		private void OnDisable()
		{
			_runtimeDungeon.Generator.OnGenerationStatusChanged -= OnDungeonGenerationStatusChanged;
		}

		private void OnDungeonGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
		{
			// We're only interested in completion events
			if (status != GenerationStatus.Complete)
				return;

			// If there's already a player instance, destroy it. We'll spawn a new one
			if (_spawnedPlayerInstance != null)
				Destroy(_spawnedPlayerInstance);

			// Find an object inside the start tile that's marked with the PlayerSpawn component
			var playerSpawn = generator.CurrentDungeon.MainPathTiles[0].GetComponentInChildren<PlayerSpawn>();

			Vector3 spawnPosition = playerSpawn.transform.position;
			_spawnedPlayerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
			//playerUI.SetPlayer(spawnedPlayerInstance);

			// All hideable objects are spawned by now,
			// we can cache some information for later use
			HideableObject.RefreshHierarchies();

			// Build our waypoint mesh
			// Lift the linecast off the ground. More accurate.
			var one = new Vector3(0, 0.25f, 0);
			waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
			//print("Waypoint count = "+waypoints.Length);
			foreach (var waypoint in waypoints)
			{
				foreach (var waypoint2 in waypoints)
				{
					if (waypoint == waypoint2)
						continue;
					if (Vector3.Distance(waypoint.transform.position, waypoint2.transform.position) > 100)
						continue;
					if (Physics.Linecast(waypoint.transform.position + one, waypoint2.transform.position + one,
						out var hit))
						if (hit.collider != null)
							continue;
					waypoint.GetComponent<Waypoint>().neighbors.Add(waypoint2.GetComponent<Waypoint>());
				}
				// if (waypoint.GetComponent<Waypoint>().neighbors.Count == 0)
				// 	Debug.Log("WTF?");
			}
		}

		private void Update()
		{
			if (landscapeBuilt || totalTiles.Count < 30)
				return;

			//print("Checking "+totalTiles.Count+" tiles");
			foreach (var x in totalTiles)
			{
				if (!x.GetComponent<Tile>())
				{
					print("Non-tile?!?");
					continue;
				}
				x.GetComponent<Tile>().CreateLandscape(landscapePrefabs[UnityEngine.Random.Range(0,landscapePrefabs.Length)]);
			}
			landscapeBuilt = true;
		}
	}
}
