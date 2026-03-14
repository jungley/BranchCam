# Changelog

All notable changes to the RydenCam (BranchCam) package will be documented in this file.

## [1.0.0] - 2026-03-12

### Added
- Visual node graph editor for authoring branching dialogue sequences.
- Camera Shot Editor window for creating and managing cinematic camera configurations.
- Four node types: Start, Dialogue, Decision, and Action.
- Four camera shot types: Portrait, Over-the-Shoulder, Frame Share, and Custom.
- Runtime DialoguePlayer component for playing sequences in-game.
- Runtime UI Toolkit-based dialogue and decision UI.
- JSON-based save/load for dialogue graphs and camera shot configurations.
- Automatic camera placement based on actor positions and shot configuration.
- Multi-actor support with preview rendering in the editor.
- ThirdPersonController example for trigger-based conversation start.
- Sample scene demonstrating the full workflow.

### Fixed
- `DialogueCameraBrain` property setter was assigning to the wrong backing field.
- `NodeDrawer.HeaderTexture` used out-of-range pixel coordinates (`SetPixel(1,1)` on a 1x1 texture).
- `NodeStateController.CreateNodePlayer` returned null for unknown node types without error handling, causing null reference exceptions.
- `ConnectionDrawer.GetConnectionPoints` did not null-check connection points, causing crashes on malformed connections.
- `CameraShotViewModel.RemoveShot` could index out of range when removing the first or last shot.
- `ButtonManager.Awake` found `DialoguePlayer` via `FindObjectOfType` but did not assign the result.
- `InGameDialogUIView.CreateButtons` crashed when `DecisionOptions` was empty (index-out-of-range on `ButtonList[0]`).
- `DirectorManager.GetMidPoint` crashed when actors had null GameObjects.
- `ThirdPersonController` crashed when no `Animator` was present on the GameObject.

### Improved
- All `UnityEditor` references in non-Editor scripts are now wrapped in `#if UNITY_EDITOR` to prevent standalone build failures.
- Comprehensive null and bounds checks added throughout runtime and editor code.
- Save flow gracefully falls back to Save As when no prior file path exists.
- Load flow validates JSON structure and skips malformed nodes instead of crashing.
- Docker (editor window docking via reflection) wrapped in try-catch for Unity version compatibility.
- `BranchLog` now prefixes all messages with `[RydenCam]` and properly logs exceptions.
- `EnumPopupExtensions` clamps selected index to prevent out-of-range errors.
- `NodeCamShotSelector` validates camera shot list before rendering popup.
- Fixed namespace typos: `CameraShotEdtior` → `CameraShotEditor`, `CamersaShotEditor` → `CameraShotEditor`, `DatatStructures` → `DataStructures`.
- Removed hardcoded user-specific file paths from sample dialogue JSON.
- Cleaned up unused imports and dead code across the project.
