using UnityEngine;

public class RandomNameGenerator : MonoBehaviour
{
    private static readonly string[] StationNames = 
    {
        "Escort Station",
        "Babylon Station",
        "Mythos Station",
        "Outlander Station",
        "Terminus Station",
        "Orbital Colony",
        "Apollo Base",
        "Azura Station",
        "Atmos Terminal",
        "Nova Terminal",
        "Guardian Base",
        "Century Station",
        "Iris Station",
        "Spectacle Terminal",
        "Heritage Station",
        "Odyssey Station",
        "Chrono Terminal",
        "Warden Terminal",
        "Sol Station",
        "Titanus Base",
        "Utopis Base",
        "Orbital Base",
        "Scout Base",
        "Prometheus Base",
        "Beacon Colony",
        "Osiris Station",
        "Aeternitas Terminal",
        "Ark Station",
        "Anemone Station",
        "Rune Station",
        "Rebus Base",
        "Orbital",
        "Luna Station",
        "Orbital Terminal",
        "Genesis Terminal",
        "Memento Station",
        "Architect",
        "Mammoth Station",
        "Halo Station",
        "Frontier Terminal",
        "Revelation Station",
        "Beacon",
    };

    private static readonly string[] PlanetNames =
    {
        "Trov N79L",
        "Zichi 796X",
        "Nichi H03",
        "Momia 0N",
        "Dapus 326",
        "Chyria 6HE",
        "Boria X021",
        "Vides 4LV9",
        "Philles IN2",
        "Phapus S766",
        "Crolla EVXT",
        "Crides 548",
        "Zilia 2LDM",
        "Drade Q3PT",
        "Strara KN",
        "Strone R02",
        "Treron 4Q0",
        "Drypso N3R",
        "Zarth 09O",
        "Treon 0Y2",
        "Mippe PAL",
        "Biuq 00HH",
        "Bomia 7LX",
        "Proto Colony",
        "Apollo Base",
        "Rogue Colony",
        "Guardian Base",
        "Crescent Colony",
        "Scout Base",
        "Elysium Colony",
        "Legacy Base",
        "Arcadia Colony",
        "Terran Colony",
        "Dawn Base"
    };
        
    public static string GetRandomStationName()
    {
        return StationNames[Random.Range(0, StationNames.Length)];
    }
    
    public static string GetRandomPlanetName()
    {
        return PlanetNames[Random.Range(0, PlanetNames.Length)];
    }
    
    public static string GetRandomUPP()
    {
        string answer = "";
        string[] rolls = {"1", "2", "3", "4", "5", "6", "7", "8","9","A","B","C"};
        for (var x = 0; x < 6; x++)
        {
            answer += rolls[Random.Range(2, 12)];
        }
        return answer;
    }
}