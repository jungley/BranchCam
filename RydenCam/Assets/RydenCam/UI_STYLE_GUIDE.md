# RydenCam Editor UI Style Guide (v1)

## Design Goals
- Prioritize clarity for long authoring sessions.
- Keep high contrast for dark-theme Unity editors.
- Use consistent spacing and typography across all editor windows.
- Avoid visual noise; reserve bright colors for actionable states.

## Color Tokens
- `CanvasBackground`: `#1A1C20`
- `PanelBackground`: `#262A30`
- `PanelBackgroundElevated`: `#2E333A`
- `ToolbarBackground`: `#1E2227`
- `BorderMuted`: `#4D5560`
- `TextPrimary`: `#EBEEF2`
- `TextSecondary`: `#AAB1BA`
- `Accent`: `#488DFF`

## Typography
- `Title`: 16px, bold (`TextPrimary`)
- `Body`: 12px, regular (`TextSecondary`)
- `Caption`: 11px, regular (`TextSecondary`)

## Spacing
- Base unit: `4px`
- Common paddings:
  - panel padding: `10-12px`
  - toolbar item gap: `4px`
  - control vertical rhythm: `8px`

## Component Rules
- **Toolbar**
  - Height around `36px`
  - Elevated button surface
  - Keep labels short and action-oriented
- **Inspector**
  - Dark panel background with inner padding
  - Section title + grouped controls
  - Avoid full-width long labels where possible
- **Nodes**
  - Keep header labels bold and compact
  - Maintain readable body text size (`12px`)
  - Use color only for node-type identity and focus

## Phase Roadmap
- **Phase 1 (now):**
  - Shared theme tokens in code
  - Modernized toolbar/panel/text styling
  - Stabilized global GUI state behavior
- **Phase 2:**
  - Better node cards, border treatment, and selection glow
  - Unified control styles for dropdowns/text fields/buttons
- **Phase 3:**
  - Optional migration to UI Toolkit for maintainable modern UI architecture
