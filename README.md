# BranchCam
The official BranchCam Repo

BranchCam is an experimental Unity editor-tool prototype for creating branching narrative sequences. This is not at all a finished or product or a ready unity package.

This project has been in development off and on several years and has been an avenue for me to improve my development skills and experiment with with Unity editor tooling and systems that I don't necessarily get an opportunity to work with in my day-to-day development work. 

Some features are incomplete and some systems have been redesigned multiple times as I learned more about a feature and how  to best change it and improve it.

## Ink integration

Write branching dialogue in [inkle's Ink](https://github.com/inkle/ink), import it into BranchCam, then assign actors and preview cinematic shots on the generated conversation graph.

1. Open **Window → BranchCam**, then click **Ink Integration** in the ribbon. The panel docks to the right of the graph; Shot Configuration can remain below it.
2. Choose an `.ink` asset, or click **Select Blacksmith Sample**. Leave Entry Knot blank for root content (or the first knot when there is no root content).
3. Click **Import Ink**. Import validates and builds the complete supported graph before replacing the current conversation. Save any unrelated graph first.
4. In **Speaker Targets**, assign scene camera-focus GameObjects to the imported speakers. Select nodes to choose Portrait, Over Shoulder, Frame Share or another shot from the scene's shot file.
5. Save the BranchCam scene. Use its normal runtime conversation playback. Outputs without a connection end the conversation.
6. Edit the `.ink` source directly in the panel's **Ink Script** editor, with syntax highlighting for speakers, knots, choices, diverts, tags and comments. **Save Script** (Ctrl+S / Cmd+S) writes the source; **Save & Refresh From Ink** also rebuilds the graph. **Reload From Disk** picks up external edits. Switching files or closing the panel prompts for unsaved edits, and saving detects external file changes before overwriting them.
7. Imported dialogue and choices are read-only on graph nodes. Refresh rebuilds narrative connections while preserving matching node IDs, positions, speaker bindings and manually selected cameras. Explicit camera tags take precedence. The embedded editor does not change which Ink constructs the importer supports.

The sample is `RydenCam/Assets/RydenCam/DialogueFiles/BlacksmithConversation.ink`: 13 dialogue lines, two decisions, four choices, and three routes to a shared farewell.

### Supported subset and metadata

The importer supports static dialogue, knots, choices (including nested choices), gathers, static diverts, `END`, and tags. It rejects variables, conditions, functions, tunnels, threads, stitches, invisible default choices, dynamic text and loops. Imports are bounded to 1,000 traversal steps and eight seconds. This is a one-way authoring workflow, not a general visualization of arbitrary Ink programs.

```ink
Blacksmith: Safe travels, stranger. # id:farewell # actor:Blacksmith # target:Player # shot:frame_share
-> END
```

Speaker prefixes supply the actor name; `actor:` overrides the prefix. `target:` sets the opposite actor. `shot:` accepts a shot name (spaces, underscores and hyphens are equivalent) or its ID. Unknown shot names stop the import so camera metadata is not silently discarded. These camera tags apply to dialogue lines; choice camera tags produce a diagnostic.

Use a unique `id:` tag on dialogue lines to preserve cinematic work when adding or rearranging source content. Without an explicit ID, matching uses Ink's runtime content paths, which can change after structural edits. Removed or unmatched nodes are rebuilt; manual narrative edits and added graph nodes are not retained on refresh.

### Implementation and verification

`InkBranchCamImporter` uses the bundled official Ink parser/compiler to validate its parsed hierarchy, then explores runtime choice snapshots into an import model. `BranchCamGraphBuilder` converts that model into existing BranchCam nodes and connections. This follows Ink's [parsed-hierarchy/runtime distinction](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md).

Run **Tools → BranchCam → Run Ink Import Checks** in Unity. The checks cover sample branches and convergence, endings, tags, refresh preservation, runtime traversal and rejection of unsupported or invalid stories. Results are written to `Temp/branchcam-ink-checks.txt` and the Unity Console.

