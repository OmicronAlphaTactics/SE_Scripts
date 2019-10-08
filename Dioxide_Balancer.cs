/* 
* Oxygen balancer script
* By Lazalatin90
* v0.1
* 
* Instructions: 
* Construct a timer block with suiting timing (which shall trigger the programmable block containing this script)
* I recommend a 30s loop.
*/ 
//-------------------------Code Start---------------------------------- 
//Out global lists
List<IMyOxygenTank> gridOxygenTanks = new List<IMyOxygenTank>();
List<IMyOxygenGenerator> gridOxygenGenerators = new List<IMyOxygenGenerator>();

//Our script entry method
void Main(){
   //Initialize
   float oxygenTanksLevel = 0.0F; 
   ListFiller();
   
   //Check oxygen tanks level
   for (int i=0; i < gridOxygenTanks.Count; i++){
      oxygenTanksLevel += gridOxygenTanks[i].GetOxygenLevel();
   }
   oxygenTanksLevel = (oxygenTanksLevel / gridOxygenTanks.Count) * 100; //Is now in percent
   
   //If oxygen level is beneath 30% turn on all oxygen generators
   if (oxygenTanksLevel <= 30){
      for (int i=0; i< gridOxygenGenerators.Count; i++){
         var act = gridOxygenGenerators[i].GetActionWithName("OnOff_On");
         act.Apply(gridOxygenGenerators[i]);
      }
   } //Else if oxygen level is above 80% turn off all generators
   else if (oxygenTanksLevel >= 80){
      for (int i=0; i< gridOxygenGenerators.Count; i++){
         var act = gridOxygenGenerators[i].GetActionWithName("OnOff_Off");
         act.Apply(gridOxygenGenerators[i]);
      }
   }
}

/** Method for setting up all lists contained **/
void ListFiller(){
   List<IMyTerminalBlock> shortList = new List<IMyTerminalBlock>();
   GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(shortList);
   //as long as we have items in our grid...
   for (int i=0; i<shortList.Count; ++i){
      //if an item in our list is an oxygen tank
      if (shortList[i] is IMyOxygenTank){
         gridOxygenTanks.Add(shortList[i] as IMyOxygenTank); 
      }
      //else if an item in our list is an oxygen generator
      else if(shortList[i] is IMyOxygenGenerator){
         gridOxygenGenerators.Add(shortList[i] as IMyOxygenGenerator);
      }
     //now we have lists of oxygen tanks and generators
   }
}
//----------End of Code--------------- 
/*
* CHANGELOG / Developer's roadmap (# marks the actual version)
* # v0.1: Initial code base;
*
* v1.0: Steam-Workshop release;
*/