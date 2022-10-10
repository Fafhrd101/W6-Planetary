using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderPatrol : Order
{
    private State state;
    private Transform target;
    private Vector3[] waypoints;
    private int nextWaypoint = 0;

    private new enum State
    {
        Patrol, // Fly between patrol waypoints
        Attack  // Engage an enemy contact
    }

    public OrderPatrol()
    {
        Name = "Patrol";
        state = State.Patrol;
    }

    private Vector3[] GetWaypoints(ShipAI controller)
    {
        Faction shipfaction = controller.ship.faction;
        List<Vector3> wpList = new List<Vector3>();

        foreach (var station in SectorNavigation.Stations)
        {
            if (station.GetComponent<Station>().faction == shipfaction)
                wpList.Add(station.transform.position + GetRandomOffset());
        }

        foreach (var jg in SectorNavigation.Jumpgates)
        {
            wpList.Add(jg.transform.position + GetRandomOffset());
        }

        // Get a few random points too
        var mapSize = SectorNavigation.sectorSize;
        for (int i = 0; i < Random.Range(2, 5); i++)
        {
            wpList.Add(new Vector3(
                Random.Range(0, mapSize) - mapSize / 2,
                Random.Range(0, mapSize) - mapSize / 2,
                Random.Range(50, 300) * Mathf.Sign(Random.value - 0.5f)));
        }

        return wpList.ToArray();
    }

    // Get a random point above or below the target, between 50 and 300 units distant
    private Vector3 GetRandomOffset()
    {
        return new Vector3(0, 0, Random.Range(50, 300) * Mathf.Sign(Random.value - 0.5f));
    }

    public override void UpdateState(ShipAI controller)
    {
        if (waypoints == null || waypoints.Length < 2)
            waypoints = GetWaypoints(controller);

        if (controller.wayPointList.Count == 0)
        {
            controller.FinishOrder();
            return;
        }

        if (state == State.Patrol)
        {
            PatrolWaypoints(controller);
        }
        if (state == State.Attack)
        {
            ShipSteering.SteerTowardsTarget(controller);
            AttackTarget(controller);
        }

        // Scan for enemies
        var enemies = SectorNavigation.GetClosestNPCShip(controller.transform, controller.ship.shipModelInfo.ScannerRange);
        if (enemies.Count > 0)
        {
            target = enemies[0].transform;
            state = State.Attack;
        }
    }

    private void PatrolWaypoints(ShipAI controller)
    {
        float distance = Vector3.Distance(waypoints[nextWaypoint], controller.transform.position);

        if (distance < 30)
        {
            nextWaypoint = (nextWaypoint + 1) % waypoints.Length;
        }

        controller.throttle = Mathf.MoveTowards(controller.throttle, 1.0f, Time.deltaTime * 0.5f);

        ShipSteering.SteerTowardsDestination(controller, waypoints[nextWaypoint]);
    }

    private void AttackTarget(ShipAI controller)
    {
        if (target == null)
        {
            state = State.Patrol;
            return;
        }

        float distance = Vector3.Distance(target.position, controller.transform.position);
        float range = controller.ship.Equipment.GetWeaponRange();

        if (distance < range)
        {
            // Predict lead
            GameObject shooter = controller.gameObject;
            float projectileSpeed = controller.ship.Equipment.Guns[0].ProjectileSpeed;

            controller.tempDest = Targeting.PredictTargetLead3D(shooter, target.gameObject, projectileSpeed);

            // Fire if on target
            Vector3 attackVector = target.position - shooter.transform.position;
            float angle = Vector3.Angle(attackVector, controller.transform.forward);

            if (angle < 10)
                controller.ship.Equipment.isFiring = true;
        }
        else
        {
            controller.tempDest = Vector3.zero;
        }

        float thr = distance > 100f ? 1f : (distance / 100f);
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);
    }

    public override void Destroy()
    {
    }

}