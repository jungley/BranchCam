using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class Docker
{
    #region Reflection Types
    private class _EditorWindow
    {
        private EditorWindow instance;
        private Type type;

        public _EditorWindow(EditorWindow instance)
        {
            this.instance = instance;
            type = instance.GetType();
        }

        public object m_Parent
        {
            get
            {
                var field = type.GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
                return field?.GetValue(instance);
            }
        }
    }

    private class _DockArea
    {
        private object instance;
        private Type type;

        public _DockArea(object instance)
        {
            this.instance = instance;
            type = instance.GetType();
        }

        public object window
        {
            get
            {
                var property = type.GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
                return property?.GetValue(instance, null);
            }
        }

        public object s_OriginalDragSource
        {
            set
            {
                var field = type.GetField("s_OriginalDragSource", BindingFlags.Static | BindingFlags.NonPublic);
                field?.SetValue(null, value);
            }
        }
    }

    private class _ContainerWindow
    {
        private object instance;
        private Type type;

        public _ContainerWindow(object instance)
        {
            this.instance = instance;
            type = instance.GetType();
        }

        public object rootSplitView
        {
            get
            {
                var property = type.GetProperty("rootSplitView", BindingFlags.Instance | BindingFlags.Public);
                return property?.GetValue(instance, null);
            }
        }
    }

    private class _SplitView
    {
        private object instance;
        private Type type;

        public _SplitView(object instance)
        {
            this.instance = instance;
            type = instance.GetType();
        }

        public object DragOver(EditorWindow child, Vector2 screenPoint)
        {
            var method = type.GetMethod("DragOver", BindingFlags.Instance | BindingFlags.Public);
            return method?.Invoke(instance, new object[] { child, screenPoint });
        }

        public void PerformDrop(EditorWindow child, object dropInfo, Vector2 screenPoint)
        {
            var method = type.GetMethod("PerformDrop", BindingFlags.Instance | BindingFlags.Public);
            method?.Invoke(instance, new object[] { child, dropInfo, screenPoint });
        }
    }
    #endregion

    public enum DockPosition
    {
        Left,
        Top,
        Right,
        Bottom
    }

    /// <summary>
    /// Docks the second window to the first window at the given position.
    /// Uses internal Unity reflection; may fail silently on unsupported Unity versions.
    /// </summary>
    public static void Dock(this EditorWindow wnd, EditorWindow other, DockPosition position)
    {
        try
        {
            var mousePosition = GetFakeMousePosition(wnd, position);

            var parent = new _EditorWindow(wnd);
            var child = new _EditorWindow(other);

            if (parent.m_Parent == null || child.m_Parent == null)
            {
                Debug.LogWarning("[RydenCam] Docker: Could not access internal Unity window parent. Docking skipped.");
                return;
            }

            var dockArea = new _DockArea(parent.m_Parent);
            if (dockArea.window == null)
            {
                Debug.LogWarning("[RydenCam] Docker: Could not access container window. Docking skipped.");
                return;
            }

            var containerWindow = new _ContainerWindow(dockArea.window);
            if (containerWindow.rootSplitView == null)
            {
                Debug.LogWarning("[RydenCam] Docker: Could not access root split view. Docking skipped.");
                return;
            }

            var splitView = new _SplitView(containerWindow.rootSplitView);
            var dropInfo = splitView.DragOver(other, mousePosition);
            dockArea.s_OriginalDragSource = child.m_Parent;
            splitView.PerformDrop(other, dropInfo, mousePosition);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RydenCam] Docker: Docking failed (likely due to Unity version incompatibility): {e.Message}");
        }
    }

    private static Vector2 GetFakeMousePosition(EditorWindow wnd, DockPosition position)
    {
        Vector2 mousePosition = Vector2.zero;

        switch (position)
        {
            case DockPosition.Left:
                mousePosition = new Vector2(20, wnd.position.size.y / 2);
                break;
            case DockPosition.Top:
                mousePosition = new Vector2(wnd.position.size.x / 2, 20);
                break;
            case DockPosition.Right:
                mousePosition = new Vector2(wnd.position.size.x - 20, wnd.position.size.y / 2);
                break;
            case DockPosition.Bottom:
                mousePosition = new Vector2(wnd.position.size.x / 2, wnd.position.size.y - 20);
                break;
        }

        return new Vector2(wnd.position.x + mousePosition.x, wnd.position.y + mousePosition.y);
    }
}
