#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory.States
{
    public class BoxColliderComponentState : RFComponentState
    {
        private bool Pressed(BoxCollider col)
        {
            var size = Mathf.Max(col.size.x, col.size.y) * 0.7f;
            return Handles.Button(
                col.transform.position, 
                col.transform.rotation, 
                size, 
                size, 
                Handles.RectangleHandleCap
            );
        }
        public BoxColliderComponentState(RagdollFactory context) : base(context)
        {
            ComponentList = new List<Component>();
            ComponentList.AddRange(Context.GetComponentsInChildren<BoxCollider>());
        }

        public override void Create()
        {
            Transform objA = Context.selectedBoneA, objB = Context.selectedBoneB;
            
            float distance = Vector3.Distance(objA.position, objB.position);
            float height = distance / 2;
            GameObject collisionObject = new GameObject(objA.name + " - " + objB.name);
            collisionObject.layer = Context.gameObject.layer;
            collisionObject.transform.SetParent(objA);
            collisionObject.transform.localPosition = Vector3.zero;
            collisionObject.transform.forward = objB.position - objA.position;
            collisionObject.transform.localScale = Vector3.one;
            BoxCollider _selectedBoxCollider = collisionObject.AddComponent<BoxCollider>();
            _selectedBoxCollider.center = Vector3.forward * height;
            _selectedBoxCollider.size = new Vector3(Context.boxColliderWidth, Context.boxColliderDepth, distance);
            
            Context.boxColliderLength = height;
            
            Undo.RegisterCreatedObjectUndo(collisionObject, "Created Box Collider Object");
            Select(_selectedBoxCollider);
            Undo.RegisterCompleteObjectUndo(Context, "Created Box Collider Object");
        }

        public override void ConvertTo(Component component)
        {
            if (component is CapsuleCollider)
            {
                BoxCollider col = SelectedComponent as BoxCollider;
                GameObject collisionObject = col.gameObject;
            
                ComponentList.Remove(col);
                
                float distance = col.size.z;
                float radius = Mathf.Max(col.size.x, col.size.y) / 2;
                float height = col.size.z / 2;

                Context.capsuleColliderLength = distance;
                Context.capsuleColliderRadius = radius;
            
                Undo.DestroyObjectImmediate(col);

                CapsuleCollider capsuleCollider = Undo.AddComponent<CapsuleCollider>(collisionObject);
                capsuleCollider.direction = 2;
                capsuleCollider.radius = radius;
                capsuleCollider.center = Vector3.forward * height;
                capsuleCollider.height = distance + capsuleCollider.radius;
            
                Undo.RegisterCompleteObjectUndo(collisionObject, "Converted Box Collider to Capsule Collider");
                
                Context.SetState(Context.CapsuleColliderComponentState);
                Context.CapsuleColliderComponentState.Select(capsuleCollider);
            }
        }

        public override void DrawSceneGUI()
        {
            foreach(BoxCollider collider in ComponentList.ToArray())
            {
                if (!collider) continue;
                Handles.color = IsSelected(collider) ? Context.selectedColor : Context.normalColor;

                if (IsSelected(collider))
                {
                    Undo.RecordObject(collider.transform, "Moved Box Collider");
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
            
            BoxCollider _selectedBoxCollider = SelectedComponent as BoxCollider;
            
            // Limit values to 0 or higher
            if(Context.boxColliderDepth < 0) Context.boxColliderDepth = 0;
            if(Context.boxColliderWidth < 0) Context.boxColliderWidth = 0;
            
            // Adjust box size and position to fit bones
            _selectedBoxCollider.center = Vector3.forward * Context.boxColliderLength / 2;
            _selectedBoxCollider.size = new Vector3(Context.boxColliderWidth, Context.boxColliderDepth, Context.boxColliderLength);
        }

        public override void Select(Component component)
        {
            base.Select(component);
            Context.boxColliderDepth = (SelectedComponent as BoxCollider).size.y;
            Context.boxColliderWidth = (SelectedComponent as BoxCollider).size.x;
            Context.boxColliderLength = (SelectedComponent as BoxCollider).size.z;
        }

        public override void Delete() => DeleteSelectedGameObject();
    }
}
#endif