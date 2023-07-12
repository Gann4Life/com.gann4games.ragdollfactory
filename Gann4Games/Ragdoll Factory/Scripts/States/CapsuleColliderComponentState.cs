using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory.States
{
    public class CapsuleColliderComponentState : RFComponentState
    {
        private bool Pressed(CapsuleCollider col)
        {
            var size = col.radius*2;
            return Handles.Button(
                col.transform.position, 
                col.transform.rotation, 
                size, 
                size, 
                Handles.CircleHandleCap
            );
        }
        
        public CapsuleColliderComponentState(RagdollFactory context) : base(context)
        {
            ComponentList = new List<Component>();
            ComponentList.AddRange(Context.GetComponentsInChildren<CapsuleCollider>());
        }

        public override void Create()
        {
            Transform objA = Context.selectedBoneA, objB = Context.selectedBoneB;
            // if (!objB.IsChildOf(objA))
            //     throw new Exception("The second bone must be child of the first bone!");
            
            float distance = Vector3.Distance(objA.position, objB.position);
            
            GameObject collisionObject = new GameObject(objA.name + " - " + objB.name);
            collisionObject.transform.SetParent(objA);
            collisionObject.transform.localPosition = Vector3.zero;
            collisionObject.transform.forward = objB.position - objA.position;
            collisionObject.transform.localScale = Vector3.one;
            CapsuleCollider selectedCapsuleCollider = Undo.AddComponent<CapsuleCollider>(collisionObject);
            selectedCapsuleCollider.direction = 2;
            selectedCapsuleCollider.radius = Context.capsuleColliderRadius;
            selectedCapsuleCollider.center = Vector3.forward * distance / 2;
            selectedCapsuleCollider.height = distance + selectedCapsuleCollider.radius;
            
            Context.capsuleColliderLength = distance;
            
            Undo.RegisterCompleteObjectUndo(Context, "Created Capsule Collider Object");
            
            Select(selectedCapsuleCollider);
        }

        public override void ConvertTo(Component component)
        {
            if (component is BoxCollider)
            {
                CapsuleCollider capsuleCollider = SelectedComponent as CapsuleCollider;
                GameObject collisionObject = capsuleCollider.gameObject;

                ComponentList.Remove(capsuleCollider);
                
                float radius = capsuleCollider.radius * 2;
                float distance = capsuleCollider.center.z * 2;
                float height = distance / 2;

                Context.boxColliderDepth = radius;
                Context.boxColliderWidth = radius;
                Context.boxColliderLength = distance;
            
                Undo.DestroyObjectImmediate(capsuleCollider);

                BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(collisionObject);
                boxCollider.center = Vector3.forward * height;
                boxCollider.size = new Vector3(radius, radius, distance);
                
                Undo.RegisterCompleteObjectUndo(collisionObject, "Converted Capsule Collider to Box Collider");
                
                Context.SetState(Context.BoxColliderComponentState);
                Context.BoxColliderComponentState.Select(boxCollider);
            }
        }

        public override void DrawSceneGUI()
        {
            foreach(CapsuleCollider collider in ComponentList.ToArray())
            {
                if (!collider) continue;
                Handles.color = Color.white;

                if (IsSelected(collider))
                {
                    Undo.RecordObject(collider.transform, "Moved Capsule Collider");
                    collider.transform.position =
                        Handles.PositionHandle(collider.transform.position, collider.transform.rotation);
                }

                switch (Context.actionTypeOnClick)
                {
                    case RagdollFactory.ActionTypeOnClick.Select:
                        if (Pressed(collider))
                            Select(collider);
                        break;
                    case RagdollFactory.ActionTypeOnClick.Delete:
                        if (Pressed(collider))
                            Delete(collider);
                        break;
                }
            }
        }

        public override void Update()
        {
            if (!SelectedComponent) return;
            
            CapsuleCollider selectedCapsuleCollider = SelectedComponent as CapsuleCollider;
            
            selectedCapsuleCollider.radius = Context.capsuleColliderRadius;
            
            // Adjust capsule size and position to fit bones
            // float distance = Vector3.Distance(selectedCapsuleCollider.transform.position, selectedCapsuleCollider.transform.position + selectedCapsuleCollider.transform.forward * );
            selectedCapsuleCollider.center = Vector3.forward * Context.capsuleColliderLength / 2;
            selectedCapsuleCollider.height = Context.capsuleColliderLength + selectedCapsuleCollider.radius;  
        }

        public override void Select(Component component)
        {
            base.Select(component);
            Context.capsuleColliderRadius = (SelectedComponent as CapsuleCollider).radius;
            Context.capsuleColliderLength = (SelectedComponent as CapsuleCollider).height - Context.capsuleColliderRadius;
        }

        public override void Delete() => DeleteSelectedGameObject();
    }
}