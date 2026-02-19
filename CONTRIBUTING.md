# Introduction
Hi hello! Thanks a lot for even considering contributing to SlopperEngine.
Since SlopperEngine is a personal project and currently not in a publically usable state, I (capsloughe) would prefer making most (if not all) currently missing systems by myself. However, if you notice any bugs, possible optimisations, or code not adhering to the style guide, PRs involving these sorts of problems are **always** welcome.
I'm always available on discord for a chat - you can DM me (`capslo`) at any time, and I'll definitely reply.

# Pull requests
Not many rules to speak of here. I am not experienced in receiving code from others, so I will not be harsh at all, and always open to discussing. That said, a couple simple rules:
- Make sure your PR has a descriptive title and description, and use vaguely professional language (no swears and proper grammar and such). 
- Make sure your code follows the code conventions (to a normal degree. "close enough" is allowed).
- Make sure all public and protected members and classes have documentation! (using `///` type comments)
- Try keeping commit names below 30 characters

# To do
Lots of things need doing in SlopperEngine. Points marked with (WIP) are ones I am currently working on and I would be mildly bothered if anyone else contributed in those areas.
- Mod loading system (WIP)
	- Mods get loaded (WIP)
	- Mods have permissions that they cannot break (WIP)
	- SerializedObject can safely be used by mods (what to do when a mod ships with or writes a `.slsc` file containing forbidden objects?)
- Proper asset loading system (WIP)
	- Assets are loaded relative to mod folder (WIP)
- Sound implementation
	- SceneObjects that play audio
	- Replaceable audio managers
- Rendering reworks
	- Models can be loaded through Assimp (WIP)
	- OpenGL functions can be batched on Scene threads, and then *those* will be executed by the render thread
	- GPUResources need a clear divide between "being created" and "being created on the GPU"
	- Actual shadows and different types of lights
	- Transparency in main renderer
	- Fixed shading language (deceptively high priority)
- Physics implementation
	- Seriously more queries should be built in than just the pose and velocity of rigidbodies. 
		- Like raycasting
		- Collision events
		- Physics material
		- The list goes on. Really everything that Bepu has and SlopperEngine does not.
	- Implementing more shapes from Bepu
	- Triggers
	- Scalable system for collision masks