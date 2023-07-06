using System;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.CollisionBuilder
{
    [CustomEditor(typeof(CollisionBuilder))]
    public class CollisionBuilderEditor : Editor
    {
        public static CollisionBuilderEditor Instance;
        
        private CollisionBuilder _target;

        private static Vector3 MousePosition => Event.current.mousePosition;
        public static Ray MouseRay => HandleUtility.GUIPointToWorldRay(MousePosition);
        
        #region Serialized Properties
        
        private SerializedProperty _capsuleRadius;
        private SerializedProperty _capsuleLength;
        private SerializedProperty _boxWidth;
        private SerializedProperty _boxDepth;
        private SerializedProperty _boxLength;

        #endregion

        private void OnEnable()
        {
            if (!Instance) 
                Instance = this;
            
            _capsuleRadius = serializedObject.FindProperty("capsuleColliderRadius");
            _capsuleLength = serializedObject.FindProperty("capsuleColliderLength");
            _boxWidth = serializedObject.FindProperty("boxColliderWidth");
            _boxDepth = serializedObject.FindProperty("boxColliderDepth");
            _boxLength = serializedObject.FindProperty("boxColliderLength");
        }

        // Draw inspector
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            serializedObject.Update();
            DrawInspectorHeader();
            DrawColliderProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInspectorHeader()
        {
            bool showBoxColliderButton = _target.currentMode == CollisionBuilder.SelectionMode.SelectCollider ||  _target.colliderBuildType != CollisionBuilder.ColliderType.Box;
            bool showCapsuleColliderbutton = _target.currentMode == CollisionBuilder.SelectionMode.SelectCollider ||  _target.colliderBuildType != CollisionBuilder.ColliderType.Capsule;
            bool showSelectColliderButton = _target.currentMode != CollisionBuilder.SelectionMode.SelectCollider;
            
            switch(_target.currentMode)
            {
                case CollisionBuilder.SelectionMode.CreateColliders:
                    EditorGUILayout.LabelField($"CREATING {_target.colliderBuildType.ToString().ToUpper()} COLLIDERS", EditorStyles.boldLabel);
                    break;
                case CollisionBuilder.SelectionMode.SelectCollider:
                    EditorGUILayout.LabelField("SELECTING COLLIDERS", EditorStyles.boldLabel);
                    break;
            }
            
            EditorGUILayout.BeginHorizontal();
            if (showBoxColliderButton && GUILayout.Button("Draw Boxes"))
            {
                _target.colliderBuildType = CollisionBuilder.ColliderType.Box;
                _target.currentMode = CollisionBuilder.SelectionMode.CreateColliders;
            }
            if (showCapsuleColliderbutton && GUILayout.Button("Draw Capsules"))
            {
                _target.colliderBuildType = CollisionBuilder.ColliderType.Capsule;
                _target.currentMode = CollisionBuilder.SelectionMode.CreateColliders;
            }
            if(showSelectColliderButton && GUILayout.Button("Select a Collider"))
                _target.currentMode = CollisionBuilder.SelectionMode.SelectCollider;
            EditorGUILayout.EndHorizontal();

            if(_target.colliders.Length > 0){
                EditorGUILayout.BeginHorizontal();
                if(_target.colliders.Length > 1 && GUILayout.Button("DELETE ALL"))
                    _target.DeleteAllColliders();
                if(GUILayout.Button("DELETE LAST"))
                    _target.DeleteLastCollider();
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawColliderProperties()
        {
            if (_target.LastSelectedCollider is CapsuleCollider)
            {
                EditorGUILayout.PropertyField(_capsuleLength);
                EditorGUILayout.PropertyField(_capsuleRadius);
            }

            if (_target.LastSelectedCollider is BoxCollider)
            {
                EditorGUILayout.PropertyField(_boxLength);
                EditorGUILayout.PropertyField(_boxWidth);
                EditorGUILayout.PropertyField(_boxDepth);
            }

            EditorGUILayout.BeginHorizontal();
            if (_target.LastSelectedCollider is CapsuleCollider && GUILayout.Button("Convert to Box Collider"))
                _target.ConvertSelectedColliderToBox();
            if (_target.LastSelectedCollider is BoxCollider && GUILayout.Button("Convert to Capsule Collider"))
                _target.ConvertSelectedColliderToCapsule();
            if(_target.LastSelectedCollider && GUILayout.Button("Delete"))
                _target.DeleteSelectedCollider();
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            _target = (CollisionBuilder)target;
            
            _target.SetMouseRay(MouseRay);
            CheckForInput();

            switch (_target.currentMode)
            {
                case CollisionBuilder.SelectionMode.CreateColliders:
                    DrawSelectableBones();
                    // DrawBoneHierarchyRecursive(_target.transform);
                    break;
                case CollisionBuilder.SelectionMode.SecondBoneSelected:
                    GUIStyle fontStyle = new GUIStyle();
                    fontStyle.fontStyle = FontStyle.Bold;
                    fontStyle.normal.textColor = Color.red;

                    DrawSelectableBones();
                    // DrawBoneHierarchyRecursive(_target.transform);
                    Handles.Label(_target.selectedBoneA.position + Vector3.up * 0.1f, "Select a second bone...", fontStyle);
                    break;
                case CollisionBuilder.SelectionMode.SelectCollider:
                    // DrawBoneHierarchyRecursive(_target.transform);
                    DrawSelectableBones();
                    DrawSelectableColliders();
                    break;
            }
            Selection.activeObject = _target.transform.gameObject;
            SceneView.RepaintAll();
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

        private void DrawSelectableColliders()
        {
            foreach (Collider collider in _target.colliders)
            {
                Transform colliderTransform = collider.transform;
                Vector3 position = colliderTransform.position;

                bool isHighlighted = _target.IsCursorLookingAt(colliderTransform);
                bool isSelected = _target.IsColliderSelected(collider);
                
                Handles.color = isHighlighted ? _target.selectedColor : _target.normalColor;
                if (isSelected) {
                    Handles.DrawSolidDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward,
                        _target.discRadius);
                    Handles.Label(position, collider.name);
                }
                else Handles.DrawWireDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward, _target.discRadius);
            }
        }

        private void CheckForInput()
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
                _target.MouseDown();
        }

        private void DrawSelectableBones()
        {
            foreach (Transform bone in _target.bones)
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
