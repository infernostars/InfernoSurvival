# Not Awesome Survival
An MCGalaxy plugin I coded to see how far I could get with my own take on survival. I no longer actively develop it.

## Download

||
|--|
[:arrow_down: Not Awesome Survival plugin](/assets/assets.zip?raw=true)|ag
## Installation
In order to run a server with NAS, it is **heavily recommended** that you start with a completely fresh [MCGalaxy download.](https://123dmwm.com/MCGalaxy/)
This is because NAS has a bunch of custom blocks and configuration that **WILL OVERWRITE** what you had before.

~~You will also need to use the "infid" version of MCGalaxy_.dll which allows for 767 blocks.~~ The download above already includes support for 767 blocks.

To install:
1. Run your server once to generate the folders, if you haven't already.
2. Shut down your server.
3. Put every file from *assets.zip* into the *plugins* folder. Just the files inside! You should not end up with an "assets" folder inside *plugins*.
4. Run your server again. NAS will generate a starting world, then do one final restart automatically.

If this works without any errors, you're done!

## Tips
Press **Q** and **E** to move the selection on your toolbar. Press **R** to toggle toolbar opened. Press **M** to move tools in the toolbar. Press **X** to delete tools.

It is very difficult to properly undo theft and grief in NAS. Therefore, the server is automatically set to whitelist-only by default.
To allow you and your friends to play, add players to the whitelist with **/whitelist add [playername]+**

It might be handy to change the spawnpoint of the main world, as it usually starts very high in the sky. You can do so with **/setspawn**

If you have logs, you can place drawers to craft with. Logs and drawers are a shared resource. Use **/faq** to see the crafting guide image.

**What has been implemented:**
* Gathering limited blocks to build and craft with
* Crafting
* Tools
* Containers to store blocks and tools
* Custom gen and infinite maps (every world is a large "chunk")
* Custom block physics
* Health, fall damage, suffocation, death, and food.

**What has -not- been implemented, and probably never will be unless someone else steps in.**
* Actual pvp
* Mobs
* A bunch of other stuff probably. Please don't bug me for new features.

**Known issues (with unknown causes):**
* Rarely, storage containers will get erased.
* Rarely, the map data will get erased (no more cave fog...)

**Unknown issues:**
* Probably a lot of things.
