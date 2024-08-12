While it should not be used extensively as it may break the flow of the game, this system should be complex enough to handle any and all dialogue requirements it needs.

It must:

- Allow for one to easily add multiple languages
- Allow for timing cues
- Allow for branching options
- Allow for quest completions
- Allow for different dialogue based on external events
- Allowed to change variables
-  Prompting characters to face different characters
- Allow for changing text colour (this should allow for accessibility changes)

This can be done by having a database store all the lines of dialogue, with all the colour and timing cues, and an external file storing the flow and event handling in a custom language which will need to be parsed.
An interaction script stores a reference to the dialogue sequence file.
When you interact, the appropriate lines are parsed and then sent to the dialogue manager which renders the conversation in the box.

Have the .dlg file store line IDs and actor IDs, then have the events.
Interaction script will scan through and assign the actor IDs based of a database

## Syntax

\\ will be the escape character
To denote line IDs and Actor IDs, the parser will search for \#Lx and \#Ax where x is the record ID for the line or actor.
** will denote bold
__ will denote italics
Colour information will be marked with "<>"
Dialogue sections will be marked in \[]
	These will allow for multiple interacts
	A number at the end of the dialog section will mark the number interact it is
	The final one will repeat
Choices are marked with |
Variables which can point to external events will be marked with @ and its value will be checked with =
	Using a -> the 'if' statement can be redirected to a dialogue section
	Selected choices will set a choice variable that the dialogue will check to warp to
