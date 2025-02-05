# Patch Notes

## Version 0.4.3
Changes:
- Began work on wallrunning
- Added builds to commits

## Version 0.4.2
Changes:
- Added a crosshair
- Changed player movement to all work in FixedUpdate instead of Update
- Fixed a bug with the Save System where the correct folders may not be created the first time
- Added empty classes for Blades and Explosives

## Version 0.4.1
Changes:
- Changed enemy movement to use a PID controller instead
- Changed SelectTarget to include a check that the entity can reach the target, does not work on planes since they do not intersect with rays

## Version 0.4.0
Changes:
- Added Soldier
- Added Drone
- Added Tank
- Added Searching to EnemyEntity
- Added Move to EnemyEntity
- Added smooth interpolation to facing angle for enemies

## Version 0.3.6
Changes:
- Added the ability to make a new save
- Added a GUI to use the load, create, and reload the list of save slots
- Added code to modify the lookup when the data is saved

## Version 0.3.5
Changes:
- Added merit, sanity, time on save, total kills to the save data
- Added the ability to list saves
- Added SaveSlot, storing temporary data about the save file to reduce memory usage

## Version 0.3.4
Changes:
- Completed more of the analyzer, missing the variable-based code
- Began Save System
- Added LevelHighscores
- Added LevelBullets
- Added PlayerData
- Added WorldFlags
- Added SerializableStructs for the save system
- Added Save
- Added Load

## Version 0.3.3
Changes:
- Completed large sections of the analyzer, missing only the 'facing' state, and the code for 'choice', 'jump', and 'if'

## Version 0.3.2
Changes:
- Added tokenizer for .dlg
- Added TreeNode for the tokenizer
- Combined Actor and Dialogue database into the same file, different tables

## Version 0.3.1
Changes:
- Created .dlg section preprocessor
- Created speech line parsing from custom syntax to Unity RichText

## Version 0.3.0
Changes:
- Added position and velocity debug HUD element
- Attempted to make a .dlg importer
- Added dialog database connection
- Added different crouch inputs
- Added sliding

## Version 0.2.3
Changes:
- Changed weapon slots to use a List for the rare occurrence of when there are no elements in the list
- Made the gun barrel to point towards the point the player is looking at
- Changed Entity variables to use properties instead
- Changed body to report its detection to the movement script

## Version 0.2.2
Changes:
- Changed weapon slots to use an array instead of different variables for wrapping
- Added raycast for picking up weapons

## Version 0.2.1
Changes:
- Started weapon interaction for the player to pick up weapons

## Version 0.2.0
Changes:
- Added Bullet
- Added Gun
- Added Entity
- Added PlayerEntity
- Added damage handling

## Version 0.1.2
Changes:
- Attempted fix at movement weirdness

## Version 0.1.1
Changes:
- Moved the player down upon crouching to maintain contact on floor
- Completely changed the movement function to mimic Titanfall 2 more
- Removed slope correction for movement
- Began changes to use OnCollisionEnter() instead of Physics.CheckSphere()
- Swapped OnCollisionEnter() to OnCollisionStay() and OnCollisionExit()
- Added FPS viewer
- Added Crouching
- Enabled the player to change dimensions based upon radius and height
- Noted weird movement oddity when moving slowly

## Version 0.1.0
Changes:
- Added looking
- Added PlayerManager to maintain a reference to the same inputs
- Added Sprinting

## Version 0.0.3
Changes:
- Added a physics material to remove the incorrect interaction with the wall
- Added a synopsis to Obsidian

## Version 0.0.2
Changes:
- Found continuous detection (sighh)
- Scrapped Collide and Slide custom movement controller
- Returned to Rigidbody movement controller
- Added Speed Clamp
- Added friction using friction multiplier
- Added drag
- Changed CalculateAccelerationMultiplier to use a switch statement
- Changed acceleration function to no longer use Mathf.Pow()
- Added Obsidian planning files

## Version 0.0.1
Changes:
- Began movement controller for player using Rigidbody
- Scrapped and changed to attempt using Collide and Slide algorithm
- Used equation to calculate acceleration based upon current speed
- Added Gravity
- Added Jump
- Added Boost
- Using prototype materials
- Added CASCapsuleVisualiser sanity test
- Added IsGrounded sanity test