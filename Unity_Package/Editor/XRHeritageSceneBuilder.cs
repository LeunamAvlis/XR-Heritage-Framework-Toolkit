#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeritageXR.Framework.Editor
{
    public static class XRHeritageSceneBuilder
    {
        public static GameObject BuildCompleteExperienceScene(ArtworkExperienceData data)
        {
            // 1. Ensure folders exist for generated prefabs
            EnsureFolder("Assets", "XRHeritageData");
            EnsureFolder("Assets/XRHeritageData", "Prefabs");

            // 2. Ensure EventSystem exists in scene so UI is clickable immediately
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }

            // Remove old rig if replacing
            GameObject existing = GameObject.Find("XR_Heritage_Complete_Experience");
            if (existing != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace Existing Rig?",
                    "'XR_Heritage_Complete_Experience' already exists in the scene. Would you like to rebuild and replace it?",
                    "Rebuild & Replace",
                    "Cancel"
                );
                if (!replace) return existing;
                Undo.DestroyObjectImmediate(existing);
            }

            // Root container
            GameObject root = new GameObject("XR_Heritage_Complete_Experience");
            Undo.RegisterCreatedObjectUndo(root, "Create Complete XR Heritage Experience");

            var contentMgr = root.AddComponent<ModernContentManager>();
            var layerToggle = root.AddComponent<ArtworkLayerToggle>();
            var tamMgr = root.AddComponent<InAppTamSurveyManager>();
            var audioSrc = root.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;

            string deviceTarget = (data != null && data.exhibitionConstraints != null && !string.IsNullOrEmpty(data.exhibitionConstraints.deviceTarget))
                ? data.exhibitionConstraints.deviceTarget
                : "MuseumTabletAR";

            // 3. Create XR Camera & Tracking Rig (AR vs VR/MR)
            BuildXRCameraRig(root.transform, deviceTarget);

            // 4. Create 3D Physical Artwork Stand, Layers & Hotspot Markers
            BuildArtworkAndHotspots(root.transform, data, layerToggle, audioSrc);

            // 5. Create Dot Prefab for Pagination
            GameObject dotPrefab = CreateOrLoadDotPrefab();

            // 6. Build Fully Wired uGUI Canvas (Chapter Card + Perspective Tabs + TAM Survey Modal)
            BuildAndWireUserInterface(root.transform, data, contentMgr, layerToggle, tamMgr, dotPrefab);

            // 7. Save as a Reusable Prefab in Assets/XRHeritageData/Prefabs/
            string prefabSafeName = (data != null && !string.IsNullOrEmpty(data.experienceId)) ? data.experienceId : "SampleHeritage";
            string prefabPath = $"Assets/XRHeritageData/Prefabs/{prefabSafeName}_CompleteRig.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.UserAction);

            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();

            EditorUtility.DisplayDialog(
                "Playable XR Scene & Prefabs Created!",
                $"Complete XR Heritage scene setup built for target '{deviceTarget}'!\n\n" +
                "• XR Camera & Tracking Rig created\n" +
                "• 3D Artwork Target & Hotspot Markers placed\n" +
                "• Interactive Museum UI & TAM Survey wired\n" +
                $"• Prefab saved to: {prefabPath}\n\n" +
                "Press PLAY (▶) in the Unity Editor to test the experience right now!",
                "Awesome!"
            );

            return root;
        }

        private static void BuildXRCameraRig(Transform parent, string deviceTarget)
        {
            bool isVRorMR = deviceTarget.IndexOf("VR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            deviceTarget.IndexOf("Headset", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            deviceTarget.IndexOf("MR", StringComparison.OrdinalIgnoreCase) >= 0;

            GameObject xrRigRoot = new GameObject(isVRorMR ? "XR_Origin_VR_MR_Rig" : "XR_Origin_Mobile_AR_Rig");
            xrRigRoot.transform.SetParent(parent, false);
            xrRigRoot.transform.localPosition = Vector3.zero;

            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrRigRoot.transform, false);
            cameraOffset.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            // Check if a Main Camera exists; if not, create the XR Camera
            Camera mainCam = Camera.main;
            GameObject camObj;
            if (mainCam != null && mainCam.transform.parent == null)
            {
                camObj = mainCam.gameObject;
                camObj.name = isVRorMR ? "XR_Headset_Camera" : "AR_Device_Camera";
                camObj.transform.SetParent(cameraOffset.transform, false);
                camObj.transform.localPosition = Vector3.zero;
                camObj.transform.localRotation = Quaternion.identity;
            }
            else
            {
                camObj = new GameObject(isVRorMR ? "XR_Headset_Camera" : "AR_Device_Camera");
                camObj.tag = "MainCamera";
                camObj.transform.SetParent(cameraOffset.transform, false);
                camObj.transform.localPosition = Vector3.zero;
                var cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
                cam.nearClipPlane = 0.05f;
                camObj.AddComponent<AudioListener>();
            }

            if (isVRorMR)
            {
                // Add Left and Right Hand Controller Anchors for OpenXR / XRI
                GameObject leftHand = new GameObject("LeftHand_Controller_Anchor");
                leftHand.transform.SetParent(cameraOffset.transform, false);
                leftHand.transform.localPosition = new Vector3(-0.25f, -0.3f, 0.4f);

                GameObject rightHand = new GameObject("RightHand_Controller_Anchor");
                rightHand.transform.SetParent(cameraOffset.transform, false);
                rightHand.transform.localPosition = new Vector3(0.25f, -0.3f, 0.4f);
            }
            else
            {
                // Add AR Session Placeholder
                GameObject arSession = new GameObject("AR_Session_Placeholder");
                arSession.transform.SetParent(xrRigRoot.transform, false);
            }
        }

        private static void BuildArtworkAndHotspots(Transform parent, ArtworkExperienceData data, ArtworkLayerToggle layerToggle, AudioSource audioSrc)
        {
            GameObject standRoot = new GameObject("Artwork_Target_Anchor");
            standRoot.transform.SetParent(parent, false);
            standRoot.transform.localPosition = new Vector3(0f, 1.5f, 1.8f);

            // Gallery Wall Backing
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Gallery_Wall_Backdrop";
            wall.transform.SetParent(standRoot.transform, false);
            wall.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            wall.transform.localScale = new Vector3(2.6f, 2.2f, 0.05f);
            SetPrimitiveColor(wall, new Color(0.16f, 0.18f, 0.22f));

            // Physical Artwork Canvas Target (1.4m x 1.1m)
            float artWidth = 1.4f;
            float artHeight = 1.1f;
            GameObject artworkMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
            artworkMesh.name = "Physical_Artwork_Surface";
            artworkMesh.transform.SetParent(standRoot.transform, false);
            artworkMesh.transform.localPosition = Vector3.zero;
            artworkMesh.transform.localScale = new Vector3(artWidth, artHeight, 1f);
            SetPrimitiveColor(artworkMesh, new Color(0.25f, 0.30f, 0.38f));

            // 3 Multi-Perspective Visual Layer Roots
            GameObject artistLayer = new GameObject("Layer_ArtistVoice_Overlays");
            artistLayer.transform.SetParent(standRoot.transform, false);

            GameObject curatorLayer = new GameObject("Layer_CuratorialLens_Overlays");
            curatorLayer.transform.SetParent(standRoot.transform, false);

            GameObject spatial3DLayer = new GameObject("Layer_Spatial3D_Reconstruction");
            spatial3DLayer.transform.SetParent(standRoot.transform, false);

            // Spawn 3D Hotspot Pins from ArtworkExperienceData
            if (data != null && data.chapters != null)
            {
                int pinNumber = 1;
                foreach (var chap in data.chapters)
                {
                    if (chap.hotspots == null) continue;
                    foreach (var hs in chap.hotspots)
                    {
                        Transform targetLayer = standRoot.transform;
                        Color pinColor = new Color(0.16f, 0.75f, 0.5f); // Physical green

                        if (hs.perspective == PerspectiveType.ArtistVoice || chap.perspective == PerspectiveType.ArtistVoice)
                        {
                            targetLayer = artistLayer.transform;
                            pinColor = new Color(0.95f, 0.62f, 0.15f); // Artist amber
                        }
                        else if (hs.perspective == PerspectiveType.CuratorialLens || chap.perspective == PerspectiveType.CuratorialLens)
                        {
                            targetLayer = curatorLayer.transform;
                            pinColor = new Color(0.25f, 0.72f, 0.95f); // Curator blue
                        }

                        GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        pin.name = $"Hotspot_{pinNumber}_{hs.id}";
                        pin.transform.SetParent(targetLayer, false);

                        // Map normalized (0..1) coordinates to local artwork surface (-0.5..0.5 * size)
                        float normX = (hs.normalizedCoordinates.x == 0 && hs.normalizedCoordinates.y == 0) ? 0.5f : hs.normalizedCoordinates.x;
                        float normY = (hs.normalizedCoordinates.x == 0 && hs.normalizedCoordinates.y == 0) ? 0.5f : hs.normalizedCoordinates.y;

                        float localX = (normX - 0.5f) * artWidth;
                        float localY = (normY - 0.5f) * artHeight;
                        float localZ = -0.03f - hs.normalizedCoordinates.z * 0.1f;

                        pin.transform.localPosition = new Vector3(localX, localY, localZ);
                        pin.transform.localScale = Vector3.one * 0.065f;
                        SetPrimitiveColor(pin, pinColor);
                        pinNumber++;
                    }
                }
            }

            // Wire references into ArtworkLayerToggle
            SerializedObject soLayer = new SerializedObject(layerToggle);
            soLayer.FindProperty("artistLayerRoot").objectReferenceValue = artistLayer;
            soLayer.FindProperty("curatorialLayerRoot").objectReferenceValue = curatorLayer;
            soLayer.FindProperty("spatial3DReconstructionRoot").objectReferenceValue = spatial3DLayer;
            soLayer.FindProperty("audioSource").objectReferenceValue = audioSrc;
            soLayer.ApplyModifiedProperties();
        }

        private static GameObject CreateOrLoadDotPrefab()
        {
            string dotPath = "Assets/XRHeritageData/Prefabs/Heritage_Pagination_Dot.prefab";
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dotPath);
            if (existingPrefab != null) return existingPrefab;

            GameObject tempDot = new GameObject("PaginationDot", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            RectTransform rt = tempDot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(14f, 14f);

            LayoutElement le = tempDot.GetComponent<LayoutElement>();
            le.preferredWidth = 14f;
            le.preferredHeight = 14f;

            Image img = tempDot.GetComponent<Image>();
            img.color = Color.white;

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempDot, dotPath);
            UnityEngine.Object.DestroyImmediate(tempDot);
            return savedPrefab;
        }

        private static void BuildAndWireUserInterface(
            Transform parent,
            ArtworkExperienceData data,
            ModernContentManager contentMgr,
            ArtworkLayerToggle layerToggle,
            InAppTamSurveyManager tamMgr,
            GameObject dotPrefab)
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Create Canvas
            GameObject canvasObj = new GameObject("Museum_XR_UI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(parent, false);

            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            scaler.matchWidthOrHeight = 0.5f;

            // 1. TOP BAR (Artwork Info & Perspective Badge)
            GameObject topBar = CreatePanel(canvasObj.transform, "Top_Header_Bar", new Color(0.08f, 0.09f, 0.12f, 0.92f));
            RectTransform topRt = topBar.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(0f, 64f);
            topRt.anchoredPosition = Vector2.zero;

            string headerLabel = (data != null && data.metadata != null && !string.IsNullOrEmpty(data.metadata.title))
                ? $"{data.metadata.title} — {data.metadata.artistName} ({data.metadata.museumOrCollection})"
                : "XR Cultural Heritage Experience";

            Text artworkHeaderTxt = CreateText(topBar.transform, "Artwork_Header_Label", headerLabel, defaultFont, 18, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRectStretch(artworkHeaderTxt.GetComponent<RectTransform>(), 24f, 0f, -320f, 0f);

            Text badgeTxt = CreateText(topBar.transform, "Perspective_Badge_Text", "👁️ ARTWORK OVERVIEW", defaultFont, 15, FontStyle.Bold, TextAnchor.MiddleRight);
            SetRectStretch(badgeTxt.GetComponent<RectTransform>(), 0f, 0f, -24f, 0f);

            // 2. PERSPECTIVE FILTER TABS (Top-Left Floating Pill Bar)
            GameObject filterBar = CreatePanel(canvasObj.transform, "Perspective_Filter_Tabs", new Color(0.12f, 0.14f, 0.18f, 0.9f));
            RectTransform filterRt = filterBar.GetComponent<RectTransform>();
            filterRt.anchorMin = new Vector2(0f, 1f);
            filterRt.anchorMax = new Vector2(0f, 1f);
            filterRt.pivot = new Vector2(0f, 1f);
            filterRt.anchoredPosition = new Vector2(24f, -76f);
            filterRt.sizeDelta = new Vector2(520f, 44f);

            HorizontalLayoutGroup filterLayout = filterBar.AddComponent<HorizontalLayoutGroup>();
            filterLayout.spacing = 8f;
            filterLayout.padding = new RectOffset(8, 8, 6, 6);
            filterLayout.childControlWidth = true;
            filterLayout.childControlHeight = true;
            filterLayout.childForceExpandWidth = true;

            Button btnAll = CreateButton(filterBar.transform, "Tab_ShowAll", "All Perspectives", defaultFont, new Color(0.22f, 0.26f, 0.34f));
            Button btnArtist = CreateButton(filterBar.transform, "Tab_ArtistVoice", "🎨 Artist's Voice", defaultFont, new Color(0.45f, 0.30f, 0.10f));
            Button btnCurator = CreateButton(filterBar.transform, "Tab_CuratorialLens", "🏛️ Curatorial Lens", defaultFont, new Color(0.12f, 0.34f, 0.48f));

            // 3. BOTTOM NARRATIVE CARD PANEL
            GameObject cardPanel = CreatePanel(canvasObj.transform, "Bottom_Narrative_Drawer", new Color(0.09f, 0.11f, 0.15f, 0.94f));
            RectTransform cardRt = cardPanel.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0f);
            cardRt.anchorMax = new Vector2(0.5f, 0f);
            cardRt.pivot = new Vector2(0.5f, 0f);
            cardRt.anchoredPosition = new Vector2(0f, 20f);
            cardRt.sizeDelta = new Vector2(860f, 220f);

            Text chapTitleTxt = CreateText(cardPanel.transform, "Chapter_Title_Text", "Chapter Title", defaultFont, 22, FontStyle.Bold, TextAnchor.UpperLeft);
            RectTransform titleRt = chapTitleTxt.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -16f);
            titleRt.sizeDelta = new Vector2(-48f, 32f);
            chapTitleTxt.color = new Color(0.92f, 0.74f, 0.42f);

            Text narrativeBodyTxt = CreateText(cardPanel.transform, "Narrative_Body_Text", "Chapter narrative text will appear here when running the scene...", defaultFont, 16, FontStyle.Normal, TextAnchor.UpperLeft);
            RectTransform bodyRt = narrativeBodyTxt.GetComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0f, 0f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.offsetMin = new Vector2(24f, 64f);
            bodyRt.offsetMax = new Vector2(-24f, -54f);

            // Navigation Footer inside Card
            Button prevBtn = CreateButton(cardPanel.transform, "Btn_Previous_Chapter", "◀ Previous", defaultFont, new Color(0.20f, 0.24f, 0.30f));
            RectTransform prevRt = prevBtn.GetComponent<RectTransform>();
            prevRt.anchorMin = new Vector2(0f, 0f);
            prevRt.anchorMax = new Vector2(0f, 0f);
            prevRt.pivot = new Vector2(0f, 0f);
            prevRt.anchoredPosition = new Vector2(24f, 14f);
            prevRt.sizeDelta = new Vector2(130f, 38f);

            Button nextBtn = CreateButton(cardPanel.transform, "Btn_Next_Chapter", "Next ▶", defaultFont, new Color(0.83f, 0.64f, 0.35f));
            nextBtn.GetComponentInChildren<Text>().color = Color.black;
            RectTransform nextRt = nextBtn.GetComponent<RectTransform>();
            nextRt.anchorMin = new Vector2(1f, 0f);
            nextRt.anchorMax = new Vector2(1f, 0f);
            nextRt.pivot = new Vector2(1f, 0f);
            nextRt.anchoredPosition = new Vector2(-24f, 14f);
            nextRt.sizeDelta = new Vector2(130f, 38f);

            // Dots Container
            GameObject dotsObj = new GameObject("Pagination_Dots_Container", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            dotsObj.transform.SetParent(cardPanel.transform, false);
            RectTransform dotsRt = dotsObj.GetComponent<RectTransform>();
            dotsRt.anchorMin = new Vector2(0.5f, 0f);
            dotsRt.anchorMax = new Vector2(0.5f, 0f);
            dotsRt.pivot = new Vector2(0.5f, 0f);
            dotsRt.anchoredPosition = new Vector2(0f, 18f);
            dotsRt.sizeDelta = new Vector2(260f, 28f);

            HorizontalLayoutGroup dotsLayout = dotsObj.GetComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 10f;
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = false;
            dotsLayout.childControlHeight = false;

            // 4. IN-SITU TAM SURVEY MODAL DIALOG
            GameObject tamModal = BuildTamSurveyModal(canvasObj.transform, data, tamMgr, defaultFont);

            // Button in Top-Right to open TAM Survey manually anytime
            Button openTamBtn = CreateButton(canvasObj.transform, "Btn_Open_TAM_Survey", "📊 Evaluate Experience (TAM)", defaultFont, new Color(0.18f, 0.22f, 0.29f));
            RectTransform openTamRt = openTamBtn.GetComponent<RectTransform>();
            openTamRt.anchorMin = new Vector2(1f, 1f);
            openTamRt.anchorMax = new Vector2(1f, 1f);
            openTamRt.pivot = new Vector2(1f, 1f);
            openTamRt.anchoredPosition = new Vector2(-24f, -76f);
            openTamRt.sizeDelta = new Vector2(220f, 40f);

            SerializedObject soTamOpen = new SerializedObject(tamMgr);
            soTamOpen.FindProperty("openSurveyButton").objectReferenceValue = openTamBtn;
            soTamOpen.ApplyModifiedProperties();

            // Wire all SerializedProperties on ModernContentManager
            SerializedObject soMgr = new SerializedObject(contentMgr);
            soMgr.FindProperty("experienceData").objectReferenceValue = data;
            soMgr.FindProperty("titleText").objectReferenceValue = chapTitleTxt;
            soMgr.FindProperty("narrativeBodyText").objectReferenceValue = narrativeBodyTxt;
            soMgr.FindProperty("perspectiveBadgeText").objectReferenceValue = badgeTxt;
            soMgr.FindProperty("nextButton").objectReferenceValue = nextBtn;
            soMgr.FindProperty("prevButton").objectReferenceValue = prevBtn;
            soMgr.FindProperty("dotsContainer").objectReferenceValue = dotsRt;
            soMgr.FindProperty("dotIndicatorPrefab").objectReferenceValue = dotPrefab;
            soMgr.FindProperty("showAllTab").objectReferenceValue = btnAll;
            soMgr.FindProperty("showArtistTab").objectReferenceValue = btnArtist;
            soMgr.FindProperty("showCuratorTab").objectReferenceValue = btnCurator;
            soMgr.ApplyModifiedProperties();
        }

        private static GameObject BuildTamSurveyModal(Transform canvasTransform, ArtworkExperienceData data, InAppTamSurveyManager tamMgr, Font font)
        {
            GameObject modalRoot = CreatePanel(canvasTransform, "TAM_Evaluation_Modal_Root", new Color(0.05f, 0.06f, 0.09f, 0.96f));
            RectTransform modalRt = modalRoot.GetComponent<RectTransform>();
            modalRt.anchorMin = new Vector2(0.5f, 0.5f);
            modalRt.anchorMax = new Vector2(0.5f, 0.5f);
            modalRt.pivot = new Vector2(0.5f, 0.5f);
            modalRt.sizeDelta = new Vector2(680f, 470f);

            Text header = CreateText(modalRoot.transform, "TAM_Modal_Title", "📊 Visitor Experience Evaluation (TAM)", font, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform hRt = header.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.anchoredPosition = new Vector2(0f, -16f);
            hRt.sizeDelta = new Vector2(-40f, 34f);
            header.color = new Color(0.92f, 0.74f, 0.42f);

            string q1 = "1. Perceived Usefulness: This experience deepened my understanding of the artwork (1-5)";
            string q2 = "2. Perceived Ease of Use: The multi-perspective interface was easy to navigate (1-5)";
            string q3 = "3. Curatorial Respect: The digital layer respected the physical artwork & space (1-5)";

            if (data != null && data.tamEvaluation != null && data.tamEvaluation.quickSurveyQuestions != null)
            {
                var qs = data.tamEvaluation.quickSurveyQuestions;
                if (qs.Count > 0) q1 = "1. " + qs[0];
                if (qs.Count > 1) q2 = "2. " + qs[1];
                if (qs.Count > 2) q3 = "3. " + qs[2];
            }

            Text q1Label; Slider s1 = CreateLabeledSlider(modalRoot.transform, "Q1_Usefulness", q1, -70f, font, out q1Label);
            Text q2Label; Slider s2 = CreateLabeledSlider(modalRoot.transform, "Q2_EaseOfUse", q2, -155f, font, out q2Label);
            Text q3Label; Slider s3 = CreateLabeledSlider(modalRoot.transform, "Q3_Relevance", q3, -240f, font, out q3Label);

            // Comment InputField
            GameObject inputBg = CreatePanel(modalRoot.transform, "Comment_InputField", new Color(0.14f, 0.16f, 0.22f, 1f));
            RectTransform inpRt = inputBg.GetComponent<RectTransform>();
            inpRt.anchorMin = new Vector2(0.5f, 1f);
            inpRt.anchorMax = new Vector2(0.5f, 1f);
            inpRt.pivot = new Vector2(0.5f, 1f);
            inpRt.anchoredPosition = new Vector2(0f, -325f);
            inpRt.sizeDelta = new Vector2(580f, 54f);

            Text inputText = CreateText(inputBg.transform, "Text", "", font, 14, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRectStretch(inputText.GetComponent<RectTransform>(), 10f, 6f, -10f, -6f);

            Text placeholder = CreateText(inputBg.transform, "Placeholder", "Optional: Leave a short comment or suggestion...", font, 14, FontStyle.Italic, TextAnchor.UpperLeft);
            placeholder.color = new Color(0.6f, 0.65f, 0.7f, 0.6f);
            SetRectStretch(placeholder.GetComponent<RectTransform>(), 10f, 6f, -10f, -6f);

            InputField inputField = inputBg.AddComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.placeholder = placeholder;

            // Submit & Skip Buttons
            Button skipBtn = CreateButton(modalRoot.transform, "Btn_Skip_Survey", "Close / Skip", font, new Color(0.24f, 0.27f, 0.33f));
            RectTransform skipRt = skipBtn.GetComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.5f, 0f);
            skipRt.anchorMax = new Vector2(0.5f, 0f);
            skipRt.pivot = new Vector2(1f, 0f);
            skipRt.anchoredPosition = new Vector2(-12f, 20f);
            skipRt.sizeDelta = new Vector2(160f, 40f);

            Button submitBtn = CreateButton(modalRoot.transform, "Btn_Submit_Survey", "✅ Submit TAM Feedback", font, new Color(0.16f, 0.68f, 0.45f));
            RectTransform subRt = submitBtn.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.5f, 0f);
            subRt.anchorMax = new Vector2(0.5f, 0f);
            subRt.pivot = new Vector2(0f, 0f);
            subRt.anchoredPosition = new Vector2(12f, 20f);
            subRt.sizeDelta = new Vector2(220f, 40f);

            // Wire to InAppTamSurveyManager
            SerializedObject soTam = new SerializedObject(tamMgr);
            soTam.FindProperty("surveyPanelRoot").objectReferenceValue = modalRoot;
            soTam.FindProperty("question1Label").objectReferenceValue = q1Label;
            soTam.FindProperty("question1Slider").objectReferenceValue = s1;
            soTam.FindProperty("question2Label").objectReferenceValue = q2Label;
            soTam.FindProperty("question2Slider").objectReferenceValue = s2;
            soTam.FindProperty("question3Label").objectReferenceValue = q3Label;
            soTam.FindProperty("question3Slider").objectReferenceValue = s3;
            soTam.FindProperty("commentInputField").objectReferenceValue = inputField;
            soTam.FindProperty("submitButton").objectReferenceValue = submitBtn;
            soTam.FindProperty("skipButton").objectReferenceValue = skipBtn;
            if (data != null && !string.IsNullOrEmpty(data.experienceId))
            {
                soTam.FindProperty("currentExperienceId").stringValue = data.experienceId;
            }
            soTam.ApplyModifiedProperties();

            modalRoot.SetActive(false); // Hidden until completed or triggered
            return modalRoot;
        }

        private static Slider CreateLabeledSlider(Transform parent, string name, string questionText, float topOffset, Font font, out Text labelOut)
        {
            GameObject group = new GameObject(name, typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform gRt = group.GetComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0.5f, 1f);
            gRt.anchorMax = new Vector2(0.5f, 1f);
            gRt.pivot = new Vector2(0.5f, 1f);
            gRt.anchoredPosition = new Vector2(0f, topOffset);
            gRt.sizeDelta = new Vector2(580f, 68f);

            labelOut = CreateText(group.transform, "Question_Label", questionText, font, 14, FontStyle.Normal, TextAnchor.UpperLeft);
            RectTransform lRt = labelOut.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 1f);
            lRt.anchorMax = new Vector2(1f, 1f);
            lRt.pivot = new Vector2(0.5f, 1f);
            lRt.anchoredPosition = Vector2.zero;
            lRt.sizeDelta = new Vector2(0f, 36f);

            GameObject sliderBar = CreatePanel(group.transform, "Likert_Slider_1_to_5", new Color(0.20f, 0.24f, 0.30f));
            RectTransform sRt = sliderBar.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0f, 0f);
            sRt.anchorMax = new Vector2(1f, 0f);
            sRt.pivot = new Vector2(0.5f, 0f);
            sRt.anchoredPosition = new Vector2(0f, 4f);
            sRt.sizeDelta = new Vector2(0f, 22f);

            GameObject fill = CreatePanel(sliderBar.transform, "Fill", new Color(0.83f, 0.64f, 0.35f));
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            SetRectStretch(fillRt, 0f, 0f, 0f, 0f);

            Slider slider = sliderBar.AddComponent<Slider>();
            slider.minValue = 1f;
            slider.maxValue = 5f;
            slider.wholeNumbers = true;
            slider.value = 5f;
            slider.fillRect = fillRt;

            return slider;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image img = panel.GetComponent<Image>();
            img.color = bgColor;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int size, FontStyle style, TextAnchor align)
        {
            GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(parent, false);
            Text t = txtObj.GetComponent<Text>();
            t.text = content;
            t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = Color.white;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font, Color bgColor)
        {
            GameObject btnObj = CreatePanel(parent, name, bgColor);
            Button btn = btnObj.AddComponent<Button>();
            Text txt = CreateText(btnObj.transform, "Label", label, font, 14, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetRectStretch(txt.GetComponent<RectTransform>(), 6f, 2f, -6f, -2f);
            return btn;
        }

        private static void SetRectStretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(right, top);
        }

        private static void SetPrimitiveColor(GameObject obj, Color c)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default"));
                mat.color = c;
                renderer.sharedMaterial = mat;
            }
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string full = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
#endif
