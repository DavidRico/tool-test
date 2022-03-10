# Character creation tool
This tool automates the creation of character prefabs, materials and configuration to add new characters to an in-game store.

The tool comprises three windows: a configuration window, a material creator window, and a character creation window.

## Character creation window
The character creation window allows a user to create a prefab for a character and add its info to the in-game store.
The process is straightforward: the user just needs to set the fields as the window shows them, and once everything is set, press the button to create the prefab and add it to the store.

![Character creation window](img/charactera.png?raw=true "Character creation window, partially set")

For example, once the user has selected the model for the character, the window shows fields for all the material slots in the model. 
As long as at least one of the slots is empty, the user cannot move forward, and quick access to the material creation window is granted with a button.

![Character creation window](img/character.png?raw=true "Character creation window")

For the final steps, the window reads the character information from a csv file provided in the configuration window. 
This is a spreadsheet provided by the design team, with the info on characters names and prices.

A toggle allows the user to show all the characters in the spreadsheet, or hide the ones that are already created in the store.
This will be especially useful if the store grows too big. 

## Material creation window
![Material creation window](img/material.png?raw=true "Material creation window")

A simple window that allows easy creation of materials. 
Here the user can set the name, color, and shader for the new material, and the window shows the texture properties in the shader, so the user can set those textures as well.

## Configuration window
![Configuration window](img/config2.png?raw=true "Configuration window")

The configuration window holds data that can be stored between sessions. 
It holds a reference to a csv file that should contain the names and prices of the characters, provided by the design team.
It also holds a reference to a scriptable object containing the store items 
(the store used to be in a singleton in the scene, but moving it into a scriptable object means the user doesn't have to save the scene every time a new character is added or edited).

Next, there are some fields to edit the default values that the character creation window will show for character collider height and radius,
and the folders where the materials and prefabs will be saved.
 
