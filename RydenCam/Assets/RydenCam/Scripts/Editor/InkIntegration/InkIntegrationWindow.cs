using System;
using RydenCam.Editor.Ribbon;
using System.IO;
using System.Linq;
using RydenCam.BranchCamEditor.Managers;
using UnityEditor;
using UnityEngine;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections.Generic;

namespace RydenCam.Editor.InkIntegration
{
    /// <summary>
    /// Editor UI for the one-way Ink workflow. Parsing and graph construction live
    /// in separate classes; this window handles selection, feedback and applying results.
    /// </summary>
    public sealed class InkIntegrationWindow : EditorWindow
    {
        [SerializeField] UnityEngine.Object source;
        [SerializeField] string entry = "";
        [SerializeField] string report = "Choose an .ink source file to generate a BranchCam conversation.";
        [SerializeField] bool failed;
        Vector2 scroll;
        RibbonRenderer ribbonRenderer;
        [SerializeField] bool documentInitialized;
        [SerializeField] string editingPath = "";
        [SerializeField] string sourceText = "";
        [SerializeField] string savedText = "";
        Vector2 editorScroll;
        GUIStyle inputStyle;
        GUIStyle highlightStyle;
        Font editorFont;
        string highlightedSource;
        string highlightedText;

        bool SourceHasEdits => sourceText != savedText;
        // One in-memory undo step. This intentionally does not write or modify Ink.
        List<Node> previousGraph;
        string previousSavePath;

        void OnEnable()
        {
            // Recreate transient styles after a script reload; their font was released.
            inputStyle = null;
            highlightStyle = null;
            highlightedSource = null;
            titleContent = new GUIContent("Ink Integration");
            minSize = new Vector2(320, 300);
            InitializeRibbon();
            UpdateUnsavedState();
        }

        void OnDisable()
        {
            if (editorFont != null) DestroyImmediate(editorFont);
            inputStyle = null;
            highlightStyle = null;
        }

        void OnGUI()
        {
            var start = NodeManager.Instance.StartNode;
            // The asset GUID survives file moves within the Unity project.
            if (!documentInitialized && source == null && !string.IsNullOrEmpty(start?.InkSourceGuid))
            {
                source = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(start.InkSourceGuid));
                entry = start.InkEntryKnot;
            }
            documentInitialized = true;
            ribbonRenderer.Draw(position.width);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUILayout.Label("Ink Integration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Write dialogue in Ink, then import or refresh the graph. Assign actor targets and cinematic shots in BranchCam.", MessageType.Info);
            var selectedSource = EditorGUILayout.ObjectField("Ink Source", source, typeof(UnityEngine.Object), false);
            if (selectedSource != source && selectedSource != null)
                SelectSource(AssetDatabase.GetAssetPath(selectedSource));
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Blacksmith Sample", EditorStyles.miniButton, GUILayout.Width(125)))
                    SelectSource("Assets/RydenCam/DialogueFiles/BlacksmithConversation.ink");
            }
            entry = EditorGUILayout.TextField(new GUIContent("Entry Knot", "Leave blank to use root content, or the first knot when there is no root content."), entry);
            string path = AssetDatabase.GetAssetPath(source);
            bool valid = path.EndsWith(".ink", StringComparison.OrdinalIgnoreCase) && File.Exists(path);
            if (valid && editingPath != path) LoadSource(path);
            using (new EditorGUI.DisabledScope(!valid && string.IsNullOrWhiteSpace(sourceText)))
            {
                bool refresh = valid && start?.InkSourceGuid == AssetDatabase.AssetPathToGUID(path);
                string importLabel = refresh ? "Refresh From Ink" : "Import Ink";
                if (GUILayout.Button(SourceHasEdits || !valid ? "Save & " + importLabel : importLabel, GUILayout.Height(30)))
                {
                    if ((valid && !SourceHasEdits) || SaveSource()) Import(editingPath, refresh);
                }
            }
            DrawSourceEditor();
            EditorGUILayout.HelpBox(report, failed ? MessageType.Error : MessageType.Info);
            
            
            if (previousGraph != null && GUILayout.Button("Undo Last Import"))
            {
                UndoLastImport();
            }
            DrawSpeakerTargets(start);
            GUILayout.Space(10);
            GUILayout.Label("Supported Ink", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Dialogue • Knots • Choices • Diverts • END\nTags: actor, target, shot, id\nVariables, conditions, loops, functions and tunnels are rejected.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }

        private void InitializeRibbon()
        {
            var definition = new RibbonDefinitionBuilder()
                .AddDropdown("File")
                .AddDropdownOption("File", "New", NewSource)
                .AddDropdownOption("File", "Open", OpenSource)
                .AddDropdownOption("File", "Save", () => SaveSource())
                .AddDropdownOption("File", "Save As", () => SaveSourceAs())
                .AddButton("Open", OpenSource)
                .AddButton("Save", () => SaveSource())
                .Build();
            ribbonRenderer = new RibbonRenderer(definition);
        }

        private void NewSource()
        {
            if (!ConfirmSourceChange()) return;
            // Keep the untitled document independent of the graph's linked Ink file.
            documentInitialized = true;
            source = null;
            editingPath = "";
            sourceText = "";
            savedText = "";
            entry = "";
            failed = false;
            report = "Write an Ink script, then save and import it into BranchCam.";
            UpdateUnsavedState();
            GUI.FocusControl(null);
        }

        private void OpenSource()
        {
            string path = EditorUtility.OpenFilePanel("Open Ink Script", "Assets", "ink");
            if (!string.IsNullOrEmpty(path)) SelectSource(path);
        }

        private void SelectSource(string path)
        {
            // Project assets provide stable GUIDs for the graph's source reference.
            string assetPath = FileUtil.GetProjectRelativePath(Path.GetFullPath(path));
            if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
                !assetPath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase))
            {
                failed = true;
                report = "Choose an .ink file inside this project's Assets folder.";
                return;
            }
            if (!ConfirmSourceChange()) return;
            LoadSource(assetPath);
            entry = "";
        }

        private bool SaveSourceAs()
        {
            string directory = string.IsNullOrEmpty(editingPath) ? "Assets" : Path.GetDirectoryName(editingPath);
            string name = string.IsNullOrEmpty(editingPath) ? "NewConversation" : Path.GetFileNameWithoutExtension(editingPath);
            string path = EditorUtility.SaveFilePanelInProject("Save Ink Script As", name, "ink",
                "Choose a location for the Ink script.", directory);
            if (string.IsNullOrEmpty(path)) return false;
            return path == editingPath ? SaveSource() : WriteSource(path);
        }

        private void DrawSourceEditor()
        {
            GUILayout.Space(8);
            string name = string.IsNullOrEmpty(editingPath) ? "Untitled.ink" : Path.GetFileName(editingPath);
            GUILayout.Label(name + (SourceHasEdits ? " *" : ""), EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(editingPath)))
                {
                    if (GUILayout.Button("Reload From Disk") && (!SourceHasEdits ||
                        EditorUtility.DisplayDialog("Reload Ink", "Discard your unsaved script edits?", "Reload", "Cancel")))
                        LoadSource(editingPath);
                    if (GUILayout.Button("Open in External Editor")) AssetDatabase.OpenAsset(source);
                }
            }

            EnsureEditorStyles();

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown && (currentEvent.control || currentEvent.command) && currentEvent.keyCode == KeyCode.S)
            {
                if (currentEvent.shift) SaveSourceAs();
                else SaveSource();
                currentEvent.Use();
            }
            editorScroll = EditorGUILayout.BeginScrollView(editorScroll, GUILayout.Height(Mathf.Max(230, position.height * 0.45f)));
            Vector2 textSize = inputStyle.CalcSize(new GUIContent(sourceText + "\n "));
            Rect textRect = GUILayoutUtility.GetRect(Mathf.Max(position.width - 50, textSize.x + 24), Mathf.Max(220, textSize.y + 20));
            GUI.SetNextControlName("InkSourceEditor");
            string editedText = GUI.TextArea(textRect, sourceText, inputStyle);
            if (editedText != sourceText)
            {
                sourceText = editedText;
                UpdateUnsavedState();
            }
            if (highlightedSource != sourceText)
            {
                highlightedSource = sourceText;
                highlightedText = InkSyntaxHighlighter.Highlight(sourceText, EditorGUIUtility.isProSkin);
            }
            if (Event.current.type == EventType.Repaint)
            {
                highlightStyle.Draw(textRect, new GUIContent(highlightedText), false, false, false, false);
                DrawSourceCaret(textRect);
            }
            EditorGUILayout.EndScrollView();
        }

        private void EnsureEditorStyles()
        {
            if (inputStyle == null)
            {
                editorFont = Font.CreateDynamicFontFromOSFont("Consolas", 13);
                inputStyle = new GUIStyle(EditorStyles.textArea)
                {
                    font = editorFont, fontSize = 13, wordWrap = false, richText = false
                };
                highlightStyle = new GUIStyle(inputStyle) { richText = true };
                // The native text area owns selection and keyboard editing.
                // A matching rich-text layer paints syntax without putting markup in the file.
                foreach (var state in new[] { inputStyle.normal, inputStyle.hover, inputStyle.active, inputStyle.focused })
                    state.textColor = Color.clear;
                foreach (var state in new[] { highlightStyle.normal, highlightStyle.hover, highlightStyle.active, highlightStyle.focused })
                {
                    state.background = null;
                    state.textColor = EditorGUIUtility.isProSkin ? new Color(0.88f, 0.88f, 0.88f) : new Color(0.12f, 0.12f, 0.12f);
                }
            }

        }

        private void DrawSourceCaret(Rect textRect)
        {
            if (focusedWindow != this || GUI.GetNameOfFocusedControl() != "InkSourceEditor") return;

            var textEditor = (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor), GUIUtility.keyboardControl);
            if (textEditor.cursorIndex != textEditor.selectIndex) return;

            // Hiding the native glyphs for syntax highlighting also hides its caret.
            // Draw an opaque caret above the colored text, using the native insertion
            // index and the same plain-text metrics so tags never affect its position.
            Vector2 cursorPosition = inputStyle.GetCursorPixelPosition(textRect,
                new GUIContent(sourceText), Mathf.Clamp(textEditor.cursorIndex, 0, sourceText.Length));
            cursorPosition -= textEditor.scrollOffset;
            var caretRect = new Rect(cursorPosition.x, cursorPosition.y, 2, inputStyle.lineHeight);
            EditorGUI.DrawRect(caretRect, EditorGUIUtility.isProSkin ? Color.white : Color.black);
        }

        private void LoadSource(string path)
        {
            try
            {
                sourceText = File.ReadAllText(path);
                savedText = sourceText;
                editingPath = path;
                source = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                documentInitialized = true;
                highlightedSource = null;
                GUI.FocusControl(null);
                UpdateUnsavedState();
            }
            catch (Exception exception)
            {
                failed = true;
                report = exception.Message;
            }
        }

        private bool SaveSource()
        {
            if (string.IsNullOrEmpty(editingPath)) return SaveSourceAs();
            try
            {
                // Do not silently overwrite edits made in Inky or another editor.
                if ((!File.Exists(editingPath) || File.ReadAllText(editingPath) != savedText) &&
                    !EditorUtility.DisplayDialog("Ink changed on disk", "The source file changed outside this editor. Replace it with your edits?", "Replace", "Cancel"))
                    return false;
                return WriteSource(editingPath);
            }
            catch (Exception exception)
            {
                failed = true;
                report = exception.Message;
                return false;
            }
        }

        private bool WriteSource(string path)
        {
            try
            {
                File.WriteAllText(path, sourceText);
                AssetDatabase.ImportAsset(path);
                // Only adopt the new file after saving succeeds.
                editingPath = path;
                source = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                savedText = sourceText;
                UpdateUnsavedState();
                failed = false;
                report = "Saved Ink script. Refresh From Ink to update the graph.";
                return true;
            }
            catch (Exception exception)
            {
                failed = true;
                report = exception.Message;
                return false;
            }
        }

        private bool ConfirmSourceChange()
        {
            if (!SourceHasEdits) return true;
            int choice = EditorUtility.DisplayDialogComplex("Unsaved Ink edits", "Save your script before switching files?", "Save", "Cancel", "Discard");
            if (choice == 0) return SaveSource();
            if (choice == 2)
            {
                sourceText = savedText;
                UpdateUnsavedState();
                return true;
            }
            return false;
        }

        private void UpdateUnsavedState()
        {
            hasUnsavedChanges = SourceHasEdits;
            saveChangesMessage = "Save the edited Ink script before closing?";
        }

        public override void SaveChanges()
        {
            if (SaveSource()) base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            sourceText = savedText;
            base.DiscardChanges();
        }

        private void DrawSpeakerTargets(StartNode start)
        {


            if (start != null && !string.IsNullOrEmpty(start.InkSourceGuid))
            {
                GUILayout.Space(10);
                GUILayout.Label("Speaker Targets", EditorStyles.boldLabel);
                foreach (var actor in start.ActorsInScene.Where(a => !string.IsNullOrEmpty(a.InkSpeakerName)))
                {
                    var target = (GameObject)EditorGUILayout.ObjectField(actor.InkSpeakerName, actor.ActorGO, typeof(GameObject), true);
                    if (target != actor.ActorGO)
                    {
                        actor.ActorGO = target;
                        RepaintPreviews();
                    }
                }
                EditorGUILayout.HelpBox("Save the BranchCam scene to keep its Ink source, speaker targets and camera settings. Refresh replaces narrative nodes and connections. Matching nodes retain camera settings and positions; # id: tags keep matches stable when restructuring Ink.", MessageType.None);
            }
        }

        private void UndoLastImport()
        {
            BranchCamGraphBuilder.Apply(previousGraph);
            FilePathSaveManager.Instance.SetLastFilePath(previousSavePath, FilePathSaveManager.LastOpened_NodeGraphKey);
            previousGraph = null;
            report = "Restored the graph from before the last import.";
            NodeGraphEditorWindow.Instance?.FrameImportedGraph();
            RepaintPreviews();
        }

        void Import(string path, bool refresh)
        {
            try
            {
                // Finish parsing and building before changing the current graph.
                // Unsupported Ink or an unknown camera tag therefore leaves it intact.
                var model = new InkBranchCamImporter().Import(File.ReadAllText(path), Path.GetFullPath(path), entry);
                var nodes = BranchCamGraphBuilder.Build(model, AssetDatabase.AssetPathToGUID(path), Path.GetFileNameWithoutExtension(path), NodeManager.Instance.Nodes.ToList());
                
                if (!refresh && NodeManager.Instance.Nodes.Count > 0 && !EditorUtility.DisplayDialog("Import Ink",
                    "Replace the current conversation graph? Save it first if you want to keep it. The Ink source file is unchanged.", "Replace Graph", "Cancel")) return;
                previousGraph = NodeManager.Instance.Nodes.ToList();
                previousSavePath = FilePathSaveManager.Instance.GetLastFilePathSaved(FilePathSaveManager.LastOpened_NodeGraphKey);
                BranchCamGraphBuilder.Apply(nodes);
                // A new import must not overwrite the unrelated scene it replaced
                // when the user next clicks Save. Refresh keeps the current save path.
                if (!refresh) FilePathSaveManager.Instance.ClearLastFilePath(FilePathSaveManager.LastOpened_NodeGraphKey);
                entry = model.Entry;
                failed = false;
                report = $"Imported {model.Nodes.Count} nodes and {model.Nodes.Values.Sum(n => n.Choices.Count)} choices. Unconnected outputs end the conversation.";
                if (model.Warnings.Count > 0) report += "\n" + string.Join("\n", model.Warnings.Distinct());
                
                NodeGraphEditorWindow.Instance?.FrameImportedGraph();
                
                RepaintPreviews();
                Repaint();
            }
            catch (Exception e)
            {
                failed = true;
                report = e.Message;
            }
        }

        static void RepaintPreviews()
        {
            NodeGraphEditorWindow.Instance?.Repaint();
            foreach (var window in Resources.FindObjectsOfTypeAll<CameraShotEditor>()) window.Repaint();
        }
    }
}
