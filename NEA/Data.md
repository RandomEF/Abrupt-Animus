The data system will be split into 2 parts: the save/load system, and a manager object that contains said data.
The reason for doing so will be so that reading variable values (especially used in the dialogue system) does not require frequent access to the file system, and serious lag from doing so.

## Save Data

### Player

Health
Position
Velocity
Body Rotation
Look Rotation
Weapon Slots
Modifier Cells layout
Merit
Sanity
Time played
Total kills

### Per Level Highscores

Time
Enemies killed
Secrets
Times died
Weapons used

### Individual Level Bullet Positions

50 per level
Time when placed in playthrough

### World Flags

Explains itself