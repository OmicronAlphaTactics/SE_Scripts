/* 
* Airlocks with audio script
* By Lazalatin90
* v0.1
* 
* Instructions: 
* 
*
*
*/ 
//-------------------------Code Start---------------------------------- 
/*
* Our string vars to identify the objects to trigger.
* Just adjust it to the names you have given to the objects, which should be triggered. Make sure that these names are unique!
* You can also use markers like "(hangar)", it does not need to be the whole name.
*/
string airlockDoor="Luftschleuse";
string airlockSpeaker="Lautsprecher Luftschleuse";
string airlockTimerBlockName="Luftschleusenzeitschaltuhr";
string airlockSensor="Luftschleusensensor";
//Our string vars to identify the objects to trigger sidewise
string portsideString="backbord";
string starboardString="steuerbord";
//Our string vars to identify whether we have the outer or inner airlock door
string innerString="innen";
string outerString="aussen";

/*
* Our integer var to determine how much time has to pass until the next step has to be made.
* Just estimate how much time each step will take to complete; I just filled in my estimations, you can alter them, if needed.
*/
float airlockDoorsLockdownTime = 3.0F; //Here no time for reactivating routine is needed, should be instantly...

//Our global lists
List<IMySpeaker> airlockSpeakers = new List<IMySpeaker>();
List<IMyDoor> airlockDoors = new List<IMyDoor>();
List<IMyTimerBlock> airlockTimerBlocks = new List<IMyTimerBlock>();
List<IMySensor> airlockSensors = new List<IMySensor>();
//Our global hangar opened variable
bool airlockDoorOpened=false; //false means here that the inner door is opened, true means that the outer door is opened.
bool airlockStateRequested=false;
string triggerAirlockSide="";
uint step=0;

//Our script entry method
void Main(string input) {
   //Initialize Lists and lastStepDone var
   ListFiller();
   bool lastStepDone=false;
   //Make sure that input is not null
   if(input == null){
      input = "";
   }
   //Check if airlock doors status has been already checked
   if(!airlockStateRequested){
      //Check only portside airlock doors
      if (input.Contains("-b")){
         for(int i=0; i < airlockDoors.Count; i++){
            if (airlockDoors[i].CustomName.Contains(airlockDoor) && airlockDoors[i].CustomName.Contains(portsideString)) {
               if (airlockDoors[i].Open && airlockDoors[i].CustomName.Contains(innerString)){
                  airlockDoorOpened = true;
                  triggerAirlockSide = "inner";
               }
               else if (airlockDoors[i].Open && airlockDoors[i].CustomName.Contains(outerString)){
                  airlockDoorOpened = true;
                  triggerAirlockSide = "outer";
               }
            airlockStateRequested = true;
            }
         }
      //else check only starboard hangar doors
      } else if (input.Contains("-s")){
         for(int i=0; i < airlockDoors.Count; i++){
            if (airlockDoors[i].CustomName.Contains(airlockDoor) && airlockDoors[i].CustomName.Contains(starboardString)) {
               if (airlockDoors[i].Open && airlockDoors[i].CustomName.Contains(innerString)){
                  airlockDoorOpened = true;
                  triggerAirlockSide = "inner";
               }
               else if (airlockDoors[i].Open && airlockDoors[i].CustomName.Contains(outerString)){
                  airlockDoorOpened = true;
                  triggerAirlockSide = "outer";
               }
            airlockStateRequested = true;
            }
         }
      }
   }
   //Go into subroutines if the time is right
   if (airlockDoorOpened){
      if (step==1){
         CloseAirlockDoors();
         TriggerTimerBlock(input, airlockDoorsLockdownTime);
      }
      else if (step==2){
         HangarInteriorDoorsLockdown(2);
         OpenHangarDoors(input);
         lastStepDone = true;
      }
   } else { 
      if (step==1){
         CloseAirlockDoors();
         TriggerTimerBlock("", airlockDoorsLockdownTime);//No need to communicate the pushed button, all doors should be closed!
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
} 
 
/** 
* Method for opening specified hangar doors 
* vars: 
* string input : Awaits input from main method to determine which hangar doors to toggle 
**/ 
void OpenAirlockDoors(string input) { 
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

/**
* Method for closing all airlock doors.
* vars: none
**/
void CloseAirlockDoors(){

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
      //if an item in our list is a door (hangar doors, interior doors, etc.)
      if (shortList[i] is IMyDoor){
         //if the door is named like in airlockDoor
         if(shortList[i].CustomName.Contains(airlockDoor)){
            airlockDoors.Add(shortList[i] as IMyDoor);
         }
      }
      //else if an item in our list is a speaker
      else if (shortList[i] is IMySpeaker){
         //if the light is named like in airlockSpeakers
         if(shortList[i].CustomName.Contains(airlockSpeaker)){
            airlockSpeakers.Add(shortList[i] as IMySpeaker);
         }
      }
      //else if an item in our list is a timer block and is named like in airlockTimerBlocks
      else if(shortList[i] is IMyTimerBlock && shortList[i].CustomName.Contains(airlockTimerBlocks)){
         airlockTimerBlocks.Add(shortList[i] as IMyTimerBlock);
      }
      //else if an item in our list is a Sensor and is named like in airlockSensor
      else if(shortList[i] is IMySensor && shortList[i].CustomName.Contains(airlockSensor)){
         airlockSensors.Add(shortList[i] as IMySensor);
      }
     //now we have lists of speakers and airlock doors; a timer block
   }
}
//----------End of script--------------- 
/*
* CHANGELOG / Developer's roadmap (# marks the actual version)
* # v0.1: Initial code base;
* v0.2: Add functional Airlock methods
* v0.3: Debug and refine code
* v0.4: 
* v0.5: 
* v0.6: 
* v0.7: 
* v0.8: Add audio functionality
* v0.9: Fully tested and reconfigured script update;
* v1.0: Steam-Workshop release;
*/