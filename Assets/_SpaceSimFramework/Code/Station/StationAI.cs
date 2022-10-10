using System;
using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// Class specifically to deal with input.
    /// </summary>
    public class StationAI : MonoBehaviour
    {
        public static event EventHandler OnOrderFinished;

        private Order _currentOrder;
        public Order.ShipOrder currentOrder;
        public Order.State currentState;
        public Transform target;
        [Tooltip("ReadOnly, if we have a target, this turret is closest to it")]
        public List<TurretHardpoint> closetTurret;
        private Station _station;
        private void Awake()
        {
            _station = GetComponent<Station>();
        }

        private void Update()
        {
            SetOrders(); // just lets us know what they are. Otherwise, useless.
        }

        private void OnDestroy()
        {
            OnOrderFinished?.Invoke(_station, EventArgs.Empty);   // Notify listeners, if there are no listeners this will be null
        }

        /// <summary>
        /// Finish current ship order and clean up.
        /// </summary>
        private void FinishOrder()
        {
            if (_currentOrder == null)
                return;

            OnOrderFinished?.Invoke(_station, EventArgs.Empty);   // Notify listeners, if there are no listeners this will be null

            _currentOrder.Destroy();
            _currentOrder = null;
        }
        

        /// <summary>
        /// Commands the ship to attack an object
        /// </summary>
        public void Attack(GameObject target)
        {
            FinishOrder();
            _currentOrder = new OrderAttack();
        }

        /// <summary>
        /// Commands the ship to attack all enemies in the area
        /// </summary>
        public void AttackAll()
        {
            FinishOrder();
            _currentOrder = new OrderAttackAll();
        }

        private void SetOrders()
        {
            if (_currentOrder == null)
                currentOrder = Order.ShipOrder.None;
            else if (_currentOrder.Name == "Attack")
                currentOrder = Order.ShipOrder.Attack;
            else if (_currentOrder.Name == "AttackAll")
                currentOrder = Order.ShipOrder.AttackAll;
            else if (_currentOrder.Name == "Patrol")
                currentOrder = Order.ShipOrder.Patrol;
            else if (_currentOrder.Name == "Move")
                currentOrder = Order.ShipOrder.Move;
            else if (_currentOrder.Name == "MovePositions")
                currentOrder = Order.ShipOrder.MovePositions;
            else if (_currentOrder.Name == "Follow")
                currentOrder = Order.ShipOrder.Follow;
            else if (_currentOrder.Name == "Idle")
                currentOrder = Order.ShipOrder.Idle;
            else if (_currentOrder.Name == "Dock")
                currentOrder = Order.ShipOrder.Dock;
            else if (_currentOrder.Name == "Trade")
                currentOrder = Order.ShipOrder.Trade;
        }
    }