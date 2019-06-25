War FX, version 1.7
2014/05/21
© 2013,2014 - Jean Moreno
=========================

PREFABS
-------
Effects are located in the "_Effects" folders.
Mobile differences:
* Regular Mesh: Effects use regular quad meshes billboarded, causing more overdraw but with less triangles drawn
* Reduced Overdraw: Effects use customized meshes to reduce overdraw, but with more triangles and with a script simulating the billboard behaviour


CARTOON FX EASY EDITOR
----------------------
Open the editor in the menu:
GameObject -> CartoonFX Easy Editor

Change the options of the editor, select the GameObject(s) you want to change, and press the corresponding buttons in the editor to apply the changes.


CARTOON FX SPAWN SYSTEM
-----------------------
CFX_SpawnSystem allows you to easily preload your effects at the beginning of a Scene and get them later, avoiding the need to call Instantiate. It is highly recommended for mobile platforms!
Create an empty GameObject and drag the script on it. You can then add GameObjects to it with its custom interface.
To get an object in your code, use CFX_SpawnSystem.GetNextObject(object), where 'object' is the original reference to the GameObject (same as if you used Instantiate).
Use the CFX_SpawnSystem.AllObjectsLoaded boolean value to check when objects have finished loading.


TROUBLESHOOTING
---------------
* Almost all prefabs have auto-destruction scripts for the Demo scene; remove them if you do not want your particle system to destroy itself upon completion.
* Mobile effects with reduced overdraw won't look right in the Editor (only in Runtime mode) because of the script simulating the billboard behaviour. Please look at the desktop versions to have an idea of how it will look in the editor.
* Some effects might lack luminosity in Linear Color Space (Unity Pro only); you can correct this issue on a case by case basis by increasing either the Tint Color of each Material, or the Start Color on the effect's Particle System Inspector.


PLEASE LEAVE A REVIEW OR RATE THE PACKAGE IF YOU FIND IT USEFUL!
Enjoy! :)


CONTACT
-------
Questions, suggestions, help needed?
Contact me at:
jean.moreno.public+unity@gmail.com

I'd be happy to see any effects used in your project, so feel free to drop me a line about that! :)


UPDATE NOTES
------------
v1.7
- updated CFX Editor
- updated max particle count for each prefab to lower memory usage
- removed all Lights for Mobile prefabs

v1.61
- updated CFX Editor

v1.6
- updated CFX Editor
 (now in Window > CartoonFX Easy Editor, and more options)
- added JMO Assets menu (Window -> JMO Assets), to check for updates or get support

v1.52
- fixed other Unity 4.1 incompatibilities

v1.51
- fixed fog for a shader

v1.5
- fixed Compilation error for CFX_SpawnSystem in Unity 4.1
- fixed Cartoon FX editor scaling, now supports "Size by Speed"

v1.4
- inclusion of Cartoon FX Spawn System

v1.3
- fix fog colors for some shaders
- better colors in linear color space

v1.2
- fix compilation error with DirectX 11

v1.1
- added a more realistic Flame Thrower (WFX_FlameThrower Big Alt)
- added Asset Labels to the prefabs


NOTES
-----
M4 Weapon model from Unity 3.5 Bootcamp Demo
Bullet Holes textures edited from Unity 3.5 Bootcamp Demo