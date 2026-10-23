# Wooded valley, roof-level flood and television revision — v0.2.1

This revision updates the existing CedarValley scene. The previous scene is preserved at `Assets/FloodSurvival/Scenes/CedarValley-before-landscape.unity`; previous runtime source is in `Archive/v0.2BeforeLandscape/`.

## Requested changes

- Removed the shelter canopy, pillars, back wall, concrete terrace, supply crates, floating shelter signs and old rectangular plateau. The route now leads to open wooded high ground. All visible route labels, radio speech, objectives and ending text use the new destination.
- The flood now rises from -2 m to **10.6 m over 75 seconds** after the unchanged 150-second warning. The tallest residential roof measures about 10.50 m, so the final water reaches every house roof; lower houses submerge sooner. The downstream advancing-front model, swimming, current and drowning remain active.
- Survival no longer checks distance to a shelter. After the full surge, standing on dry ground above **11.6 m** wins. The marked eastern hilltop remains around 15.18 m. The collider-supported earth path follows the existing evacuation route.
- Replaced the 22 rounded mountain placeholders and artificial perimeter walls with one continuous 720 x 720 m Terrain, a 513-point heightfield, irregular ridgelines, slopes and a saddle behind the visible dam. Rocky and vegetated terrain layers use free CC0 Poly Haven textures. The final forest contains **1,409 pines in 58 spatial patches**, with greener lower slopes and exposed rock on steep/high terrain. Pine geometry is combined to limit renderer count.
- Improved the TV's fitted bezel, stand and power indicator. A dedicated **1280 x 720 render texture** displays the original animated cartoon on its physical screen. The cartoon now has rounded characters, a swishing fox tail, flapping sunbird wings, clouds and layered scenery. Breaking news includes a fictional presenter, speaking/blinking animation, dam diagram, timed headlines, live badge, countdown and seamless scrolling evacuation ticker. A brief signal interruption accompanies the existing alert sound. Captions and seated prompts are placed away from the screen; the seated crosshair and unnecessary HUD elements are hidden.

The screen camera renders only its programme layer and switches off when the player is over 24 m away. Existing news narration, optional subtitles, controls, inventory, NPC panic and restart flow remain integrated. The television programme is original project content; no outside cartoon footage was used.

## Sources and important files

- New authoring: `Assets/FloodSurvival/Editor/LandscapeRevision.cs` and `TelevisionUpgrade.cs`.
- Updated runtime: `FloodManager.cs`, `ObjectiveSystem.cs`, `GameManager.cs`, `FirstPersonController.cs`, `TelevisionScreen.cs`, `OpeningSequence.cs`, `SurvivalUI.cs` and radio WAV.
- Saved assets: `Assets/FloodSurvival/Landscape/`, including TerrainData/layers, combined pine meshes, persisted navigation and TV render texture/materials.
- Free textures: Poly Haven `rock_face_03`, `forest_ground_04` and `aerial_grass_rock`. Official asset pages, CC0 notice and authoring download hashes are documented in `Documentation/ASSET_CREDITS.md` and `Art/PolyHavenLandscape/`. Existing free Kenney models, Unity packages and original speech workflow remain unchanged.

## Validation

Current results are recorded in `Verification/v0.2-landscape/Playtest.md`. The primary sequence runs the full opening, 150-second warning and 75-second flood at normal speed, with virtual Input System controls, arranged item approaches, a real sprint up the trail, eight resident paths, both endings and restart checks. Additional scene/presentation checks and Windows build details are appended after completion.

## Limits

The town and pine models remain stylized. Terrain uses an authored deterministic heightfield and blended photographic surfaces; it is not a scanned landscape. Flooding still uses one clipped advancing surface/global level, without indoor water masking or a fluid solver. The TV presenter is illustrated, and speech is synthetic. Hands-on standalone keyboard testing, subjective audio and performance across representative PCs remain manual verification items.


## Completed verification

- **67 full-loop checks passed** after removing the shelter, rebuilding navigation, upgrading the TV and raising the flood. The main 12-second opening, 150-second countdown and 75-second surge ran at normal speed. The harness exercised virtual keyboard/mouse input, house supplies/capacity, eight complete NPC routes, a real sprint up the trail, progressive flooding, swimming/current, victory, drowning and restart. Item/hazard viewpoints were arranged by teleports; this was not an unaided human route test.
- The final tree-density/ground-colour pass changed presentation only. After it, **23 additional scene/runtime checks passed**: no shelter/old mountain objects, no stale destination text, all eleven roofs below the final water level, persisted terrain/layers, 1,409 pines, complete route and resident navigation, persisted 720p TV framing, actual coloured TV pixels and character/ticker animation, pause, distant-TV culling, roof-level water, victory on a different dry mountain, and clean restart.
- The existing ten saved-asset/reference checks also passed on the final scene reload. Final focused Play mode had **zero console errors and zero warnings**. `RevisionChecks.md` records the additional checks; `TV-cartoon-final.png`, `TV-news-final.png` and `Roof-level-flood.png` show the final presentation.
- A late-exit regression was fixed: going outside after a breach no longer overwrites the collapse message with the pre-collapse cracking alert.

The larger forest and terrain increase geometry/shadow cost; combined patches and the distance-gated TV camera limit overhead. Representative lower-end Windows profiling is still required. This revision's Windows build results are appended below; native keyboard and subjective audio verification remain manual, as documented for v0.2.

## Windows export

The final Windows x86_64 build succeeded with **zero errors**, 192,100,250 bytes (about 183.2 MiB) before documentation/ZIP packaging. The first export with the terrain shaders took 873.2 seconds. Unity reported the same 501 nonfatal package shader warnings as v0.2; the machine-readable report is `Verification/v0.2-landscape/BuildResult.json`. No terrain/game C# build error occurred.

Final player: `Builds/v0.2.1/Windows/FloodSurvival.exe`. Portable archive: `Builds/FloodSurvival-v0.2.1-Windows-Portable.zip`. This revision was verified in the Editor and compiled for Windows; a fresh native player keyboard/route test was not performed. The v0.2 native-input limitation remains a manual check rather than a claim of standalone testing success.
