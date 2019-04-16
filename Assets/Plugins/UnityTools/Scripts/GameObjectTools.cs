﻿using System.Collections.Generic;
using UnityEngine;

namespace UnityTools
{
    public static class GameObjectTools
    {
        static readonly List<Component> componentBuffer;

        static GameObjectTools()
        {
            componentBuffer = new List<Component>();
        }

        public static void AddChild(this Transform parent, Transform to)
        {
            to.parent = parent;
        }

        public static List<T> GetComponentNonAlloc<T>(this Component c, List<T> buffer)
        {
            return c.gameObject.GetComponentNonAlloc<T>(buffer);
        }

        public static List<T> GetComponentNonAlloc<T>(this GameObject go, List<T> buffer)
        {
            componentBuffer.Clear();
            go.GetComponents(typeof(T), componentBuffer);
            if (buffer != null)
            {
                buffer.Clear();
                buffer.Capacity = Mathf.Max(buffer.Capacity, componentBuffer.Count);
            }
            else
            {
                buffer = new List<T>(componentBuffer.Count);
            }

            foreach (var c in componentBuffer)
            {
                if (c is T t)
                {
                    buffer.Add(t);
                }
            }

            return buffer;
        }


        public static List<T> GetComponentsInChildrenNonAlloc<T>(this Component c, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            return c.gameObject.GetComponentsInChildrenNonAlloc<T>(buffer, includeInactiveComponents);
        }

        public static List<T> GetComponentsInChildrenNonAlloc<T>(this GameObject go, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            if (buffer == null)
            {
                buffer = new List<T>();
            }
            else
            {
                buffer.Clear();
            }

            GetComponentsInChildrenInternal(go, buffer, includeInactiveComponents, includeInactiveGameObject);
            return buffer;
        }

        static void GetComponentsInChildrenInternal<T>(GameObject go, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            ProcessGameObject(go, buffer, includeInactiveComponents);

            var transform = go.transform;
            var childCount = transform.childCount;
            for (var c = 0; c < childCount; c += 1)
            {
                var cgo = transform.GetChild(c).gameObject;
                if (includeInactiveGameObject || cgo.activeSelf)
                {
                    GetComponentsInChildrenInternal<T>(cgo, buffer, includeInactiveComponents, includeInactiveGameObject);
                }
            }
        }

        public static List<T> GetComponentsInParentsNonAlloc<T>(this Component c, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            return c.gameObject.GetComponentsInParentsNonAlloc<T>(buffer, includeInactiveComponents);
        }

        public static List<T> GetComponentsInParentsNonAlloc<T>(this GameObject go,
                                                                List<T> buffer,
                                                                bool includeInactiveComponents = false,
                                                                bool includeInactiveGameObject = false)
        {
            if (buffer == null)
            {
                buffer = new List<T>();
            }
            else
            {
                buffer.Clear();
            }

            GetComponentsInParentsInternal(go, buffer, includeInactiveComponents, includeInactiveGameObject);
            return buffer;
        }

        static void GetComponentsInParentsInternal<T>(GameObject go,
                                                      List<T> buffer,
                                                      bool includeInactiveComponents = false,
                                                      bool includeInactiveGameObject = false)
        {
            ProcessGameObject(go, buffer, includeInactiveComponents);

            var parentTransform = go.transform.parent;
            while (parentTransform != null)
            {
                var cgo = parentTransform.gameObject;
                if (includeInactiveGameObject == false && !cgo.activeSelf)
                {
                    return;
                }

                ProcessGameObject(cgo, buffer, includeInactiveComponents);
                parentTransform = cgo.transform.parent;
            }
        }

        public static void ProcessGameObject<T>(GameObject go, List<T> buffer, bool includeInactive)
        {
            componentBuffer.Clear();
            try
            {
                go.GetComponents(typeof(T), componentBuffer);
                foreach (var c in componentBuffer)
                {
                    if (c is T t && ShouldInclude(c, includeInactive))
                    {
                        buffer.Add(t);
                    }
                }
            }
            finally
            {
                componentBuffer.Clear();
            }
        }

        static bool ShouldInclude(Component c, bool includeInactive)
        {
            if (includeInactive) return true;
            if (c is MonoBehaviour m)
            {
                return m.enabled;
            }

            return true;
        }

        public static bool IsMatched(this LayerMask m, GameObject g)
        {
            var gl = 1 << g.layer;
            return (m.value & gl) != 0;
        }

        public static bool IsEqual(this RectInt r, RectInt other)
        {
            return r.x == other.x && r.y == other.y && r.width == other.width && r.height == other.height;
        }

        public static string GetPath(this GameObject go)
        {
            return GetPath(go.transform);
        }

        public static string GetPath(this Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        public static void DrawCone(Transform transform, float totalFOV = 70, float rayRange = 10)
        {
            DrawCone(transform.position, transform.forward, totalFOV, rayRange);
        }

        public static void DrawCone(Vector3 position, Vector3 forward, float totalFOV = 70, float rayRange = 10)
        {
            var halfFOV = totalFOV / 2.0f;
            var baseRotation = Quaternion.LookRotation(forward, Vector3.up);
            var leftRayRotation = baseRotation * Quaternion.Euler(-halfFOV, 0, 0);
            var rightRayRotation = baseRotation * Quaternion.Euler(halfFOV, 0, 0);
            var topRayRotation = baseRotation * Quaternion.Euler(0, -halfFOV, 0);
            var bottomRayRotation = baseRotation * Quaternion.Euler(0, halfFOV, 0);

            var leftRayDirection = leftRayRotation * Vector3.forward;
            var rightRayDirection = rightRayRotation * Vector3.forward;
            var topRayDirection = topRayRotation * Vector3.forward;
            var bottomRayDirection = bottomRayRotation * Vector3.forward;
            Gizmos.DrawRay(position, leftRayDirection * rayRange);
            Gizmos.DrawRay(position, rightRayDirection * rayRange);
            Gizmos.DrawRay(position, topRayDirection * rayRange);
            Gizmos.DrawRay(position, bottomRayDirection * rayRange);
            Gizmos.DrawRay(position, forward * rayRange);

            var cosHalf = Mathf.Cos(totalFOV * Mathf.Deg2Rad / 2);
            var cos = Mathf.Sin(totalFOV * Mathf.Deg2Rad / 2);

            DrawWireArc(position, baseRotation * Vector3.up, topRayDirection * rayRange, totalFOV, rayRange);
            DrawWireArc(position, baseRotation * Vector3.left, rightRayDirection * rayRange, totalFOV, rayRange);
            DrawWireDisc(position + forward * rayRange * cosHalf, forward, rayRange * cos);
            DrawWireDisc(position + forward * rayRange * cosHalf / 2, forward, rayRange * cos / 2);
        }

        public static void DrawWireArcSimple(Vector3 position, float radius, Vector3 rayA, Vector3 rayB)
        {
            var rayAN = rayA.normalized;
            var rayBN = rayB.normalized;
            var up = Vector3.Cross(rayAN, rayBN);
            var dot = Vector3.Dot(rayAN, rayBN);
            var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            DrawWireArc(position, up, rayA, angle, radius);
        }

        public static void DrawWireArc(Vector3 position, Vector3 up, Vector3 from, float angle, float radius)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = Gizmos.color;
            UnityEditor.Handles.DrawWireArc(position, up, from, angle, radius);
#endif
        }

        public static void DrawWireDisc(Vector3 position, Vector3 normal, float radius)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = Gizmos.color;
            UnityEditor.Handles.DrawWireDisc(position, normal, radius);
#endif
        }
    }
}