# SlopperEgine Design Justification

## Introduction
In my time exploring other game engines, i have found several things i enjoyed and enjoyed less about them. 
Every single one has problems - there is no denying that. SlopperEngine will be no different, sadly - but i 
will be trying my best to fix the issues that bother me most about extant game engines. The main design 
will be emulating the classic gameobject with parent/and child layout, but with a higher consistency than 
unity's gameobject/component split, and more freedom than godot's node/script split.
This being said, there are three grips and as such three pillars SlopperEngine will be designed around:
Flexibility. The engine exists to aid the developer, not to limit them.
Responsiveness. The editor, and especially the engine, should never halt.
Simplicity. The engine should be easy to grasp and master, but most importantly to use.

## SlopperEngine will be flexible
SlopperEngine's main goal is flexibility. Often while using a game engine, the engine gets in the users way. 
SlopperEngine's behaviour should be fully configurable - the order of update events, the order of updates 
themselves, the rendering pipeline, and anything else. Ideally these elements should be swappable in runtime - 
the user should not have to restart their game to change a setting. SlopperEngine should be so flexible 
that games can be embedded in other games with no effort, and elements of games should be fully modifiable 
like the engine itself.
This goal will be achieved through, among others, making the entire engine in c#. This allows any developer 
to extend the systems of the engine without having to rely on specific integrations or compiling the engine 
themselves. 
The most radical design decision is to limit the use of c#'s feature of static methods/values. They will only be 
used to interact with OpenGL, with the file system, and caching anything that is not scene specific. No 
exceptions - everything else should and will register at the local scene.
The specific architecture used is more complex and will be elaborated upon in the UML diagram.

## SlopperEngine will be responsive 
Ideally, the engine and games using the engine will start in a matter of seconds - preferably less than one. 
There should never be any delay between the user's input and the engine's response. The editor should be 
fully initialized before the user can make any inputs, and should NEVER freeze. Loading screens should be 
minimized as much as possible. This will be done by minimally unloading assets, only doing so when RAM is 
pressured. Furthermore, filesize will be kept low (as later elaborated upon) which will improve loading 
times by virtue of having less to load.
All this will be done by separating user interaction from the lower parts of the scene tree - the engine hosting 
the program always processes inputs. This is not currently implemented as of December 2024, as with the rest 
of a proper windowing/threading system.

## SlopperEngine will be simple
The engine itself, or at least the initial version, will have a limit of 50 megabytes of size, as i feel 
most game engines have excessive bloat issues. Every feature should be easily accessible, with at most a 
drag-and-drop package, and these will mostly be bare bones as i intend to customize most aspects of the 
engine greatly in my own games. Moreover, use of external libraries should be limited to open source, free
software, and as few as possible. Smaller libraries in filesize are always preferred.

Following these three principles will ensure the long term usability for SlopperEngine for my purposes, for
any sort of interactive program.