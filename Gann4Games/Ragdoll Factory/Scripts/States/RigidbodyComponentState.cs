using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory.States
{
    public class RigidbodyComponentState : RFComponentState
    {
        private bool Pressed(Rigidbody rb)
        {
            return Handles.Button(
                rb.transform.position, 
                SceneView.currentDrawingSceneView.camera.transform.rotation,
                rb.mass * Context.discRadius, 
                rb.mass * Context.discRadius, 
                Handles.DotHandleCap
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

        public override void DrawGUI()
        {
            foreach(Rigidbody rb in ComponentList.ToArray())
            {
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
                        {
                            Select(rb);
                            Delete();
                        }
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
    }
}