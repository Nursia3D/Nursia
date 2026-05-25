## Overview
While it's possible code all Nursia scenes in C#. The recommended approach is to design it in the Nursia Scene Editor.

## Installation
`dotnet tool install --global nrs-editor`

## Update
`dotnet tool install --global nrs-editor`

## Running
In order to run it, go to the folder that is supposed to contain Nursia scenes and execute `nrs-editor .`

On the first run, it'll create configuration file `project.nursia`

## Navigation
The navigation around the scene is done through [CameraInputController](camera-input-controller.md)

## Basic Usage
Following video demonstrates how scene from [Quick Start Tutorial](../index.md) could be drawn in the Scene Editor:

https://youtu.be/LoegI7KVPMM?si=Jk_13AKRiBLdRFkS

## Scene Format
Scenes at stored in JSON format.

I.e. this is scene created in the above video: https://github.com/Nursia3D/Nursia/blob/main/Samples/Nursia.Samples.Tutorial/Assets/Scenes/main.scene

It could be edited in a text editor. Which sometimes necessary to do things that Nursia Scene Editor can't do. Like changing order of the nodes. If you modified a scene in a text editor and would like to reload it in the Nursia Scene Editor. Then click on the "File/Reload Current Item" menu item.

## Loading Scene

Scenes are loaded through [XNAssets](https://github.com/rds1983/XNAssets)
Sample code:
```c#
  AssetManager assetManager = AssetManager.CreateFileAssetManager("/path/to/assets");
  StoredScene scene = assetManager.LoadStoredScene("Scenes/main.scene");
```

Now it could be rendered using following code:
```c#
  _renderer.Render(scene.Root, scene.Camera);
```

where `_renderer` is ForwardRenderer.





