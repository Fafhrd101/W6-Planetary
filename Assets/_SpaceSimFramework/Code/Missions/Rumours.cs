using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rumours : Singleton<Rumours>
{
    private static string _sector = "x" + (int) Random.Range(0, 10) + "y" + (int) Random.Range(0, 10);
    
    private static string[] RumourList = new string[]
    {
        $" Increased pirate activity in sector {_sector}",
        $" Rare cargo shipment in sector {_sector}",
        $" New crime boss in sector {_sector}",
        " Revolution on nearby world",
        " Heist on nearby space station",
        $" New smuggler base in sector {_sector}",
        $" Popular band on tour in sector {_sector}",
        " Plague on nearby planet",
        " Blockaded planet looking for pilots to run the blockade",
        $" Ships disappearing in sector {_sector}",
        " New planet discovered",
        $" Title dispute over lands in sector {_sector}",
        " Rare metal discovered in nearby asteroid belt",
        " New station will be built",
        $" Racial tensions in colony in sector {_sector}",
        $" Meteor shower in sector {_sector}",
        $" Derelict ship found in sector {_sector}",
        $" Large passenger liner disappeared in sector {_sector}",
        $" Alien tomb discovered in sector {_sector}",
        " The conflict of two warring planets is about to spill over",
        " New drug on the market",
        $" Terraforming gone terribly wrong in sector {_sector}",
        " Team of scientists needs a good pilot to observe planetary rings",
        " A planet’s young prince has been kidnapped",
        " Colony’s water supply poisoned",
        " New colony founded nearby",
        " Trade negotiations collapsing",
        " Messages without clear source received in certain sector",
        " Distress signal in uncharted space",
        " Asteroid going to hit populated planet",
        $" Hackers moved into sector {_sector}",
        " Colony infested with strange insects",
        " Illegal gambling nearby",
        " Comet coming back through",
        " Slave uprising on a certain planet",
        " Ghost ship spotted",
        " New ship model released",
        " New company charter issued",
        $" Slave traders in sector {_sector}",
        " Increased spaceport security on popular planet",
        " A government looking for privateers",
        $" Sentient androids need help in sector {_sector}",
        $" Ancient ruins discovered in sector {_sector}",
        $" Planet destroyed in sector {_sector}",
        $" Mutiny on slave ship in sector {_sector}",
        $" Company looking for exploration team in sector {_sector}",
        $" Asteroid mining operation plagued by pirates in sector {_sector}",
        " Wedding of the century",
        $" Supernova threatening population of a planet in sector {_sector}",
        " Team of scientists looking for transport to distant planet",
        " Political scandal on nearby planet",
        " Shipyard workers trying to unionize",
        " Black Hole cult threatening mass suicide",
        $" Mercenary unit recruiting in sector {_sector}",
        $" Odd space debris in sector {_sector}",
        " A government looking for smugglers",
        " New nebula dust harvesting technology invented",
        $" Large passenger liner stolen in sector {_sector}",
        $" Abandoned space station found in sector {_sector}",
        " Trading fleet looking for escorts",
        " An old spacer is telling tales of a utopian planet",
        " Colony needs new water supply",
        " Riot on prison asteroid",
        " Space circus lost a most exotic animal",
        $" Colony wiped out in sector {_sector}",
        " Pirates are guarding a planet with a large force",
        " Imp‐like creature spotted on a space station",
        " Cartel blocking several trade routes",
        " High bounty on political leader’s head",
        $" Plague ship spotted in sector {_sector}",
        " Kidnappings on certain planet",
        " Terrorist group issued manifesto",
        " Distant planet’s air gives immortality",
        " Large transport ship crippled by a computer virus",
        " Distant research station issued distress signal",
        " Team of scientists need help exploring a new planet",
        " Mutiny on large trade vessel",
        " Cartel looking for smugglers",
        " High bounty on a cartel boss’s head",
        $" New species discovered in sector {_sector}",
        " Salesman selling “nebula dust” with special properties",
        $" War between crime bosses in sector {_sector}",
        " At the edge of charted space an alien oracle offers prophecies",
        " Massive chunk of ice spotted in space"
    };

    public static string GenerateRumour()
    {
        return "Rumour is...\n"+RumourList[Random.Range (0, RumourList.Length)];
    }

}
