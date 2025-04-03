using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace VNEngine.EditorTools
{
    public class NodeUtility : EditorWindow
    {
        [MenuItem("VNEngine/Mark All VNEngine Nodes Dirty")]
        public static void MarkAllNodesDirty()
        {
            int count = 0;

            var ifNodes = GameObject.FindObjectsOfType<IfNode>();
            var alterStatNodes = GameObject.FindObjectsOfType<AlterStatNode>();
            var dialogueNodes = GameObject.FindObjectsOfType<DialogueNode>();

            foreach (var node in ifNodes)
            {
                EditorUtility.SetDirty(node);
                EditorSceneManager.MarkSceneDirty(node.gameObject.scene);
                count++;
            }

            foreach (var node in alterStatNodes)
            {
                EditorUtility.SetDirty(node);
                EditorSceneManager.MarkSceneDirty(node.gameObject.scene);
                count++;
            }

            foreach (var node in dialogueNodes)
            {
                EditorUtility.SetDirty(node);
                EditorSceneManager.MarkSceneDirty(node.gameObject.scene);
                count++;
            }

            Debug.Log($"Marked {count} VNEngine node(s) as dirty.");
        }
    }
}