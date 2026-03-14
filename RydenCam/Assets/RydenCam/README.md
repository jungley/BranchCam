# RydenCam (BranchCam) — Branching Dialogue & Camera System for Unity

RydenCam is a visual node-graph editor and runtime system for authoring branching dialogue sequences with automatic cinematic camera placement. It is designed for RPG-style conversations where characters talk, the camera dynamically frames each shot, and the player can make decisions that branch the story.

## Features

- **Visual Node Graph Editor** — Create dialogue sequences by connecting Start, Dialogue, Decision, and Action nodes in an intuitive graph window.
- **Automatic Camera Shot Composition** — Configure camera shots (Portrait, Over-the-Shoulder, Frame Share, Custom) with distance and angle presets. The camera positions itself automatically based on actor positions.
- **Camera Shot Editor** — A dedicated editor window for managing and previewing camera shot configurations.
- **Decision Branching** — Present the player with choices that redirect the conversation flow to different paths.
- **Action Nodes** — Trigger arbitrary methods on GameObjects as part of the dialogue sequence.
- **Multi-Actor Support** — Define multiple actors in a scene with automatic preview placement.
- **UI Toolkit Integration** — Runtime dialogue UI built with Unity's UI Toolkit for modern, styleable interfaces.
- **Save/Load** — Dialogue graphs and camera shot configurations are saved as JSON files and can be loaded at runtime.

## Requirements

- **Unity 2021.3 LTS** or newer (UI Toolkit features require 2021+)
- **Cinemachine** package (for virtual camera control)
- **Newtonsoft.Json** (included in Packages)

## Getting Started

### 1. Import the Package

Import the RydenCam folder into your Unity project's `Assets/` directory.

### 2. Open the Editor

Go to **BranchCam > Launch Editor** in the Unity menu bar. This opens the node graph editor and the camera shot editor.

### 3. Create a Conversation

1. **Right-click** in the graph to add a **Start Node**.
2. In the Start Node inspector, click **Add Actor** and assign your scene GameObjects.
3. **Right-click** again to add **Dialogue Nodes**, **Decision Nodes**, or **Action Nodes**.
4. **Connect nodes** by clicking an output point and then clicking an input point on another node.
5. Type dialogue text directly in the node or the inspector panel.
6. Use **File > Save** or **File > Save As** to save your dialogue graph as a JSON file.

### 4. Configure Camera Shots

1. Click **Shot Configuration** in the editor ribbon to open the Camera Shot Editor.
2. Add new shots, set the shot type (Portrait, Over-the-Shoulder, Frame Share, Custom), and adjust distance/angle.
3. Save your camera shot configuration via the Camera Shot Editor's ribbon.

### 5. Set Up the Runtime

1. Add a **DialoguePlayer** component to a GameObject in your scene.
2. Assign the **Dialogue Camera** (a GameObject with a CinemachineVirtualCamera) and the **Camera Brain**.
3. Click **Choose Dialogue Folder** in the Inspector to select which dialogue JSON file to load.
4. Add a **ButtonManager** component to a GameObject with a **UIDocument** for the dialogue UI.
5. Tag your trigger collider with `RydenConvo` so the **ThirdPersonController** (or your own controller) can start the conversation on trigger enter.

### 6. Play

Enter Play Mode. Walk into the trigger zone to start the conversation. The dialogue UI will display, the camera will frame the shot, and the player can navigate decisions with keyboard or mouse.

## Folder Structure

```
Assets/RydenCam/
├── DialogueFiles/        # Sample dialogue JSON files
├── EditorSettings/       # Editor configuration
├── Prefabs/              # NPC and camera prefabs
├── SampleScene/          # Example scene
├── Scripts/
│   ├── BranchCamEditor/  # Core editor data models, managers, extensions
│   ├── Common/           # Shared enums, constants, logging
│   ├── DialogueGameUI/   # Runtime dialogue player and UI
│   ├── Editor/           # Unity Editor windows, drawers, ribbon
│   ├── Managers/         # Button/input management
│   ├── NodeCommands/     # Node behavior logic
│   ├── SequenceData/     # Actor and conversation data
│   └── Utilities/        # Global settings, helpers
└── UIToolkit/            # UXML, USS, and UI assets
```

## Node Types

| Node | Description |
|------|-------------|
| **Start Node** | Entry point of every conversation. Define actors here. |
| **Dialogue Node** | Displays one or more lines of dialogue for an actor. |
| **Decision Node** | Presents the player with branching choices. |
| **Action Node** | Invokes methods on GameObjects during the sequence. |

## Camera Shot Types

| Shot | Description |
|------|-------------|
| **Portrait** | Frames a single actor at the configured distance and angle. |
| **Over-the-Shoulder** | Frames the speaker from behind the other actor. |
| **Frame Share** | Frames both actors in the same shot. |
| **Custom** | Place the camera manually in the scene. |

## License

See the included LICENSE file for terms of use.

## Support

For questions or issues, please contact the developer or open an issue in the project repository.
