using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory
{
    [CustomEditor(typeof(RagdollFactory))]
    public partial class RagdollFactoryEditor : Editor
    {
        public static RagdollFactoryEditor Instance;

        private RagdollFactory _target;


        private GUIStyle GUI_ALERT_STYLE = new GUIStyle();

        #region Serialized Properties

        // Capsule settings
        private SerializedProperty _capsuleRadius;
        private SerializedProperty _capsuleLength;
        // Box settings
        private SerializedProperty _boxWidth;
        private SerializedProperty _boxDepth;
        private SerializedProperty _boxLength;
        // Joint settings
        private SerializedProperty _jointAxis;
        private SerializedProperty _jointLowXLimit;
        private SerializedProperty _jointHighXLimit;
        private SerializedProperty _jointYLimit;
        private SerializedProperty _jointZAngle;
        // Rigidbody settings

        #endregion

        private int _componentToCreate = 0;
        private int _componentMode = 0;
        
        private void OnEnable()
        {
            _target = (RagdollFactory)target;
            
            GUI_ALERT_STYLE.fontStyle = FontStyle.Bold;
            GUI_ALERT_STYLE.normal.textColor = Color.red;

            if (!Instance)
                Instance = this;

            _capsuleRadius = serializedObject.FindProperty("capsuleColliderRadius");
            _capsuleLength = serializedObject.FindProperty("capsuleColliderLength");

            _boxWidth = serializedObject.FindProperty("boxColliderWidth");
            _boxDepth = serializedObject.FindProperty("boxColliderDepth");
            _boxLength = serializedObject.FindProperty("boxColliderLength");

            _jointAxis = serializedObject.FindProperty("jointAxis");
            _jointLowXLimit = serializedObject.FindProperty("jointLowXLimit");
            _jointHighXLimit = serializedObject.FindProperty("jointHighXLimit");
            _jointYLimit = serializedObject.FindProperty("jointYLimit");
            _jointZAngle = serializedObject.FindProperty("jointZLimit");
        }

        // Draw inspector
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            DrawHelpBox();
            serializedObject.Update();
            DrawInspectorHeader();
            DrawCurrentComponentProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCurrentComponentProperties()
        {
            switch (_target.componentType)
            {
                case RagdollFactory.ComponentType.Capsule:
                    DrawCapsuleColliderProperties();
                    break;
                case RagdollFactory.ComponentType.Box:
                    DrawBoxColliderProperties();
                    break;
                case RagdollFactory.ComponentType.ConfigurableJoint:
                    DrawJointProperties();
                    break;
            }
        }

        private void DrawInspectorHeader()
        {
            _target.componentType = (RagdollFactory.ComponentType)GUILayout.Toolbar((int)_target.componentType, new string[]
            {
                "Capsule", "Box", "Joint", "Rigidbody"
            });
            
            _target.actionTypeOnClick = (RagdollFactory.ActionTypeOnClick)GUILayout.Toolbar((int)_target.actionTypeOnClick, new string[]
            {
                "Create", "Select", "Delete"
            });
            GUILayout.Space(15);
        }

        private void DrawJointProperties()
        {
            EditorGUILayout.PropertyField(_jointAxis);
            EditorGUILayout.PropertyField(_jointLowXLimit);
            EditorGUILayout.PropertyField(_jointHighXLimit);
            EditorGUILayout.PropertyField(_jointYLimit);
            EditorGUILayout.PropertyField(_jointZAngle);

            if (GUILayout.Button("Delete Joint"))
            {
                _target.DeleteSelectedJoint();
                // _target.DeleteSelectedRigidbody();
            }
        }
        private void DrawCapsuleColliderProperties()
        {
            EditorGUILayout.PropertyField(_capsuleLength);
            EditorGUILayout.PropertyField(_capsuleRadius);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Convert to Box Collider"))
                _target.ConvertSelectedColliderToBox();
            if (_target.LastSelectedCollider && GUILayout.Button("Delete"))
                _target.DeleteSelectedCollider();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBoxColliderProperties()
        {
            EditorGUILayout.PropertyField(_boxLength);
            EditorGUILayout.PropertyField(_boxWidth);
            EditorGUILayout.PropertyField(_boxDepth);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Convert to Capsule Collider"))
                _target.ConvertSelectedColliderToCapsule();
            if (GUILayout.Button("Delete"))
                _target.DeleteSelectedCollider();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Shows relevant information about how to perfom a specific action in the choosen tool/button/mode
        /// </summary>
        private void DrawHelpBox()
        {
            string message = "";
            MessageType messageType = MessageType.Info;
            switch (_target.componentType)
            {
                case RagdollFactory.ComponentType.Capsule:
                    if (_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Create)
                        message += _target.IsFirstBoneSelected
                            ? "Now select the end point for your collider."
                            : "Select the bone that's going to have a collider.";
                    else if(_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Select)
                        message += "Select a collider in the scene view with left click to begin editing its properties.";
                    else if (_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Delete)
                        message += "Select a collider in the scene view with left click to delete it.";
                    break;
                case RagdollFactory.ComponentType.Box:
                    if (_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Create)
                        message += _target.IsFirstBoneSelected
                            ? "Now select the end point for your collider."
                            : "Select the bone that's going to have a collider.";
                    else if(_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Select)
                        message += "Select a collider in the scene view with left click to begin editing its properties.";
                    else if (_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Delete)
                        message += "Select a collider in the scene view with left click to delete it.";
                    
                    break;
                case RagdollFactory.ComponentType.ConfigurableJoint:
                    if(_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Create)
                        message += _target.IsFirstBoneSelected 
                            ? "Select the bone that is going to have the joint connecting to the first bone."
                            : "Select the bone that is going to be the parent of the joint you want to create.";
                    else if (_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Select)
                        message += "Select a joint in the scene view with left click to edit its properties.";
                    else if (_target.actionTypeOnClick == RagdollFactory.ActionTypeOnClick.Delete)
                        message += "Select a join in the scene view with Left Click to delete it.";
                    
                    break;
                case RagdollFactory.ComponentType.Rigidbody:
                    messageType = MessageType.Warning;
                    message += "Rigidbodies not supported yet.";
                    break;
            }
            EditorGUILayout.HelpBox(message, messageType);
        }
    }
}
