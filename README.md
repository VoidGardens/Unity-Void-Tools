# Void Tools

A collection of small, focused Unity Editor productivity tools. Most live under the **VoidGardens** editor menu or in component context menus, and all ship in the `VoidTools` assembly (`VoidTools.asmdef`), Editor-only.

## Requirements
- Unity 2019.1 or newer

## Installation
Drop the `Unity-Void-Tools` folder into your project's `Assets` folder. Everything here is wrapped for the Editor only and will not affect player builds.

## Tools

### Auto Rename
`VoidGardens/Auto Rename` (window) - `VoidGardens/Rename Selection Now` (Ctrl/Cmd+Shift+R) - source: `AutoRenamer.cs`
Normalizes selected Project assets and Hierarchy GameObjects to `Capitalized_Words_With_Underscores`. Splits camelCase/PascalCase and letter/number boundaries, folds spaces/dashes/dots into underscores, and strips filler words like "draft" or "diffuse". Shows a live preview before renaming.

### Clean Name Copier
Inspector header button
Adds a small "N" button to the GameObject Inspector that copies the object's name to the clipboard with `(Clone)` and trailing `(#)` suffixes removed - handy for grabbing the "real" name of a runtime-instantiated object.

### Collection Object Mover
`VoidGardens/Collection Object Mover`
A drag-and-drop window: drop empty "collection" GameObjects into it, then click a collection to reparent your current selection under it. Tracks scene vs. prefab-stage context separately and prunes stale references automatically.

### Copy Full Path
`VoidGardens/Copy Full Path`
Copies the full OS filesystem path of the selected Project asset's *containing folder* to the clipboard - file name and extension are stripped. If a folder itself is selected, its own full path is copied as-is. No need to reveal anything in Explorer/Finder first.

### Prefaber
`VoidGardens/Prefaber/Process Selected`
Prepares one or more selected scene objects to become prefabs: unpacks any prefab/model instance, resets the object's local transform, wraps it in a fresh empty parent (keeping the original name), and renames the object itself with a `_mesh` suffix.

### Quick Screenshot
`VoidGardens/Screenshot/...` (Ctrl/Cmd+Shift+S to capture)
Captures a timestamped PNG screenshot of the Game view while in Play Mode. Configure and reveal the save folder from the same menu (defaults to a `Screenshots` folder next to `Assets`).

### Select By Material
Context menu on `Renderer` / `Material` components
- **Select Objects With Missing Materials** - finds every renderer in the scene/prefab that has a missing material slot.
- **Select Objects in Scene or Prefab (By Material)** - selects every renderer using a given material.
- **Select Objects in Scene or Prefab (By Main Texture)** - selects every renderer whose material shares the same main texture.

All three show a cancelable progress bar on large scenes.

### Transform Group Manipulator
`VoidGardens/Transform Group Manipulator`
Bulk-edits Position, Rotation, or Scale across the current selection using Add/Subtract/Multiply/Divide/Set operations, with per-axis toggles and local/world space options. Also includes a "Center Parent to Children Bounds" action that recenters a parent's pivot on its children's combined render bounds without moving the children visually.

## License
See `LICENSE.md`.
