# Backstreets

![Logo](https://github.com/CyTheRabbit/com.cy.backstreets/raw/main/Documentation%7E/Art/logo%402.png)

This module is a part of backstreets project – game set in non-euclidean two-dimensional space.
It contains integration with unity runtime and unity editor, as well as assemblies that internally use unity libraries.

## Important types

- [FieldOfView](Runtime/FOV/FieldOfView.cs)
- [FieldOfViewBuilder](Runtime/FOV/Builder/FieldOfViewBuilder.cs)
- [FOVMeshBuilder](Runtime/FOV/MeshBuilder/FOVMeshBuilder.cs)

## Glossary

- *Pocket* – pocket dimension, a part of non-euclidean space that has euclidean properties. Basically subscenes that share
  their coordinate space.
- *Portal* – line that connect two pockets together.
- *FOV* – field of view, area that is directly visible from the origin in 2D space. FOV can cover area of multiple
  pockets.

## To do

- Add sample scenes
- Implement runtime rendering with URP
- Pocket management based on visibility/proximity

## Samples

TBD