﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory.States
{
    public abstract class RFComponentState
    {
        protected RagdollFactory Context { get; set; }
        public List<Component> ComponentList { get; protected set; }
        public Component SelectedComponent { get; protected set; }
        public Component HighlightedComponent { get; protected set; }

        public bool IsSelected(Component component)
        {
            return SelectedComponent == component;
        }

        public bool HasComponentSelected => SelectedComponent != null;
        
        public RFComponentState(RagdollFactory context)
        {
            Context = context;
        }

        #region Actions
        public abstract void Create();
        
        /// <summary>
        /// Deletes the selected component and removes it from the history.
        /// </summary>
        public virtual void Delete()
        {
            ComponentList.Remove(SelectedComponent);
            Undo.DestroyObjectImmediate(SelectedComponent);
        }
        
        /// <summary>
        /// Selects an element and then deletes it.
        /// </summary>
        /// <param name="component"></param>
        public void Delete(Component component)
        {
            Select(component);
            Delete();
        }

        public void DeleteAll()
        {
            while (ComponentList.Count > 0)
                Delete(ComponentList[0]);
        }
        
        /// <summary>
        /// Deletes the game object from the selected component and removes it from the history.
        /// </summary>
        public virtual void DeleteSelectedGameObject()
        {
            ComponentList.Remove(SelectedComponent);
            try
            {
                Undo.DestroyObjectImmediate(SelectedComponent.gameObject);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogWarning("UNABLE TO DELETE COLLIDER, DELETING COMPONENT INSTEAD (Are you trying to delete a collider attached to a model's bone?)\nERROR: " + e.Message);
                Undo.DestroyObjectImmediate(SelectedComponent);
            }
        }
        
        /// <summary>
        /// Selects the component, if it isn't on the history, add it.
        /// </summary>
        /// <param name="component"></param>
        public virtual void Select(Component component)
        {
            if(!ComponentList.Contains(component))
                ComponentList.Add(component);
            
            SelectedComponent = component;
            EditorGUIUtility.PingObject(SelectedComponent);
        }

        public virtual void Deselect()
        {
            SelectedComponent = null;
        }
        
        public abstract void ConvertTo(Component component);
        #endregion
        
        public abstract void Update();

        public abstract void DrawSceneGUI();
        
        protected T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            if(target.TryGetComponent(out T component)) 
                return component;
            
            T comp = target.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(comp, "Created component " + comp.name);
            return comp;
        }
    }
}
#endif