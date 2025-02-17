# Bootstrap Package

## Description

This package enables you to create an addressable folder containing prefabs that will be automatically spawned into the scene. This allows you to enter Play Mode from any scene and have your game systems spawn seamlessly. Since the bootstrap prefabs are added and edited directly in the project folder, it decentralizes your workflow, making it more flexible and efficient.

## Setup

1. **Create a Bootstrap Folder**  
   - Choose a folder that will be your bootstrap folder and make it addressable.

2. **Set the Folder Address**  
   - The address of the folder should reflect its path inside the `Assets` folder (as it does by default, e.g. `Assets/Bootstrap`).  
   - Set this address as the **Bootstrap Folder Address** in `Project Settings > Bootstrap Settings`.

## Attributes

### Ordering Bootstrap Systems

Use the following attributes to control the spawn and execution order of your systems:  
- `[BootstrapOrder(int order)]` (overrides `BootstrapBefore` or `BootstrapAfter`)
- `[BootstrapBefore(typeof(T))]`  
- `[BootstrapAfter(typeof(T))]`  

The first script on the prefab that contains these attributes will determine its order.
