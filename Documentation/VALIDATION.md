> Historical v0.1 evidence. For the current upgrade see [v0.2 completion and validation](V02_COMPLETION.md).

# Validation and remaining limits

## Reproducible gameplay verification

The Editor harness lives at `Assets/FloodSurvival/Editor/PrototypeVerification.cs`. Its human-readable result is written to `Verification/Playtest.md`, alongside screenshots of the emergency, full inventory, rising water, flooded town, victory and defeat.

The harness uses virtual Input System keyboard/mouse events for movement, mouse look, jump, sprint, E pickup, Tab inventory, Escape pause and F flashlight. Teleports arrange interaction, wall collision, ramp-entry and drowning scenarios; they do not stand in for the tested movement up the ridge. It also checks one-time collection, home completion, all four supplies, complete NPC paths, medicine consumption and full scene reset after both outcomes.

The primary countdown is allowed to expire at **normal timeScale 1 for 150 seconds of gameplay**. The primary flood then rises for its full 75 seconds before victory. A second, accelerated hazard setup explicitly forces the breach and final water level to test drowning and the defeat/restart branch. This separate setup does not claim to be another natural countdown run.

The completed run recorded **52 passing checks**, including the natural countdown, full surge, both outcomes and both restart paths. The final Editor console had zero errors and zero warnings. Initial test iterations found and corrected a ramp/plateau junction that caused partial NPC paths and an interaction ray hitting the player's capsule when looking down. The verification clock was also adjusted to use gameplay time for movement checks, avoiding false speed failures caused by Editor stalls. The report ends with `COMPLETE`.

After that run, a focused presentation pass corrected the post-breach timer label wrapping, updated phase captions, kept waypoints clear of the control legend, made the health/air bars scale without a sprite dependency, and improved resident neck/torso alignment. The flood presentation was checked in Play mode with a clean console. The full timed run preceded these presentation-only changes; the core countdown, navigation, inventory, health and state logic were unchanged.

A subsequent packaging audit caught ScriptableObject item changes that had remained in Editor memory instead of being marked dirty and saved. `PrototypeBuilder` now explicitly marks item definitions dirty. `AssetPersistenceVerification.Verify()` unloads all four item assets, reloads each from disk, and checks their types and display names before reopening the scene and checking manager wiring and missing scripts. All five saved-asset checks passed; see `Verification/SavedAssets.md`. A separate regression run after that reload passed another **52 checks**, recorded in `Verification/ColdReloadPlaytest.md`; only its countdown/flood are accelerated, so the original real-time evidence remains separate. Virtual input is isolated from desktop input during automated runs, and Input System focus settings are restored afterward.

## Performance observations and design

The Editor ran the active scene near its configured 90 FPS limit during sampled portions on an AMD Radeon RX 9060 XT. A single observed frame was about 12.8 ms. These observations include Editor overhead and are not a representative benchmark or guarantee on other PCs. About 1.1 GB of Unity allocated memory was observed in the Editor process; this includes Editor/package state and does not measure standalone player memory.

- A single small-mesh water plane provides the complete rising surface, with one authoritative level and no fluid solver.
- Navigation is baked and persisted; agents do not rebake the world at runtime. Eight residents use Unity's agent movement and lightweight procedural limbs.
- World buildings/trees and most environment meshes are static. Supplied low-poly models and one atlas keep asset costs modest.
- UI text is refreshed at roughly 12.5 Hz. Inventory quantities use a fixed four-slot array. No gameplay loop continuously instantiates pickups or residents.
- Six spatial looping sirens share one generated clip. Wind and short effects are synthesized once per attempt and disposed when the scene unloads.
- Draw-call reduction, representative CPU/GPU profiling, LODs and quality presets are the next optimization work. The assembled primitive interiors and multiple materials can increase batches. Real-time lighting should be checked on weaker GPUs.

## Known limitations / manual checks

- This is a stylized functional prototype. Interiors, dam damage, residents and particles use simple geometry/procedural animation. There are no animated production-quality human characters.
- Warning text is accompanied by sirens; a spoken announcement is not included. Audio playback was checked programmatically; subjective volume balance and listening comfort need a human listening pass.
- Water rises uniformly and can visually pass through building interiors. There are no local currents, buoyancy, boats or hydrodynamic forces. The shelter remains above the final water level.
- Food and drinking water are tracked preparation supplies. Hunger/thirst, crafting, the full five days, vehicles, enemies and multiplayer are deliberately absent.
- Returning home and completing the supply checklist affect the outcome summary; reaching the shelter determines survival.
- No persistence beyond mouse sensitivity and audio volume, no rebind screen, and no gamepad support in v0.1.
- HUD layouts were visually inspected at 16:9 resolutions. Ultrawide and small-screen behavior, accessibility scaling and multiple monitor configurations need further checks.
- Automated Editor tests verify the core loop; separate player build smoke results are recorded below when available. Cross-PC performance and a complete unaided human route test remain outside the automated evidence.

## Windows build and player smoke test

The Windows x86_64 build succeeded with zero errors, approximately 138 MB before ZIP compression. The machine-readable final result is `Verification/BuildResult.json`. Unity emitted 512 nonfatal warnings from existing package shaders: 509 Sentis/inference compute and hidden shader variants, two stripped Core debug shaders, and one deprecated TMP shader pragma. The flood shader and game C# produced no build errors. The unused template inference packages are a good candidate for a later dependency audit; they were retained to preserve the existing Unity tooling setup.

The standalone executable launched, displayed the main menu correctly, accepted the BEGIN EMERGENCY mouse button, started at 02:30, displayed the emergency broadcast/HUD, and showed residents moving. Keyboard automation did not produce a confirmed inventory/pause response. The user then stopped Computer Use with physical Escape, so window automation was stopped. **Standalone keyboard controls, the complete unaided route, and subjective audio remain manual verification items**; the corresponding Input System controls and full loop did pass the Editor harness.

The standalone log exposed an agent/surface initialization-order warning. Agents are now saved disabled and enabled in `Evacuee.Start`, after scene surfaces have registered in OnEnable. A focused Editor regression confirmed all eight agents enabled, bound to NavMesh, and had complete paths with zero console warnings/errors; see `Verification/NPCStartup.md`. The final executable includes this fix, but its native UI was not retested after Computer Use was stopped. Existing Unity analytics services also logged DNS failures on this machine; these did not prevent local play.

Final build: `Builds/v0.1/Windows/FloodSurvival.exe`. The earlier `Builds/Windows` directory is an intermediate build. The final ZIP includes controls, credits and retained font/package licenses.

