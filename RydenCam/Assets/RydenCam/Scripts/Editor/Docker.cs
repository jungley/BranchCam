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
            type = typeof(EditorWindow);
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

        public bool PerformDrop(EditorWindow child, object dropInfo, Vector2 screenPoint)
        {
            var method = type.GetMethod("PerformDrop", BindingFlags.Instance | BindingFlags.Public);
            return method?.Invoke(instance, new object[] { child, dropInfo, screenPoint }) is bool success && success;
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
    public static bool Dock(this EditorWindow wnd, EditorWindow other, DockPosition position)
    {
        try
        {
            var mousePosition = GetFakeMousePosition(wnd, position);

            var parent = new _EditorWindow(wnd);
            var child = new _EditorWindow(other);

            if (parent.m_Parent == null || child.m_Parent == null)
            {
                Debug.LogWarning("[BranchCam] Docker: Could not access internal Unity window parent. Docking skipped.");
                return false;
            }

            var dockArea = new _DockArea(parent.m_Parent);
            if (dockArea.window == null)
            {
                Debug.LogWarning("[BranchCam] Docker: Could not access container window. Docking skipped.");
                return false;
            }

            var containerWindow = new _ContainerWindow(dockArea.window);
            if (containerWindow.rootSplitView == null)
            {
                Debug.LogWarning("[BranchCam] Docker: Could not access root split view. Docking skipped.");
                return false;
            }

            // Use the graph's immediate split: the root skips nested split views.
            var splitParent = parent.m_Parent.GetType().GetProperty("parent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(parent.m_Parent);
            var splitView = new _SplitView(splitParent ?? containerWindow.rootSplitView);
            var dropInfo = splitView.DragOver(other, mousePosition);
            if (dropInfo == null) return false;
            dockArea.s_OriginalDragSource = child.m_Parent;
            try
            {
                return splitView.PerformDrop(other, dropInfo, mousePosition);
            }
            finally
            {
                dockArea.s_OriginalDragSource = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[BranchCam] Docker: Docking failed (likely due to Unity version incompatibility): {e.Message}");
            return false;
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
                // The drop location selects the edge; Unity sizes the resulting pane.
                mousePosition = new Vector2(wnd.position.size.x / 2, wnd.position.size.y - 20f);
                break;
        }

        return new Vector2(wnd.position.x + mousePosition.x, wnd.position.y + mousePosition.y);
    }
}
