using UnityEditor;

[InitializeOnLoad]
internal static class OpenCameraWindowOnce
{
    static OpenCameraWindowOnce()
    {
        EditorApplication.delayCall += () => new NodeGraphViewModel().OpenCameraShotEditor();
    }
}
