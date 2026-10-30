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
        private ArtworkExperienceData lastImportedData;
        private Vector2 scrollPos;

        [MenuItem("Tools/XR Heritage Framework/Experience Setup Wizard", false, 5)]
        [MenuItem("Tools/XR Heritage Framework/Import Experience Spec (JSON)", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<XRHeritageSpecImporter>("XR Heritage Wizard");
            window.minSize = new Vector2(490, 560);
            window.Show();
        }

        [MenuItem("Tools/XR Heritage Framework/Build Playable XR Sample Scene", false, 15)]
        public static void QuickBuildScene()
        {
            ArtworkExperienceData data = Selection.activeObject as ArtworkExperienceData;
            if (data == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ArtworkExperienceData");
                if (guids.Length > 0)
                {
                    data = AssetDatabase.LoadAssetAtPath<ArtworkExperienceData>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
            XRHeritageSceneBuilder.BuildCompleteExperienceScene(data);
        }

        [MenuItem("Tools/XR Heritage Framework/Install Required XR SDKs", false, 20)]
        public static void ShowSdkInstaller()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "XR Heritage Framework: SDK Installer",
                "Select which hardware stack you are targeting for this museum project:\n\n" +
                "• Mobile AR: Installs AR Foundation, ARCore (Android), and ARKit (iOS).\n" +
                "• VR / OpenXR: Installs OpenXR, XR Plugin Management, and XR Interaction Toolkit.\n" +
                "• Mixed Reality: Installs OpenXR and AR Foundation for spatial passthrough.",
                "Mobile AR SDKs",
                "Cancel",
                "VR / OpenXR SDKs"
            );

            if (choice == 0)
            {
                XRHeritageSDKManager.InstallMobileArSdk();
            }
            else if (choice == 2)
            {
                XRHeritageSDKManager.InstallVrOpenXrSdk();
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Space(8);
            GUILayout.Label("🏛️ XR Heritage Framework — Guided Experience Wizard", EditorStyles.boldLabel);
            GUILayout.Label("Follow the 3 steps below to co-design, import, and build a playable museum XR experience.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);

            // =====================================================================
            // STEP 1: AI CONCEPTION & VISUAL PDF REPORT STUDIO
            // =====================================================================
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("STEP 1: Co-Creation, Visual Maquettes & PDF Report", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Use our AI Master Prompt or open the Visual Co-Creation Studio to preview spatial gallery maquettes, export a PDF Exhibition Dossier, and download the .json file.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Copy Master AI Prompt", GUILayout.Height(28)))
            {
                CopyMasterPromptToClipboard();
            }
            if (GUILayout.Button("📐 Visual Studio & PDF Report", GUILayout.Height(28)))
            {
                Application.OpenURL("https://htmlpreview.github.io/?https://github.com/LeunamAvlis/XR-Heritage-Framework-Toolkit/blob/main/Web_Companion/index.html");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // =====================================================================
            // STEP 2: IMPORT JSON SPECIFICATION & AUTO-INSTALL XR SDKS
            // =====================================================================
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("STEP 2: Import JSON Spec & Check Required SDKs", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Select the .json specification file generated in Step 1 to create your Unity ScriptableObject and install the matching AR/VR SDKs.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);

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

            EditorGUILayout.Space(6);

            if (GUILayout.Button("🚀 1. Import JSON & Check Required SDKs", GUILayout.Height(36)))
            {
                ExecuteImport();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📱 Install Mobile AR SDKs", GUILayout.Height(24)))
            {
                XRHeritageSDKManager.InstallMobileArSdk();
            }
            if (GUILayout.Button("🥽 Install VR / OpenXR SDKs", GUILayout.Height(24)))
            {
                XRHeritageSDKManager.InstallVrOpenXrSdk();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // =====================================================================
            // STEP 3: BUILD PLAYABLE SAMPLE SCENE & PREFABS
            // =====================================================================
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("STEP 3: Build Playable Sample Scene & Prefabs", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Automatically generates the XR/AR Camera Rig, 3D Artwork Stand with spatial Hotspot Markers, fully wired Multi-Perspective Museum UI Canvas, In-App TAM Survey Modal, and saves reusable Prefabs.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4);

            lastImportedData = (ArtworkExperienceData)EditorGUILayout.ObjectField("Experience Data Asset:", lastImportedData, typeof(ArtworkExperienceData), false);

            EditorGUILayout.Space(6);

            GUI.backgroundColor = new Color(0.88f, 0.72f, 0.38f);
            if (GUILayout.Button("🎨 2. Build Complete Playable Scene & Prefabs", GUILayout.Height(40)))
            {
                SetupSceneHierarchy();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            GUILayout.Label("Target Output: 'Assets/XRHeritageData/' & 'Assets/XRHeritageData/Prefabs/'", EditorStyles.miniLabel);

            EditorGUILayout.EndScrollView();
        }

        private void CopyMasterPromptToClipboard()
        {
            string prompt = MasterPromptText;
            EditorGUIUtility.systemCopyBuffer = prompt;
            EditorUtility.DisplayDialog("Prompt Copied!", 
                "The XR Heritage Master AI Prompt has been copied to your clipboard!\n\nPaste it into ChatGPT, Claude, Perplexity, or Gemini to generate both:\n1. Your Visual & Curatorial Exhibition Report\n2. Your Unity .json Specification", 
                "OK");
        }

        private const string MasterPromptText = @"You are the XR Heritage Architect, an expert AI system developed from doctoral research at the School of Arts, Universidade Católica Portuguesa (UCP). Your mission is to assist museum curators, contemporary artists, exhibition designers, and researchers in conceptualizing, negotiating, and specifying Extended Reality (AR, VR, MR) experiences for cultural heritage and museum exhibitions.

### CORE PRINCIPLES
1. Artwork Primacy & Artist Autonomy (Non-Negotiable): Technology must serve the artwork, never overshadow or compete with it. Distinctly protect the artist's voice. Differentiate meaningful mediation from digital gimmickry.
2. Multi-Perspective Co-Design: Synthesize Curatorial Perspective, Artist Perspective, and Visitor Experience.
3. Dual-Deliverable Output: When the co-design is complete, ALWAYS provide BOTH:
   - DELIVERABLE A: A comprehensive Exhibition Conception Report (suitable for saving/printing as PDF) answering all project parameters (Experience Type AR/VR/MR/XR, Author/Artist, Curator, Artwork Metadata, Spatial Ergonomics, Artist Autonomy Charter, Multi-Perspective Chapters, TAM Survey, and a Visual Spatial Maquette Diagram + AI Image Generation Prompt for a 3D gallery mockup).
   - DELIVERABLE B: A valid HeritageExperienceSpec JSON code block ready for import into the Unity Package and the Visual Co-Creation Studio.

### INTAKE DIAGNOSTIC
Before proposing solutions, confirm:
1. User Role: Curator | Artist | XR Developer | Educator | Researcher
2. Artwork Medium: 2D (Paint/Photo) | 3D (Sculpture/Relic) | Spatial/Installation | Intangible Heritage
3. Exhibition Space & Conditions: Lighting, noise, crowd flow
4. Primary Goal: Revealing drafts | Curatorial context | Material deconstruction | Accessibility
5. Target Device: Tablet AR | Phone AR | Spatial MR | VR";


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

                lastImportedData = data;
                Selection.activeObject = data;

                bool buildNow = EditorUtility.DisplayDialog("Success! Experience Data Imported", 
                    $"Experience Data successfully created!\n\n" +
                    $"• Title: {data.metadata.title}\n" +
                    $"• Artist: {data.metadata.artistName}\n" +
                    $"• Modality: {data.exhibitionConstraints.deviceTarget}\n" +
                    $"• Chapters: {data.chapters.Count}\n" +
                    $"• Saved at: {assetPath}\n\n" +
                    "Would you like to automatically build the complete playable XR Sample Scene & Prefabs now?", 
                    "Yes, Build Scene & Prefabs!",
                    "Later");

                if (buildNow)
                {
                    XRHeritageSceneBuilder.BuildCompleteExperienceScene(data);
                }

                // Check target hardware and prompt to install required Unity SDKs
                XRHeritageSDKManager.CheckAndPromptSdkForExperience(data);
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
            if (lastImportedData == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ArtworkExperienceData");
                if (guids.Length > 0)
                {
                    lastImportedData = AssetDatabase.LoadAssetAtPath<ArtworkExperienceData>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            XRHeritageSceneBuilder.BuildCompleteExperienceScene(lastImportedData);
        }
    }
}
#endif
