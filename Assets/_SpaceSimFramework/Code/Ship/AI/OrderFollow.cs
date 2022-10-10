using UnityEngine;

public class OrderFollow : Order
{
    private Vector3 formationOffset;
    private Ship targetShip;
    private ShipAI controller;

    public OrderFollow(ShipAI controller, Ship target)
    {
        // Give a random formation offset
        formationOffset = target.GetWingmanPosition(controller.transform.GetComponent<Ship>());
        targetShip = target;
        this.controller = controller;
        Name = "Follow";
    }

    public override void UpdateState(ShipAI controller)
    {
        if (CheckExitCondition(controller))
        {
            controller.FinishOrder();
            return;
        }
       
        controller.tempDest = targetShip.transform.position + formationOffset;
        ShipSteering.SteerTowardsTarget(controller);
    }

    protected bool CheckExitCondition(ShipAI controller)
    {
        if (targetShip == null) // Ship being followed is gone/destroyed
        {
            // Defend!
            controller.AttackAll();
            return true;
        }

        float distance = Vector3.Distance(controller.tempDest, controller.transform.position);


        float thr = distance > 80f ? 1f : 0f;
        thr = distance > 700f ? 3f : thr;   // Supercruise
        controller.throttle = Mathf.MoveTowards(controller.throttle, thr, Time.deltaTime * 0.5f);

        return false;
    }

    public override void Destroy()
    {
        if(targetShip != null)
            targetShip.RemoveWingman(controller.ship);
    }
}