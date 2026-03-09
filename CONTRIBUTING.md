# Introduction
Hi hello! Thanks a lot for even considering contributing to SlopperEngine.
Since SlopperEngine is a personal project and currently not in a publically usable state, I (capsloughe) would prefer making most (if not all) currently missing systems by myself. However, if you notice any bugs, possible optimisations, or code not adhering to the style guide, PRs involving these sorts of problems are **always** welcome.
I'm always available on discord for a chat - you can join the server if you combine "https://discord.gg/xsMV" and "zQpQPS"

# Pull requests
Not many rules to speak of here. I am not experienced in receiving code from others, so I will not be harsh at all, and always open to discussing. That said, a couple simple rules:
- Make sure your PR has a descriptive title and description, and use vaguely professional language (no swears and proper grammar and such). 
- Make sure your code follows the code conventions (to a normal degree. "close enough" is allowed).
- Make sure all public and protected members and classes have documentation! (using `///` type comments)
- Try keeping commit names below 50 characters
- Make sure that when you PR, the project actually compiles and runs.

# To do
Lots of things need doing in SlopperEngine. Points marked with (WIP) are ones I am currently working on and I would be mildly bothered if anyone else contributed in those areas.
- Mod safety
	- SerializedObject can safely be used by mods (what to do when a mod ships with or writes a `.slsc` file containing forbidden objects?) (high priority, but I don't feel like doing this rn).
	- Unloading mods (required for feature complete editor)
- Sound implementation
	- SceneObjects that play audio
	- Replaceable audio managers
- Rendering reworks
	- Models can be loaded through Assimp (WIP)
	- OpenGL functions can be batched on Scene threads, and then *those* will be executed by the render thread
	- GPUResources need a clear divide between "being created" and "being created on the GPU"
	- Actual shadows and different types of lights
	- Normal map implementation
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
- Windowing rework
	- SlopperEngine.Windowing.Window *contains* a NativeWindow instead of inheriting from it
	- MainContext can be used as a window (for significant performance improvements)
	- MainContext has loading screen and start splash
- UI improvements
	- Actual font loading
	- Fallback font does not need to load assets to work (and is hardcoded in the engine)
- Default assets are procedural instead of loaded from EngineAssets
- Logger class
	- The logger has generic log functions so logs can be sorted by type
	- The logger can collapse similar logs into a single item
	- The logger can output into files