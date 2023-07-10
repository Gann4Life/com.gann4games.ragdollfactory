# Ragdoll Factory
A tool to create ragdolls easily in Unity by clicking on the bones you want to have rigidbodies, joints and collisions.
You can build a complete ragdoll in under five minutes with this, useful if you use a lot of ragdolls and they also vary in shapes or anatomy.

### Made in Unity 2021.3.20f1 - Can't guarantee it works on older versions.
This version also seems to have a bug with the gizmos for colliders, so they might not appear in the scene view. They still work, though.

## Installation
* Make sure you already have the unity editor open.
* Download and **open** the .unitypackage file from the [releases page](https://github.com/Gann4Life/RagdollFactory/releases).
* A window will appear asking you to import the package, click on `Import`.

## Usage
1. Once imported, right click on your model.
2. Look for `MrGann` tab and then click on `RagdollFactory`
3. Begin creating with your preferred workflow.

All tabs have three modes: `Create`, `Edit` and `Delete`. You can change between them with the buttons found in the inspector.

### Collision tabs (Capsule & Box)
* `Create` mode: Click on the bone you want to start creating a collider from, then click on the bone you want to end the collider in.
* `Edit` mode: Click on the collider you want to edit, then change the values you need from the inspector window.
* `Delete` mode: Click on the collider you want to delete.
### Configurable Joint tab
* `Create` mode: Click on the bone you want to start creating a joint from, then click on the bone you want to end the joint in. (The joint will be created on the second selected bone)
* `Edit` mode: Click on the joint you want to edit, then change the values you need from the inspector window.
* `Delete` mode: Click on the joint you want to delete.
### Rigidbodies tab
* `Create` mode: Click on the bone you want to create a rigidbody on.
* `Edit` mode: Click on the rigidbody you want to edit, then change the values you need from the inspector window.
* `Delete` mode: Click on the rigidbody you want to delete.
