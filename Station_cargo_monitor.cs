/* 
* Station Cargo Monitor
* By Dragonhost
* v1.3.2
* 
* Instructions:
*
*	Mining Display
* 	If you want to use 2 screens for the mining list place them in a vertical line. Name the first display like this:
* 	"[Mining] A.1" and the second display "[Mining] A.2". When you only use one display name it like this: "[Mining] A.1"
* 
*Sources:
*< Brendan Jurd > "direvus" <https://gist.github.com/direvus/4025060310a9a62e1fc13888ab7f8bc9>
*
*/ 
//-------------------------Code Start---------------------------------- 

////////// Variables //////////
List<IMyCargoContainer> OreContainers = new List<IMyCargoContainer>(); //List of used ore containers
List<IMyCargoContainer> IngotContainers = new List<IMyCargoContainer>(); //List of used ingot containers
//List<IMyTextPanel> OreDisplays = new List<IMyTextPanel>(); //List of displays that will be used as ore monitor
//List<IMyTextPanel> IngotDisplays = new List<IMyTextPanel>(); //List of displays that will be used as ingot monitor
//List<IMyTextPanel> MiningDisplays = new List<IMyTextPanel>(); //List of displays that will be used as mining monitor

bool SystemInitialized = false;
int	updateTimer = 0,
		updateFrequency = 5,
		OreFilledBar = 0,
		IngotFilledBar = 0,
		Limit_IronIngot = 100000,
		Limit_NickelIngot = 10000,
		Limit_SiliconIngot = 10000,
		Limit_CobaltIngot = 10000,
		Limit_MagnesiumIngot = 1000,
		Limit_SilverIngot = 5000,
		Limit_GoldIngot = 2000,
		Limit_UraniumIngot = 500,
		Limit_PlatinumIngot = 1000,		
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
Dictionary<string, float> MiningLimits = new Dictionary<string, float>();
Dictionary<string, IMyTextPanel> MiningDisplayList = new Dictionary<string, IMyTextPanel>();
Dictionary<string, IMyTextPanel> OreDisplayList = new Dictionary<string, IMyTextPanel>();
Dictionary<string, IMyTextPanel> IngotDisplayList = new Dictionary<string, IMyTextPanel>();
			
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
	BuildMiningDictionary();
}

/** Method for setting up all lists contained **/
void BuildMiningDictionary() {
		MiningLimits["Iron"] = (float)Limit_IronIngot;
		MiningLimits["Nickel"] = (float)Limit_NickelIngot;
		MiningLimits["Silicon"] = (float)Limit_SiliconIngot;
		MiningLimits["Cobalt"] = (float)Limit_CobaltIngot;
		MiningLimits["Magnesium"] = (float)Limit_MagnesiumIngot;
		MiningLimits["Silver"] = (float)Limit_SilverIngot;
		MiningLimits["Gold"] = (float)Limit_GoldIngot;
		MiningLimits["Uranium"] = (float)Limit_UraniumIngot;
		MiningLimits["Platinum"] = (float)Limit_PlatinumIngot;
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
				string[] SplitString = new string[0];
				char[] seperators = new char[] { ']', '.' };
				SplitString = shortList[i].CustomName.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
				if (SplitString.Count() > 2)    {
				OreDisplayList[(SplitString[1] + SplitString[2]).Remove(0, 1)] = (shortList[i] as IMyTextPanel);
				}
			}
			else if(shortList[i].CustomName.Contains(IngotDisplayTag)) {
				//IngotDisplays.Add(shortList[i] as IMyTextPanel);
				string[] SplitString = new string[0];
				char[] seperators = new char[] { ']', '.' };
				SplitString = shortList[i].CustomName.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
				if (SplitString.Count() > 2)    {
				IngotDisplayList[(SplitString[1] + SplitString[2]).Remove(0, 1)] = (shortList[i] as IMyTextPanel);
				}
			}
			else if (shortList[i].CustomName.Contains(MiningDisplayTag))	{
				//MiningDisplays.Add(shortList[i] as IMyTextPanel);
				//Build a list of text panel with unique name
				string[] SplitString = new string[0];
				char[] seperators = new char[] { ']', '.' };
				SplitString = shortList[i].CustomName.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
				if (SplitString.Count() > 2)    {
					MiningDisplayList[(SplitString[1] + SplitString[2]).Remove(0, 1)] = (shortList[i] as IMyTextPanel);
				}
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
	OreList = "Ore cargo list:\n";
	var pairs = OreTotals.ToList();
	for (int i = 0; i < pairs.Count; i++) {
		OreList += BuildString((pairs[i].Key), String.Format("{0} kg", (pairs[i].Value)), 10) + "\n";
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
	IngotList = "Ingot cargo list:\n";
	var pairs = IngotTotals.ToList();
	for (int i = 0; i < pairs.Count; i++) {
		IngotList += BuildString((pairs[i].Key), String.Format("{0} kg", (pairs[i].Value)), 10) + "\n";
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

/** Method to build a string of specific length with start and end text **/
public string BuildString(string StartText, string EndText, int NumCharacters)
{
    string NewStartText = StartText;
    for(int i = 0; i < (NumCharacters - StartText.Length); i++)	{
        NewStartText += " ";
    }
	return String.Format("{0,5}{1,15}", NewStartText, EndText);
}

/** Method to check ingot limits and create a Mining list **/
public void CheckIngotLimits()
{
    MiningList = "Ores to mine :\n";
    var LimitPairs = MiningLimits.ToList();
    for (int i = 0; i < LimitPairs.Count; i++)    {
        if (IngotTotals.ContainsKey(LimitPairs[i].Key)) {
            if (IngotTotals[LimitPairs[i].Key] < LimitPairs[i].Value)   {
                MiningList += BuildString((LimitPairs[i].Key), String.Format("{0} kg", (LimitPairs[i].Value - IngotTotals[LimitPairs[i].Key])), 10) + "\n";
            }
            else    {
                continue;
            }
        }
        else if (!IngotTotals.ContainsKey(LimitPairs[i].Key))   {
            MiningList += BuildString((LimitPairs[i].Key), String.Format("{0} kg", (LimitPairs[i].Value)), 10) + "\n";
        }
    }
}

/** Method for updating the mining text panels **/
public void UpdateMiningDisplays() {
    string[] SplitString = new string[0];
    char[] seperators = new char[] { '\n' };
    var List = MiningDisplayList.ToList();
	List.Sort((m1, m2) => string.Compare(m1.Key, m2.Key));
	
	//Echo(MiningList);
	SplitString = MiningList.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
	
    // Message output for all mining text panels
    for (int i = 0; i < List.Count; ++i)
    {
        string Index = "";
        Index = List[i].Key;
        Index = Index.Remove(1, 1);
		
		if (List.Count>(i+1))   {
            if (List[i + 1].Key.Contains(Index))	{
                List[i].Value.WriteText("", false);//Clear text panel
				List[i].Value.Font = "Monospace";
				List[i].Value.FontSize = (float)1.8;
                List[i + 1].Value.WriteText("", false);//Clear text panel
				List[i + 1].Value.Font = "Monospace";
				List[i + 1].Value.FontSize = (float)1.8;
                for (int j = 0; j < SplitString.Count(); ++j)	{
                    if (j < 8)	{
                        List[i].Value.WriteText(SplitString[j] + "\n", true);
                    }
                    else	{
                        List[i + 1].Value.WriteText(SplitString[j] + "\n", true);
                    }
                }
				i++;
            }
			else    {
            List[i].Value.Font = "Monospace";
            List[i].Value.FontSize = (float)1.6;
            List[i].Value.WriteText("", false);//Clear text panel
				for (int k = 0; k < (SplitString.Count()); ++k)    {
					List[i].Value.WriteText(SplitString[k] + "\n", true);
				}
			}
        }
        else    {
			List[i].Value.Font = "Monospace";
            List[i].Value.FontSize = (float)1.6;
            List[i].Value.WriteText("", false);//Clear text panel
            for (int l = 0; l < (SplitString.Count()); ++l)    {
                List[i].Value.WriteText(SplitString[l] + "\n", true);
            }
        }
    }
}		

/** Method for updating the ore text panels **/
public void UpdateOreDisplays()
{
    string[] SplitString = new string[0];
    char[] seperators = new char[] { '\n' };
    var List = OreDisplayList.ToList();
    List.Sort((m1, m2) => string.Compare(m1.Key, m2.Key));
    OreFilledBar = (int)(BarLength * OrePercentUsed); //calculate displayed bar length
    OreBar = "Ore cargo fill indicator:\n["; //Reinit variable
	SplitString = OreList.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
    // Fill bar with "fill mark"
    for (int i = OreFilledBar; i > 0; i--)
    {
        OreBar += BarFill;
    }

    // Fill bar with "empty marks"
    for (int i = BarLength - OreFilledBar; i > 0; i--)
    {
        OreBar += BarEmpty;
    }

    // Build message
    OreCargoIndicator = OreBar + "]\nOre cargo load: " + OrePercentFull + "%\n";

    // Message output for all ore text panels
    for (int i = 0; i < List.Count; ++i)
    {
        string Index = "";
        Index = List[i].Key;
        Index = Index.Remove(1, 1);
        if (List.Count > (i + 1))
        {
            if (List[i + 1].Key.Contains(Index))
            {
                List[i].Value.WriteText(OreCargoIndicator + "\n", false);
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1.8;
                List[i + 1].Value.WriteText("", false);//Clear text panel
                List[i + 1].Value.Font = "Monospace";
                List[i + 1].Value.FontSize = (float)1.8;
                for (int j = 0; j < SplitString.Count(); ++j)
                {
                    if (j < 4)
                    {
                        List[i].Value.WriteText(SplitString[j] + "\n", true);
                    }
                    else
                    {
                        List[i + 1].Value.WriteText(SplitString[j] + "\n", true);
                    }
                }
                i++;
            }
            else
            {
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1;
                List[i].Value.WriteText(OreCargoIndicator + "\n", false);
                for (int k = 0; k < (SplitString.Count()); ++k)
                {
                    List[i].Value.WriteText(SplitString[k] + "\n", true);
                }
            }
        }
        else
        {
            List[i].Value.Font = "Monospace";
            List[i].Value.FontSize = (float)1;
            List[i].Value.WriteText(OreCargoIndicator + "\n", false);
            for (int l = 0; l < (SplitString.Count()); ++l)
            {
                List[i].Value.WriteText(SplitString[l] + "\n", true);
            }
        }
    }
}

/** Method for updating the ore text panels **/
public void UpdateIngotDisplays()
{
    string[] SplitString = new string[0];
    char[] seperators = new char[] { '\n' };
    var List = IngotDisplayList.ToList();
    List.Sort((m1, m2) => string.Compare(m1.Key, m2.Key));

    //Echo(MiningList);
    SplitString = IngotList.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

    IngotFilledBar = (int)(BarLength * IngotPercentUsed); //calculate displayed bar length
    IngotBar = "Ingot cargo usage:\n["; //Reinit variable
                                               // Fill bar with "fill mark"
    for (int i = IngotFilledBar; i > 0; i--)
    {
        IngotBar += BarFill;
    }
    // Fill bar with "empty marks"
    for (int i = BarLength - IngotFilledBar; i > 0; i--)
    {
        IngotBar += BarEmpty;
    }

    // Build message
    IngotCargoIndicator = IngotBar + "]\nIngot cargo load: " + IngotPercentFull + "%\n";

    // Message output for all ore text panels
    for (int i = 0; i < List.Count; ++i)
    {
        string Index = "";
        Index = List[i].Key;
        Index = Index.Remove(1, 1);

        if (List.Count > (i + 1))
        {
            if (List[i + 1].Key.Contains(Index))
            {
                List[i].Value.WriteText(IngotCargoIndicator + "\n");
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1.8;
                List[i + 1].Value.WriteText("", false);//Clear text panel
                List[i + 1].Value.Font = "Monospace";
                List[i + 1].Value.FontSize = (float)1.8;
                for (int j = 0; j < SplitString.Count(); ++j)
                {
                    if (j < 4)
                    {
                        List[i].Value.WriteText(SplitString[j] + "\n", true);
                    }
                    else
                    {
                        List[i + 1].Value.WriteText(SplitString[j] + "\n", true);
                    }
                }
                i++;
            }
            else
            {
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1;
                List[i].Value.WriteText(IngotCargoIndicator + "\n");
                for (int k = 0; k < (SplitString.Count()); ++k)
                {
                    List[i].Value.WriteText(SplitString[k] + "\n", true);
                }
            }
        }
        else
        {
            List[i].Value.Font = "Monospace";
            List[i].Value.FontSize = (float)1;
            List[i].Value.WriteText(IngotCargoIndicator + "\n");
            for (int l = 0; l < (SplitString.Count()); ++l)
            {
                List[i].Value.WriteText(SplitString[l] + "\n", true);
            }
        }
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
* v1.2.1 add limits for all ingots
* v1.2.2 rework of the check mechanism to work with limit dictionary
* v1.x add colors for ingots under the limit (currently not possible)
* v1.3 add multi screen support for mining displays
* v1.3.1 rearrange line format for mining list and set font
* #v1.3.2 add multi screen support for all displays
* v1.3.3 fix bug of alse mining list calculation, don't calculate it 1:1
* v1.4 programmatically set font size based on multi screen usage or not
* v1.5 also check refinery, assembler for ores / ingots
*/
