using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gann4Games.RagdollFactory
{
    public partial class RagdollFactoryEditor
    {
        private static Vector3 MousePosition => Event.current.mousePosition;
        public static Ray MouseRay => HandleUtility.GUIPointToWorldRay(MousePosition);

        private void OnSceneGUI()
        {
            if (!_target) return;

            _target.SetMouseRay(MouseRay);
            CheckForInput();

            switch (_target.actionTypeOnClick)
            {
                case RagdollFactory.ActionTypeOnClick.Create:
                    if(_target.IsFirstBoneSelected)
                        Handles.Label(_target.selectedBoneA.position + Vector3.up * 0.1f, "Select a second bone...", GUI_ALERT_STYLE);
                    switch (_target.componentType)
                    {
                        case RagdollFactory.ComponentType.Box:
                            DrawSelectableBones();
                            break;
                        case RagdollFactory.ComponentType.Capsule:
                            DrawSelectableBones();
                            break;
                        case RagdollFactory.ComponentType.ConfigurableJoint:
                            DrawSelectableBones();
                            DrawJointHierarchy();
                            DrawSelectableJoints();
                            break;
                        case RagdollFactory.ComponentType.Rigidbody:
                            DrawSelectableBones();
                            DrawSelectableRigidbodies();
                            break;
                    }
                    break;
                case RagdollFactory.ActionTypeOnClick.Select:
                    DrawSelectableElements();
                    break;
                case RagdollFactory.ActionTypeOnClick.Delete:
                    DrawSelectableElements();
                    break;
            }

            Selection.activeObject = _target.transform.gameObject;
            SceneView.RepaintAll();
        }

        private void DrawSelectableElements()
        {
            switch (_target.componentType)
            {
                case RagdollFactory.ComponentType.Box:
                    DrawSelectableColliders();
                    break;
                case RagdollFactory.ComponentType.Capsule:
                    DrawSelectableColliders();
                    break;
                case RagdollFactory.ComponentType.ConfigurableJoint:
                    DrawSelectableJoints();
                    DrawJointHierarchy();
                    break;
                case RagdollFactory.ComponentType.Rigidbody:
                    DrawSelectableRigidbodies();
                    break;
            }
        }
        
        private void CheckForInput()
        {
            Event e = Event.current;
            KeyCode keyCode = e.keyCode;
            if (e.type == EventType.MouseDown && e.button == 0)
                _target.MouseDown();

            if (e.type == EventType.KeyDown && keyCode == KeyCode.Escape)
                _target.DeselectBones();
        }

        private void DrawSelectableRigidbodies()
        {
            foreach (Rigidbody rb in _target.Rigidbodies)
            {
                Transform rbTransform = rb.transform;
                Vector3 position = rbTransform.position;
                
                bool isHighlighted = _target.IsCursorLookingAt(rbTransform);
                bool isSelected = _target.IsRigidbodySelected(rb);

                string kinematicLabel = rb.isKinematic ? "KINEMATIC" : "DYNAMIC";
                Color kinematicColor = Color.cyan * _target.normalColor;
                
                Handles.color = isHighlighted ? _target.selectedColor : _target.normalColor;
                Handles.color = rb.isKinematic ? kinematicColor : Handles.color;
                Handles.color = isSelected ? _target.selectedColor : Handles.color;

                Handles.DrawSolidDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward,
                    _target.discRadius * rb.mass);
            }
        }
        
        private void DrawSelectableJoints()
        {
            foreach (ConfigurableJoint joint in _target.Joints)
            {
                Transform jointTransform = joint.transform;
                Vector3 position = jointTransform.position;

                bool isHighlighted = _target.IsCursorLookingAt(jointTransform);
                bool isSelected = _target.IsJointSelected(joint);

                Handles.color = isHighlighted ? _target.selectedColor : _target.normalColor;
                if (isSelected)
                {
                    Handles.DrawSolidDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward,
                        _target.discRadius);
                    Handles.Label(position, "JOINT " + joint.name);
                }
                else
                    Handles.DrawWireDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward,
                        _target.discRadius);
            }
        }

        private void DrawSelectableColliders()
        {
            foreach (Collider collider in _target.Colliders)
            {
                Transform colliderTransform = collider.transform;
                Vector3 position = colliderTransform.position;

                bool isHighlighted = _target.IsCursorLookingAt(colliderTransform);
                bool isSelected = _target.IsColliderSelected(collider);

                Handles.color = isHighlighted ? _target.selectedColor : _target.normalColor;
                if (isSelected)
                {
                    Handles.DrawSolidDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward,
                        _target.discRadius);
                    Handles.Label(position, collider.name);
                }
                else
                    Handles.DrawWireDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward,
                        _target.discRadius);
            }
        }

        private void DrawBoneHierarchyRecursive(Transform bone)
        {
            if (bone.childCount == 0) return;
            Handles.color = _target.normalColor;
            for (int i = 0; i < bone.childCount; i++)
            {
                Transform childBone = bone.GetChild(i);
                Handles.DrawDottedLine(bone.position, childBone.position, 5f);
                // Handles.DrawWireDisc(bone.position, SceneView.currentDrawingSceneView.camera.transform.forward, _target.discRadius);
                DrawBoneHierarchyRecursive(childBone);
            }
        }

        private void DrawJointHierarchy()
        {
            if (_target.Joints.Length == 0) return;


            foreach (ConfigurableJoint joint in _target.Joints)
            {
                bool hasConnectedBody = joint.connectedBody;
                Transform jointTransform = joint.transform;

                Handles.color = hasConnectedBody ? Color.yellow : Color.red;
                Handles.DrawWireDisc(jointTransform.position,
                    SceneView.currentDrawingSceneView.camera.transform.forward, _target.discRadius);
                if (hasConnectedBody) Handles.DrawDottedLine(jointTransform.position, joint.connectedBody.position, 5f);
                else Handles.Label(jointTransform.position, "MISSING CONNECTION", GUI_ALERT_STYLE);
                VisualizeJointLimits(joint);
            }
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

        private void DrawSelectableBones()
        {
            foreach (Transform bone in _target.Bones)
            {
                bool isHighlighted = _target.IsCursorLookingAt(bone);
                float radius = isHighlighted ? _target.capsuleColliderRadius : _target.discRadius;
                Vector3 position = bone.position;

                Handles.color = isHighlighted ? _target.selectedColor : _target.normalColor;
                Handles.DrawWireDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward, radius);
                if (isHighlighted) Handles.Label(position, bone.name);
            }
        }
    }
}