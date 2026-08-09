# Changelog

All notable changes to Void Tools are documented in this file.

## [Unreleased]
### Changed
- **Auto Rename** - Source file renamed from `CamelCaseRenamer.cs` to `AutoRenamer.cs` to match what the tool actually does (converts names to `Capitalized_Words_With_Underscores`, not camelCase). No behavior change; menu paths and shortcut are unaffected.
- **Copy Full Path** (`CopyFullPath.cs`) - Now copies only the *containing folder* path for file assets (file name and extension are stripped). Folder assets still copy their own full path as before.

## [1.0.0] - 2026-08-09
### Added
- **Auto Rename** (`AutoRenamer.cs`) - Normalizes selected Project assets and Hierarchy GameObjects to the `Capitalized_Words_With_Underscores` convention. Splits camelCase/PascalCase and letter/number boundaries, folds existing separators (space, `-`, `.`) into underscores, and strips filler words (`draft`, `diffuse`, `decor`). Preview window at `VoidGardens/Auto Rename`, plus an instant-apply shortcut `VoidGardens/Rename Selection Now` (Ctrl/Cmd+Shift+R).
- **Clean Name Copier** (`CleanNameCopier.cs`) - Adds a small "N" button to the GameObject Inspector header that copies the object's name to the clipboard with any `(Clone)` suffix and trailing `(#)` counter stripped.
- **Collection Object Mover** (`CollectionObjectMover.cs`) - Drag-and-drop window (`VoidGardens/Collection Object Mover`) for registering "collection" GameObjects, then reparenting the current Hierarchy selection into one with a single click. Undo-safe, and tracks scene vs. prefab-stage context separately.
- **Copy Full Path** (`CopyFullPath.cs`) - Copies the absolute OS filesystem path of the selected Project asset's containing folder to the clipboard (`VoidGardens/Copy Full Path`), without needing to open the file explorer.
- **Prefaber** (`Prefaber.cs`) - Prepares selected scene objects for prefabbing (`VoidGardens/Prefaber/Process Selected`): fully unpacks any prefab/model instance, resets the object's local transform to identity, wraps it in a new empty parent that inherits the original name, and appends `_mesh` to the original object's name. Supports multi-selection and is fully undoable.
- **Quick Screenshot** (`QuickScreenshot.cs`) - Captures a timestamped PNG of the Game view while in Play Mode (`VoidGardens/Screenshot/Capture Play Mode Screenshot`, Ctrl/Cmd+Shift+S), with menu commands to set and reveal the save folder.
- **Select By Material** (`SelectByMaterial.cs`) - Adds inspector context-menu commands: select every renderer sharing a missing-material slot (on `Renderer`), and select every renderer using a given material or its main texture (on `Material`). Shows a cancelable progress bar for large scenes.
- **Transform Group Manipulator** (`TransformGroupGameObject.cs`) - Window (`VoidGardens/Transform Group Manipulator`) for bulk-editing Position, Rotation, or Scale across the current selection using Add/Subtract/Multiply/Divide/Set operations, per-axis toggles, and local/world space options. Also includes a "Center Parent to Children Bounds" utility that recenters a parent's pivot on its children's combined render bounds without moving the children visually.

### Fixed
- Corrected changelog entries to match actual tool behavior (e.g. Auto Rename converts names to `Capitalized_Words_With_Underscores`, not camelCase; "Copie" -> "Copier"; "Mayerial" -> "Material").
