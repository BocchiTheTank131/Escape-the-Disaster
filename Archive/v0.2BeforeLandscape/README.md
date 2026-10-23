# FLOOD SURVIVAL: 5 DAYS

Unity 6000.6.4f1 / URP / Windows x86_64 / **Prototype v0.2**

## Play the upgrade

This is the existing Cedar Valley project upgraded in place. Open this folder in Unity, open `Assets/FloodSurvival/Scenes/CedarValley.unity`, press Play and click **START STORY**. The saved scene is the build entry point; references, humanoid animations, materials and navigation data are already connected. No paid assets or extra setup are required.

Windows player: `Builds/v0.2/Windows/FloodSurvival.exe`. Portable package: `Builds/FloodSurvival-v0.2-Windows-Portable.zip`. Extract the complete ZIP and keep the executable, `_Data`, DLLs and UnityPlayer.dll together.

You begin seated on your couch watching the original **Cloud Fox & Sunbird** cartoon. Look around with the mouse. After 12 seconds, breaking news interrupts with the requested fictional dam warning, spoken synthetic reporter audio and optional subtitles. **The full 150-second timer starts with that warning. Press E to stand immediately**; you do not have to wait for the report to finish. After seeing the opening, a replay skip is available in the main menu or with Space while seated.

Collect supplies from the kitchen, bedroom and entrance table. The house waypoint changes to Ridge Shelter after gathering two house supplies or leaving the house. Cedar Market and Moreno Garage hold optional extras. Leaving the house triggers a separate cracking event, startled birds, dam dust and a second alert. The dam remains intact until the timer expires.

Follow the amber posts east to the concrete ridge trail. The trail starts near `(36, 0, -46)` and reaches **Ridge Shelter at 15 m**, safely above the final 9 m flood. The flood front advances downstream from the dam while water rises over 75 seconds. Reach the shelter and remain there until the surge stabilizes to win. Leaving early is allowed; no item checklist locks out victory.

| Control | Action |
|---|---|
| WASD / mouse | Move / look (look only while seated) |
| Left Shift | Sprint |
| Space | Jump; hold to swim up in deep water; replay intro skip while seated |
| E | Stand after the warning; collect a targeted item within 3.1 m |
| I | Inspect a targeted supply or the family album |
| Tab | Open / close inventory; time continues |
| F | Equip / toggle a collected flashlight |
| R | Consume batteries to recharge the flashlight |
| H | Consume first aid to restore up to 40 health |
| V | Listen to the collected emergency radio advisory |
| Escape | Close inventory, or pause / resume |

The bag begins with **6 slots** and expands to **12** with the backpack. Radio costs two slots; backpack itself costs none. Duplicate radios, flashlights and backpacks cannot be collected. First aid, canned food (+12 health) and bottled water (+8 health on dry ground) can be used from the inventory. There are no hunger/thirst meters. Deep water slows movement, pushes downstream and drains air; holding Space helps you surface. Mouse sensitivity, master volume, subtitles and opening-seen preference are saved locally; supplies, health, water and timers reset each attempt.

## Systems and files

```text
Assets/FloodSurvival/
  Art/                    Existing Suburban art; new CC0 furniture, cars, survivors
  Animations/             EvacueeV02 humanoid idle/run controller
  Audio/                  Authored reporter, resident and radio speech WAVs
  Icons/                  Seven original item icons
  Items/                  Seven ItemDefinition ScriptableObjects
  Prefabs/                Existing pickups, new item pickups, EvacueeV02
  Scenes/CedarValley.unity Upgraded, directly playable scene
  Scenes/CedarValley-v0.1-backup.unity  Previous scene preserved
  Scripts/
    GameManager.cs        States, single warning/countdown, outcomes and restart
    OpeningSequence.cs    Seated intro, timed news, stand/skip, outside fracture
    TelevisionScreen.cs   Original animated cartoon and breaking-news screen
    FirstPersonController.cs CharacterController/Input System, inspect/use/swim
    ItemDefinition.cs     Item kind, icon, description and slot metadata
    InventorySystem.cs    Quantities, six/twelve-slot capacity and consumption
    PickupItem.cs         Capacity-aware idempotent pickup
    InspectableObject.cs  Environmental descriptions
    ObjectiveSystem.cs    Evacuation stages and house/shelter waypoint
    NPCManager.cs         Warning and immediate evacuation dispatch
    Evacuee.cs            Validated NavMesh start/wander, home stop, humanoid run
    FloodManager.cs       Deterministic moving front, water level, current/debris
    FloatingObject.cs     Lightweight authored buoyancy/drift
    AtmosphereAudio.cs    Cached effect synthesis, spatial audio and speech
    SurvivalUI.cs         uGUI/TMP HUD, icons, captions, settings and menus
  Editor/
    PrototypeUpgradeV02.cs Additive upgrader and saved-content authoring helpers
    PrototypeV02Verification.cs End-to-end Play mode verification
    AssetPersistenceVerification.cs Disk reload/reference checks
    PrototypeBuilder.cs   Legacy v0.1 generator; replaces scene edits if invoked
  Settings/               Persisted v0.2 NavMesh, bird mesh and Volume profile
  Shaders/FloodWater.shader One clipped surface with advancing foam edge
Documentation/            Credits/licenses, v0.2 report, roadmap, earlier evidence
Verification/v0.2/        Actual runtime report, screenshots and build result
Archive/v0.1Source/       Source/document backup before upgrading
```

## Editing and building

Edit the saved scene normally. The v0.2 upgrade menu is idempotent and detects its completion marker. **Do not use the Legacy v0.1 generator on the upgraded scene:** it is retained for history and will regenerate the old layout. The v0.1 source, scene and Windows player remain available separately.

Configure `GameManager.preparationDuration` (default 150), `OpeningSequence.peacefulDuration` (default 12), and `FloodManager` timing/front/current values in the Inspector. Minimum preparation Inspector value is 30 seconds. The opening permits standing as soon as the warning starts. Event guards prevent duplicate warning/countdown/outside/breach transitions.

After moving static furniture, walls, cars or terrain, rebake the existing **NavMeshSurface** on `Cedar Valley • Environment`, persist its data and save the scene. Navigation uses physical colliders; keep doors and the ridge ramp open. Residents/player use Ignore Raycast layer; collectibles use Default. Avoid placing items inside furniture colliders. The eight animated residents are enabled after surfaces register, and local sampling prevents binding onto a roof above their indoor start.

Build Windows from File > Build Profiles using the configured CedarValley scene. Windows x86_64/Mono support is installed on this machine. Editor authoring/QA scripts do not ship in the player. No speech program or online service is required at runtime.

## Verification, licenses and limits

See `Verification/v0.2/Playtest.md` and `Documentation/V02_COMPLETION.md` for the actual test scope/results. To rerun, enter Play mode at the main menu and choose **Flood Survival > Verify v0.2 full loop (Play mode)**. Do not play simultaneously. The primary 12-second opening, 150-second preparation and 75-second flood run at timeScale 1; virtual Input System devices test keys/mouse, and teleports arrange item/hazard checks. Secondary failure and no-supply cases accelerate setup. Focus settings, virtual devices and saved opening/subtitle preferences are restored afterward.

All external art is free CC0. Existing Unity packages, font and offline authoring-tool notices are documented in `Documentation/ASSET_CREDITS.md`. The TV cartoon is original. Speech is synthesized and intentionally robotic. The environment is stylized low-poly, with imported furniture/characters/cars alongside simple structural geometry. Water uses one moving front and global level; it does not model rooms, rivers or full fluid dynamics. No full five-day simulation, combat, multiplayer, vehicle driving, boats or advanced destruction is included.

