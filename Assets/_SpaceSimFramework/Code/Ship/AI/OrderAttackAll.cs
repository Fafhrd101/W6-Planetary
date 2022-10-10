using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderAttackAll : Order
{


    private State state;
    private Transform target;
    private float minrange = 150f;
    private float timer;
    // Evasion properties
    private float evadeTimer, previousArmorValue;

    public OrderAttackAll()
    {
        Name = "AttackAll";
    }

    public override void UpdateState(ShipAI controller)
    {
        if (CheckTransitions(controller))
            controller.FinishOrder();
        ComputeState(controller);
    }

    private bool CheckTransitions(ShipAI controller)
    {
        if (controller.ship.stationDocked != "none")
            target = null; // Force a relook
        
        // Check if target is gone/destroyed
        if (target == null && evadeTimer <= 0) 
        {
            // Scan for enemies
            var enemies = SectorNavigation.GetClosestNPCShip(controller.transform, controller.ship.shipModelInfo.ScannerRange);
            if (enemies.Count > 0)
            {
                target = enemies[0].transform;
                state = State.Chase;
            }
            else
            {
                state = State.Idle;
            }
        }
        float distance = target != null ? Vector3.Distance(target.position, controller.transform.position) : 0f;
        float gunrange = controller.ship.Equipment.GetWeaponRange();

        if (timer > 0)
            timer -= Time.deltaTime;

        if (state == State.Chase && distance < gunrange)
        {
            state = State.Shoot;
        }
        else if (state == State.Shoot && distance < minrange)
        {
            state = State.GetDistance;
            timer = Random.Range(3, 7);
            if (target is not null) controller.tempDest = target.position + Vector3.up * 30;
        }
        else if (state == State.Shoot && distance > gunrange)
        {
            state = State.Chase;
        }
        else if (state == State.GetDistance && distance > gunrange / 3)
        {
            state = State.Chase;
        }
        else if (state == State.GetDistance && timer < 0)
        {
            state = State.Chase;
        }
        else if (state == State.Idle)
        {
            var enemies = SectorNavigation.GetClosestNPCShip(controller.transform, 
                controller.ship.shipModelInfo.ScannerRange);
            if (enemies.Count > 0)
            {
                target = enemies[0].transform;
                state = State.Chase;
            }
        }
        // Evasion transitions
        if (previousArmorValue / controller.ship.maxArmor > 0.75f && controller.ship.armor / controller.ship.maxArmor < 0.75f)
        {
            evadeTimer = Random.value * 5 + 3;
            state = State.Evade;
        }
        if (previousArmorValue / controller.ship.maxArmor > 0.5f && controller.ship.armor / controller.ship.maxArmor < 0.5f)
        {
            evadeTimer = Random.value * 5 + 3;
            state = State.Evade;
        }
        if (previousArmorValue / controller.ship.maxArmor > 0.25f && controller.ship.armor / controller.ship.maxArmor < 0.25f)
        {
            evadeTimer = Random.value * 5 + 3;
            state = State.Evade;
        }

        previousArmorValue = controller.ship.armor;

        return false;
    }

    private void ComputeState(ShipAI controller)
    {
        float distance = target != null ? Vector3.Distance(target.position, controller.transform.position) : 0f;
        float thr = distance > 100f ? 1f : (distance / 100f);
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);
        controller.currentState = state; // just a visual for the inspector
        controller.target = target; // Let the boss know
        switch (state)
        {
            case State.Chase:
                controller.tempDest = Vector3.zero;
                ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.Shoot:
                // Predict lead
                GameObject shooter = controller.gameObject;
                float projectileSpeed = controller.ship.Equipment.Guns[0].ProjectileSpeed;

                controller.tempDest = Targeting.PredictTargetLead3D(shooter, target.gameObject, projectileSpeed);

                // Fire if on target
                Vector3 attackVector = target.position - shooter.transform.position;
                float angle = Vector3.Angle(attackVector, controller.transform.forward);
                if (angle < 15)
                    controller.ship.Equipment.isFiring = true;

                ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.GetDistance:
                controller.throttle = Mathf.MoveTowards(controller.throttle, 1f, Time.deltaTime * 0.5f);
                if (Vector3.Distance(controller.transform.position, controller.tempDest) < 5f)
                    controller.tempDest = Vector3.zero;
                if (controller.tempDest != Vector3.zero)
                    ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.Idle:
                if (controller.tempDest == Vector3.zero)
                    controller.tempDest = GenerateNextWaypoint(controller.transform);

                if (distance < 30)
                {
                    controller.tempDest = GenerateNextWaypoint(controller.transform);
                }

                controller.throttle = Mathf.MoveTowards(controller.throttle, 0.5f, Time.deltaTime * 0.5f);
                ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.Evade:
                // Jam the throttle and rudder (yaw left, pitch up)
                controller.throttle = 1f;
                // Force multiplier is 100.0f from ShipPhysics
                if (Random.value < .5)
                {
                    controller.angularTorque = new Vector3(
                        -controller.ship.Physics.angularForce.x,
                        -controller.ship.Physics.angularForce.y,
                        -controller.ship.Physics.angularForce.z) * 100.0f;
                }
                else
                {
                    controller.angularTorque = new Vector3(
                        controller.ship.Physics.angularForce.x,
                        controller.ship.Physics.angularForce.y,
                        controller.ship.Physics.angularForce.z) * 100.0f;
                }

                evadeTimer -= Time.deltaTime;
                if (evadeTimer < 0) {
                    state = State.Chase;
                }

                break;
            default:
                break;
        }
    }

    private Vector3 GenerateNextWaypoint(Transform currPos)
    {
        Vector3 randomDirection = new Vector3(Random.Range(-200, 200),
            Random.Range(-200, 200),
            Random.Range(-200, 200));

        randomDirection = currPos.position + randomDirection;

        return randomDirection;
    }

    public override void Destroy()
    {
    }

}