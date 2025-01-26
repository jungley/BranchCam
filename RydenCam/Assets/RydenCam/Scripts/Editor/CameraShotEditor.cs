using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;


public class CustomEditorWindowLauncher : EditorWindow
{
    public ScriptViewEditor window1;
    public CameraShotEditor window2;
    [MenuItem("Window/Launch Side by Side")]
    public static void LaunchWindows1()
    {
        var launcher = CreateInstance<CustomEditorWindowLauncher>();
        launcher.minSize = new Vector2(300, 300);
        launcher.maxSize = new Vector2(300, 300);
        //launcher.position = new Rect(1, 1, 1, 1); // Set position and size
        launcher.Show();

        launcher.window1 = EditorWindow.GetWindow<ScriptViewEditor>();
        launcher.window1.titleContent = new GUIContent("Script View");

        launcher.window2 = EditorWindow.GetWindow<CameraShotEditor>();
        launcher.window2.titleContent = new GUIContent("Camera Shot Editor View");
    }

    void OnGUI()
    {
        Docker.Dock(window2, window1, Docker.DockPosition.Right);
        Close();

    }
}


public class ScriptViewEditor : EditorWindow
{
    private string scriptContent = ""; // To hold the text content


    private void OnGUI()
    {
        GUILayout.Label("Script View", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // A large text area for the user to write their script
        scriptContent = EditorGUILayout.TextArea(scriptContent, GUILayout.Height(position.height - 50));
    }
}


public class CameraShotEditor : EditorWindow
{
    private List<string> cameraShots = new List<string>();
    private bool makeShotActorSpectic;
    private string selectedActor;
    private string selectedType;
    private string selectedDistance;
    private string selectedAngle;
    private string shotName;
    private float distanceValue = 1f;

    private Vector2 scrollPos;

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        // First Section
        GUILayout.BeginVertical(GUILayout.Width(position.width / 3));
        GUILayout.Label("Camera Shots", EditorStyles.boldLabel);
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 40));
        for (int i = 0; i < cameraShots.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(cameraShots[i]))
            {
                Debug.Log($"Selected Shot: {cameraShots[i]}");
            }
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                cameraShots.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Second Section
        GUILayout.BeginVertical(GUILayout.Width(position.width / 3));
        makeShotActorSpectic = EditorGUILayout.Toggle("Make Shot Actor Spectic", makeShotActorSpectic);
        selectedActor = EditorGUILayout.TextField("Actor", selectedActor);
        selectedType = EditorGUILayout.TextField("Type", selectedType);
        selectedDistance = EditorGUILayout.TextField("Distance", selectedDistance);
        selectedAngle = EditorGUILayout.TextField("Angle", selectedAngle);
        shotName = EditorGUILayout.TextField("Shot Name", shotName);

        if (GUILayout.Button("Save to QuickList"))
        {
            if (!string.IsNullOrEmpty(shotName))
            {
                cameraShots.Add(shotName);
                shotName = string.Empty; // Clear input field after saving
            }
        }
        GUILayout.EndVertical();

        // Third Section
        GUILayout.BeginVertical(GUILayout.Width(position.width / 3));
        GUILayout.Label("Image Placeholder", EditorStyles.boldLabel);
        // Placeholder for the image texture
        GUILayout.Box(GUIContent.none, GUILayout.Width(100), GUILayout.Height(100));

        distanceValue = EditorGUILayout.Slider("Scale", distanceValue, 1f, 20f);
        GUILayout.Label($"Distance value: {distanceValue}");
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }
}
