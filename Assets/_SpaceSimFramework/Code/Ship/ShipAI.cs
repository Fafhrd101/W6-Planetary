using System;
using System.Collections;
using System.Collections.Generic;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
    /// Class specifically to deal with input.
    /// </summary>
    public class ShipAI : MonoBehaviour
    {
        public static event EventHandler OnOrderFinished;
        public Order CurrentOrder;
        public Order.ShipOrder currentOrder;
        public Order.State currentState;
        public Transform target;
        private float _ageTimer;
        public int ageInMin;
        // Don't execute the current order if the ship is undocking
        [HideInInspector]
        public bool isUndocking = false;

        // Variables used by AI orders
        [HideInInspector] public List<Transform> wayPointList;
        [HideInInspector] public int nextWayPoint;
        [HideInInspector] public Vector3 tempDest;
        [HideInInspector] public float throttle;

        // Used by Steering Action
        [HideInInspector] public Vector3 angularTorque;
        [HideInInspector] public PIDController PidAngle, PidVelocity;
        [HideInInspector] public float pidP = 10;
        [HideInInspector] public float pidI = 0.5f;
        [HideInInspector] public float pidD = 0.5f;
        [HideInInspector] public Rigidbody rBody;
        [HideInInspector] public Ship ship;
        private Renderer _mRenderer;
        private bool _isWaitingForMap = false;
        // Which order is being given in on the map
        private string _mapWaitingOrder;
        // Reference to map coroutine to ensure it cannot be open twice
        private Coroutine _mapWaitCoroutine = null;

        // Collision avoidance overrides any order execution
        private bool _avoidCollision = false;
        // Temp destination will be overriden if avoiding a collision, so remember the original
        private Vector3 _savedTempDest = Vector3.zero;


        private void Awake()
        {
            ship = GetComponent<Ship>();
            rBody = GetComponent<Rigidbody>();
            _mRenderer = GetComponentInChildren<Renderer>();
            
            PidAngle = new PIDController(pidP, pidI, pidD);
            PidVelocity = new PIDController(pidP, pidI, pidD);

            wayPointList = new List<Transform>();
            _ageTimer = Time.time;
        }

        private void Update()
        {
            _ageTimer += Time.deltaTime;
            ageInMin = (int) _ageTimer / 60;
            
            SetOrders(); // just lets us know what they are. Otherwise, useless.
            
            // Wait for user to select location on map
            if (_isWaitingForMap && _mapWaitCoroutine == null)
            {
                _mapWaitCoroutine = StartCoroutine(nameof(WaitForMapPosition));
            }
            if (!_isWaitingForMap && _mapWaitCoroutine != null)
            {
                StopCoroutine(_mapWaitCoroutine);
                _mapWaitCoroutine = null;
            }

            if (ship.isPlayerControlled)
                return;

            // Collision avoidance is priority, order is secondary
            if(_avoidCollision)
            {
                ShipSteering.SteerTowardsTarget(this);
                // First turn toward avoidance waypoint, then fly to it
                var transform1 = transform;
                throttle = Vector3.Angle(transform1.forward, tempDest - transform1.position) < 10 ? 0.5f : 0f;
                if (!(Vector3.Distance(transform.position, tempDest) < 10)) return;
                tempDest = _savedTempDest;
                _avoidCollision = false;
            }
            else if(!isUndocking)
            {
                if (CurrentOrder != null)
                {
                    // Disable collision detection when docking for obvious reasons
                    if(!(CurrentOrder.Name == "Dock" && ((OrderDock)CurrentOrder).InFinalApproach) &&
                       !(CurrentOrder.Name == "Trade" && ((OrderTrade)CurrentOrder).InFinalApproach)) { 
                        CheckForwardCollisions();
                    }
                    // Update the order
                    CurrentOrder.UpdateState(this);
                }
                else
                {
                    throttle = 0f;
                }
            }
        }

        private void OnDestroy()
        {
            OnOrderFinished?.Invoke(ship, EventArgs.Empty);   // Notify listeners, if there are no listeners this will be null
        }

        /// <summary>
        /// Finish current ship order and clean up.
        /// </summary>
        public void FinishOrder()
        {
            if (CurrentOrder == null)
                return;

            OnOrderFinished?.Invoke(ship, EventArgs.Empty);   // Notify listeners, if there are no listeners this will be null

            CurrentOrder.Destroy();
            CurrentOrder = null;
            tempDest = Vector3.zero;
            _avoidCollision = false;

            wayPointList.Clear();
            if (ship.faction == Player.Instance.playerFaction)
                ConsoleOutput.PostMessage(name + " has completed the order.");
            throttle = 0;

            if (ship == Ship.PlayerShip)
                ship.isPlayerControlled = true;
        }

        // When giving some orders (like move to destination, dock or attack) a map is opened to 
        // select a target/destination
        #region map-dependent commands
        private IEnumerator WaitForMapPosition()
        {
            while (true)
            {
                CanvasViewController.Instance.IsMapOpenForSelection = true;
                Transform selectedObject = CanvasViewController.Instance.GetMapSelectedObject();
                if (selectedObject != null)
                {
                    _isWaitingForMap = false;
                    if(_mapWaitingOrder == "MoveTo")
                        MoveTo(selectedObject);
                    else if (_mapWaitingOrder == "DockAt")
                        DockAt(selectedObject.gameObject);
                    else if (_mapWaitingOrder == "Follow")
                        Follow(selectedObject);

                    yield return null;
                }
                tempDest = CanvasViewController.Instance.GetMapSelectedPosition();
                if (tempDest != Vector3.zero)
                {
                    _isWaitingForMap = false;
                    MoveTo(tempDest);
                    yield return null;
                }
                yield return null;
            }
        }

        private static void OpenMap()
        {
            // Clear map previous map selections
            CanvasViewController.Instance.IsMapOpenForSelection = true;
            CanvasViewController.Instance.SetMapSelectedObject(null);
            CanvasViewController.Instance.SetMapSelectedPosition(Vector2.zero);
            // Open map for selection
            CanvasViewController.Instance.ToggleMap();
        }

        private void UndockIfNecessary()
        {
            GameObject stationDocked = SectorNavigation.GetStationByID(ship.stationDocked);
            if (stationDocked != null)
            {
                stationDocked.GetComponent<Station>().UndockShip(ship.gameObject);
            }
        }

        #endregion map-dependent commands

        #region collision avoidance
        private readonly Vector3 _upOffset = new Vector3(0, 3, 0);
        private readonly Vector3 _downOffset = new Vector3(0, -3, 0);
        private readonly Vector3 _leftOffset = new Vector3(-3, 0, 0);
        private readonly Vector3 _rightOffset = new Vector3(3, 0, 0);

        private void CheckForwardCollisions()
        {
            if (throttle < 0.05f)
                return;

            // Shoot 4 raycasts from each tip of the ship

            float minDistance = 200;
            var avoidancePosition = Vector3.zero;

            var transform1 = transform;
            Debug.DrawRay(transform1.position + _upOffset, transform1.forward);
            if (Physics.Raycast(transform.position+_upOffset, transform.forward, out var hit, 100))
            {
                if(hit.transform.CompareTag("Asteroid") || hit.transform.CompareTag("Station") ||
                   hit.transform.CompareTag("StationParts") || hit.transform.CompareTag("Planet"))
                    if(hit.distance < minDistance)
                    {
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        var transform2 = transform;
                        avoidancePosition = transform2.position + transform2.up * minDistance - transform2.right * minDistance;
                    }
            }

            var transform3 = transform;
            Debug.DrawRay(transform3.position + _downOffset, transform3.forward);
            if (Physics.Raycast(transform.position + _downOffset, transform.forward, out hit, 100))
            {
                if (hit.transform.CompareTag("Asteroid") || hit.transform.CompareTag("Station") ||
                    hit.transform.CompareTag("StationParts") || hit.transform.CompareTag("Planet"))
                    if (hit.distance < minDistance)
                    {
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        var transform2 = transform;
                        avoidancePosition = transform2.position + transform2.up * minDistance - transform2.right * minDistance;
                    }
            }

            var transform4 = transform;
            Debug.DrawRay(transform4.position + _rightOffset, transform4.forward);
            if (Physics.Raycast(transform.position + _rightOffset, transform.forward, out hit, 100))
            {
                if (hit.transform.CompareTag("Asteroid") || hit.transform.CompareTag("Station") ||
                    hit.transform.CompareTag("StationParts") || hit.transform.CompareTag("Planet"))
                    if (hit.distance < minDistance)
                    {
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        var transform2 = transform;
                        avoidancePosition = transform2.position + transform2.up * minDistance - transform2.right * minDistance;
                    }
            }

            var transform5 = transform;
            Debug.DrawRay(transform5.position + _leftOffset, transform5.forward);
            if (Physics.Raycast(transform.position + _leftOffset, transform.forward, out hit, 100))
            {
                if (hit.transform.CompareTag("Asteroid") || hit.transform.CompareTag("Station") || 
                    hit.transform.CompareTag("StationParts") || hit.transform.CompareTag("Planet"))
                    if (hit.distance < minDistance)
                    {
                        _savedTempDest = tempDest;
                        minDistance = hit.distance;
                        var transform2 = transform;
                        avoidancePosition = transform2.position + transform2.up * minDistance - transform2.right * minDistance;
                    }
            }

            if ((int)minDistance != 200)
            {
                tempDest = avoidancePosition;
                _avoidCollision = true;
            }
        }
        #endregion collision avoidance

        // Autopilot commands
        #region commands
        /// <summary>
        /// Commands the ship to move to a given object.
        /// </summary>
        /// <param name="destination"></param>
        public void MoveTo(Transform destination)
        {
            FinishOrder();
            ship.isPlayerControlled = false;
            if (destination == null)
            {
                _isWaitingForMap = true;
                _mapWaitingOrder = "MoveTo";
                OpenMap();
            }
            else
            {
                wayPointList.Clear();
                wayPointList.Add(destination);
                nextWayPoint = 0;

            CurrentOrder = new OrderMove();
            if (ship.faction == Player.Instance.playerFaction)
                ConsoleOutput.PostMessage(name + " command Move accepted");
        }
        UndockIfNecessary();
    }

        /// <summary>
        /// Commands the ship to move to a specified position.
        /// </summary>
        /// <param name="position">world position of destination</param>
        public void MoveTo(Vector3 position)
        {
            FinishOrder();
            tempDest = position;
            if (tempDest == Vector3.zero)
                return;
            ship.isPlayerControlled = false;

            CurrentOrder = new OrderMove();
            UndockIfNecessary();

        if(ship.faction == Player.Instance.playerFaction)
            ConsoleOutput.PostMessage(name + " command Move accepted");
    }

        /// <summary>
        /// Commands the ship to move through the given waypoints. Once the last one is reached,
        /// the route is restarted from the first waypoint.
        /// </summary>
        /// <param name="waypoints"></param>
        public void PatrolPath(Transform[] waypoints)
        {
            FinishOrder();
            ship.isPlayerControlled = false;
            CurrentOrder = new OrderPatrol();

            wayPointList.Clear();

            UndockIfNecessary();
            if (ship.faction == Player.Instance.playerFaction)
                ConsoleOutput.PostMessage(name + " command Patrol accepted");
        }

        /// <summary>
        /// Commands the ship to move randomly at low speed, roughly in the same area.
        /// </summary>
        public void Idle()
        {
            FinishOrder();
            ship.isPlayerControlled = false;
            CurrentOrder = new OrderIdle();
            tempDest = transform.position;

            UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to follow player ship
        /// </summary>
        public void FollowMe()
        {
            FinishOrder();
            // Cant chase its own tail
            if (ship == Ship.PlayerShip)
                return;

            ship.isPlayerControlled = false;
            CurrentOrder = new OrderFollow(this, Ship.PlayerShip);

            wayPointList.Clear();
            wayPointList.Add(Ship.PlayerShip.transform);
            nextWayPoint = 0;

            UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to follow a target
        /// </summary>
        public void Follow(Transform target)
        {
            FinishOrder();
            ship.isPlayerControlled = false;
            if (target == null)
            {
                _isWaitingForMap = true;
                _mapWaitingOrder = "Follow";
                OpenMap();
            }
            else
            {
                wayPointList.Clear();
                wayPointList.Add(target);
                nextWayPoint = 0;
                CurrentOrder = new OrderFollow(this, target.GetComponent<Ship>());
            }
        UndockIfNecessary();
    }

        /// <summary>
        /// Commands the ship to dock at a station, or jump at a gate
        /// </summary>
        public void DockAt(GameObject dockable)
        {
            FinishOrder();
            UndockIfNecessary();
            if (dockable == null)
            {
                _isWaitingForMap = true;
                _mapWaitingOrder = "DockAt";
                OpenMap();
            }
            else
            {
                ship.isPlayerControlled = false;
                GameObject[] dockWaypoints = null;
                if (dockable.CompareTag("Station"))
                {
                    Station dockingTarget = dockable.GetComponent<Station>();
                    CurrentOrder = new OrderDock(dockingTarget, ship);
                    try
                    {
                        dockWaypoints = dockingTarget.RequestDocking(gameObject);
                    }
                    catch (DockingException)
                    {
                        FinishOrder();
                        return;
                    }
                }
                else if (dockable.CompareTag("Jumpgate"))
                {
                    CurrentOrder = new OrderDock();
                    dockWaypoints = dockable.GetComponent<Jumpgate>().DockWaypoints;
                }
                wayPointList.Clear();

                if (dockWaypoints != null)
                    foreach (var t in dockWaypoints)
                        wayPointList.Add(t.transform);

                nextWayPoint = 0;

            if (ship.faction == Player.Instance.playerFaction)
            {
                ConsoleOutput.PostMessage(name + " command Dock accepted");
                //print("Docking order accepted");
            }
        }
    }

        /// <summary>
        /// Commands the ship to move around a list of positions
        /// </summary>
        public void MoveWaypoints(List<Vector3> positions)
        {
            FinishOrder();
            ship.isPlayerControlled = false;
            if (positions is {Count: > 0})
                CurrentOrder = new OrderMovePositions(positions);

            UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to attack an object
        /// </summary>
        public void Attack(GameObject _target)
        {
            FinishOrder();
            CurrentOrder = new OrderAttack();
            ship.isPlayerControlled = false;

            wayPointList.Clear();
            wayPointList.Add(_target.transform);
            nextWayPoint = 0;

        UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to attack all enemies in the area
        /// </summary>
        public void AttackAll()
        {
            FinishOrder();
            CurrentOrder = new OrderAttackAll();
            ship.isPlayerControlled = false;

            wayPointList.Clear();
            nextWayPoint = 0;

        UndockIfNecessary();
        }

        /// <summary>
        /// Commands the ship to trade in the area
        /// </summary>
        public void AutoTrade()
        {
            FinishOrder();
            CurrentOrder = new OrderTrade();
            ship.isPlayerControlled = false;

            wayPointList.Clear();
            nextWayPoint = 0;

            UndockIfNecessary();
            if (ship.faction == Player.Instance.playerFaction)
                ConsoleOutput.PostMessage(name + " command AutoTrade accepted");
        }

        /// <summary>
        /// Commands the ship to dock immediately
        /// </summary>
        public void EmergencyDock()
        {
            var dock = SectorNavigation.Instance.GetClosestStation(this.transform, Ship.PlayerShip.shipModelInfo.ScannerRange,
                Int32.MaxValue);

            if (dock != null)
            {
                DockAt(dock[0]);
                //print("attempting to dock at "+dock[0]);
                //return;
            }
            //print("Can't find anywhere to dock!");
        }
        
        #endregion commands

        private void SetOrders()
        {
            if (CurrentOrder == null)
                currentOrder = Order.ShipOrder.None;
            else if (CurrentOrder.Name == "Attack")
                currentOrder = Order.ShipOrder.Attack;
            else if (CurrentOrder.Name == "AttackAll")
                currentOrder = Order.ShipOrder.AttackAll;
            else if (CurrentOrder.Name == "Patrol")
                currentOrder = Order.ShipOrder.Patrol;
            else if (CurrentOrder.Name == "Move")
                currentOrder = Order.ShipOrder.Move;
            else if (CurrentOrder.Name == "MovePositions")
                currentOrder = Order.ShipOrder.MovePositions;
            else if (CurrentOrder.Name == "Follow")
                currentOrder = Order.ShipOrder.Follow;
            else if (CurrentOrder.Name == "Idle")
                currentOrder = Order.ShipOrder.Idle;
            else if (CurrentOrder.Name == "Dock")
                currentOrder = Order.ShipOrder.Dock;
            else if (CurrentOrder.Name == "Trade")
                currentOrder = Order.ShipOrder.Trade;
        }
    }