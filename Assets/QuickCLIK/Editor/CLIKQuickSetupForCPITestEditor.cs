using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrazyLabsHubs.Editor
{

    public class CLIKQuickSetupForCPITestEditor : EditorWindow
    {

        SceneAsset gameSceneAsset;
        string teamName;
        string gameName;

        [MenuItem("Quick CLIK/Copy BundleID to clipboard")]
        static void CopyBundleIDToClipboard()
        {
            var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            GUIUtility.systemCopyBuffer = bundleId;
        }

        [MenuItem("Quick CLIK/Quick setup for CPI test")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            CLIKQuickSetupForCPITestEditor window = (CLIKQuickSetupForCPITestEditor)EditorWindow.GetWindow(typeof(CLIKQuickSetupForCPITestEditor));
            window.Show();
            window.teamName = PlayerSettings.companyName;
            window.gameName = PlayerSettings.productName;

            window.iOSSelected = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            window.androidSelected = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
        }

        bool iOSSelected = false;
        bool androidSelected = false;

        bool createDirectionaLight = true;
        bool step2Enabled = true;

        void OnGUI()
        {
            BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            iOSSelected = GUILayout.Toggle(iOSSelected, "iOS", "Button");
            if (EditorGUI.EndChangeCheck())
            {
                if (iOSSelected)
                {
                    androidSelected = false;
                }
            }

            EditorGUI.BeginChangeCheck();
            androidSelected = GUILayout.Toggle(androidSelected, "Android", "Button");
            if (EditorGUI.EndChangeCheck())
            {
                if (androidSelected)
                {
                    iOSSelected = false;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            //first level scene
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Step 1", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Create Directional Light?");
            createDirectionaLight = EditorGUILayout.Toggle(createDirectionaLight);
            EditorGUILayout.HelpBox("Should directional light be added to QuickClick scene to fix loading level scene with lighting being dark.", MessageType.Info);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("First Level (Game Scene)");

            gameSceneAsset = (SceneAsset)EditorGUILayout.ObjectField(gameSceneAsset, typeof(SceneAsset), false);
            EditorGUILayout.EndHorizontal();
            var hasScene = gameSceneAsset != null;
            if (!hasScene)
            {
                EditorGUILayout.HelpBox("You need to specify first level (scene) of your game!", MessageType.Error);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //bundle id check

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Step 2", EditorStyles.boldLabel);
            step2Enabled = EditorGUILayout.Toggle(step2Enabled);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            var hasDefaultCompanyAndProductName = step2Enabled;

            if (step2Enabled)
            {
                hasDefaultCompanyAndProductName = string.Equals(teamName, "Default Company") || string.Equals(teamName, "DefaultCompany");

                EditorGUILayout.BeginHorizontal();

                var defaultLabelWidth = EditorGUIUtility.labelWidth;
                teamName = EditorGUILayout.TextField(teamName);
                gameName = EditorGUILayout.TextField(gameName);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Company Name (Team)");
                EditorGUILayout.LabelField("Product Name (Game)");

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Your bundle id will look like this:", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                var x = teamName.Trim();
                x = Regex.Replace(x, @"\s+", "");
                var y = gameName.Trim();
                y = Regex.Replace(y, @"\s+", "");
                var bundleId = string.Format("com.{0}.{1}", x, y).ToLower();
                EditorGUILayout.LabelField(bundleId);
                GUI.enabled = true;
                if (GUILayout.Button("copy to clipboard"))
                {
                    GUIUtility.systemCopyBuffer = bundleId;
                }
                EditorGUILayout.EndHorizontal();

                if (hasDefaultCompanyAndProductName)
                {
                    EditorGUILayout.HelpBox("You need to specify valid bundle id! (usualy in format com.TeamName.GameName", MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Bundle ID step will be skipped.", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //finish
            EditorGUILayout.LabelField("Step 3", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUI.enabled = hasScene && !hasDefaultCompanyAndProductName;

            if (GUILayout.Button("Finish"))
            {
                if (iOSSelected && EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                }
                else if (androidSelected && EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                }

                //Create the empty scene with tt plugin
                var scenePath = @"Assets/QuickSetupScene.unity";
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                if (!hasDefaultCompanyAndProductName)
                {
                    PlayerSettings.companyName = teamName;
                    PlayerSettings.productName = gameName;
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, string.Format("com.{0}.{1}", teamName, gameName).ToLower());
                }

                //Create a GameObject that will contain loader script for ttplugins
                GameObject go = new GameObject();
                go.name = "QuickSetupManager";
                var quickCLICK = go.AddComponent<QuickLoadCLIK>();
                quickCLICK.sceneToLoadName = gameSceneAsset.name;
                CreateDirectionalLight();

                //Add component here for the activating the tt plugins
                EditorSceneManager.SaveScene(scene, scenePath);

                //Add created Scene to the build settings
                var existingScenes = EditorBuildSettings.scenes;
                var withoutDuplicates = existingScenes.GroupBy(x => x.path).Select(y => y.First()).ToList();

                var alreadyContainsScene = withoutDuplicates.Any(x => string.Equals(x.path, scenePath));

                if (alreadyContainsScene)
                {
                    var sceneToRemove = withoutDuplicates.SingleOrDefault(x => string.Equals(x.path, scenePath));
                    if (sceneToRemove != null)
                    {
                        withoutDuplicates.Remove(sceneToRemove);
                    }
                }

                EditorBuildSettings.scenes = AddSceneToBuildAtIndex(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), 0).ToArray();

                EditorBuildSettings.scenes = AddSceneToBuildAtIndex(gameSceneAsset, 1).ToArray();

                //Change the unity build settings for android
                //Unity supported versions as per https://sites.google.com/tabtale.com/clhelpcenter/clik-plugin
                //2019.2.17, 2019.3.15, 2019.4.28, 2020.1.14 

                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, 0);
                PlayerSettings.stripEngineCode = false;

                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

#if UNITY_2020_1_OR_NEWER

                PlayerSettings.Android.minifyWithR8 = false;
                PlayerSettings.Android.minifyDebug = false;

                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)31;
#endif

#if UNITY_2020_3_OR_NEWER
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
#endif
            }

            GUI.enabled = true;
        }

        public List<EditorBuildSettingsScene> AddSceneToBuildAtIndex(SceneAsset whichScene, int slot)
        {
            var existingScenes = EditorBuildSettings.scenes.ToList();

            //clamp the slot if we are trying to add a scene at slot 5 lets say but there are only two scenes
            slot = Mathf.Clamp(slot, 0, existingScenes.Count);

            //if the scene exists already we just need to move it to the index
            var scene = existingScenes.FirstOrDefault(x => x.path == AssetDatabase.GetAssetOrScenePath(whichScene));
            var sceneAlreadyInExisting = scene != null;

            if (sceneAlreadyInExisting)
            {
                existingScenes.Remove(scene);

            }
            else
            {
                scene = new EditorBuildSettingsScene(AssetDatabase.GetAssetOrScenePath(whichScene), true);

            }
            existingScenes.Insert(slot, scene);
            return existingScenes;
        }

        void CreateDirectionalLight()
        {
            var directionalLightPrefab = Resources.Load("QuickDirectionalLight");
            var directionalLight = GameObject.Instantiate(directionalLightPrefab);
        }
    }
}