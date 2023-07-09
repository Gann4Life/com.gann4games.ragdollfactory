using UnityEditor;
using UnityEngine;

namespace Gann4Games.RagdollFactory
{
    public partial class RagdollFactory
    {
        [MenuItem("GameObject/MrGann/Ragdoll Factory", false, priority = 1)]
        private static void CreateCollisionBuilder(MenuCommand menuCommand)
        {
            GameObject obj = Selection.activeObject as GameObject;
            obj.AddComponent<RagdollFactory>();
            Undo.RegisterCreatedObjectUndo(obj, "Created Collision Builder");
        }

        [MenuItem("GameObject/MrGann/Ragdoll Factory", true, priority = 1)]
        private static bool CreateCollisionBuilderValidation(MenuCommand menuCommand)
        {
            return Selection.activeObject is GameObject &&
                   !((GameObject)Selection.activeObject).GetComponent<RagdollFactory>();
        }
    }
}