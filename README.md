# FLOOD SURVIVAL: 5 DAYS

Unity 6 / URP / Windows PC / Prototype v0.1

## Play

Open this folder in **Unity 6000.6.4f1**. Open `Assets/FloodSurvival/Scenes/CedarValley.unity`, press Play, and click **BEGIN EMERGENCY**. The scene is already configured as the first build scene. No manual reference wiring or paid assets are required.

Launch the final Windows player at `Builds/v0.1/Windows/FloodSurvival.exe`, or extract `Builds/FloodSurvival-v0.1-Windows.zip` and run its executable. Keep the accompanying `_Data` folder, DLLs, and UnityPlayer.dll together. The older `Builds/Windows` folder is an intermediate smoke-test build.

You begin on Main Street in Cedar Valley. Your home is the flat-roofed house to the left, southwest of the market. The amber waypoint leads to it. Cedar Market is the open-fronted store on the right side of Main Street. Collect supplies from its shelves and your kitchen/table at home.

The emergency lasts **150 seconds of active gameplay**. Pausing freezes it; opening the inventory does not. When the dam breaches, follow the amber posts up the concrete trail east of your house. **Ridge Shelter is at 15 m**, above the final 9 m flood. The surge rises for 75 seconds. Reach the shelter and remain there until the surge stabilizes to win. You may evacuate early; victory is evaluated after the breach and full surge.

Returning home and collecting all four essentials are tracked accomplishments shown at the end. Survival depends on reaching the shelter; missing supplies does not silently lock out victory. This prototype covers the warning and first evacuation, not the complete five-day survival simulation.

| Control | Action |
|---|---|
| WASD / mouse | Walk / look |
| Left Shift | Sprint |
| Space | Jump |
| E | Pick up the item under the crosshair, within 3.1 m |
| Tab | Open or close emergency bag |
| F | Equip/toggle the flashlight after collecting it |
| H | Consume one medicine to restore up to 40 health |
| Escape | Close inventory, or pause/resume |

Mouse sensitivity and master volume are adjustable in the pause menu and saved locally. Flashlight ownership and other inventory items reset on each attempt. Food and water are preparation items in v0.1; hunger, thirst, crafting, vehicles and combat are outside this build.

## Implemented systems

- Explorable town with ten Kenney residential buildings, player home and market interiors, roads, sidewalks, gardens, trees, streetlights, mountains, visible dam and elevated shelter.
- CharacterController movement with smoothed acceleration, gravity, jumping, collision, sprint, mouse look, slight head bob, and configurable sensitivity through Unity Input System.
- State-driven main menu, 150-second emergency, panic activation, timed escalation messages, dam breach, flood, pause, inventory, victory and defeat.
- Eight NavMesh residents with idle wandering, urgent evacuation, building avoidance and procedural limb animation. The scene includes a persisted AI Navigation bake.
- Four ScriptableObject item definitions, reusable pickup prefabs, quantity inventory, single-collection protection, medicine consumption and flashlight equipment.
- Objective text and clamped screen waypoint with distance. The objective changes from home to the elevated shelter at breach.
- A single rising water surface with animated URP shader, deterministic global height, shallow-water movement penalty, oxygen depletion below the surface, and drowning damage. Air recovers above the water.
- Dam center disappearance, breach particles, synthesized collapse rumble, camera shake, six spatial sirens, and valley wind ambience.
- uGUI / TextMeshPro HUD, inventory, settings, pause and outcome screens with Input System event routing.
- Safe scene-based restart, configured Windows build scene, and an Editor authoring/verification workflow.

## Project structure

```text
Assets/FloodSurvival/
  Art/KenneySuburban/      CC0 imported FBX meshes, atlas and license
  Editor/
    PrototypeBuilder.cs    Rebuilds the dedicated scene and saved assets
    PrototypeVerification.cs  End-to-end Play mode checks with real input events
    AssetPersistenceVerification.cs  Explicit unload/reload of saved item assets
  Items/                  Four ItemDefinition assets
  Materials/              URP palette, atlas, storm sky and flood materials
  Prefabs/                Four reusable pickups and procedural evacuee
  Scenes/CedarValley.unity Complete playable scene
  Scripts/
    GameManager.cs        State transitions, countdown, escalation, outcome/restart
    FirstPersonController.cs Movement, input, aiming, interaction, air/health
    InventorySystem.cs    Item quantities, consumption and preparedness
    ItemDefinition.cs     ScriptableObject schema / item kinds
    PickupItem.cs         Idempotent collection
    ObjectiveSystem.cs    Home tracking and shelter navigation
    FloodManager.cs       Breach, stable rising water height and surge duration
    NPCManager.cs         Panic activation
    Evacuee.cs            NavMesh movement and procedural animation
    AtmosphereAudio.cs    Synthesized siren, wind, rupture and pickup sounds
    SurvivalUI.cs         uGUI creation, bindings, HUD and menu presentation
  Settings/               Saved NavMesh and post-processing profile
  Shaders/FloodWater.shader Animated single-surface URP water
Assets/TextMesh Pro/       TMP essential resources and font license
Documentation/            Credits, validation notes and v0.2 plan
Verification/             Live playtest report and screenshots
```

`Assets/Scenes/SampleScene.unity` is retained from the original template. The new scene is the build entry point. Existing unrelated project changes were preserved.

## Editing and rebuilding

Edit the saved scene normally. **Flood Survival > Build Prototype Scene** regenerates the dedicated prototype scene and its generated assets; it replaces scene-level hand edits, so save a copy before using it after customization. It does not delete the original template scene. TMP essentials and all packages are already installed.

World dimensions are in meters. The ground is at 0 m; water rises from -2 m to 9 m. The concrete trail starts around `(36, 0, -46)` and reaches the terrace at `(84, 15, -46)`. The shelter objective is `(88, 15.1, -46)`. Keep the ramp/terrace seam walkable when editing.

If geometry changes, rebake the **NavMeshSurface** on `Cedar Valley • Environment` in the Inspector and save its NavMesh data and scene. Pickups and player are outside the static environment hierarchy. Player/NPC objects use Ignore Raycast layer; pickups use Default, so interaction rays ignore the player's capsule.

Build from **File > Build Profiles > Windows**, using the configured scene list. An already installed Windows build module is required. Recommended architecture is x86_64; the prototype does not require IL2CPP. The Editor-only QA code is excluded from player builds.

## Verification

See `Verification/Playtest.md` for the recorded end-to-end checks and `Documentation/VALIDATION.md` for build/manual-test limits. To rerun, start the scene in Play mode, leave it at the main menu, then choose **Flood Survival > Run Full Play Mode Verification**. Allow roughly four minutes of active simulation plus Editor overhead. It sends virtual keyboard/mouse input, arranges pickup/hazard scenarios by teleport, walks the evacuation ramp, observes the natural 150-second countdown and 75-second flood, then tests drowning and restart. Do not play simultaneously with this automated run.

## Assets and scope

Only free assets and the existing official Unity packages are used. See `Documentation/ASSET_CREDITS.md` for exact sources, package versions, and retained licenses. The visual direction is a coherent stylized town with functional placeholders, rather than photorealistic production art. Water has consistent global height but does not simulate currents or indoor occlusion. Dam destruction and citizens are intentionally simple.

The executable targets Windows; performance across mid-range gaming hardware still needs profiling on representative devices. See `Documentation/ROADMAP.md` for the v0.2 priorities.
