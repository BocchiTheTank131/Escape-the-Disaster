# Free assets and dependencies — Prototype v0.2

No paid, trial-only, subscription-required or pirated content was used. Official source pages and archive licenses were checked on 23 October 2026. Original downloads are retained outside the Unity project in `../SourceAssets/`.

## Imported art

| Resource | Official source | License | Use / retained license |
|---|---|---|---|
| Kenney City Kit (Suburban) 2.0 | https://kenney.nl/assets/city-kit-suburban | CC0 | Existing homes, trees, atlas; `Assets/FloodSurvival/Art/KenneySuburban/License.txt` |
| Kenney Furniture Kit, archive 2.0 | https://kenney.nl/assets/furniture-kit | CC0 | Sofa, TV, kitchen, bedroom, garage, books, plants and boxes; `Art/furniture-kit/License.txt` |
| Kenney Car Kit | https://kenney.nl/assets/car-kit | CC0 | Five parked vehicles; `Art/car-kit/License.txt` |
| Kenney Animated Characters Survivors, archive 1.1 | https://kenney.nl/assets/animated-characters-survivors | CC0 | Human survivor mesh, PNG skins, idle/run clips; `Art/Survivors/License.txt` |

`Art/` paths above are relative to `Assets/FloodSurvival/`. Each included Kenney license explicitly permits personal, educational and commercial projects. Attribution is optional; credit to Kenney is retained here and alongside the player. CC0 deed: https://creativecommons.org/publicdomain/zero/1.0/ . No zombie/enemy gameplay is used.

FBX materials were converted to URP materials. Human animation clips use their own humanoid avatars and retarget onto the survivor model. Redundant skin source SVGs that exceeded Unity's gradient-key importer limit were moved to `Archive/SurvivorSkinSVG/`; the original PNG skins and source archive remain intact.

## Speech authoring tool

Original scripted reporter, resident and radio dialogue was rendered offline with **eSpeak NG 1.52.0**, a free, open-source speech synthesizer. Release/source: https://github.com/espeak-ng/espeak-ng/releases/tag/1.52.0 . Tool license: GNU GPL v3, https://github.com/espeak-ng/espeak-ng/blob/1.52.0/COPYING . The license is retained as `Documentation/Licenses/eSpeak-NG-GPL3.txt`.

The game ships authored WAV output only; it does **not** embed or require the eSpeak program, phoneme data or speech library. GPL section 2 distinguishes running a program and its output from conveying covered program code. The text is original project dialogue (including the user's requested fictional news announcement), not a reading of third-party copyrighted material. No outside TV footage, celebrity imitation or paid voice service is used.

The authoring MSI was read and extracted portably without installation. The optional tool/download and reproducible rendering helper remain in `../SourceAssets/espeak-ng/`, outside the game and player package. Do not include that whole authoring-tool directory when distributing the game. Synthetic English voices are deliberately a prototype compromise.

## Official Unity packages

No new Unity package dependency was added for v0.2. Existing packages remain pinned in `Packages/manifest.json` and `Packages/packages-lock.json`.

| Package | Version | Purpose |
|---|---|---|
| com.unity.render-pipelines.universal | 17.6.0 | URP rendering and Volume grading |
| com.unity.inputsystem | 1.20.0 | Player input and uGUI events |
| com.unity.ai.navigation | 2.0.14 | NavMesh surfaces, bake and agents |
| com.unity.ugui | 2.6.0 | uGUI and TextMeshPro |
| com.unity.test-framework | 1.8.0 | Existing test infrastructure |
| com.unity.pipeline | 0.9.0-exp.1 | Existing local Editor tooling |

URP, Input System, AI Navigation and uGUI name the Unity Companion License for Unity-dependent projects in their package `LICENSE.md`. Copies inspected during v0.1 remain in `Documentation/Licenses/`. Usage remains subject to those terms and the user's Unity Editor license. https://unity.com/legal/licenses/unity-companion-license . Other original template packages were retained.

## Font and original content

Liberation Sans is supplied with TMP essentials under **SIL Open Font License 1.1**. Copyright and license: `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`; also copied into player licenses.

The CLOUD FOX & SUNBIRD TV cartoon, cartoon music, bird silhouette, item icons, UI, shaders, simplified structural interiors/dam, deterministic flood, particles, gameplay code, sirens, wind, crack/collapse effects, footsteps and pickup sounds were authored for this project. The cartoon is a small original animated uGUI program with its own synthesized tune. All effect clips are synthesized once per attempt, not downloaded recordings. No generated image service or external audio library is required at runtime.

## Landscape and television revision (v0.2.1)

Poly Haven `rock_face_03` and `forest_ground_04` 2k diffuse/OpenGL normal textures are used for the continuous mountain terrain. Sources: https://polyhaven.com/a/rock_face_03 and https://polyhaven.com/a/forest_ground_04 . Poly Haven's official asset license declares **CC0 1.0**, including commercial use: https://polyhaven.com/license . Asset downloads and SHA-256 hashes are retained in `Assets/FloodSurvival/Art/PolyHavenLandscape/Sources.json`, with `License.txt` beside the textures. Powered by Poly Haven — https://polyhaven.com . The API was used only to obtain authoring files; the game has no online API dependency.

The continuous heightfield, branching pine meshes, earth trail, television bezel, antialiased cartoon graphics and fictional CVN presenter/map/ticker are original project content. The existing original cartoon title, news text and free offline speech workflow are retained. The radio advisory was re-rendered to direct players to wooded high ground after removing the shelter.
The final lower slopes also use `aerial_grass_rock` 1k diffuse/OpenGL normal JPGs, from https://polyhaven.com/a/aerial_grass_rock under the same CC0 terms. Its exact authoring URLs and hashes are included in Sources.json.
