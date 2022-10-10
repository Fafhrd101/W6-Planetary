
using UnityEngine;

public class OrderAttack : Order
{

    private State state;
    private Transform target;
    private float minrange = 150f;
    private float previousArmorValue;
    private float evadeTimer;

    public OrderAttack()
    {
        Name = "Attack";
        state = State.Chase;
    }

    public override void UpdateState(ShipAI controller)
    {
        if(CheckTransitions(controller))
            controller.FinishOrder();
        ComputeState(controller);
    }

    private bool CheckTransitions(ShipAI controller)
    {
        target = controller.wayPointList[controller.nextWayPoint];

        // Check if target is gone/destroyed
        if (target == null || controller.ship.stationDocked != "none")
            return true;

        float distance = Vector3.Distance(target.position, controller.transform.position);
        float gunrange = controller.ship.Equipment.GetWeaponRange();

        if (state == State.Chase && distance < gunrange)
        {
            state = State.Shoot;
        }
        else if (state == State.Shoot && distance < minrange)
        {
            state = State.GetDistance;
            controller.tempDest = target.position + Vector3.up * 30;
        }
        else if (state == State.Shoot && distance > gunrange)
        {
            state = State.Chase;
        }
        else if (state == State.GetDistance && distance > gunrange/3)
        {
            state = State.Chase;
        }
        if (previousArmorValue/controller.ship.maxArmor> 0.75f && controller.ship.armor/ controller.ship.maxArmor < 0.75f) {
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
        if (target == null)
            return;

        float distance = Vector3.Distance(target.position, controller.transform.position);
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
                controller.throttle = 1f;
                if (Vector3.Distance(controller.transform.position, controller.tempDest) < 5f)
                    controller.tempDest = Vector3.zero;
                if (controller.tempDest != Vector3.zero)
                    ShipSteering.SteerTowardsTarget(controller);
                break;
            case State.Evade:
                // Jam the throttle and rudder (yaw left, pitch up)
                controller.throttle = 1f;
                // Force multiplier is 100.0f from ShipPhysics
                controller.angularTorque = new Vector3(
                    -controller.ship.Physics.angularForce.x,
                    -controller.ship.Physics.angularForce.y,
                    -controller.ship.Physics.angularForce.z) *100.0f;

                evadeTimer -= Time.deltaTime;
                if (evadeTimer == 0)
                    state = State.Chase;

                break;
            default:
                break;
        }
    }

    public override void Destroy()
    {
    }


}