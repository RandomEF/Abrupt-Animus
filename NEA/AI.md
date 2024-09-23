Enemy AI can be split into 3 parts:
- An overlord script which maintains the enemy state and the switching between the subsidiary AIs
- A roaming AI which allows the enemy to move around normally while also gathering data
	Could have booleans which dictate whether the enemy has been seen and is alive, in which case swaps the searching pattern
- A combat AI which allows the enemy to attack its target and evade it