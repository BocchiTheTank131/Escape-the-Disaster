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
                if(!item || item.kind!=kind || string.IsNullOrEmpty(item.displayName))throw new Exception("Persisted item invalid: "+path);
                report.AppendLine("- PASS — "+kind+": "+item.displayName+" / correct kind and text after disk reload");
            }
            EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
            int missing=0;
            foreach(var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach(var t in go.GetComponentsInChildren<Transform>(true))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if(missing!=0)throw new Exception("Missing scene scripts: "+missing);
            var game=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if(!game || !game.player || !game.ui || !game.objectives || !game.npcs || !game.flood || !game.audioSystem)throw new Exception("System wiring lost after scene reload");
            report.AppendLine("- PASS — Scene reload has zero missing scripts and all manager references intact");
            Directory.CreateDirectory("Verification"); File.WriteAllText("Verification/SavedAssets.md",report.ToString());
            return "All four item definitions reloaded from disk; scene references intact";
        }
    }
}
