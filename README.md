# Nice Bootstrap Unity Package

## Description

This package allows for easy decentralized bootsrap of your project and avoiding clutter of prefab hierarchies and scenes, minimizing potential merge conflicts when adding and editing features of your game game. It spawns contents of an addressable bootstrap folder into the scene in a controlled order and with hierarchy mirroring the folder hierarchy.

## Setup

- Choose a folder that will be your bootstrap folder and make it addressable.
- The address of the folder should reflect its path inside your project (as it does by default, e.g. `Assets/Bootstrap`).
- Set this address as the **Bootstrap Folder Address** in `Project Settings > Bootstrap Settings`.

## Order Attributes

Use the following attributes to control the spawn and execution order of your systems:
- `[BootstrapOrder(int order)]` (overrides `BootstrapBefore` or `BootstrapAfter`)
- `[BootstrapBefore(typeof(T))]`
- `[BootstrapAfter(typeof(T))]`

The first script on the prefab that contains these attributes will determine its bootstrap order.
