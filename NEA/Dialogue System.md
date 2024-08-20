While it should not be used extensively as it may break the flow of the game, this system should be complex enough to handle any and all dialogue requirements it needs.

It must:

- Allow for one to easily add multiple languages
- Allow for timing cues
- Allow for branching options
- Allow for quest completions
- Allow for different dialogue based on external events
- Allowed to change variables
- Prompting characters to face different characters
- Allow for changing text colour (this should allow for accessibility changes)

This can be done by having a database store all the lines of dialogue, with all the colour and timing cues, and an external file storing the flow and event handling in a custom language which will need to be parsed.
An interaction script stores a reference to the dialogue sequence file.
When you interact, the appropriate lines are parsed and then sent to the dialogue manager which renders the conversation in the box.

Have the .dlg file store line IDs and actor IDs, then have the events.
Interaction script will scan through and assign the actor IDs based of a database

## Syntax

\\ will be the escape character
To denote line IDs and Actor IDs, the parser will search for \#Lx and \#Ax where x is the record ID for the line or actor.
\* will denote bold
_ will denote italics
\- will denote pauses, with the number of - being the number of 0.1 seconds that will be waited.
Colour information will be marked with "<>"

Dialogue sections will be marked in \[]
	These will allow for multiple interacts
	The final one will repeat
	-> will take the the lines to the next section
Choices are marked with | where the first choice is considered the title
Variables which can point to external events will be marked with @ and its value will be checked with the basic comparison operators
	Using a -> the 'if' statement can be redirected to a dialogue section
	Selected choices will set a choice variable that the dialogue will check to warp to
Each line will have the actorIDs followed by :: and then the line ID
Having actor IDs followed by actor IDs will prompt the first actor to face the second actor