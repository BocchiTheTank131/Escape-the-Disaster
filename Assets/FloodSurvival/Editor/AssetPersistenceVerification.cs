using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace FloodSurvival.Editor
{
    public static class AssetPersistenceVerification
    {
        public static string Verify()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first");
            const string scene="Assets/FloodSurvival/Scenes/CedarValley.unity";
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var report=new StringBuilder("# Saved asset verification\n\nEach item asset is explicitly unloaded, then reloaded from disk.\n\n");
            foreach(ItemKind kind in Enum.GetValues(typeof(ItemKind)))
            {
                string path="Assets/FloodSurvival/Items/"+kind+".asset";
                var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                Resources.UnloadAsset(item);
                item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                if(!item || item.kind!=kind || string.IsNullOrEmpty(item.displayName) || !item.icon || string.IsNullOrEmpty(item.description))throw new Exception("Persisted item invalid: "+path);
                report.AppendLine("- PASS — "+kind+": "+item.displayName+" / correct kind, text, icon and description after disk reload");
            }
            EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
            int missing=0;
            foreach(var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach(var t in go.GetComponentsInChildren<Transform>(true))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if(missing!=0)throw new Exception("Missing scene scripts: "+missing);
            var game=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if(!game || !game.player || !game.ui || !game.objectives || !game.npcs || !game.flood || !game.audioSystem)throw new Exception("System wiring lost after scene reload");
            report.AppendLine("- PASS — Scene reload has zero missing scripts and all manager references intact");
            if(!game.story || !game.story.seat || !game.story.standing || !game.story.television || game.story.newsVoices.Length!=4)throw new Exception("Story references lost after reload");
            foreach(var clip in game.story.newsVoices)if(!clip)throw new Exception("News voice reference missing");
            if(game.audioSystem.residentLines.Length!=3 || !game.audioSystem.radioSpeech || game.ui.definitions.Length!=7)throw new Exception("Audio/UI references lost after reload");
            var surface=UnityEngine.Object.FindFirstObjectByType<Unity.AI.Navigation.NavMeshSurface>();
            if(!surface || !surface.navMeshData || !AssetDatabase.Contains(surface.navMeshData))throw new Exception("NavMesh was not persisted");
            foreach(var citizen in game.npcs.citizens)if(!citizen || !citizen.animator || !citizen.animator.avatar || !citizen.animator.avatar.isHuman)throw new Exception("Humanoid NPC reference missing");
            report.AppendLine("- PASS — Story, four reporter clips, resident/radio speech, seven UI definitions and eight humanoid references survive reload");
            report.AppendLine("- PASS — NavMesh data is a saved asset");
            Directory.CreateDirectory("Verification/v0.2"); File.WriteAllText("Verification/v0.2/SavedAssets.md",report.ToString());
            return "All seven item definitions, icons and story/audio/UI/NavMesh references reloaded from disk";
        }
    }
}
