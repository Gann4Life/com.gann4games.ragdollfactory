using UnityEditor;
using UnityEngine;

namespace Gann4Games.CollisionBuilder
{
    public partial class CollisionBuilder
    {
        [MenuItem("GameObject/MrGann/Collision Builder", false, priority = 1)]
        private static void CreateCollisionBuilder(MenuCommand menuCommand)
        {
            GameObject obj = Selection.activeObject as GameObject;
            obj.AddComponent<CollisionBuilder>();
            Undo.RegisterCreatedObjectUndo(obj, "Created Collision Builder");
        }

        [MenuItem("GameObject/MrGann/Collision Builder", true, priority = 1)]
        private static bool CreateCollisionBuilderValidation(MenuCommand menuCommand)
        {
            return Selection.activeObject is GameObject &&
                   !((GameObject)Selection.activeObject).GetComponent<CollisionBuilder>();
        }
    }
}