/* 
* Ship Cargo Indicator
* By Dragonhost
* v1.1
* 
* Instructions:
*
*Sources:
*
*/ 
//-------------------------Code Start---------------------------------- 

////////// Variables //////////
IMyTextPanel HUD;
bool SystemInitialized = false;
int updateTimer = 0,
    updateFrequency = 5,
    BarLength = 20,
    OreFilledBar = 0;
string BarFill = "|",
    BarEmpty = ".",
    OreBar = "Ore cargo:[",
    ScreenName = "HUD1",
    OreCargoIndicator = "";
double OrePercentFull;
float OreUsedVolume = 0.0f,
    OreMaxVolume = 0.0f,
    OrePercentUsed = 0;

List<IMyTerminalBlock> allCargo = new List<IMyTerminalBlock>();
List<IMyCockpit> allCockpit = new List<IMyCockpit>();
List<IMyShipDrill> allDrills = new List<IMyShipDrill>();
List<IMyShipConnector> allConnectors = new List<IMyShipConnector>();

////////// Methods //////////

/** Method for initializing the script **/
void Initialize()	{
	SystemInitialized = true; // set system to init
	Runtime.UpdateFrequency = UpdateFrequency.Update10; //get system tick clock
	HUD = (IMyTextPanel)GetTerminalBlockByName(ScreenName); // Get refference to "named" LCD-display and save it to HUD
	GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(allCargo, b => b.CubeGrid == Me.CubeGrid); // Get all cargo blocks on the same grid as this PB
    GridTerminalSystem.GetBlocksOfType<IMyCockpit>(allCockpit, b => b.CubeGrid == Me.CubeGrid); // Get all cargo blocks on the same grid as this PB
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(allDrills, b => b.CubeGrid == Me.CubeGrid); // Get all cargo blocks on the same grid as this PB
    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(allConnectors, b => b.CubeGrid == Me.CubeGrid); // Get all cargo blocks on the same grid as this PB
}

/** Method to get block by name **/
IMyTerminalBlock GetTerminalBlockByName(string name)	{
	return GridTerminalSystem.GetBlockWithName(name);
}

/** Method for updating the displays **/
void UpdateHUD()	{
    OreFilledBar = (int)(BarLength * OrePercentUsed); //calculate displayed bar length
	OreBar = "["; //Reinit variable
    for (int i = OreFilledBar; i > 0; i--)	{
        OreBar += BarFill;
    }
    for (int i = BarLength - OreFilledBar; i > 0; i--)	{
        OreBar += BarEmpty;
    }
    OreCargoIndicator = OreBar + "]\n" + "Load: " + OrePercentFull + "%";
    Echo(OreCargoIndicator);
    HUD.WriteText(OreCargoIndicator);
}

/** Method to calculate used cargo space **/
void CalculateCargo()	{
	OreUsedVolume = 0;
    OreMaxVolume = 0;
    for(int i=0; i < allCargo.Count; i++)	{
        OreUsedVolume += (float)allCargo[i].GetInventory(0).CurrentVolume;
        OreMaxVolume += (float)allCargo[i].GetInventory(0).MaxVolume;
    }
	for (int i = 0; i < allCockpit.Count; i++)	{
        OreUsedVolume += (float)allCockpit[i].GetInventory(0).CurrentVolume;
        OreMaxVolume += (float)allCockpit[i].GetInventory(0).MaxVolume;
    }
    for (int i = 0; i < allDrills.Count; i++)	{
        OreUsedVolume += (float)allDrills[i].GetInventory(0).CurrentVolume;
        OreMaxVolume += (float)allDrills[i].GetInventory(0).MaxVolume;
    }
    for (int i = 0; i < allConnectors.Count; i++)	{
        OreUsedVolume += (float)allConnectors[i].GetInventory(0).CurrentVolume;
        OreMaxVolume += (float)allConnectors[i].GetInventory(0).MaxVolume;
    }
    OrePercentUsed = OreUsedVolume / OreMaxVolume;
    OrePercentFull = Math.Round(100 * (OreUsedVolume / OreMaxVolume), 2);
}

/** Main Programm **/
public void Main(string argument)	{
		
	// Initialize script when not already done
	if(SystemInitialized == false)	{
		Initialize();
	}
	
	if (HUD == null)	{
		Echo ("\n No LCD with name: " + ScreenName);
	}
	else	{
		CalculateCargo();
		// Update loop for display
		updateTimer++;
		if(updateTimer>updateFrequency)	{
			UpdateHUD();
			updateTimer = 0;
		}
	}	
}

//----------End of script--------------- 
/*
* CHANGELOG / Developer's roadmap (# marks the actual version)
* v0.1: Initial code base;
* v1.0 release;
* #v1.1 also include drill and connector cargo
*/