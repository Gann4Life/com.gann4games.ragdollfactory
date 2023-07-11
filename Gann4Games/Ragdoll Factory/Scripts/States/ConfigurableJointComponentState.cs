using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Gann4Games.RagdollFactory.States
{
    public class ConfigurableJointComponentState : RFComponentState
    {
        private bool Pressed(ConfigurableJoint col)
        {
            Handles.color = Context.selectedColor;
            return Handles.Button(
                col.transform.position, 
                col.transform.rotation, 
                0.05f, 
                0.05f, 
                Handles.CylinderHandleCap
            );
        }
        private GUIStyle GUI_ALERT_STYLE = new GUIStyle();
        private ArcHandle arcHandleXLow = new ArcHandle();
        private ArcHandle arcHandleXHigh = new ArcHandle();
        private ArcHandle arcHandleY = new ArcHandle();
        private ArcHandle arcHandleZ = new ArcHandle();
        public ConfigurableJointComponentState(RagdollFactory context) : base(context)
        {
            ComponentList = new List<Component>();
            ComponentList.AddRange(Context.GetComponentsInChildren<ConfigurableJoint>());
            
            GUI_ALERT_STYLE.fontStyle = FontStyle.Bold;
            GUI_ALERT_STYLE.normal.textColor = Color.red;

            arcHandleXLow.SetColorWithRadiusHandle(Color.red, 0.2f);
            arcHandleXHigh.SetColorWithRadiusHandle(Color.red, 0.2f);
            arcHandleY.SetColorWithRadiusHandle(Color.green, 0.2f);
            arcHandleZ.SetColorWithRadiusHandle(Color.blue, 0.2f);
        }

        public override void Create()
        {
            Transform objA = Context.selectedBoneA, objB = Context.selectedBoneB;

            Rigidbody rigidbodyA = GetOrAddComponent<Rigidbody>(objA.gameObject);
            Rigidbody rigidbodyB = GetOrAddComponent<Rigidbody>(objB.gameObject);
            ConfigurableJoint joint = GetOrAddComponent<ConfigurableJoint>(objB.gameObject);

            
            ConfigurableJoint lastSelectedJoint = joint;
            Rigidbody lastSelectedRigidbody = rigidbodyB;
            
            lastSelectedJoint.connectedBody = rigidbodyA;
            lastSelectedJoint.xMotion = ConfigurableJointMotion.Locked;
            lastSelectedJoint.yMotion = ConfigurableJointMotion.Locked;
            lastSelectedJoint.zMotion = ConfigurableJointMotion.Locked;
            lastSelectedJoint.angularXMotion = ConfigurableJointMotion.Limited;
            lastSelectedJoint.angularYMotion = ConfigurableJointMotion.Limited;
            lastSelectedJoint.angularZMotion = ConfigurableJointMotion.Limited;
            
            lastSelectedJoint.axis = Context.jointAxis;
            lastSelectedJoint.lowAngularXLimit = new SoftJointLimit() {limit = -Context.jointLowXLimit};
            lastSelectedJoint.highAngularXLimit = new SoftJointLimit() {limit = Context.jointHighXLimit};
            lastSelectedJoint.angularYLimit = new SoftJointLimit() {limit = Context.jointYLimit};
            lastSelectedJoint.angularZLimit = new SoftJointLimit() {limit = Context.jointZLimit};
        
            Context.RigidbodyComponentState.Select(rigidbodyA);
            Context.RigidbodyComponentState.Select(rigidbodyB);
            Select(joint);
        }

        public override void ConvertTo(Component component)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawGUI()
        {
            foreach (ConfigurableJoint joint in ComponentList.ToArray())
            {
                if(IsSelected(joint)) DisplayJointHandles(joint);
                else VisualizeJointLimits(joint);
                
                switch (Context.actionTypeOnClick)
                {
                    case RagdollFactory.ActionTypeOnClick.Create:
                        DisplayJointStatus(joint);
                        break;
                    case RagdollFactory.ActionTypeOnClick.Select:
                        if (Pressed(joint))
                            Select(joint);
                        break;
                    case RagdollFactory.ActionTypeOnClick.Delete:
                        DisplayJointStatus(joint);
                        if (Pressed(joint))
                        {
                            Select(joint);
                            Delete();
                        }
                        break;
                }
            }
        }

        private void DisplayJointStatus(ConfigurableJoint joint)
        {
            bool hasConnectedBody = joint.connectedBody;
            Transform jointTransform = joint.transform;
                
            Handles.color = hasConnectedBody ? Color.yellow : Color.red;
            Handles.DrawWireDisc(jointTransform.position,
                SceneView.currentDrawingSceneView.camera.transform.forward, Context.discRadius);
            if (hasConnectedBody) Handles.DrawDottedLine(jointTransform.position, joint.connectedBody.position, 5f);
            else Handles.Label(jointTransform.position, "MISSING CONNECTION", GUI_ALERT_STYLE);
        }
        
        private void DisplayJointHandles(ConfigurableJoint joint)
        {
            if (!joint) return;
            
            Vector3 normalXDirection = joint.transform.TransformDirection(joint.axis);
            Vector3 fromXDirection = Vector3.Cross(joint.transform.TransformDirection(joint.axis), joint.transform.up);

            Vector3 normalYDirection = joint.transform.TransformDirection(Vector3.Cross(joint.axis, -Vector3.forward));
            Vector3 fromYDirection = Vector3.Cross(joint.transform.TransformDirection(joint.axis), normalYDirection);

            Vector3 normalZDirection = joint.transform.TransformDirection(Vector3.Cross(joint.axis, -Vector3.up));
            Vector3 fromZDirection = Vector3.Cross(joint.transform.TransformDirection(joint.axis), normalZDirection);
            
            // Load arc values
            arcHandleXLow.angle = Context.jointLowXLimit;
            arcHandleXHigh.angle = -Context.jointHighXLimit;
            arcHandleY.angle = Context.jointYLimit;
            arcHandleZ.angle = Context.jointZLimit;

            arcHandleXLow.radius = 0.1f;
            arcHandleXHigh.radius = 0.1f;
            arcHandleY.radius = 0.1f;
            arcHandleZ.radius = 0.1f;
            
            // Draw opposite handles for preview
            Handles.color = Color.green * new Color(1, 1, 1, 0.2f);
            Handles.DrawSolidArc(joint.transform.position, normalYDirection, fromYDirection, -joint.angularYLimit.limit,
                0.1f);
            Handles.color = Color.blue * new Color(1, 1, 1, 0.2f);
            Handles.DrawSolidArc(joint.transform.position, normalZDirection, fromZDirection, -joint.angularZLimit.limit,
                0.1f);
            
            // Draw handles taking into account joint axis
            Matrix4x4 handleXMatrix = Matrix4x4.TRS(
                joint.transform.position, 
                Quaternion.LookRotation(fromXDirection, normalXDirection), 
                Vector3.one);
            
            Matrix4x4 handleYMatrix = Matrix4x4.TRS(
                joint.transform.position, 
                Quaternion.LookRotation(fromYDirection, normalYDirection), 
                Vector3.one);
            
            Matrix4x4 handleZMatrix = Matrix4x4.TRS(
                joint.transform.position, 
                Quaternion.LookRotation(fromZDirection, normalZDirection), 
                Vector3.one);

            
            Handles.color = Color.white;
            
            using (new Handles.DrawingScope(handleXMatrix))
            {
                arcHandleXLow.DrawHandle();
                arcHandleXHigh.DrawHandle();
            }

            using (new Handles.DrawingScope(handleYMatrix))
                arcHandleY.DrawHandle();

            using (new Handles.DrawingScope(handleZMatrix))
                arcHandleZ.DrawHandle();

            // Dispose values to arc handles with clamped values
            Context.jointLowXLimit = Mathf.Clamp(arcHandleXLow.angle, 0, 180);
            Context.jointHighXLimit = Mathf.Clamp(-arcHandleXHigh.angle, 0, 180);
            Context.jointYLimit = Mathf.Clamp(arcHandleY.angle, 0, 180);
            Context.jointZLimit = Mathf.Clamp(arcHandleZ.angle, 0, 180);

            Update();
        }
        private void VisualizeJointLimits(ConfigurableJoint joint, float radius = 0.1f, float opacity = 0.25f)
        {
            if (!joint) return;

            Vector3 normalXDirection = joint.transform.TransformDirection(joint.axis);
            Vector3 fromXDirection = Vector3.Cross(joint.transform.TransformDirection(joint.axis), joint.transform.up);

            Vector3 normalYDirection = joint.transform.TransformDirection(Vector3.Cross(joint.axis, -Vector3.forward));
            Vector3 fromYDirection = Vector3.Cross(joint.transform.TransformDirection(joint.axis), normalYDirection);

            Vector3 normalZDirection = joint.transform.TransformDirection(Vector3.Cross(joint.axis, -Vector3.up));
            Vector3 fromZDirection = Vector3.Cross(joint.transform.TransformDirection(joint.axis), normalZDirection);

            // Angular X Limits
            Handles.color = Color.red * new Color(1, 1, 1, 1);
            Handles.DrawDottedLine(joint.transform.position, joint.transform.position + normalXDirection * radius / 2,
                5);
            Handles.color = Color.red * new Color(1, 1, 1, opacity);
            Handles.DrawSolidArc(joint.transform.position, normalXDirection, fromXDirection,
                -joint.lowAngularXLimit.limit, radius);
            Handles.DrawSolidArc(joint.transform.position, normalXDirection, fromXDirection,
                -joint.highAngularXLimit.limit, radius);

            // Angular Y Limits
            Handles.color = Color.green * new Color(1, 1, 1, 1);
            Handles.DrawDottedLine(joint.transform.position, joint.transform.position + normalYDirection * radius / 2,
                5);
            Handles.color = Color.green * new Color(1, 1, 1, opacity);
            Handles.DrawSolidArc(joint.transform.position, normalYDirection, fromYDirection, joint.angularYLimit.limit,
                radius);
            Handles.DrawSolidArc(joint.transform.position, normalYDirection, fromYDirection, -joint.angularYLimit.limit,
                radius);

            // Angular Z Limits
            Handles.color = Color.blue * new Color(1, 1, 1, 1);
            Handles.DrawDottedLine(joint.transform.position, joint.transform.position + normalZDirection * radius / 2,
                5);
            Handles.color = Color.blue * new Color(1, 1, 1, opacity);
            Handles.DrawSolidArc(joint.transform.position, normalZDirection, fromZDirection, joint.angularZLimit.limit,
                radius);
            Handles.DrawSolidArc(joint.transform.position, normalZDirection, fromZDirection, -joint.angularZLimit.limit,
                radius);
        }
        
        public override void Update()
        {
            if (!SelectedComponent) return;

            ConfigurableJoint lastSelectedJoint = SelectedComponent as ConfigurableJoint;
            
            Context.jointAxis.Normalize();
            
            var lowAngularXLimit = lastSelectedJoint.lowAngularXLimit;
            var highAngularXLimit = lastSelectedJoint.highAngularXLimit;
            var angularYLimit = lastSelectedJoint.angularYLimit;
            var angularZLimit = lastSelectedJoint.angularZLimit;
            
            lowAngularXLimit.limit = -Context.jointLowXLimit;
            highAngularXLimit.limit = Context.jointHighXLimit;
            angularYLimit.limit = Context.jointYLimit;
            angularZLimit.limit = Context.jointZLimit;
            
            lastSelectedJoint.lowAngularXLimit = lowAngularXLimit;
            lastSelectedJoint.highAngularXLimit = highAngularXLimit;
            lastSelectedJoint.angularYLimit = angularYLimit;
            lastSelectedJoint.angularZLimit = angularZLimit;
            lastSelectedJoint.axis = Context.jointAxis;
        }

        public override void Select(Component component)
        {
            base.Select(component);
            ConfigurableJoint lastSelectedJoint = SelectedComponent as ConfigurableJoint;            
            
            // Obtain original values
            Context.jointAxis = lastSelectedJoint.axis;
            Context.jointLowXLimit = -lastSelectedJoint.lowAngularXLimit.limit;
            Context.jointHighXLimit = lastSelectedJoint.highAngularXLimit.limit;
            Context.jointYLimit = lastSelectedJoint.angularYLimit.limit;
            Context.jointZLimit = lastSelectedJoint.angularZLimit.limit;
        }

        public override void Delete()
        {
            Undo.DestroyObjectImmediate(SelectedComponent.gameObject.GetComponent<Rigidbody>());
            base.Delete();
        }
    }
}