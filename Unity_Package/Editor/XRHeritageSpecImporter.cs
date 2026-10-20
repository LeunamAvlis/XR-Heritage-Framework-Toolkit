#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace HeritageXR.Framework.Editor
{
    public class XRHeritageSpecImporter : EditorWindow
    {
        private string jsonFilePath = "";
        private TextAsset selectedJsonAsset;

        [MenuItem("Tools/XR Heritage Framework/Import Experience Spec (JSON)", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<XRHeritageSpecImporter>("XR Heritage Importer");
            window.minSize = new Vector2(450, 360);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("🏛️ XR Heritage Framework: Spec Importer", EditorStyles.boldLabel);
            GUILayout.Label("Load an AI-generated Experience Specification (JSON) into Unity.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);

            selectedJsonAsset = (TextAsset)EditorGUILayout.ObjectField("JSON Asset (in project):", selectedJsonAsset, typeof(TextAsset), false);

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

            EditorGUILayout.Space(14);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(4);
            GUILayout.Label("🤖 AI Conception Layer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Need to generate a spec? Use our master AI prompt:", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Copy Master AI Prompt", GUILayout.Height(28)))
            {
                CopyMasterPromptToClipboard();
            }
            if (GUILayout.Button("🌐 Open on GitHub", GUILayout.Height(28)))
            {
                Application.OpenURL("https://github.com/LeunamAvlis/XR-Heritage-Framework-Toolkit/blob/main/ALL_IN_ONE_PROMPT.txt");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("Target Output: 'Assets/XRHeritageData/'", EditorStyles.miniLabel);
        }

        private void CopyMasterPromptToClipboard()
        {
            string prompt = MasterPromptText;
            EditorGUIUtility.systemCopyBuffer = prompt;
            EditorUtility.DisplayDialog("Prompt Copied!", 
                "The XR Heritage Master AI Prompt has been copied to your clipboard!\n\nPaste it into ChatGPT, Claude, Perplexity, or any LLM assistant to start designing your exhibition.", 
                "OK");
        }

        private const string MasterPromptText = @"You are the XR Heritage Architect, an expert AI system developed from doctoral research at the School of Arts, Universidade Católica Portuguesa (UCP). Your mission is to assist museum curators, contemporary artists, exhibition designers, and researchers in conceptualizing, negotiating, and specifying Extended Reality (AR, VR, MR) experiences for cultural heritage and museum exhibitions.

### CORE PRINCIPLES
1. Artwork Primacy & Artist Autonomy (Non-Negotiable): Technology must serve the artwork, never overshadow or compete with it. Distinctly protect the artist's voice. Differentiate meaningful mediation from digital gimmickry.
2. Multi-Perspective Co-Design: Synthesize Curatorial Perspective, Artist Perspective, and Visitor Experience.
3. Structured Output: Provide concise responses and output a valid HeritageExperienceSpec JSON for Unity.

### INTAKE DIAGNOSTIC
Before proposing solutions, confirm:
1. User Role: Curator | Artist | XR Developer | Educator | Researcher
2. Artwork Medium: 2D (Paint/Photo) | 3D (Sculpture/Relic) | Spatial/Installation | Intangible Heritage
3. Exhibition Space & Conditions: Lighting, noise, crowd flow
4. Primary Goal: Revealing drafts | Curatorial context | Material deconstruction | Accessibility
5. Target Device: Tablet AR | Phone AR | Spatial MR | VR

### OUTPUT FORMAT
Provide the final experience specification as a valid JSON block containing: experienceId, metadata, exhibitionConstraints, chapters (with PhysicalArtwork, ArtistVoice, CuratorialLens perspectives and hotspots), and tamEvaluation.";


        private void ExecuteImport()
        {
            string rawJson = "";

            if (selectedJsonAsset != null)
            {
                rawJson = selectedJsonAsset.text;
            }
            else if (!string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath))
            {
                rawJson = File.ReadAllText(jsonFilePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid JSON specification file.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(rawJson))
            {
                EditorUtility.DisplayDialog("Error", "Selected JSON file is empty.", "OK");
                return;
            }

            try
            {
                // Ensure target directory exists
                string targetDir = "Assets/XRHeritageData";
                if (!AssetDatabase.IsValidFolder(targetDir))
                {
                    AssetDatabase.CreateFolder("Assets", "XRHeritageData");
                }

                // Preprocess JSON for Unity's JsonUtility (convert string enums to integer values)
                string preparedJson = PreprocessJsonEnums(rawJson);

                // Create ScriptableObject instance
                ArtworkExperienceData data = ScriptableObject.CreateInstance<ArtworkExperienceData>();

                // Use standard JsonUtility (reliable with ScriptableObjects)
                JsonUtility.FromJsonOverwrite(preparedJson, data);

                // Fallback check: if experienceId is still empty, attempt direct regex extraction for core fields
                if (string.IsNullOrEmpty(data.experienceId))
                {
                    Debug.LogWarning("[XR Heritage] Standard deserializer yielded empty ID. Running deep parser...");
                    PopulateFallbackData(rawJson, data);
                }

                string safeName = string.IsNullOrEmpty(data.experienceId) ? "HeritageExperience" : data.experienceId;
                string assetPath = $"{targetDir}/{safeName}_Data.asset";

                // Save asset to database
                AssetDatabase.CreateAsset(data, assetPath);
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = data;
                EditorUtility.DisplayDialog("Success!", 
                    $"Experience Data successfully created!\n\nTitle: {data.metadata.title}\nArtist: {data.metadata.artistName}\nChapters: {data.chapters.Count}\nSaved at: {assetPath}", 
                    "OK");

                // Auto-bind to ModernContentManager in the open scene if present
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

        // Unity's JsonUtility requires enums to be numeric values (0, 1, 2) rather than string names
        private string PreprocessJsonEnums(string json)
        {
            string processed = json;

            // PerspectiveType enums
            processed = Regex.Replace(processed, "\"perspective\"\\s*:\\s*\"PhysicalArtwork\"", "\"perspective\": 0", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"perspective\"\\s*:\\s*\"ArtistVoice\"", "\"perspective\": 1", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"perspective\"\\s*:\\s*\"CuratorialLens\"", "\"perspective\": 2", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"perspective\"\\s*:\\s*\"ComparativeAnalysis\"", "\"perspective\": 3", RegexOptions.IgnoreCase);

            // MediaContentType enums
            processed = Regex.Replace(processed, "\"contentType\"\\s*:\\s*\"Image\"", "\"contentType\": 0", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"contentType\"\\s*:\\s*\"AudioVoiceover\"", "\"contentType\": 1", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"contentType\"\\s*:\\s*\"VideoNarrative\"", "\"contentType\": 2", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"contentType\"\\s*:\\s*\"Model3D\"", "\"contentType\": 3", RegexOptions.IgnoreCase);
            processed = Regex.Replace(processed, "\"contentType\"\\s*:\\s*\"InteractiveComparisonSlider\"", "\"contentType\": 4", RegexOptions.IgnoreCase);

            return processed;
        }

        // Robust fallback in case of nested mismatch
        private void PopulateFallbackData(string json, ArtworkExperienceData data)
        {
            var matchId = Regex.Match(json, "\"experienceId\"\\s*:\\s*\"([^\"]+)\"");
            if (matchId.Success) data.experienceId = matchId.Groups[1].Value;

            var matchTitle = Regex.Match(json, "\"title\"\\s*:\\s*\"([^\"]+)\"");
            if (matchTitle.Success) data.metadata.title = matchTitle.Groups[1].Value;

            var matchArtist = Regex.Match(json, "\"artistName\"\\s*:\\s*\"([^\"]+)\"");
            if (matchArtist.Success) data.metadata.artistName = matchArtist.Groups[1].Value;

            var matchYear = Regex.Match(json, "\"creationYear\"\\s*:\\s*\"([^\"]+)\"");
            if (matchYear.Success) data.metadata.creationYear = matchYear.Groups[1].Value;

            var matchMedium = Regex.Match(json, "\"medium\"\\s*:\\s*\"([^\"]+)\"");
            if (matchMedium.Success) data.metadata.medium = matchMedium.Groups[1].Value;

            var matchDimensions = Regex.Match(json, "\"physicalDimensions\"\\s*:\\s*\"([^\"]+)\"");
            if (matchDimensions.Success) data.metadata.physicalDimensions = matchDimensions.Groups[1].Value;

            var matchMuseum = Regex.Match(json, "\"museumOrCollection\"\\s*:\\s*\"([^\"]+)\"");
            if (matchMuseum.Success) data.metadata.museumOrCollection = matchMuseum.Groups[1].Value;

            var matchCurator = Regex.Match(json, "\"curatorName\"\\s*:\\s*\"([^\"]+)\"");
            if (matchCurator.Success) data.metadata.curatorName = matchCurator.Groups[1].Value;
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
