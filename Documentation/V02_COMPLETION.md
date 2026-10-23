# Prototype v0.2 — implementation and completion report

## Upgrade delivered

The existing `CedarValley.unity` scene, URP/Input System/AI Navigation packages, CharacterController, managers, pickups, HUD/menu framework, restart flow and original town were retained and extended. The v0.1 scene, source and Windows build are preserved. The legacy scene generator is clearly separated in its menu so it is not mistaken for the v0.2 upgrade.

- **Opening:** seated mouse look on an imported sofa, original animated Cloud Fox & Sunbird TV program/music, 12-second peaceful lead-in, breaking-news screen/alert, exact requested fictional news dialogue as four speech WAVs and timed optional subtitles. The full configurable 150 seconds starts with news; E stands immediately. Replay skip unlocks after seeing the introduction. The player is otherwise free to move throughout the report.
- **Evacuation objectives:** listen/stand, optional house supplies, optional town supplies, leave the risk area, follow the ridge trail, survive the surge. Clear house-supply/shelter markers and distances replace the old return-home loop. An outside-only fracture event has guarded cracks, dust, leaks, flying birds, slight shake, sound and the second evacuation alert; it does not breach the dam early.
- **Inventory:** seven ScriptableObject definitions with original icons/descriptions; E collection, I inspection, 6-slot capacity, backpack upgrade to 12, capacity rejection without losing world items, unique equipment, pickup sounds, medicine/food/water use, charged flashlight and consumable batteries, emergency radio advisory. Supplies improve options but cannot lock victory.
- **Town:** furnished living room, kitchen, bedroom and entrance; family album/photograph; imported furniture and room details; existing residential buildings and enterable market; new enterable Moreno Garage; five parked vehicles, environmental stock/boxes, rocks and clearer route signage/beacon. Existing trees, roads, sidewalks, fences, lights, valley, dam and ridge remain. Real-time light/fog/Volume grading shifts toward emergency atmosphere.
- **Residents:** eight reusable imported humanoids with valid idle/run animation, calm wandering/indoor starts, staggered panic voices, some short home stops and other immediate evacuees, urgent evacuation after outside fracture. Navigation uses the persisted existing surface, now rebaked for new furniture/vehicles. Local floor-aware sampling and complete-path checks prevent roof binding and invalid wandering destinations.
- **Flood:** existing single surface upgraded with a deterministic downstream front, shader clipping/foam edge, rising authoritative level, breach debris/dust, flood roar, camera shake and bounded floating boxes. Local water queries remain dry ahead of the front. Shallow/deep movement penalties, Space swimming, downstream current, oxygen depletion, drowning and safe elevated shelter are integrated into the existing controller/outcome loop.
- **UI/audio:** existing uGUI/TMP HUD and menus retained, seven-icon inventory, notifications, battery/capacity readouts, captions toggle, saved sensitivity/volume, health/air, waypoint, pause/restart and both endings. Original synthesized footsteps vary by surface; there are TV, birds, sirens, cracking, collapse, pickup, speech and flood sounds. Audio pauses with gameplay. The clean opening hides unnecessary HUD elements.

## Important code

`OpeningSequence.cs` and `TelevisionScreen.cs` own story/TV timing; `GameManager.cs` owns guarded game-state/countdown transitions. `FirstPersonController.cs`, `InventorySystem.cs`, `ItemDefinition.cs`, `PickupItem.cs` and `InspectableObject.cs` handle controls/items. `ObjectiveSystem.cs` owns evacuation guidance. `NPCManager.cs` / `Evacuee.cs` dispatch and navigate residents. `FloodManager.cs`, `FloatingObject.cs` and `Shaders/FloodWater.shader` own the deterministic flood. `AtmosphereAudio.cs` and `SurvivalUI.cs` extend the existing audio/presentation systems.

Authoring lives in `Editor/PrototypeUpgradeV02.cs`; runtime regression coverage lives in `Editor/PrototypeV02Verification.cs`. All paths above are under `Assets/FloodSurvival/`. The saved scene and references are ready to play. README contains controls, configuration, structure and build instructions.

## Assets and dependencies

Kenney Suburban (existing), Furniture Kit, Car Kit and Animated Characters Survivors are **CC0**, with archive licenses retained. No new Unity runtime package was added. URP 17.6.0, Input System 1.20.0, AI Navigation 2.0.14 and uGUI 2.6.0 remain pinned. Liberation Sans retains its SIL OFL notice. Original authored WAV speech was rendered with free GPL3 eSpeak NG 1.52.0 as an offline authoring tool; no eSpeak code/data/binary ships in the player. All sources, uses and license copies are listed in `ASSET_CREDITS.md` and `Licenses/`.

## Test evidence

Runtime verification and Windows build results are recorded below after the final run. `Verification/v0.2/Playtest.md` is the detailed live record; screenshots accompany it. Earlier v0.1 results in `VALIDATION.md` remain historical evidence, not claims about v0.2.

## Known limits and performance

The environment is a coherent stylized prototype, with imported art alongside simple walls, dam geometry and collectible shapes. The TV program is an original small animated cartoon rather than video footage. Speech is intelligible synthetic English rather than human voice acting; subjective listening/mix quality remains a human check. Not every town building is enterable, and there are no interior doors or sophisticated NPC social activities.

Flood height is global behind one straight advancing front. The front is deterministic, not a terrain/channel fluid solver; water can visually enter buildings without doorway occlusion. Floating boxes use authored drift/bob, not full physical buoyancy. Debris is bounded animation, not advanced destruction. Swimming/current provide an escape chance but do not model real hydrodynamics. Victory requires the designated Ridge Shelter after the full surge, rather than every possible rooftop or elevated spot.

Input rebinding, controller support, broader aspect-ratio/accessibility testing, unaided human route testing and representative multi-PC profiling remain future work. No claim is made that a single Editor measurement proves performance on typical gaming PCs. A sampled preparation frame on this machine (Radeon RX 9060 XT) was about 11.2 ms at the 90 FPS target; approximately 1,158 MiB of Unity allocated memory included the Editor and packages, not standalone memory.

One water surface, eight agents, persisted navigation, a fixed seven-kind inventory, cached clips, pooled resident voice sources, precreated debris and roughly 12.5 Hz HUD updates bound runtime costs. Added furniture/material variants, skinned characters, transparent effects, real-time lights and imported unused art increase Editor size/batching work. Profile the standalone build, audit unused template packages, add quality presets and reduce static-material variants before expanding population or map size.

No vehicle driving, boats, enemies/combat, multiplayer, complex crafting, full five-day simulation or expensive fluid/destruction simulation was introduced. Recommended v0.3 priorities are in `ROADMAP.md`.

## Verified runtime results — 23 October 2026

The v0.2 end-to-end Editor harness completed **67 passing checks**. Its primary 12-second opening, 150-second countdown and 75-second flood ran at normal timeScale 1, with confirmed advancing frames while the Editor was unfocused. Virtual keyboard/mouse devices exercised seated look/movement restriction, E stand/pickup, I inspection, F/R flashlight and batteries, V radio, Tab inventory, Escape pause/resume, sprint up the real ridge ramp, Space swimming and H first aid. Teleports arranged item approaches, the flood observation point and hazard branches; this was not an unaided human navigation test.

The run verified exact news subtitle/audio references and playback state, one emergency event despite repeated calls, all seven pickup kinds and duplicate rejection, capacity/full-bag handling, backpack expansion, house collection progress, outside cracking/birds/dust/second alert before collapse, eight humanoid residents with complete paths, natural breach timing, dry ground ahead of the front, progressive water arrival, drifting boxes, full-surge victory, deep-water oxygen/damage/defeat, replay skip, configured preparation duration and clean restarts. A separately accelerated branch survived with an empty bag. Preferences, focus behavior and virtual devices were restored by the harness.

Two issues found during development were fixed before the successful run: a flashlight buried in the dining-table collider (also corrected supply support heights), and a resident selecting the roof from an overly broad NavMesh sample. The final test reported zero runtime console errors and zero warnings. A later presentation-only adjustment separates captions from the open inventory and hides the HUD behind that panel; its focused visual/regression check is recorded separately.

After stopping Play mode, the saved scene was explicitly closed/reopened and all seven item assets unloaded/reloaded from disk. **Ten saved-asset checks passed**, including item types/text/icons/descriptions, zero missing scripts, core manager wiring, story/TV/voice/UI/humanoid references and persisted NavMesh data. See `Verification/v0.2/SavedAssets.md`. These checks avoid relying solely on values held in Editor memory.

The final focused check passed **14 additional runtime assertions** for caption/bag separation, hidden background HUD, continued countdown and flashlight drain with the bag open, battery loading, light/radio buttons, all three consumable USE buttons, passive backpack behavior, pause, settings sliders and another clean restart. See `Verification/v0.2/FinalPresentation.md`. The inventory caption layout was visually reviewed. The final focused run also had zero console errors/warnings. A flashlight charge update was moved ahead of the inventory input guard so time and battery use remain consistent while browsing the bag.

## Windows build

Unity's Windows x86_64 build **succeeded with zero errors** in 113.6 seconds. Unity reported 142,373,170 bytes (about 135.8 MiB) before documentation/ZIP packaging. The full machine-readable BuildReport is `Verification/v0.2/BuildResult.json`.

There were **501 nonfatal build warnings**: 498 from existing Sentis/inference shader variants, two other existing package/debug-shader stripping warnings and one deprecated TextMeshPro shader pragma. No game C# or flood-shader build error occurred. Existing unused inference/template packages remain candidates for the v0.3 dependency/performance audit; the upgrade did not add those packages or remove tooling dependencies.

The final Windows executable was launched and visually inspected: main menu, Start Story mouse click, seated cartoon animation, breaking-news transition and advancing countdown worked. Desktop-injected E/Escape keyboard actions did not confirm a response. **Standalone keyboard movement/standing/inventory/pause and the complete hands-on Windows route remain unverified.** This is an automation limitation/open verification item, not a claim of success or proof of a game defect. The corresponding controls and full game loop did pass the Editor Input System harness. See `Verification/v0.2/WindowsSmoke.md`. Subjective listening quality and cross-PC performance also remain manual checks.

Final deliverables are `Builds/v0.2/Windows/FloodSurvival.exe` and `Builds/FloodSurvival-v0.2-Windows-Portable.zip`, with play instructions, credits and retained licenses. The Unity scene is directly playable; no v0.1 rebuild or manual reference wiring is needed.

