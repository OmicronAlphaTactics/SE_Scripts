/* 
* Hangar doors with warning flashlights script (2 doors)
* By Lazalatin90
* v0.7
* 
* Instructions: 
* Construct a timer block with a 1s timing (which shall trigger the programmable block containing this script using the parameters mentioned later)
* Set the hangar doors with the specified names! 
* Parameters which are to be given for specific hangar doors are: -b (portside), -s (starboard) 
* If no parameter is specified, it is assumed that both doors should be triggered. 
*/ 
//-------------------------Code Start---------------------------------- 
/*
* Our string vars to identify the objects to trigger.
* Just adjust it to the names you have given to the objects, which should be triggered. Make sure that these names are unique!
* You can also use markers like "(hangar)", it does not need to be the whole name.
*/
string airtightHangarDoor="Hangartor";
string hangarInteriorDoor="Hangart�r";
string hangarWarningLight="Hangarwarnlicht";
string hangarAirVent="Hangarl�fter";
string hangarTimerBlockName="Hangarzeitschaltuhr";
string hangarTextPanelName="Hangartexttafel";
//Our string vars to identify the objects to trigger sidewise
string portsideAirtightHangarDoor="Hangartor backbord";
string starboardAirtightHangarDoor="Hangartor steuerbord";
string portsideWarningLight="Hangarwarnlicht backbord";
string starboardWarningLight="Hangarwarnlicht steuerbord";
/*
* Our integer vars to determine how much time has to pass until the next step has to be made.
* Just estimate how much time each step will take to complete; I just filled in my estimations, you can alter them, if needed.
*/
float hangarInteriorDoorsLockdownTime = 3.0F; //Here no time for reactivating routine is needed, should be instantly...
//float airVentDeRePressurizeRoomTime = 10.0F; //Is a placeholder for a to-be-added-later feature.
float airtightHangarDoorsTogglingTime = 9.0F; //Depends on how big your hangar doors are. I think 6 seconds should be enough...

//Our global lists
List<IMyInteriorLight> hangarWarningLights = new List<IMyInteriorLight>();
List<IMyDoor> airtightHangarDoors = new List<IMyDoor>();
List<IMyDoor> hangarInteriorDoors = new List<IMyDoor>();
List<IMyAirVent> hangarAirVents = new List<IMyAirVent>();
List<IMyTimerBlock> hangarTimerBlocks = new List<IMyTimerBlock>();
List<IMyTextPanel> hangarTextPanels = new List<IMyTextPanel>();
//Our global hangar opened variable
bool hangarOpened=false;
bool hangarStateRequested=false;
uint step=1;

//Our script entry method
void Main(string input) {
   //Initialize Lists and lastStepDone var
   ListFiller();
   bool lastStepDone=false;
   //Make sure that input is not null
   if(input == null){
      input = "";
   }
   //Check if hangar doors are opened
   if(!hangarStateRequested){
      //Check only portside hangar doors
      if (input.Contains("-b")){
      for(int i=0; i < airtightHangarDoors.Count; i++){
         if (airtightHangarDoors[i].CustomName.Contains(portsideAirtightHangarDoor)) {
            if (airtightHangarDoors[i].Open){
               hangarOpened = true;
            }
         hangarStateRequested = true;
         }
      }
      //else check only starboard hangar doors
      } else if (input.Contains("-s")){
         for(int i=0; i < airtightHangarDoors.Count; i++){
            if (airtightHangarDoors[i].CustomName.Contains(starboardAirtightHangarDoor)) {
               if (airtightHangarDoors[i].Open){
                  hangarOpened = true;
               }
            hangarStateRequested = true;
            }
         }
      //else check all hangar doors
      } else {
         for (int j = 0; j < airtightHangarDoors.Count; j++){
            if (airtightHangarDoors[j].Open){
               hangarOpened = true;
            }
         }
         hangarStateRequested = true;
      }
   }
   //Go into subroutines if the time is right
   if (!hangarOpened){
      if (step==1){
         HangarInteriorDoorsLockdown(1);
         TriggerTimerBlock(input, hangarInteriorDoorsLockdownTime);
      }
      else if (step==2){
         HangarInteriorDoorsLockdown(2);
         OpenHangarDoors(input);
         lastStepDone = true;
      }
   } else { 
      if (step==1){
         CloseHangarDoors();
         TriggerTimerBlock("", airtightHangarDoorsTogglingTime);//No need to communicate the pushed button, all doors should be closed!
      }
      else if (step==2){
         HangarInteriorDoorsActivate();
         lastStepDone = true;
      }
   }
   step++;
   if(lastStepDone){
      hangarStateRequested=false;
      step=1;
      hangarOpened=false;
   }
   Output();
} 
 
/** 
* Method for opening specified hangar doors 
* vars: 
* string input : Awaits input from main method to determine which hangar doors to toggle 
**/ 
void OpenHangarDoors(string input) { 
   // If the portside hangar doors should be opened
   if (input.Contains("-b")){ 
      for(int i=0; i < airtightHangarDoors.Count; i++){
         if (airtightHangarDoors[i].CustomName.Contains(portsideAirtightHangarDoor)) {
            //Never got why we have to apply an action to an object instead of just triggering it... but okay, it's not my API (maybe it's about security issues?)
            var act = airtightHangarDoors[i].GetActionWithName("Open_On");
            act.Apply(airtightHangarDoors[i]);
         }
      }
      WarningLightsOn(input);
   //else if the starboard hangar doors should be opened
   } else if (input.Contains("-s")){
      for(int i=0; i < airtightHangarDoors.Count; i++){
         if (airtightHangarDoors[i].CustomName.Contains(starboardAirtightHangarDoor)) {
            var act = airtightHangarDoors[i].GetActionWithName("Open_On");
            act.Apply(airtightHangarDoors[i]);
         }
         WarningLightsOn(input);
      }
      //WarningLightsOn(input);
   //else all hangar doors should be opened
   } else {
      for(int i=0; i < airtightHangarDoors.Count; i++){
         var act = airtightHangarDoors[i].GetActionWithName("Open_On");
         act.Apply(airtightHangarDoors[i]);
      }
      WarningLightsOn(input);  
   }
}
 
/** Method for closing all hangar doors **/ 
void CloseHangarDoors(){
   for (int i=0; i < airtightHangarDoors.Count; i++){
      var act = airtightHangarDoors[i].GetActionWithName("Open_Off");
      act.Apply(airtightHangarDoors[i]);
      WarningLightsOff();
   }
}

/** 
* Method for setting hangarWarningLights into warning-mode 
* vars: 
* string input : Awaits input from main method to determine which warning lights to toggle 
**/ 
void WarningLightsOn(string input){
   for(int i=0; i < hangarWarningLights.Count; i++){
      if(input.Contains("-b")){
         if (hangarWarningLights[i].CustomName.Contains(portsideWarningLight)){
            var act = hangarWarningLights[i].GetActionWithName("OnOff_On");
            act.Apply(hangarWarningLights[i]); 
         } 
      } else if(input.Contains("-s")){
         if (hangarWarningLights[i].CustomName.Contains(starboardWarningLight)){
            var act = hangarWarningLights[i].GetActionWithName("OnOff_On");
            act.Apply(hangarWarningLights[i]);  
         }
      } else {
         var act = hangarWarningLights[i].GetActionWithName("OnOff_On");
         act.Apply(hangarWarningLights[i]); 
      }
   }
} 
 
/** Method for setting hangarWarningLights into normal-mode **/ 
void WarningLightsOff(){ 
   for(int i=0; i < hangarWarningLights.Count; i++){
      var act = hangarWarningLights[i].GetActionWithName("OnOff_Off");
      act.Apply(hangarWarningLights[i]); 
   }  
}

/** Method for locking up and deactivate all interior doors leading to the hangar **/ 
void HangarInteriorDoorsLockdown(uint lockStep){
   for(int i=0; i < hangarInteriorDoors.Count; i++){
      if(lockStep==1){
         var actClose = hangarInteriorDoors[i].GetActionWithName("Open_Off");
         actClose.Apply(hangarInteriorDoors[i]);
      }
      if(lockStep==2){
         var actDeactivate = hangarInteriorDoors[i].GetActionWithName("OnOff_Off");
         actDeactivate.Apply(hangarInteriorDoors[i]);
      }
   }
}

/** Method for activating all interior doors leading to the hangar **/
void HangarInteriorDoorsActivate(){
   for(int i=0; i < hangarInteriorDoors.Count; i++){
      var actActivate = hangarInteriorDoors[i].GetActionWithName("OnOff_On");
      actActivate.Apply(hangarInteriorDoors[i]);
   }
}

/** 
* Method for triggering the desired timer block
* vars: 
* string input : Awaits input from main method to determine which timer block to toggle 
* float triggerDelay : Awaits delay to set the timer block to, only 1s or more allowed
**/ 
void TriggerTimerBlock(string input, float triggerDelay) {
   // If the portside hangar doors should be opened/closed
   if (input.Contains("-b")){
      for(int i=0; i < hangarTimerBlocks.Count; i++){
         if (hangarTimerBlocks[i].CustomName.Contains("(portside)")) {
            hangarTimerBlocks[i].SetValue("TriggerDelay", triggerDelay);
            var act = hangarTimerBlocks[i].GetActionWithName("Start");
            act.Apply(hangarTimerBlocks[i]);
         }
      }
      WarningLightsOn(input);
   //else if the starboard hangar doors should be opened/closed
   } else if (input.Contains("-s")){
      for(int i=0; i < hangarTimerBlocks.Count; i++){
         if (hangarTimerBlocks[i].CustomName.Contains("(starboard)")) {
            hangarTimerBlocks[i].SetValue("TriggerDelay", triggerDelay);
            var act = hangarTimerBlocks[i].GetActionWithName("Start");
            act.Apply(hangarTimerBlocks[i]);
         }
      }
      WarningLightsOn(input);
   //else all hangar doors should be opened/closed
   } else {
      for(int i=0; i < hangarTimerBlocks.Count; i++){
         if (hangarTimerBlocks[i].CustomName.Contains("(both)")){
            hangarTimerBlocks[i].SetValue("TriggerDelay", triggerDelay);
            var act = hangarTimerBlocks[i].GetActionWithName("Start");
            act.Apply(hangarTimerBlocks[i]);
         }         
      } 
   } 
} 

/** Method for setting up all lists contained **/
void ListFiller(){
   List<IMyTerminalBlock> shortList = new List<IMyTerminalBlock>();
   GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(shortList);
   //as long as we have items in our grid...
   for (int i=0; i<shortList.Count; ++i){
      //if an item in our list is an air vent and is named like in hangarAirVent
      if (shortList[i] is IMyAirVent && shortList[i].CustomName.Contains(hangarAirVent)){
         hangarAirVents.Add(shortList[i] as IMyAirVent); 
      }
      //if an item in our list is a door (hangar doors, interior doors, etc.)
      else if (shortList[i] is IMyDoor){
         //if the door is named like in airtightHangarDoor
         if(shortList[i].CustomName.Contains(airtightHangarDoor)){
            airtightHangarDoors.Add(shortList[i] as IMyDoor);
         }
         //else if the door is named like in hangarInteriorDoor
         else if(shortList[i].CustomName.Contains(hangarInteriorDoor)){
            hangarInteriorDoors.Add(shortList[i] as IMyDoor);
         }
      }
      //else if an item in our list is an interior light
      else if (shortList[i] is IMyInteriorLight){
         //if the light is named like in hangarWarningLight
         if(shortList[i].CustomName.Contains(hangarWarningLight)){
            hangarWarningLights.Add(shortList[i] as IMyInteriorLight);
         }
      }
      //else if an item in our list is a timer block and is named like in hangarTimerBlockName
      else if(shortList[i] is IMyTimerBlock && shortList[i].CustomName.Contains(hangarTimerBlockName)){
         hangarTimerBlocks.Add(shortList[i] as IMyTimerBlock);
      }
      //else if an item in our list is a text panel and is named like in hangarTextPanelName
      else if(shortList[i] is IMyTextPanel && shortList[i].CustomName.Contains(hangarTextPanelName)){
         hangarTextPanels.Add(shortList[i] as IMyTextPanel);
      }
     //now we have lists of interior lights, doors and air vents; a timer block
   }
}

/** Output global vars to text panel **/
void Output(){
   for(int i=0; i<hangarTextPanels.Count; i++){
      hangarTextPanels[i].WritePublicText("Hangar opened: "+ hangarOpened.ToString() + "\nState requested: "+hangarStateRequested.ToString()+"\nStep: "+step.ToString(), false);
   }
}
//----------End of script--------------- 
/*
* CHANGELOG / Developer's roadmap (# marks the actual version)
* v0.1: Initial code base;
* v0.2: Added methods for steering warning lights;
*         Added lists for vents and hangar interior doors for future features;
* v0.3: Added functionality for sidewise active warning lights;
* v0.4: Added variables in order to make it easy to adapt the script to different namings;
* v0.5: Added methods for a lockdown of interior doors leading to the hangar if it should be opened;
* v0.6: Added functionality which now requires 3 timer blocks in order to manage a correct order of closing doors and opening the hangar;
*           Added placeholders for future updates;
* # v0.7: Cleanup Code and terminate Bugs.
* v0.8: Added functionality for de-/repressurizing the hangar before/after opening/closing it;
* v0.9: Fully tested and reconfigured script update;
* v1.0: Steam-Workshop release;
* v1.1: Refurbish TextPanel output;
*/