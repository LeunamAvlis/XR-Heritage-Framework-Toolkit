#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace HeritageXR.Framework.Editor
{
    public class XRHeritageSpecImporter : EditorWindow
    {
        private string jsonFilePath = "";
        private TextAsset selectedJsonAsset;
        private Vector2 scrollPos;

        [MenuItem("Tools/XR Heritage Framework/Import Experience Spec (JSON)", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<XRHeritageSpecImporter>("XR Heritage Importer");
            window.minSize = new Vector2(450, 350);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("🏛️ XR Heritage Framework: Spec Importer", EditorStyles.boldLabel);
            GUILayout.Label("Load an AI-generated Experience Specification (JSON) into Unity.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);

            selectedJsonAsset = (TextAsset)EditorGUILayout.ObjectField("JSON Asset:", selectedJsonAsset, typeof(TextAsset), false);

            if (selectedJsonAsset != null)
            {
                jsonFilePath = AssetDatabase.GetAssetPath(selectedJsonAsset);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                jsonFilePath = EditorGUILayout.TextField("Or File Path:", jsonFilePath);
                if (GUILayout.Button("Browse...", GUILayout.Width(75)))
                {
                    string path = EditorUtility.OpenFilePanel("Select Heritage Spec JSON", Application.dataPath, "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        jsonFilePath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(12);

            if (GUILayout.Button("🚀 Import & Generate ScriptableObject", GUILayout.Height(38)))
            {
                ExecuteImport();
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button("🎨 Setup Scene with Heritage UI Rig", GUILayout.Height(30)))
            {
                SetupSceneHierarchy();
            }

            EditorGUILayout.Space(10);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("Active Specs in Project: Look in 'Assets/XRHeritageData/'", EditorStyles.miniLabel);
        }

        private void ExecuteImport()
        {
            string jsonText = "";

            if (selectedJsonAsset != null)
            {
                jsonText = selectedJsonAsset.text;
            }
            else if (!string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath))
            {
                jsonText = File.ReadAllText(jsonFilePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid JSON specification file.", "OK");
                return;
            }

            try
            {
                // Create target directory
                string targetDir = "Assets/XRHeritageData";
                if (!AssetDatabase.IsValidFolder(targetDir))
                {
                    AssetDatabase.CreateFolder("Assets", "XRHeritageData");
                }

                // Parse into ScriptableObject
                ArtworkExperienceData data = ScriptableObject.CreateInstance<ArtworkExperienceData>();
                EditorJsonUtility.FromJsonOverwrite(jsonText, data);

                string safeName = string.IsNullOrEmpty(data.experienceId) ? "ImportedExperience" : data.experienceId;
                string assetPath = $"{targetDir}/{safeName}_Data.asset";

                AssetDatabase.CreateAsset(data, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = data;
                EditorUtility.DisplayDialog("Success!", $"Experience Data created at:\n{assetPath}", "OK");

                // Auto-bind to ModernContentManager in scene if present
                var mgr = UnityEngine.Object.FindAnyObjectByType<ModernContentManager>();
                if (mgr != null)
                {
                    Undo.RecordObject(mgr, "Assign Experience Data");
                    SerializedObject serializedObj = new SerializedObject(mgr);
                    SerializedProperty prop = serializedObj.FindProperty("experienceData");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = data;
                        serializedObj.ApplyModifiedProperties();
                        Debug.Log("[XR Heritage Framework] Auto-assigned data to ModernContentManager in open scene.");
                    }
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to parse JSON:\n{ex.Message}", "OK");
                Debug.LogError(ex);
            }
        }

        private void SetupSceneHierarchy()
        {
            GameObject existing = GameObject.Find("XR_Heritage_Experience_Rig");
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Notice", "XR_Heritage_Experience_Rig already exists in the scene.", "OK");
                Selection.activeGameObject = existing;
                return;
            }

            GameObject rig = new GameObject("XR_Heritage_Experience_Rig");
            rig.AddComponent<ModernContentManager>();
            rig.AddComponent<ArtworkLayerToggle>();
            rig.AddComponent<InAppTamSurveyManager>();

            Undo.RegisterCreatedObjectUndo(rig, "Create Heritage XR Rig");
            Selection.activeGameObject = rig;
            EditorUtility.DisplayDialog("Setup Complete", "Created 'XR_Heritage_Experience_Rig' with ModernContentManager, ArtworkLayerToggle, and InAppTamSurveyManager.", "OK");
        }
    }
}
#endif
