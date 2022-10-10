public abstract class Order
{
    public enum ShipOrder
    {
        None,
        Attack,      
        AttackAll,
        Dock,
        Follow, 
        Idle,
        Move,
        MovePositions,
        Patrol,
        Trade
    }
    
    public enum State
    {
        Idle, Chase, Shoot, Evade, GetDistance 
    }
    public string Name;

    public abstract void UpdateState(ShipAI controller);
    public abstract void Destroy();
}