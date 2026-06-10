# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6 (6000.4.10f1) project. Open and run via the Unity Editor — there is no CLI build command configured yet.

## Key Packages

- **Input System** 1.19.0 — use `InputSystem_Actions.inputactions` as the central action asset; avoid the legacy `Input` class.
- **2D Feature** (Tilemap, Sprite, Physics2D) — this is a 2D project.
- **Test Framework** 1.6.0 — run tests via Unity Editor > Window > General > Test Runner.
- **Timeline** 1.8.12, **uGUI** 2.0.0, **Visual Scripting** 1.9.11 also included.

## Scripts Folder Layout

```
Assets/Scripts/
  AI/       — enemy/NPC behaviour
  Data/     — ScriptableObjects, data containers
  Player/   — player controller, input handling
  Systems/  — game-wide managers and systems
  World/    — level, environment, world logic
```

Place new scripts in the matching folder. All scripts live under `Assets/Scripts/`.

## Unity Conventions

- Use `[SerializeField] private` instead of `public` for Inspector-exposed fields.
- `MonoBehaviour` lifecycle order: `Awake` → `OnEnable` → `Start` → `Update`/`FixedUpdate` → `OnDisable` → `OnDestroy`.
- Prefer `ScriptableObject` (in `Data/`) for configuration and shared state over singletons.
- Every new `MonoBehaviour` or `ScriptableObject` needs a corresponding `.meta` file — Unity generates this automatically when the file is created inside the Editor or placed in the `Assets/` tree while the Editor is open.
