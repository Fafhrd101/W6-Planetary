using UnityEngine;
using UnityEngine.SceneManagement;

public class SectorShipLoader : MonoBehaviour
{
    public bool battle = true;
    public bool formations = false;
    private Vector3[] escortOffsets = { new Vector3(50, 0, 0), new Vector3(50, 50, 0), new Vector3(0, 50, 0), new Vector3(100, 50, 0), new Vector3(0, 100, 0)};
    public GameObject[] goodGuys;
    public GameObject[] badGuys;
    
    // Start is called before the first frame update
    private void Start()
    {
        battle = Random.value < 0.5 || SceneManager.GetActiveScene().name == "StartScenario";
        formations = Random.value < 0.15;
        for(var x = 0; x < Random.Range(1,5); x++)
            SpawnRandomShips();
        
        if (battle)
        {
            var good = Random.Range(5, 10);
            var enemy = Random.Range(5, 10);

            for (var x = 0; x < good; x++)
            {
                var pos = new Vector3(x + 75, Random.Range(-75, 75), 250);
                SpawnFactionedShip(false, pos);
            }

            for (var y = 0; y < enemy; y++)
            {
                var pos = new Vector3(y + 75, Random.Range(-75, 75), -250);
                SpawnFactionedShip(true, pos);
            }
        }

        // if (formations)
        // {
        //     Vector3 pos = new Vector3(500, Random.Range(-50, 50), 250);
        //     Ship escortLeader = SpawnFactionedLeaderShip(Random.value > 0.5f, GenerateRandomSector.GetRandomPosition()).GetComponent<Ship>();
        //     escortLeader.GetComponent<ShipAI>().currentOrder = Order.ShipOrder.Patrol;
        //     int numEscorts = Random.Range(1, 5);
        //     for (int e_i = 0; e_i < numEscorts; e_i++)
        //     {
        //         Ship escort = GameObject.Instantiate(fighterPrefabs[Random.Range(0, fighterPrefabs.Length)], 
        //             Vector3.zero, Quaternion.identity).GetComponent<Ship>();
        //         escort.IsPlayerControlled = false;
        //         escort.faction = escortLeader.faction;
        //         string letter = e_i == 0 ? "Alpha" : e_i == 1 ? "Beta" : e_i == 2 ? "Gamma" : e_i == 3 ? "Delta"
        //             : e_i == 4 ? "Epsilon" : "Zeta" ;
        //         escort.gameObject.name = "Escort" + letter + "-" + escort.ShipModelInfo.modelName;
        //         escort.name = "Escort" + letter + "-" + escort.ShipModelInfo.modelName;
        //         // Assign order to ship
        //         escort.gameObject.GetComponent<ShipAI>().Follow(escortLeader.transform);
        //         escort.gameObject.transform.position += escortOffsets[e_i];
        //     }
        // }
        
    }

    void SpawnFactionedShip(bool enemy, Vector3 spawnPos)
    {
        var shipToSpawn = Random.Range(0, enemy?goodGuys.Length:badGuys.Length);
        var ship = GameObject.Instantiate(
            enemy?goodGuys[shipToSpawn]:badGuys[shipToSpawn], spawnPos, Quaternion.identity).GetComponent<Ship>();
        ship.isPlayerControlled = false;
        ship.faction = ObjectFactory.Instance.GetFactionFromName(enemy ? "Raider" : "EarthAlliance");
        ship.gameObject.name = "(Battle)"+ship.shipModelInfo.modelName;
        ship.name = ship.faction.name+ship.shipModelInfo.modelName;
        ship.AIInput.currentOrder = Order.ShipOrder.AttackAll;
    }
    
    Ship SpawnFactionedLeaderShip(bool enemy, Vector3 spawnPos)
    {
        var shipToSpawn = Random.Range(0,  enemy?goodGuys.Length:badGuys.Length);
        var ship = GameObject.Instantiate(
            enemy?goodGuys[shipToSpawn]:badGuys[shipToSpawn], spawnPos, Quaternion.identity).GetComponent<Ship>();
        ship.isPlayerControlled = false;
        ship.faction = ObjectFactory.Instance.GetFactionFromName(enemy ? "Raider" : "EarthAlliance");
        if (SceneManager.GetActiveScene().name == "StartScenario")
            ship.faction = ObjectFactory.Instance.GetFactionFromName("Raider");
        ship.gameObject.name = "(Leader) "+ship.shipModelInfo.modelName;
        ship.name = ship.faction.name+ship.shipModelInfo.modelName;
        ship.AIInput.currentOrder = Order.ShipOrder.AttackAll;
        return ship;
    }

    private Ship SpawnRandomShips()
    {
        var shipToSpawn = Random.Range(0, ObjectFactory.Instance.Ships.Length);
        var ship = GameObject.Instantiate(
            ObjectFactory.Instance.Ships[shipToSpawn], GenerateRandomSector.GetRandomPosition(), Quaternion.identity).GetComponent<Ship>();
        ship.isPlayerControlled = false;
        var randomFaction = ObjectFactory.Instance.Factions[Random.Range(0, ObjectFactory.Instance.Factions.Length - 1)];
        // if (SceneManager.GetActiveScene().name == "StartScenario")
        //     randomFaction = ObjectFactory.Instance.GetFactionFromName("Raider");
        ship.faction = randomFaction;
        ship.gameObject.name = "(Random)"+ship.shipModelInfo.modelName;
        ship.name = ship.faction.name+ship.shipModelInfo.modelName;
        ship.AIInput.currentOrder = Order.ShipOrder.AttackAll;
        return ship;
    }
}
