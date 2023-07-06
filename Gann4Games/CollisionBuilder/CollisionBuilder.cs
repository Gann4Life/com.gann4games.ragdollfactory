using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;

namespace Gann4Games.CollisionBuilder
{
    [ExecuteInEditMode]
    public partial class CollisionBuilder : MonoBehaviour
    {
        public Transform[] bones => GetComponentsInChildren<Transform>().Where(bone => bone.GetComponent<Collider>() == false).ToArray();
        public Collider[] colliders => GetComponentsInChildren<Collider>();
        [HideInInspector] public Transform selectedBoneA;
        [HideInInspector] public Transform selectedBoneB;
        
        public SelectionMode currentMode;
        public ColliderType colliderBuildType;

        //[Header("Capsule Collider Settings")]
        public float capsuleColliderRadius = 0.1f;
        public float capsuleColliderLength;
        
        //[Header("Box Collider Settings")]
        public float boxColliderWidth = 0.1f;
        public float boxColliderDepth = 0.1f;
        public float boxColliderLength;
        

        [Header("Gizmos Settings")]
        public bool showGizmos = true;
        public bool showNames = false;
        public float discRadius = 0.02f;
        public Color normalColor = new Color(1, 1, 1, 0.25f);
        public Color selectedColor = new Color(1, 1, 0, 0.25f);

        private Ray _mouseRay;

        private CapsuleCollider _selectedCapsuleCollider;
        private BoxCollider _selectedBoxCollider;
        public Collider LastSelectedCollider { get; private set; }
        
        private List<Collider> _colliderHistory = new List<Collider>();

        public enum SelectionMode
        {
            CreateColliders, SecondBoneSelected, SelectCollider
        }
        
        public enum ColliderType
        {
            Capsule, Box
        }

        // Cursor data is sent through the custom editor script to this function.
        public void SetMouseRay(Ray ray) => _mouseRay = ray;

        private Transform ClosestBoneToCursor()
        {
            Transform closest = bones[0];
            float closestAccuarcy = CursorSelectionAccuarcy(closest);
            foreach (Transform currentBone in bones)
            {
                float boneAccuarcy = CursorSelectionAccuarcy(currentBone);
                if (boneAccuarcy > closestAccuarcy)
                {
                    closest = currentBone;
                    closestAccuarcy = boneAccuarcy;
                }
            }
            return closest;
        }

        public bool IsColliderSelected(Collider col)
        {
            return col == _selectedBoxCollider || col == _selectedCapsuleCollider;
        }

        private Collider ClosestColliderToCursor()
        {
            if(colliders.Length == 0) return null;
            
            Collider closest = colliders[0];
            float closestAccuarcy = CursorSelectionAccuarcy(closest.transform);
            foreach (Collider currentCollider in colliders)
            {
                float accuarcy = CursorSelectionAccuarcy(currentCollider.transform);
                if(accuarcy > closestAccuarcy)
                {
                    closest = currentCollider;
                    closestAccuarcy = accuarcy;
                }
            }
            return closest;
        }

        private float CursorSelectionAccuarcy(Transform obj)
        {
            Vector3 objPosition = obj.position;
            Vector3 dirTowardsObj = (objPosition - _mouseRay.origin).normalized;
            return Vector3.Dot(dirTowardsObj, _mouseRay.direction);
        }
        
        public bool IsCursorLookingAt(Transform obj)
        {
            return CursorSelectionAccuarcy(obj) > 0.99f && (obj == ClosestBoneToCursor() || obj == ClosestColliderToCursor());
        }

        /// <summary>
        /// Execute an action when left mouse click is pressed.
        /// </summary>
        public void MouseDown()
        {
            switch (currentMode)
            {
                case SelectionMode.CreateColliders:
                    selectedBoneA = ClosestBoneToCursor();
                    currentMode = SelectionMode.SecondBoneSelected;
                    break;
                case SelectionMode.SecondBoneSelected:
                    selectedBoneB = ClosestBoneToCursor();
                    currentMode = SelectionMode.CreateColliders;
                    CreateCollider();
                    break;
                case SelectionMode.SelectCollider:
                    SelectCollider();
                    break;
            }
        }

        private void SelectCollider()
        {
            Collider closestCollider = ClosestColliderToCursor();
            if (closestCollider == null)
                return;
            
            LastSelectedCollider = closestCollider;
            
            // Check collider types
            if (closestCollider is CapsuleCollider)
            {
                _selectedCapsuleCollider = closestCollider as CapsuleCollider;
                capsuleColliderRadius = _selectedCapsuleCollider.radius;
                capsuleColliderLength = _selectedCapsuleCollider.center.z * 2;
                colliderBuildType = ColliderType.Capsule;
            }
            else if (closestCollider is BoxCollider)
            {
                _selectedBoxCollider = closestCollider as BoxCollider;
                boxColliderWidth = _selectedBoxCollider.size.x;
                boxColliderDepth = _selectedBoxCollider.size.y;
                boxColliderLength = _selectedBoxCollider.center.z * 2;
                colliderBuildType = ColliderType.Box;
            }
            else
            {
                throw new Exception("Collider type not supported!");
            }
        }

        private void CreateCollider()
        {
            switch (colliderBuildType)
            {
                case ColliderType.Capsule:
                    CreateCapsuleCollider(selectedBoneA, selectedBoneB);
                    LastSelectedCollider = _selectedCapsuleCollider;
                    _colliderHistory.Add(_selectedCapsuleCollider);
                    break;
                case ColliderType.Box:
                    CreateBoxCollider(selectedBoneA, selectedBoneB);
                    LastSelectedCollider = _selectedBoxCollider;
                    _colliderHistory.Add(_selectedBoxCollider);
                    break;
            }
        }

        private void CreateCapsuleCollider(Transform objA, Transform objB)
        {
            if (!objB.IsChildOf(objA))
                throw new Exception("The second bone must be child of the first bone!");
            
            float distance = Vector3.Distance(objA.position, objB.position);
            
            GameObject collisionObject = new GameObject(objA.name + " - " + objB.name);
            collisionObject.transform.SetParent(objA);
            collisionObject.transform.localPosition = Vector3.zero;
            collisionObject.transform.forward = objB.position - objA.position;
            collisionObject.transform.localScale = Vector3.one;
            _selectedCapsuleCollider = collisionObject.AddComponent<CapsuleCollider>();
            _selectedCapsuleCollider.direction = 2;
            _selectedCapsuleCollider.radius = capsuleColliderRadius;
            _selectedCapsuleCollider.center = Vector3.forward * distance / 2;
            _selectedCapsuleCollider.height = distance + _selectedCapsuleCollider.radius;
            
            capsuleColliderLength = distance;
            
            Undo.RegisterCreatedObjectUndo(collisionObject, "Created Capsule Collider Object");
            Undo.RegisterCompleteObjectUndo(this, "Created Capsule Collider Object");
        }

        private void CreateBoxCollider(Transform objA, Transform objB)
        {
            if (!objB.IsChildOf(objA))
                throw new Exception("The second bone must be child of the first bone!");
            
            float distance = Vector3.Distance(objA.position, objB.position);
            float height = distance / 2;
            GameObject collisionObject = new GameObject(objA.name + " - " + objB.name);
            collisionObject.transform.SetParent(objA);
            collisionObject.transform.localPosition = Vector3.zero;
            collisionObject.transform.forward = objB.position - objA.position;
            collisionObject.transform.localScale = Vector3.one;
            _selectedBoxCollider = collisionObject.AddComponent<BoxCollider>();
            _selectedBoxCollider.center = Vector3.forward * height;
            _selectedBoxCollider.size = new Vector3(boxColliderWidth, boxColliderDepth, distance);
            
            boxColliderLength = height;
            
            Undo.RegisterCreatedObjectUndo(collisionObject, "Created Box Collider Object");
            Undo.RegisterCompleteObjectUndo(this, "Created Box Collider Object");
        }

        /// <summary>
        /// Converts a box collider to a capsule collider
        /// </summary>
        public void ConvertSelectedColliderToCapsule()
        {
            BoxCollider col = _selectedBoxCollider;
            GameObject collisionObject = col.gameObject;
            
            float distance = col.size.z;
            float radius = Mathf.Max(col.size.x, col.size.y) / 2;
            float height = col.size.z / 2;

            capsuleColliderLength = distance;
            capsuleColliderRadius = radius;
            
            Undo.DestroyObjectImmediate(col);

            CapsuleCollider capsuleCollider = Undo.AddComponent<CapsuleCollider>(collisionObject);
            capsuleCollider.direction = 2;
            capsuleCollider.radius = radius;
            capsuleCollider.center = Vector3.forward * height;
            capsuleCollider.height = distance + capsuleCollider.radius;
            _selectedCapsuleCollider = capsuleCollider;

            _colliderHistory[_colliderHistory.Count - 1] = capsuleCollider;
            
            Undo.RegisterCompleteObjectUndo(collisionObject, "Converted Box Collider to Capsule Collider");
            
            colliderBuildType = ColliderType.Capsule;

            LastSelectedCollider = capsuleCollider;
        }

        /// <summary>
        /// Converts a capsule collider into a box collider
        /// </summary>
        public void ConvertSelectedColliderToBox()
        {
            CapsuleCollider capsuleCollider = _selectedCapsuleCollider;
            GameObject collisionObject = capsuleCollider.gameObject;

            float radius = capsuleCollider.radius * 2;
            float distance = capsuleCollider.center.z * 2;
            float height = distance / 2;

            boxColliderDepth = radius;
            boxColliderWidth = radius;
            boxColliderLength = distance;
            
            Undo.DestroyObjectImmediate(capsuleCollider);

            BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(collisionObject);
            boxCollider.center = Vector3.forward * height;
            boxCollider.size = new Vector3(radius, radius, distance);
            _selectedBoxCollider = boxCollider;
            
            _colliderHistory[_colliderHistory.Count - 1] = boxCollider;
            
            Undo.RegisterCompleteObjectUndo(collisionObject, "Converted Capsule Collider to Box Collider");
            
            colliderBuildType = ColliderType.Box;
            LastSelectedCollider = boxCollider;
        }
        public void DeleteSelectedCollider()
        {
            DeleteCollider(LastSelectedCollider);
        }
        public void DeleteLastCollider()
        {
            Collider collider = _colliderHistory[_colliderHistory.Count - 1];
            DeleteCollider(collider);
        }

        public void DeleteCollider(Collider collider)
        {
            _colliderHistory.Remove(collider);
            try
            {
                Undo.DestroyObjectImmediate(collider.gameObject);
                Undo.RegisterCompleteObjectUndo(this, "Deleted collider");
            }
            catch (InvalidOperationException e)
            {
                Debug.LogWarning("UNABLE TO DELETE GAMEOBJECT! Removing component instead.\nMore information:\n" + e);
                Undo.DestroyObjectImmediate(collider);
                Undo.RegisterCompleteObjectUndo(this, "Deleted collider component");
            }
        }

        public void DeleteAllColliders()
        {
            // Using an int variable because the length of the list changes with every deleted element.
            // Therefore it creates bugs if you delete all colliders at once because the loop will stop before deleting all colliders.
            int timesToRepeat = _colliderHistory.Count; //_colliderHistory.Count;
            for(int i = 0; i < timesToRepeat; i++)
                DeleteLastCollider();//DeleteCollider(colliders[i]);
        }

        private void OnValidate()
        {
            ValidateCapsuleColliderValues();
            ValidateBoxColliderValues();
        }

        private void ValidateCapsuleColliderValues()
        {
            if (!_selectedCapsuleCollider) return;

            // Limit values to 0 or higher
            if(capsuleColliderRadius < 0) capsuleColliderRadius = 0;
            if (capsuleColliderLength < 0) capsuleColliderLength = 0;
            
            _selectedCapsuleCollider.radius = capsuleColliderRadius;
            
            // Adjust capsule size and position to fit bones
            float distance = Vector3.Distance(_selectedCapsuleCollider.transform.position, _selectedCapsuleCollider.transform.position + _selectedCapsuleCollider.transform.forward * capsuleColliderLength);
            _selectedCapsuleCollider.center = Vector3.forward * distance / 2;
            _selectedCapsuleCollider.height = distance + _selectedCapsuleCollider.radius;   
        }
        
        private void ValidateBoxColliderValues()
        {
            if (!_selectedBoxCollider) return;
            
            // Limit values to 0 or higher
            if(boxColliderDepth < 0) boxColliderDepth = 0;
            if(boxColliderWidth < 0) boxColliderWidth = 0;
            
            // Adjust box size and position to fit bones
            _selectedBoxCollider.center = Vector3.forward * boxColliderLength / 2;
            _selectedBoxCollider.size = new Vector3(boxColliderWidth, boxColliderDepth, boxColliderLength);
        }

        private void OnEnable()
        {
            if(_colliderHistory.Count == 0 && colliders.Length > 0)
                _colliderHistory.AddRange(colliders);
        }
    }
}


