using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MyGame.Debugging
{
    /// <summary>
    /// 调试辅助工具集。仅在编辑器/开发构建下生效。
    /// </summary>
    public static class DebugUtils
    {
        private static readonly StringBuilder Sb = new StringBuilder(1024);

        /// <summary>
        /// 对象状态快照：把常用属性打成可读文本。
        /// </summary>
        public static string Describe(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return "<null / destroyed>";
            }

            Sb.Clear();
            Sb.Append(obj.GetType().Name);
            Sb.Append(" name=").Append(obj.name);
            Sb.Append(" id=").Append(obj.GetInstanceID());

            if (obj is Transform t)
            {
                Sb.Append(" pos=").Append(t.position.ToString("F2"));
                Sb.Append(" rot=").Append(t.eulerAngles.ToString("F1"));
                Sb.Append(" scale=").Append(t.lossyScale.ToString("F2"));
                Sb.Append(" activeSelf=").Append(t.gameObject.activeSelf);
                Sb.Append(" activeHierarchy=").Append(t.gameObject.activeInHierarchy);
            }

            if (obj is Rigidbody rb)
            {
                Sb.Append(" vel=").Append(rb.velocity.ToString("F2"));
                Sb.Append(" angVel=").Append(rb.angularVelocity.ToString("F2"));
                Sb.Append(" isKinematic=").Append(rb.isKinematic);
                Sb.Append(" sleeping=").Append(rb.IsSleeping());
            }

            if (obj is Renderer r)
            {
                Sb.Append(" enabled=").Append(r.enabled);
                Sb.Append(" mats=").Append(r.sharedMaterials.Length);
                Sb.Append(" sortingOrder=").Append(r.sortingOrder);
            }

            if (obj is Camera cam)
            {
                Sb.Append(" cullingMask=").Append(cam.cullingMask);
                Sb.Append(" clearFlags=").Append(cam.clearFlags);
            }

            return Sb.ToString();
        }

        /// <summary>
        /// 层级树 dump，定位引用与层级问题。
        /// </summary>
        public static void DumpHierarchy(Transform root, int maxDepth = 5)
        {
            if (root == null)
            {
                Debug.LogWarning("[DebugUtils] root 为空");
                return;
            }

            Sb.Clear();
            DumpRecursive(root, 0, maxDepth);
            Debug.Log(Sb.ToString());
        }

        private static void DumpRecursive(Transform node, int depth, int maxDepth)
        {
            if (depth > maxDepth)
            {
                return;
            }

            Sb.Append(new string(' ', depth * 2));
            Sb.Append("- ").Append(node.name);

            var components = node.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Sb.Append(" [Missing Script]");
                }
            }

            Sb.AppendLine();

            for (int i = 0; i < node.childCount; i++)
            {
                DumpRecursive(node.GetChild(i), depth + 1, maxDepth);
            }
        }

        /// <summary>
        /// 列出场景中缺失脚本的 GameObject。
        /// </summary>
        public static void FindMissingScripts()
        {
            var missing = new List<string>();
            var all = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None);

            for (int i = 0; i < all.Length; i++)
            {
                var components = all[i].GetComponents<Component>();
                for (int c = 0; c < components.Length; c++)
                {
                    if (components[c] == null)
                    {
                        missing.Add(GetHierarchyPath(all[i].transform));
                        break;
                    }
                }
            }

            if (missing.Count == 0)
            {
                Debug.Log("[DebugUtils] 未发现丢失脚本。");
            }
            else
            {
                Debug.LogWarning($"[DebugUtils] 发现 {missing.Count} 个丢失脚本:\n{string.Join("\n", missing)}");
            }
        }

        /// <summary>
        /// 获取 GameObject 在场景中的层级路径。
        /// </summary>
        public static string GetHierarchyPath(Transform t)
        {
            if (t == null)
            {
                return "<null>";
            }

            var sb = new StringBuilder(t.name);
            var parent = t.parent;
            while (parent != null)
            {
                sb.Insert(0, parent.name + "/");
                parent = parent.parent;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 在场景中按类型查找所有实例（含未激活）。
        /// </summary>
        public static List<T> FindAll<T>() where T : Component
        {
            var list = new List<T>();
            var all = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            list.AddRange(all);
            return list;
        }
    }

    /// <summary>
    /// 可挂载到任意对象上的状态查看器，右键菜单触发。
    /// </summary>
    public class StateInspector : MonoBehaviour
    {
        [ContextMenu("Debug/Dump Self")]
        private void DumpSelf()
        {
            Debug.Log($"[StateInspector] {DebugUtils.Describe(this)}");
        }

        [ContextMenu("Debug/Dump Hierarchy")]
        private void DumpHierarchy()
        {
            DebugUtils.DumpHierarchy(transform);
        }

        [ContextMenu("Debug/Find Missing Scripts")]
        private static void FindMissing()
        {
            DebugUtils.FindMissingScripts();
        }
    }
}
