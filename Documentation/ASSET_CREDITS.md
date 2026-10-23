# Free assets and dependencies

No paid, trial-only or pirated assets were used. No purchases or paid services are required for this prototype content.

## Kenney City Kit (Suburban) 2.0

- Author: Kenney — https://kenney.nl
- Official source: https://kenney.nl/assets/city-kit-suburban
- Download: https://kenney.nl/media/pages/assets/city-kit-suburban/2c871b7af2-1745479373/kenney_city-kit-suburban_20.zip
- License: **Creative Commons Zero (CC0)**. The included license explicitly permits personal, educational and commercial use. Attribution is optional; Kenney is credited here.
- License text retained at `Assets/FloodSurvival/Art/KenneySuburban/License.txt`.
- Used for residential buildings, trees and color atlas. FBX files are the author's originals. A URP material using the supplied atlas is assigned to renderers in the generated scene.
- Original ZIP and extracted source are retained beside the project in `../SourceAssets/`.

## Official Unity packages

The project already had these packages installed; no additional package dependency was introduced for gameplay:

| Package | Version | Purpose |
|---|---|---|
| com.unity.render-pipelines.universal | 17.6.0 | URP rendering, post-processing |
| com.unity.inputsystem | 1.20.0 | Player controls and UI events |
| com.unity.ai.navigation | 2.0.14 | NavMeshSurface and navigation baking |
| com.unity.ugui | 2.6.0 | uGUI and TextMeshPro |
| com.unity.test-framework | 1.8.0 | Existing test infrastructure |
| com.unity.pipeline | 0.9.0-exp.1 | Existing local Editor tooling; not a gameplay requirement |

URP, Input System, AI Navigation, and uGUI each include a `LICENSE.md` naming the **Unity Companion License for Unity-dependent projects**. The package cache license texts were inspected and copied to `Documentation/Licenses/` for reference. Package usage remains subject to those licenses and the user's Unity Editor license. The exact dependency graph remains in `Packages/manifest.json` and `Packages/packages-lock.json`. Other packages from the original URP template were retained.

Unity Companion License: https://unity.com/legal/licenses/unity-companion-license

## TextMeshPro font

Liberation Sans is supplied with TMP essentials. Its **SIL Open Font License 1.1** and copyright notices are retained at `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. The font is used in menu, HUD and environmental text. Redistribute the license with the project/player documentation.

## Original prototype content

The home/market interiors, road layout, ridge shelter, dam, item shapes, procedural NPCs, materials, water shader, UI and C# gameplay are authored for this project. Siren, wind, rupture rumble and pickup confirmation are generated mathematically in `AtmosphereAudio.cs`; there are no downloaded audio files or external audio attribution obligations. No generated image service or paid asset library was used.
