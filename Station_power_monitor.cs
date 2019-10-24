/* 
* Station power monitor
* By Dragonhost
* v1.1.1
* 
* Instructions:
*
* 
*Sources:
*
*/ 
//-------------------------Code Start---------------------------------- 

////////// Variables //////////
bool SystemInitialized = false;
int updateTimer = 0,
            updateFrequency = 5,
            BarLength = 20,
            BatteryFilledBar = 0,
			SolarPanelFilledBar = 0;
string BarFill = "|",
            BarEmpty = ".",
            BatteryDisplayTag = "[Battery]",
			SolarDisplayTag = "[Solar]",
            BatteryBar = "",
            BatteryIndicator = "",
			SolarPanelBar = "",
            SolarPanelIndicator = "";
double BatteryPercentFull,
			BatteryPercentUsed,
			SolarPanelPercentUsed,
            SolarPanelPercentFull;
float BatteryCurrentCapacity = 0.0f,
            BatteryMaxCapacity = 0.0f,
            BatteryMaxOutput = 0.0f,
            BatteryCurrentOutput = 0.0f,
            BatteryCurrentInput = 0.0f,
            BatteryMaxInput = 0.0f,
			SolarPanelCurrentOutput = 0.0f,
            SolarPanelMaxOutput = 0.0f,
            SolarPanelMaxAvailableOutput = 0.0f;

Dictionary<string, IMyTextPanel> BatteryDisplayList = new Dictionary<string, IMyTextPanel>();
Dictionary<string, IMyTextPanel> SolarDisplayList = new Dictionary<string, IMyTextPanel>();

List<IMyBatteryBlock> StationBatterys = new List<IMyBatteryBlock>(); //List of station batterys
List<IMySolarPanel> StationSolarPanels = new List<IMySolarPanel>(); //List of station solar panels

////////// Methods //////////

/** Method for initializing the script **/
void Initialize() {
	SystemInitialized = true; // set system to init
	Runtime.UpdateFrequency = UpdateFrequency.Update10; //get system tick clock
	ListFiller();
}

/** Method for setting up all lists contained **/
void ListFiller()
{
    List<IMyTerminalBlock> shortList = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(shortList, b => b.CubeGrid == Me.CubeGrid);
    //as long as we have items in our grid...
    for (int i = 0; i < shortList.Count; ++i)	{
        //if an item in our list is a battery
        if (shortList[i] is IMyBatteryBlock)	{
            StationBatterys.Add(shortList[i] as IMyBatteryBlock);
        }
        //else if an item in our list is a text panel
        else if (shortList[i] is IMyTextPanel)	{
            //if the text panel is named like in OreDisplay
            if (shortList[i].CustomName.Contains(BatteryDisplayTag))	{
                string[] SplitString = new string[0];
                char[] seperators = new char[] { ']', '.' };
                SplitString = shortList[i].CustomName.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                if (SplitString.Count() > 2)	{
                    BatteryDisplayList[(SplitString[1] + SplitString[2]).Remove(0, 1)] = (shortList[i] as IMyTextPanel);
                }
            }
			else if (shortList[i].CustomName.Contains(SolarDisplayTag))	{
                string[] SplitString = new string[0];
                char[] seperators = new char[] { ']', '.' };
                SplitString = shortList[i].CustomName.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                if (SplitString.Count() > 2)	{
                    SolarDisplayList[(SplitString[1] + SplitString[2]).Remove(0, 1)] = (shortList[i] as IMyTextPanel);
                }
            }
        }
		//if an item in our list is a solar panel
        else if (shortList[i] is IMySolarPanel)	{
            StationSolarPanels.Add(shortList[i] as IMySolarPanel);
        }
    }
}

/** Method for calculating the battery capacity **/
void CalculateBatteryCapacity()	{
    BatteryCurrentCapacity = 0;
    BatteryMaxCapacity = 0;
	BatteryMaxOutput = 0;
	BatteryCurrentOutput = 0;
	BatteryCurrentInput = 0;
	BatteryMaxInput = 0;	
    BatteryBar = "Indicator: \n[";
    for (int i = 0; i < StationBatterys.Count; i++)	{
        BatteryCurrentCapacity += (float)Math.Round(StationBatterys[i].CurrentStoredPower,2);
        BatteryMaxCapacity += (float)Math.Round(StationBatterys[i].MaxStoredPower,2);
        BatteryMaxOutput += (float)Math.Round(StationBatterys[i].MaxOutput,2);
        BatteryCurrentOutput += (float)Math.Round(StationBatterys[i].CurrentOutput,2);
        BatteryCurrentInput += (float)Math.Round(StationBatterys[i].CurrentInput,2);
        BatteryMaxInput += (float)Math.Round(StationBatterys[i].MaxInput,2);
    }
    BatteryPercentUsed = BatteryCurrentCapacity / BatteryMaxCapacity;
    BatteryPercentFull = Math.Round(100 * (BatteryCurrentCapacity / BatteryMaxCapacity), 2);

    BatteryFilledBar = (int)(BarLength * BatteryPercentUsed);
    for(int i= BatteryFilledBar; i>0; i--)	{
        BatteryBar += BarFill;
    }
    for (int i = BarLength - BatteryFilledBar; i > 0; i--)	{
        BatteryBar += BarEmpty;
    }
	BatteryIndicator = "Battrey Monitor\n \n";
    BatteryIndicator += BatteryBar + "]\n";
	BatteryIndicator += "Remaining capacity: " + BatteryPercentFull + " %\n \n";
	BatteryIndicator += "Technical status:\n";
    BatteryIndicator += "Current capacity: " + BatteryCurrentCapacity + " MW \n";
    BatteryIndicator += "Max capacity: " + BatteryMaxCapacity + " MW \n";
    BatteryIndicator += "Current output: " + BatteryCurrentOutput + " MW \n";
    BatteryIndicator += "Max output: " + BatteryMaxOutput + " MW \n";
    BatteryIndicator += "Current input: " + BatteryCurrentInput + " MW \n";
    BatteryIndicator += "Max input: " + BatteryMaxInput + " MW \n";
}

/** Method for calculating the battery capacity **/
void CalculateSolarCapacity()
{
    SolarPanelCurrentOutput = 0;
    SolarPanelMaxOutput = 0;
	SolarPanelMaxAvailableOutput = 0;
    SolarPanelBar = "Indicator: \n[";
    for (int i = 0; i < StationSolarPanels.Count; i++)	{
        SolarPanelCurrentOutput += (float)Math.Round(StationSolarPanels[i].CurrentOutput, 2);
        SolarPanelMaxOutput += (float)Math.Round(StationSolarPanels[i].MaxOutput, 2);
        SolarPanelMaxAvailableOutput += (float)Math.Round(0.12,2);
    }
	if (SolarPanelMaxOutput != 0)	{
        SolarPanelPercentUsed = SolarPanelCurrentOutput / SolarPanelMaxAvailableOutput;
        SolarPanelPercentFull = Math.Round(100 * (SolarPanelCurrentOutput / SolarPanelMaxAvailableOutput), 2);
	}
	else	{
		SolarPanelPercentUsed = 0;
		SolarPanelPercentFull = Math.Round(100 * (SolarPanelCurrentOutput / SolarPanelMaxAvailableOutput), 2);
	}

    SolarPanelFilledBar = (int)(BarLength * SolarPanelPercentUsed);
    for (int i = SolarPanelFilledBar; i > 0; i--)	{
        SolarPanelBar += BarFill;
    }
    for (int i = BarLength - SolarPanelFilledBar; i > 0; i--)	{
        SolarPanelBar += BarEmpty;
    }
    SolarPanelIndicator = "Solar PanelMonitor\n \n";
    SolarPanelIndicator += SolarPanelBar + "]\n";
    SolarPanelIndicator += "Power usage: " + Math.Round(SolarPanelPercentFull,2) + " %\n\n";
	SolarPanelIndicator += "Current output: " + Math.Round(SolarPanelCurrentOutput,2) + " MW\n";
    SolarPanelIndicator += "Max output: " + Math.Round(SolarPanelMaxOutput,2) + " MW \n";
    SolarPanelIndicator += "Max av. output: " + Math.Round(SolarPanelMaxAvailableOutput,2) + " MW \n";
}

/** Method for updating the mining text panels **/
public void UpdateBatteryDisplays()	{
    string[] SplitString = new string[0];
    char[] seperators = new char[] { '\n' };
    var List = BatteryDisplayList.ToList();
    List.Sort((m1, m2) => string.Compare(m1.Key, m2.Key));
    SplitString = BatteryIndicator.Split(seperators, StringSplitOptions.None);
    // Message output for all mining text panels
    for (int i = 0; i < List.Count; ++i)	{
        string Index = "";
        Index = List[i].Key;
        Index = Index.Remove(1, 1);

        if (List.Count > (i + 1))	{
            if (List[i + 1].Key.Contains(Index))	{
                List[i].Value.WriteText("", false);//Clear text panel
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1.7;
                List[i + 1].Value.WriteText("", false);//Clear text panel
                List[i + 1].Value.Font = "Monospace";
                List[i + 1].Value.FontSize = (float)1.7;
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
            else	{
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1;
                List[i].Value.WriteText("", false);//Clear text panel
                for (int k = 0; k < (SplitString.Count()); ++k)	{
                    List[i].Value.WriteText(SplitString[k] + "\n", true);
                }
            }
        }
        else	{
            List[i].Value.Font = "Monospace";
            List[i].Value.FontSize = (float)1;
            List[i].Value.WriteText("", false);//Clear text panel
            for (int l = 0; l < (SplitString.Count()); ++l)	{
                List[i].Value.WriteText(SplitString[l] + "\n", true);
            }
        }
    }
}

/** Method for updating the mining text panels **/
public void UpdateSolarDisplays()	{
    string[] SplitString = new string[0];
    char[] seperators = new char[] { '\n' };
    var List = SolarDisplayList.ToList();
    List.Sort((m1, m2) => string.Compare(m1.Key, m2.Key));
    SplitString = SolarPanelIndicator.Split(seperators, StringSplitOptions.None);
    // Message output for all mining text panels
    for (int i = 0; i < List.Count; ++i)	{
        string Index = "";
        Index = List[i].Key;
        Index = Index.Remove(1, 1);

        if (List.Count > (i + 1))	{
            if (List[i + 1].Key.Contains(Index))	{
                List[i].Value.WriteText("", false);//Clear text panel
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1.7;
                List[i + 1].Value.WriteText("", false);//Clear text panel
                List[i + 1].Value.Font = "Monospace";
                List[i + 1].Value.FontSize = (float)1.7;
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
            else	{
                List[i].Value.Font = "Monospace";
                List[i].Value.FontSize = (float)1;
                List[i].Value.WriteText("", false);//Clear text panel
                for (int k = 0; k < (SplitString.Count()); ++k)	{
                    List[i].Value.WriteText(SplitString[k] + "\n", true);
                }
            }
        }
        else	{
            List[i].Value.Font = "Monospace";
            List[i].Value.FontSize = (float)1;
            List[i].Value.WriteText("", false);//Clear text panel
            for (int l = 0; l < (SplitString.Count()); ++l)	{
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
        CalculateBatteryCapacity();
		CalculateSolarCapacity();
        UpdateBatteryDisplays();		
		UpdateSolarDisplays();		
		updateTimer = 0;
	}
}

//----------End of script--------------- 
/*
* CHANGELOG / Developer's roadmap (# marks the actual version)
* v0.1: Initial code base;
* v1.0 release;
* v1.1 display solar panel power stats
* #v1.1.1 fix bug with incorrect rounding of numbers on the display
* v1.3 display reactor power stats
* v2.0 battery capacity warning
*/
