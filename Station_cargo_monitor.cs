/* 
* Station Cargo Monitor
* By Dragonhost
* v1.2.1
* 
* Instructions: 
* 
*Sources:
*< Brendan Jurd > "direvus" <https://gist.github.com/direvus/4025060310a9a62e1fc13888ab7f8bc9>
*
*/ 
//-------------------------Code Start---------------------------------- 

////////// Variables //////////
List<IMyCargoContainer> OreContainers = new List<IMyCargoContainer>(); //List of used ore containers
List<IMyCargoContainer> IngotContainers = new List<IMyCargoContainer>(); //List of used ingot containers
List<IMyTextPanel> OreDisplays = new List<IMyTextPanel>(); //List of displays that will be used as ore monitor
List<IMyTextPanel> IngotDisplays = new List<IMyTextPanel>(); //List of displays that will be used as ingot monitor
List<IMyTextPanel> MiningDisplays = new List<IMyTextPanel>(); //List of displays that will be used as mining monitor

bool SystemInitialized = false;
int	updateTimer = 0,
		updateFrequency = 5,
		OreFilledBar = 0,
		IngotFilledBar = 0,
		Limit_IronIngot = 100000,
		Limit_Nickel = 10000,
		Limit_Silicon = 10000,
		Limit_Cobalt = 10000,
		Limit_Magnesium = 1000,
		Limit_Silver = 5000,
		Limit_Gold = 2000,
		Limit_Uranium = 500,
		Limit_Platinum = 1000,		
		BarLength = 20;
string Spacing1 = "    ",
			 BarFill = "|",
			 BarEmpty = ".",
			 OreDisplayTag = "[Ore]",
			 IngotDisplayTag = "[Ingot]",
			 MiningDisplayTag = "[Mining]",
			 OreContainerTag = "[Ore]",
			 IngotContainerTag = "[Ingot]",
			 OreBar = "Ore cargo:[",
			 IngotBar = "Ore cargo:[",
			 OreCargoIndicator = "",
			 IngotCargoIndicator = "",
			 OreList = "",
			 IngotList = "",
			 MiningList = "Ores to mine : ";
double OrePercentFull,
			 IngotPercentFull;
float OreUsedVolume = 0.0f,
			IngotUsedVolume = 0.0f,
			OreMaxVolume = 0.0f,
			IngotMaxVolume = 0.0f,
			IngotPercentUsed = 0,
			OrePercentUsed = 0;
			
Dictionary<string, float> OreTotals = new Dictionary<string, float>();
Dictionary<string, float> IngotTotals = new Dictionary<string, float>();
			
//-----------------------------------------------------------------
static string ingot_type = "MyObjectBuilder_Ingot";
static string ore_type = "MyObjectBuilder_Ore";
//-----------------------------------------------------------------

////////// Methods //////////

/** Method for initializing the script **/
void Initialize() {
	SystemInitialized = true; // set system to init
	Runtime.UpdateFrequency = UpdateFrequency.Update10; //get system tick clock
	ListFiller();
}

/** Method for setting up all lists contained **/
void ListFiller() {
  List<IMyTerminalBlock> shortList = new List<IMyTerminalBlock>();
  GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(shortList, b => b.CubeGrid == Me.CubeGrid);
  //as long as we have items in our grid...
  for (int i=0; i<shortList.Count; ++i) {
    //if an item in our list is a cargo container
    if (shortList[i] is IMyCargoContainer) {
      //if the container is named like in OreContainer
      if(shortList[i].CustomName.Contains(OreContainerTag)) {
        OreContainers.Add(shortList[i] as IMyCargoContainer);
      }
		else if(shortList[i].CustomName.Contains(IngotContainerTag)) {
			IngotContainers.Add(shortList[i] as IMyCargoContainer);
		}
  }
  //else if an item in our list is a text panel
  else if (shortList[i] is IMyTextPanel) {
  	//if the text panel is named like in OreDisplay
    if(shortList[i].CustomName.Contains(OreDisplayTag)) {
            OreDisplays.Add(shortList[i] as IMyTextPanel);
    }
		else if(shortList[i].CustomName.Contains(IngotDisplayTag)) {
            IngotDisplays.Add(shortList[i] as IMyTextPanel);
         }
		 else if(shortList[i].CustomName.Contains(MiningDisplayTag)) {
            MiningDisplays.Add(shortList[i] as IMyTextPanel);
         }
      }
     //now we have lists of ore containers and ore text panels
   }
}

/** Method for calculating available and unused cargo space **/
void CalculateOreCargoUsage() {
	OreUsedVolume = 0;
	OreMaxVolume = 0;
	for(int i=0;i<OreContainers.Count; i++)	{
		OreUsedVolume += (float)OreContainers[i].GetInventory(0).CurrentVolume;
		OreMaxVolume += (float)OreContainers[i].GetInventory(0).MaxVolume;
	}
	OrePercentUsed = OreUsedVolume / OreMaxVolume;
	OrePercentFull = Math.Round(100 * (OreUsedVolume / OreMaxVolume),2);
}

/** Method for calculating available and unsed ingot space **/
void CalculateIngotCargoUsage() {
	IngotUsedVolume = 0;
	IngotMaxVolume = 0;
	for(int i=0;i<IngotContainers.Count; i++)	{
		IngotUsedVolume += (float)IngotContainers[i].GetInventory(0).CurrentVolume;
		IngotMaxVolume += (float)IngotContainers[i].GetInventory(0).MaxVolume;
	}
	IngotPercentUsed = IngotUsedVolume / IngotMaxVolume;
	IngotPercentFull = Math.Round(100 * (IngotUsedVolume / IngotMaxVolume),2);
}

/** Method for listing available ores  **/
void CalculateOreCargo() {
	OreTotals = new Dictionary<string, float>();
	// search through all ore containers
	for(int i=0; i < OreContainers.Count; i++) {
		// search through all inventor items
		for(int j=0; j < OreContainers[i].InventoryCount; j++) {
			List<MyInventoryItem> items = new List<MyInventoryItem>();
			IMyInventory inventory = OreContainers[i].GetInventory(j);
			inventory.GetItems(items, null);
			for(int k=0; k < items.Count; k++) {
				if(!IsMainType(items[k], ore_type)) {
					continue;
				}
				string subtype = GetOreType(items[k]);
				float amount = (float)items[k].Amount;
				if(OreTotals.ContainsKey(subtype))	{
					OreTotals[subtype] += (float)Math.Round(amount,2);
				}
				else {
					OreTotals[subtype] = (float)Math.Round(amount,2);
				}
			}
		}
	}
	OreList = Spacing1 + "Ore cargo list:\n";
	var pairs = OreTotals.ToList();
  for (int i = 0; i < pairs.Count; i++) {
  	OreList += Spacing1 + String.Format("{0} {1} kg\n", pairs[i].Key, pairs[i].Value);
  }
}

/** Method for listing available ingots  **/
void CalculateIngotCargo() {
	IngotTotals = new Dictionary<string, float>();
	// search through all ore containers
	for(int i=0; i < IngotContainers.Count; i++) {
		// search through all items in inventory of current container
		for(int j=0; j < IngotContainers[i].InventoryCount; j++) {
			List<MyInventoryItem> items = new List<MyInventoryItem>();
			IMyInventory inventory = IngotContainers[i].GetInventory(j);
			inventory.GetItems(items, null);
			for(int k=0; k < items.Count; k++) {
				if(!IsMainType(items[k], ingot_type))	{
					continue;
				}
				string subtype = GetIngotType(items[k]);
				float amount = (float)items[k].Amount;
				if(IngotTotals.ContainsKey(subtype))	{
					IngotTotals[subtype] += (float)Math.Round(amount,2);
				}
				else {
					IngotTotals[subtype] = (float)Math.Round(amount,2);
				}
			}
		}
	}
	IngotList = Spacing1 + "Ingot cargo list:\n";
	var pairs = IngotTotals.ToList();
	for (int i = 0; i < pairs.Count; i++) {
		IngotList += Spacing1 + String.Format("{0} {1} kg\n", pairs[i].Key, pairs[i].Value);
	}
}

/** Method to check for item type **/
bool IsMainType(MyInventoryItem item, string maintype) {
    return (item.Type.ToString().Contains(maintype));
}

/** Method to check for item type **/
string GetOreType(MyInventoryItem item) {
	return (item.Type.ToString().Remove(0,ore_type.Length+1));
}

/** Method to check for item type **/
string GetIngotType(MyInventoryItem item) {
	return (item.Type.ToString().Remove(0,ingot_type.Length+1));
}

/** Method to check ingot limits and create a minig list **/
public void CheckIngotLimits()
{
	MiningList = "Ores to mine : ";
    var pairs = IngotTotals.ToList();
    for (int i = 0; i < pairs.Count; i++) {
        if (pairs[i].Key == "Iron"){
            if (pairs[i].Value <= Limit_IronIngot) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_IronIngot - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Nickel"){
            if (pairs[i].Value <= Limit_Nickel) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Nickel - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Silicon"){
            if (pairs[i].Value <= Limit_Silicon) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Silicon - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Cobalt"){
            if (pairs[i].Value <= Limit_Cobalt) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Cobalt - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Magnesium"){
            if (pairs[i].Value <= Limit_Magnesium) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Magnesium - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Silver"){
            if (pairs[i].Value <= Limit_Silver) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Silver - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Gold"){
            if (pairs[i].Value <= Limit_Gold) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Gold - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Uranium"){
            if (pairs[i].Value <= Limit_Uranium) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Uranium - pairs[i].Value));
            }
        }
		else if (pairs[i].Key == "Platinum"){
            if (pairs[i].Value <= Limit_Platinum) {
                MiningList += "\n" + Spacing1 + String.Format("{0}", pairs[i].Key) + Spacing1 + String.Format("{0} kg \n", (Limit_Platinum - pairs[i].Value));
            }
        }
    }
}

/** Method for updating the mining text panels **/
public void UpdateMiningDisplays() {
    // Message output for all ore text panels
    for (int i = 0; i < MiningDisplays.Count; ++i) {
        MiningDisplays[i].WriteText(MiningList);
    }
}		

/** Method for updating the ore text panels **/
void UpdateOreDisplays() {
	OreFilledBar = (int)(BarLength * OrePercentUsed); //calculate displayed bar length
	OreBar = "Ore cargo fill indicator:["; //Reinit variable
	
	// Fill bar with "fill mark"
	for (int i = OreFilledBar; i > 0; i--) {
		OreBar += BarFill;
	}
	
	// Fill bar with "empty marks"
	for (int i = BarLength - OreFilledBar; i > 0; i--) {  
		OreBar += BarEmpty;
	} 
	
	// Build message
	OreCargoIndicator = Spacing1 + OreBar + "]" + "\n" + Spacing1 + "Ore cargo load: " + OrePercentFull + "%" + "\n"; 
	
	// Message output for all ore text panels
	for (int i=0; i<OreDisplays.Count; ++i) {
	   OreDisplays[i].WriteText(OreCargoIndicator  + "\n" + OreList + "\n");
   }
}

/** Method for updating the ore text panels **/
void UpdateIngotDisplays() {
	IngotFilledBar = (int)(BarLength * IngotPercentUsed); //calculate displayed bar length
	IngotBar = "Ingot cargo fill indicator:["; //Reinit variable
	// Fill bar with "fill mark"
	for (int i = IngotFilledBar; i > 0; i--) {
		IngotBar += BarFill;
	}
	// Fill bar with "empty marks"
	for (int i = BarLength - IngotFilledBar; i > 0; i--) {  
		IngotBar += BarEmpty;
	} 
	
	// Build message
	IngotCargoIndicator = Spacing1 + IngotBar + "]" + "\n" + Spacing1 + "Ingot cargo load: " + IngotPercentFull + "%" + "\n"; 
	
	// Message output for all ore text panels
	for (int i=0; i<IngotDisplays.Count; ++i) {
	  IngotDisplays[i].WriteText(IngotCargoIndicator  + "\n" + IngotList + "\n");
  }
}

/** Main Programm **/
public void Main(string argument)  {
	//Make sure that input is not null
	if(argument == null) {
      argument = "";
	}
	
	if(!SystemInitialized) {
		Initialize();
	}
	
	updateTimer++;
	if(updateTimer>updateFrequency) {	
		CalculateOreCargoUsage();
		CalculateIngotCargoUsage();
		CalculateOreCargo();
		CalculateIngotCargo();
		UpdateOreDisplays();
		UpdateIngotDisplays();
		CheckIngotLimits();
		UpdateMiningDisplays();
		updateTimer = 0;
	}
}

//----------End of script--------------- 
/*
* CHANGELOG / Developer's roadmap (# marks the actual version)
* v0.1: Initial code base;
* v1.0 release;
* v1.1 add ingot cargo display
* v1.1.1 patch naming of file
* v1.1.2 rework code to contributing standards and fix some typos
* v1.2 add limits for ingots and create a mining list
* #v1.2.1 add limits for all ingots
* v1.2.2 rework check mechanism to work with limit dictionary
* v1.3 add colors for ingots under the limit
* v1.4 add multi screen support
*/
