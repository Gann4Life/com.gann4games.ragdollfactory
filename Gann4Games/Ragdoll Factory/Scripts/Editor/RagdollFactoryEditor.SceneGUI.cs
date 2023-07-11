using Gann4Games.RagdollFactory.States;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gann4Games.RagdollFactory
{
    public partial class RagdollFactoryEditor
    {

        private void OnSceneGUI()
        {
            if (!_target) return;

            CheckForInput();

            _target.CurrentComponent.DrawGUI();
            switch (_target.actionTypeOnClick)
            {
                case RagdollFactory.ActionTypeOnClick.Create:
                    DrawSelectableBones();
                    break;
            }
            
            SceneView.RepaintAll();
        }
        
        private void CheckForInput()
        {
            Event e = Event.current;
            KeyCode keyCode = e.keyCode;
            // if (e.type == EventType.MouseDown && e.button == 0)
            //     _target.MouseDown();

            if (e.type == EventType.KeyDown && keyCode == KeyCode.Escape)
                _target.DeselectBones();
        }

        private void DrawSelectableBones()
        {
            foreach (Transform bone in _target.Bones)
            {
                Handles.color = _target.normalColor;

                if (_target.selectedBoneA == bone)
                {
                    Handles.color = _target.selectedColor;
                    Handles.Label(bone.position, "Waiting for second bone...", GUI_ALERT_STYLE);
                }
                
                bool bonePressed = Handles.Button(
                    bone.position, 
                    SceneView.currentDrawingSceneView.camera.transform.rotation, 
                    _target.discRadius, 
                    _target.discRadius*2, 
                    Handles.CircleHandleCap       
                    );
                
                if (bonePressed)
                {
                    if (!_target.IsFirstBoneSelected && !(_target.CurrentComponent is RigidbodyComponentState))
                    {
                        _target.selectedBoneA = bone;
                    }
                    else
                    {
                        _target.selectedBoneB = bone;
                        _target.CurrentComponent.Create();
                        _target.DeselectBones();
                    }
                }
            }
        }
    }
}