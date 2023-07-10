using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace Gann4Games.RagdollFactory
{
    [ExecuteInEditMode]
    public partial class RagdollFactory : MonoBehaviour
    {
        public Transform[] Bones => GetComponentsInChildren<Transform>().Where(bone => bone.GetComponent<Collider>() == false).ToArray();

        public Collider[] Colliders => GetComponentsInChildren<Collider>();
        public Collider LastSelectedCollider { get; private set; }
        public Rigidbody[] Rigidbodies => GetComponentsInChildren<Rigidbody>();
        public Rigidbody LastSelectedRigidbody { get; private set; }
        public ConfigurableJoint[] Joints => GetComponentsInChildren<ConfigurableJoint>();
        public ConfigurableJoint LastSelectedJoint { get; private set; }
        
        public Transform selectedBoneA;
        public Transform selectedBoneB;
        
        public ComponentType componentType;
        public ActionTypeOnClick actionTypeOnClick;

        //[Header("Capsule Collider Settings")]
        public float capsuleColliderRadius = 0.1f;
        public float capsuleColliderLength;
        
        //[Header("Box Collider Settings")]
        public float boxColliderWidth = 0.1f;
        public float boxColliderDepth = 0.1f;
        public float boxColliderLength;
        
        // Joint settings
        public Vector3 jointAxis = new(1, 0, 0);
        [Range(0, 180)] public float jointLowXLimit = 0;
        [Range(0, 180)] public float jointHighXLimit = 0;
        [Range(0, 180)] public float jointYLimit = 0;
        [Range(0, 180)] public float jointZLimit = 0;

        // Rigidbody settings
        public float rigidbodyMass = 1;
        public float rigidbodyDrag = 0;
        public float rigidbodyAngularDrag = 0.05f;
        public bool rigidbodyUseGravity = true;
        public bool rigidbodyIsKinematic = false;
        
        [Header("Gizmos Settings")]
        public bool showGizmos = true;
        public bool showNames = false;
        public float discRadius = 0.02f;
        public Color normalColor = new Color(1, 1, 1, 0.25f);
        public Color selectedColor = new Color(1, 1, 0, 0.25f);

        private Ray _mouseRay;

        
        private CapsuleCollider _selectedCapsuleCollider;
        private BoxCollider _selectedBoxCollider;

        private List<Component> _componentHistory = new List<Component>();

        public bool IsFirstBoneSelected => selectedBoneA != null;

        public enum ActionTypeOnClick
        {
            Create, Select, Delete
        }
        
        public enum ComponentType
        {
            Capsule, Box, ConfigurableJoint, Rigidbody
        }

        // Cursor data is sent through the custom editor script to this function.
        public void SetMouseRay(Ray ray) => _mouseRay = ray;

        
        #region Closest elements to cursor
        // Checkers for close components - Yes, there's a code smell here.
        private Transform ClosestBoneToCursor()
        {
            Transform closestObject = Bones[0];
            float closestAccuarcy = CursorSelectionAccuracy(closestObject);
            foreach (Transform currentBone in Bones)
            {
                float currentAccuarcy = CursorSelectionAccuracy(currentBone);
                if (currentAccuarcy > closestAccuarcy)
                {
                    closestObject = currentBone;
                    closestAccuarcy = currentAccuarcy;
                }
            }
            return closestObject;
        }
        private Collider ClosestColliderToCursor()
        {
            if(Colliders.Length == 0) return null;
            
            Collider closestObject = Colliders[0];
            float closestAccuarcy = CursorSelectionAccuracy(closestObject.transform);
            foreach (Collider currentCollider in Colliders)
            {
                float currentAccuarcy = CursorSelectionAccuracy(currentCollider.transform);
                if(currentAccuarcy > closestAccuarcy)
                {
                    closestObject = currentCollider;
                    closestAccuarcy = currentAccuarcy;
                }
            }
            return closestObject;
        }
        private Rigidbody ClosestRigidbodyToCursor()
        {
            if (Rigidbodies.Length == 0) return null;

            Rigidbody closestObject = Rigidbodies[0];
            float closestAccuarcy = CursorSelectionAccuracy(closestObject.transform);
            foreach (Rigidbody currentRigidbody in Rigidbodies)
            {
                float currentAccuarcy = CursorSelectionAccuracy(currentRigidbody.transform);
                if (currentAccuarcy > closestAccuarcy)
                {
                    closestObject = currentRigidbody;
                    closestAccuarcy = currentAccuarcy;
                }
            }

            return closestObject;
        }
        private ConfigurableJoint ClosestJointToCursor()
        {
            if (Joints.Length == 0) return null;
            
            ConfigurableJoint closestObject = Joints[0];
            float closestAccuarcy = CursorSelectionAccuracy(closestObject.transform);
            foreach(ConfigurableJoint currentJoint in Joints)
            {
                float currentAccuarcy = CursorSelectionAccuracy(currentJoint.transform);
                if(currentAccuarcy > closestAccuarcy)
                {
                    closestObject = currentJoint;
                    closestAccuarcy = currentAccuarcy;
                }
            }
            return closestObject;
        }
        #endregion

        #region Selected elements check
        /// <summary>
        /// Checks if the given collider is being currently selected.
        /// </summary>
        /// <param name="col">The collider to check</param>
        /// <returns>If it is selected or not</returns>
        public bool IsColliderSelected(Collider col) => col == LastSelectedCollider;
        
        /// <summary>
        /// Checks if the given rigidbody is being currently selected.
        /// </summary>
        /// <param name="obj">The rigidbody to check</param>
        /// <returns>If it is selected or not</returns>
        public bool IsRigidbodySelected(Rigidbody rb) => rb == LastSelectedRigidbody;
        
        /// <summary>
        /// Checks if the given configurable joint is being currently selected.
        /// </summary>
        /// <param name="joint">The joint to check</param>
        /// <returns>If it is selected or not</returns>
        public bool IsJointSelected(ConfigurableJoint joint) => joint == LastSelectedJoint;
        #endregion
        
        private float CursorSelectionAccuracy(Transform obj)
        {
            Vector3 objPosition = obj.position;
            Vector3 dirTowardsObj = (objPosition - _mouseRay.origin).normalized;
            return Vector3.Dot(dirTowardsObj, _mouseRay.direction);
        }

        /// <summary>
        /// Checks if the given object is one of the closest from all the supported types.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsClosestOfAnyType(Transform obj) => obj == ClosestBoneToCursor() || obj == ClosestColliderToCursor() || obj == ClosestRigidbodyToCursor() || obj == ClosestJointToCursor();
        
        public bool IsCursorLookingAt(Transform obj) =>  CursorSelectionAccuracy(obj) > 0.99f && IsClosestOfAnyType(obj);

        /// <summary>
        /// Execute an action when left mouse click is pressed.
        /// </summary>
        public void MouseDown()
        {
            switch (actionTypeOnClick)
            {
                case ActionTypeOnClick.Create:
                    bool useFirstBone = componentType != ComponentType.Rigidbody && !IsFirstBoneSelected;
                    if(useFirstBone) SelectFirstBone();
                    else
                    {
                        SelectSecondBone();
                        CreateComponent();
                    }
                    break;
                case ActionTypeOnClick.Select:
                    SelectComponent();
                    break;
                case ActionTypeOnClick.Delete:
                    SelectComponent();
                    DeleteSelectedComponent();
                    break;
            }
        }

        private void DeleteSelectedComponent()
        {
            switch (componentType)
            {
                case ComponentType.Box:
                    DeleteSelectedCollider();
                    break;
                case ComponentType.Capsule:
                    DeleteSelectedCollider();
                    break;
                case ComponentType.ConfigurableJoint:
                    DeleteSelectedJoint();
                    break;
                case ComponentType.Rigidbody:
                    DeleteSelectedRigidbody();
                    break;
            }
        }

        private void SelectFirstBone()
        {
            selectedBoneA = ClosestBoneToCursor();
        }

        private void SelectSecondBone()
        {
            selectedBoneB = ClosestBoneToCursor();
        }
        
        public void DeselectBones()
        {
            selectedBoneA = null;
            selectedBoneB = null;
        }
        
        private void SelectComponent()
        {
            switch (componentType)
            {
                case ComponentType.Box:
                    SelectCollider();
                    break;
                case ComponentType.Capsule:
                    SelectCollider();
                    break;
                case ComponentType.ConfigurableJoint:
                    SelectJoint();
                    break;
                case ComponentType.Rigidbody:
                    SelectRigidbody();
                    break;
            }
        }

        private void SelectRigidbody()
        {
            Rigidbody closestRigidbody = ClosestRigidbodyToCursor();
            if(closestRigidbody == null)
                return;
            
            LastSelectedRigidbody = closestRigidbody;
            
            // Obtain original values
            rigidbodyMass = closestRigidbody.mass;
            rigidbodyDrag = closestRigidbody.drag;
            rigidbodyAngularDrag = closestRigidbody.angularDrag;
            rigidbodyUseGravity = closestRigidbody.useGravity;
            rigidbodyIsKinematic = closestRigidbody.isKinematic;
        }

        private void SelectJoint()
        {
            ConfigurableJoint closestJoint = ClosestJointToCursor();
            if(closestJoint == null)
                return;
            
            LastSelectedJoint = closestJoint;
            LastSelectedRigidbody = closestJoint.GetComponent<Rigidbody>();
            
            // Obtain original values
            jointAxis = closestJoint.axis;
            jointLowXLimit = -closestJoint.lowAngularXLimit.limit;
            jointHighXLimit = closestJoint.highAngularXLimit.limit;
            jointYLimit = closestJoint.angularYLimit.limit;
            jointZLimit = closestJoint.angularZLimit.limit;
            
            componentType = ComponentType.ConfigurableJoint;
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
                componentType = ComponentType.Capsule;
            }
            else if (closestCollider is BoxCollider)
            {
                _selectedBoxCollider = closestCollider as BoxCollider;
                boxColliderWidth = _selectedBoxCollider.size.x;
                boxColliderDepth = _selectedBoxCollider.size.y;
                boxColliderLength = _selectedBoxCollider.center.z * 2;
                componentType = ComponentType.Box;
            }
            else
            {
                throw new Exception("Collider type not supported!");
            }
        }

        /// <summary>
        /// Creates the component based on the two selected bones, the bones will be deselected afterwards.
        /// </summary>
        private void CreateComponent()
        {
            switch (componentType)
            {
                case ComponentType.Capsule:
                    CreateCapsuleCollider(selectedBoneA, selectedBoneB);
                    break;
                case ComponentType.Box:
                    CreateBoxCollider(selectedBoneA, selectedBoneB);
                    break;
                case ComponentType.ConfigurableJoint:
                    CreateOrSelectConfigurableJoint(selectedBoneA, selectedBoneB);
                    break;
                case ComponentType.Rigidbody:
                    CreateRigidbody(selectedBoneB);
                    break;
            }
            DeselectBones();
        }

        

        #region Component creation
        private T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            if(target.TryGetComponent(out T component)) 
                return component;
            
            T comp = target.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(comp, "Created component " + comp.name);
            return comp;
        }

        private void CreateRigidbody(Transform obj)
        {
            Rigidbody rb = GetOrAddComponent<Rigidbody>(obj.gameObject);
            LastSelectedRigidbody = rb;
            LastSelectedRigidbody.mass = rigidbodyMass;
            LastSelectedRigidbody.drag = rigidbodyDrag;
            LastSelectedRigidbody.angularDrag = rigidbodyAngularDrag;
            LastSelectedRigidbody.useGravity = rigidbodyUseGravity;
            LastSelectedRigidbody.isKinematic = rigidbodyIsKinematic;
                
            _componentHistory.Add(rb);
        }
        
        private void CreateOrSelectConfigurableJoint(Transform objA, Transform objB)
        {
            if (!objB.IsChildOf(objA))
                throw new Exception("The second bone must be child of the first bone!");

            Rigidbody rigidbodyA = GetOrAddComponent<Rigidbody>(objA.gameObject);
            Rigidbody rigidbodyB = GetOrAddComponent<Rigidbody>(objB.gameObject);
            ConfigurableJoint joint = GetOrAddComponent<ConfigurableJoint>(objB.gameObject);
            
            LastSelectedJoint = joint;
            LastSelectedRigidbody = rigidbodyB;
            
            LastSelectedJoint.connectedBody = rigidbodyA;
            LastSelectedJoint.xMotion = ConfigurableJointMotion.Locked;
            LastSelectedJoint.yMotion = ConfigurableJointMotion.Locked;
            LastSelectedJoint.zMotion = ConfigurableJointMotion.Locked;
            LastSelectedJoint.angularXMotion = ConfigurableJointMotion.Limited;
            LastSelectedJoint.angularYMotion = ConfigurableJointMotion.Limited;
            LastSelectedJoint.angularZMotion = ConfigurableJointMotion.Limited;
            
            LastSelectedJoint.axis = jointAxis;
            LastSelectedJoint.lowAngularXLimit = new SoftJointLimit() {limit = -jointLowXLimit};
            LastSelectedJoint.highAngularXLimit = new SoftJointLimit() {limit = jointHighXLimit};
            LastSelectedJoint.angularYLimit = new SoftJointLimit() {limit = jointYLimit};
            LastSelectedJoint.angularZLimit = new SoftJointLimit() {limit = jointZLimit};

            _componentHistory.Add(joint);
            _componentHistory.Add(rigidbodyB);
            _componentHistory.Add(rigidbodyA);
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
            
            LastSelectedCollider = _selectedCapsuleCollider;
            _componentHistory.Add(_selectedCapsuleCollider);
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
            
            
            LastSelectedCollider = _selectedBoxCollider;
            _componentHistory.Add(_selectedBoxCollider);
        }
        #endregion

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

            _componentHistory[_componentHistory.Count - 1] = capsuleCollider;
            
            Undo.RegisterCompleteObjectUndo(collisionObject, "Converted Box Collider to Capsule Collider");
            
            componentType = ComponentType.Capsule;

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
            
            _componentHistory[_componentHistory.Count - 1] = boxCollider;
            
            Undo.RegisterCompleteObjectUndo(collisionObject, "Converted Capsule Collider to Box Collider");
            
            componentType = ComponentType.Box;
            LastSelectedCollider = boxCollider;
        }

        public void TryDeleteObject(Component component)
        {
            try
            {
                DeleteObject(component);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogWarning("UNABLE TO DELETE GAMEOBJECT! Removing component instead.\nMore information:\n" + e);
                DeleteComponent(component);   
            }
        }

        public void DeleteSelectedRigidbody()
        {
            DeleteComponent(LastSelectedRigidbody);
        }

        public void DeleteSelectedJoint()
        {
            ConfigurableJoint nextSelection = LastSelectedJoint.connectedBody?.GetComponent<ConfigurableJoint>();
            DeleteComponent(LastSelectedJoint);
            LastSelectedJoint = nextSelection;
            DeleteSelectedRigidbody();
        }
        
        public void DeleteSelectedCollider()
        {
            TryDeleteObject(LastSelectedCollider);
        }
        
        public void DeleteLastComponent()
        {
            Component component = _componentHistory[_componentHistory.Count - 1];
            DeleteComponent(component);
        }

        public void DeleteComponent(Component component)
        {
            _componentHistory.Remove(component);
            Undo.DestroyObjectImmediate(component);
            Undo.RegisterCompleteObjectUndo(this, "Deleted collider component");
        }

        public void DeleteObject(Component component)
        {
            _componentHistory.Remove(component);
            Undo.DestroyObjectImmediate(component.gameObject);
            Undo.RegisterCompleteObjectUndo(this, "Deleted object");
        }

        public void DeleteAllColliders()
        {
            // Using an int variable because the length of the list changes with every deleted element.
            // Therefore it creates bugs if you delete all colliders at once because the loop will stop before deleting all colliders.
            int timesToRepeat = _componentHistory.Count; //_colliderHistory.Count;
            for (int i = 0; i < timesToRepeat; i++)
            {
                Component component = _componentHistory[i];

                if (!(component is Collider)) continue;
                
                TryDeleteObject(component);
                i--;
            }
        }

        private void OnValidate()
        {
            ValidateCapsuleColliderValues();
            ValidateBoxColliderValues();
            ValidateJointValues();
            ValidateRigidbodyValues();
        }

        private void ValidateRigidbodyValues()
        {
            if (!LastSelectedRigidbody) return;

            LastSelectedRigidbody.mass = rigidbodyMass;
            LastSelectedRigidbody.drag = rigidbodyDrag;
            LastSelectedRigidbody.angularDrag = rigidbodyAngularDrag;
            LastSelectedRigidbody.useGravity = rigidbodyUseGravity;
            LastSelectedRigidbody.isKinematic = rigidbodyIsKinematic;
        }

        private void ValidateJointValues()
        {
            if (!LastSelectedJoint) return;

            jointAxis.Normalize();
            
            var lowAngularXLimit = LastSelectedJoint.lowAngularXLimit;
            var highAngularXLimit = LastSelectedJoint.highAngularXLimit;
            var angularYLimit = LastSelectedJoint.angularYLimit;
            var angularZLimit = LastSelectedJoint.angularZLimit;
            
            lowAngularXLimit.limit = -jointLowXLimit;
            highAngularXLimit.limit = jointHighXLimit;
            angularYLimit.limit = jointYLimit;
            angularZLimit.limit = jointZLimit;
            
            LastSelectedJoint.lowAngularXLimit = lowAngularXLimit;
            LastSelectedJoint.highAngularXLimit = highAngularXLimit;
            LastSelectedJoint.angularYLimit = angularYLimit;
            LastSelectedJoint.angularZLimit = angularZLimit;
            LastSelectedJoint.axis = jointAxis;
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
            if(_componentHistory.Count == 0 && Colliders.Length > 0)
                _componentHistory.AddRange(Colliders);
        }
    }
}


