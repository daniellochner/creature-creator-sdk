![Creature Creator Logo](./Logo.png)

Here is a guide on how to create mods for Creature Creator!

# Installation

First, download [Unity 6000.1.12](https://unity.com/releases/editor/whats-new/6000.1.12) and the [latest version of the SDK](https://github.com/daniellochner/creature-creator-sdk/releases)!

Open Unity Hub, and create a new project by selecting **Unity 6000.1.12** and then scrolling down to the **"3D (Built-In Render Pipeline)"** template. Enter a name and then click the button to create your project.

Drag and drop the SDK Unity package into your project files, and then click the "Import" button to add it to your project. If you ever need to update the SDK. you can follow the same process.

Great! You are now all setup and ready to get started!


# Getting Started

Before we begin creating our own mod, let’s test the examples to see if everything is working as expected!

Go to your project window and double click on the "Example" scene file in `Assets/Items/Maps/Example` to load the example map.

Next, select the config file and navigate to the top menu bar again and click on "Creature Creator > Build and Test". This will start the build process for the map, as well as the unlockable part and pattern, which are referenced as dependencies in the config file!

Once completed, the executable will be launched with command line arguments pointing to your mod! Note that if the executable cannot be found, click "Creature Creator > Locate Creature Creator.exe". The application should be installed in the "~/Steam/steamapps/common/" directory

The game should open and load into the example scene with adventure mode!


# Creating Mods

To create a new mod, go to the top menu bar and select “Creature Creator > New”, and then choose a name! This will create a new folder for your mod and select the config file.

## Pattern
The pattern is the most simple. Just open up the image file and edit it! You can copy and paste from an existing pattern if you’d like.

## Body Part:
You can edit the stats in the config file.

Drag your model into the folder. In the import settings, enable “Read/Write mode”!
Then double click on the prefab and drag and drop your model into it. Please remember to remove the previous “Model” game object.

Transformations can be added by creating blend shapes/shape keys in your 3D modelling software. For 

You can make your parts colourable by creating materials for your parts named “Body_Primary” or “Body_Secondary”. These will match your creature’s body by default and can be overridden in the paint menu.

Limbs are another special case and need the correct structure. There should be an empty parent with two children, namely the “Model” (with the skinned mesh renderer) and “Root” (with all the bones as children).

The thumbnail is set automatically, but you can include your own render by creating one in “/Exclude/thumb.png”.

## Maps
The easiest way to get started with maps is by looking at the example and seeing how it is done.

## Thumbnail
Position the thumbnail camera in-scene. The thumbnail will be generated at build time.

## Minimap
Select the minimap info prefab in-scene and then view from the top. Assign your render of the minimap in the component. You can then scale the root game object to ensure it fits to your map.

## Proxies
Proxies are prefabs that you can drag and drop into the scene to enable more complicated behaviours!

### Platforms
These are the editing platforms around the world that players will be able to teleport to and edit their creature at.

### Unlockable Parts and Patterns
You will need to set the IDs of each of the parts and patterns in the steam workshop.

### Water
You can use your own water in the scene. Then drag and drop the water prefab and position it to be over your water.

### Dark Area
Specifying a dark area will ensure creatures’ use bioluminescent parts in these areas.



## Uploading to the Workshop

Fantastic! You should now have a custom body part, pattern or map that has been tested, and is ready to uplod to the Steam workshop!

Before you can upload, you'll need to make sure that all the required modules are installed via Unity Hub for the different platforms. This includes "Windows Build Support (Mono)", "Mac Build Support (IL2CPP)", "Linux Build Support (Mono)", "iOS Build Support" and "Android Build Support".

Now you can navigate to the top menu bar again with your mod's config selected and click on "Creature Creator > Upload to Workshop".

This will launch the game and begin the upload process, and once complete, open the Steam workshop for your mod!


Hope this helps! If you need any help, please feel free to send a message on the community Discord server! I'm looking forward to downloading your mods in-game!

Thanks!
