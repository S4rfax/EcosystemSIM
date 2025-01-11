ECOSYSTEM Parameteränderungen: 

Simulation geändert
1. Rewardsystem angepasst für Carnivore: Reward erhöht (20 -> 50)
2. Speed erhöht für Carnivore: 50% schneller als Herbivore (4->6)
   
   results in Multitraining 2.15

Training geändert:
yaml File - Änderunfen
1. Entropy verdoppelt für Carnivore: beta 0.005 -> 0.01
2. Learning rate erhöht: 0.003->0.005
3. NN erhöht: 50% mehr Neuronen, mehr Layer (256 -> 512; 1 -> 2)

  results in Multitraining 2.17
