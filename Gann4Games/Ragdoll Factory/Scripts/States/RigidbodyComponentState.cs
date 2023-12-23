#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory.States
{
    
    public class RigidbodyComponentState : RFComponentState
    {
        private bool Pressed(Rigidbody rb)
        {
            // return Handles.Button(
            //     rb.transform.position, 
            //     SceneView.currentDrawingSceneView.camera.transform.rotation,
            //     rb.mass * Context.discRadius, 
            //     rb.mass * Context.discRadius, 
            //     Handles.DotHandleCap
            // );
            
            Handles.DrawSolidDisc(
                rb.transform.position,
                SceneView.currentDrawingSceneView.camera.transform.forward,
                rb.mass * Context.discRadius);
            
            return Handles.Button(
                rb.transform.position,
                SceneView.currentDrawingSceneView.camera.transform.rotation,
                rb.mass * Context.discRadius,
                rb.mass * Context.discRadius,
                Handles.CircleHandleCap
                );
            
        }

        public RigidbodyComponentState(RagdollFactory components) : base(components)
        {
            ComponentList = new List<Component>();
            ComponentList.AddRange(Context.GetComponentsInChildren<Rigidbody>());
        }

        public override void Create()
        {
            Rigidbody rb = GetOrAddComponent<Rigidbody>(Context.selectedBoneB.gameObject);
            Select(rb);
            Update();
        }

        public override void ConvertTo(Component component)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawSceneGUI()
        {
            Handles.DrawWireDisc(CenterOfMass(), SceneView.currentDrawingSceneView.camera.transform.forward, Context.discRadius * 2);

            foreach(Rigidbody rb in ComponentList.ToArray())
            {
                if(!rb) continue;

                
                Handles.color = Context.normalColor * (rb.isKinematic ? Color.red : Color.green);
                Handles.color = IsSelected(rb) ? Context.selectedColor : Handles.color;

                switch (Context.actionTypeOnClick)
                {
                    case RagdollFactory.ActionTypeOnClick.Create:
                        Handles.DrawSolidDisc(
                            rb.transform.position,
                            SceneView.currentDrawingSceneView.camera.transform.forward,
                            rb.mass * Context.discRadius);
                        break;
                    case RagdollFactory.ActionTypeOnClick.Select:
                        if (Pressed(rb))
                            Select(rb);
                        break;
                    case RagdollFactory.ActionTypeOnClick.Delete:
                        if (Pressed(rb))
                            Delete(rb);
                        break;
                }
            }
        }

        public override void Update()
        {
            if (!SelectedComponent) return;

            Rigidbody rb = (Rigidbody) SelectedComponent;
            rb.mass = Context.rigidbodyMass;
            rb.drag = Context.rigidbodyDrag;
            rb.angularDrag = Context.rigidbodyAngularDrag;
            rb.useGravity = Context.rigidbodyUseGravity;
            rb.isKinematic = Context.rigidbodyIsKinematic;
        }

        public override void Select(Component component)
        {
            base.Select(component);
            Rigidbody rb = SelectedComponent as Rigidbody;
            Context.rigidbodyMass = rb.mass;
            Context.rigidbodyDrag = rb.drag;
            Context.rigidbodyAngularDrag = rb.angularDrag;
            Context.rigidbodyUseGravity = rb.useGravity;
            Context.rigidbodyIsKinematic = rb.isKinematic;
        }

        /// <summary>
        /// Returns the center of mass based on all current rigidbodies.
        /// </summary>
        /// <returns></returns>
        private Vector3 CenterOfMass()
        {
            Vector3 CoM = Vector3.zero;
            float mass = 0f;
 
            foreach (Rigidbody part in ComponentList)
            {
                CoM += part.worldCenterOfMass * part.mass;
                mass += part.mass;
            }
 
            CoM /= mass;
            return CoM;
        }
    }
}
#endif