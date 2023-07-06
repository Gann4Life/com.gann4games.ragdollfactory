# CollisionBuilder
Build unity collisions easily by just clicking on the bones you want collisions in.

<iframe width="560" height="315" src="https://www.youtube.com/embed/5-ruj6EmvuQ" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

### Made in Unity 2021.3.20f1 - Can't guarantee it works on other versions.


## Getting started
* To begin creating collisions, right click on your skeleton's root bone or the master parent and look for the `MrGann > Collision Builder` item in the menu.

* In the inspector you will see a new component. You are currently going to be drawing/creating capsule colliders. You can change it to use boxes or select any existing colliders with the buttons found in the inspector.

* To begin creating collisions, first select one of the bones/transforms with `Left Click` on the scene viewport and *then* select where you want the collider to look at. So in short terms, first select the bone A and then the bone B.

* Notice how the model has circles, those are all the skeleton transforms that can be selected.

* When you hover over one of those bones, the circle will change its size and color, indicating 1. That you are going to select that bone; and 2. How big the capsule collider is going to be (does not apply for box colliders)

* The first click will indicate that you need to select a second bone.

* Once you click on a second bone, the collider will be created between these two transforms, and automatically using the distance between transforms.

* Now that you have your first collider, more options appear in the inspector.
  There you can modify the last created or selected collider, delete it, convert it to a box collider or viceversa, or delete all colliders.
