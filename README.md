# XR Heritage Framework & Unity Toolkit (v2.2)

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com)
[![Web Studio](https://img.shields.io/badge/Web%20Studio-Live%20Demo-brightgreen.svg)](https://leunamavlis.github.io/XR-Heritage-Framework-Toolkit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Research: UCP Porto](https://img.shields.io/badge/Research-School_of_Arts_UCP-green.svg)](https://artes.porto.ucp.pt)

An open-source, doctoral-research-backed framework bridging **AI-assisted co-design**, **multi-perspective curatorial mediation**, and **production-ready XR implementation** for museums, artists, and cultural heritage institutions.

Developed by **Manuel Silva** with supervision from **Prof. Luís Teixeira** at the School of Arts, Portuguese Catholic University (*Universidade Católica Portuguesa - UCP*), Porto.

---

## 🌟 Choose Your Track

Whether you are conceptualizing an exhibition or building the real-time application, start with the track tailored to your role:

| 🎨 Track 1: Conceptual & Curatorial Track<br>**(Artists, Curators, Researchers, Designers)** | 💻 Track 2: Technical Production Track<br>**(XR Developers, Creative Technologists, Unity Devs)** |
| :--- | :--- |
| **Zero coding. Zero game engines. Zero Git commands.**<br><br>1. **Co-Design with AI**: Copy the [**`ALL_IN_ONE_PROMPT.txt`**](ALL_IN_ONE_PROMPT.txt) into ChatGPT, Claude, or Gemini. The AI adapts its tone directly to your role (Artist, Curator, Researcher).<br>2. **Open the Web Studio**: Launch the [**Online Visual Co-Creation Studio**](https://leunamavlis.github.io/XR-Heritage-Framework-Toolkit/) directly in your browser.<br>3. **Preview & Export**: Inspect interactive spatial maquettes, add your 3D visual renders, and generate a print-ready **Exhibition Conception Dossier (PDF)**.<br>4. **Handoff**: Download the generated `.json` spec to give to your developer *(or follow our gentle guided on-ramp to test it in Unity yourself!)* | **1-Click Engine Automation & Verified Package.**<br><br>1. **Install Package**: Add `https://github.com/LeunamAvlis/XR-Heritage-Framework-Toolkit.git?path=/Unity_Package` in Unity's Package Manager (UPM).<br>2. **Guided Wizard**: Open `Tools > XR Heritage Framework > Experience Setup Wizard`.<br>3. **1-Click Scene Generation**: Import the artist's `.json` to automatically configure the XR Camera Rig (AR or VR/MR), 3D Artwork Anchor with spatial Hotspot pins, multi-perspective uGUI canvas, and automated TAM survey logging.<br>4. **Auto-SDK Management**: Installs official AR Foundation or OpenXR dependencies automatically. |

---

## 🏛️ Why This Framework?

Museum XR experiences often suffer from a disconnect between curatorial intent, artist vision, and technical execution:
- **Curators** worry about technological gimmicks overshadowing authentic artworks.
- **Artists** need their authorial boundaries, voice, and creative process protected without intrusive overlays.
- **Developers & Designers** need concrete affordances and rapid prototyping tools instead of ambiguous essays.
- **Researchers** need validated empirical instruments (Technology Acceptance Model - TAM) to evaluate visitor impact.

This repository resolves these tensions with an **end-to-end, two-track pipeline**:

```
┌────────────────────────────────────────────────────────────────────────┐
│                        ROLE INTAKE (ADAPTIVE TONE)                     │
│    (Artist | Curator | Researcher | Spatial Designer | Developer)      │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                 TRACK 1: CONCEPTUAL & VISUAL STUDIO                    │
│             (Web Browser: GitHub Pages / ChatGPT / Claude)             │
│                                                                        │
│  • Adaptive co-design dialogue (protects artist primacy & autonomy)    │
│  • Interactive Device Viewport & Floorplan Spatial Maquettes           │
│  • Visual Renders & Reference Gallery Integration                     │
│  • Outputs: Print-Ready PDF Dossier + Heritage Spec (.json)            │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
              "Would you like to hand this off or test it in Unity?"
                                    │
         ┌──────────────────────────┴──────────────────────────┐
         ▼                                                     ▼
┌───────────────────────────────┐     ┌─────────────────────────────────┐
│     HANDOFF TO DEVELOPER      │     │  TRACK 2: UNITY XR WIZARD       │
│ Send the PDF dossier & .json  │     │ 1-Click Unity Setup Wizard:     │
│ to the museum technical team. │     │ auto-installs SDKs, builds rig, │
│                               │     │ places 3D pins, wires TAM UI.   │
└───────────────────────────────┘     └─────────────────────────────────┘
```

---

## 🎨 For Artists, Curators & Researchers (Track 1)

### Step 1: Start the Co-Design Dialogue
1. Copy the master prompt from [**`ALL_IN_ONE_PROMPT.txt`**](ALL_IN_ONE_PROMPT.txt).
2. Paste it into your preferred AI chat (ChatGPT, Claude, Gemini, Perplexity).
3. **Select your role first.** The AI will immediately shift its language:
   - **For Artists:** Focuses on creative genesis, materiality, sketches, and non-interference boundaries.
   - **For Curators:** Focuses on thematic discourse, dialogue with adjacent works, and visitor flow.
   - **For Researchers:** Focuses on visitor evaluation metrics, dwell time, and TAM research questions.

### Step 2: Visual Studio & PDF Exhibition Dossier
1. Open the [**Online Visual Co-Creation Studio**](https://leunamavlis.github.io/XR-Heritage-Framework-Toolkit/) in any modern browser.
2. Paste the JSON block from your AI chat into the box **"📋 …or paste the JSON block from the AI chat"** and click **✅ Load Pasted JSON**.
3. Upload your 3D exhibition maquette renders or reference photos using **🖼️ Add Renders / Reference Images**.
4. Test the **Interactive Viewport** (click the pins, test perspective filters).
5. Click **🖨️ Save Visual Report (PDF)** to export an illustrated dossier for your museum board, artist agreement, or funding application.
6. Click **📦 Export Unity .JSON** to save the file for technical production.

---

## 💻 For Developers & Technologists (Track 2)

### Installing the Unity Package (UPM)
Add the package to your Unity project (Unity 2021.3 LTS or newer):
1. In Unity, open **Window > Package Manager**.
2. Click **`+` > Add package from git URL...**
3. Paste:
   ```
   https://github.com/LeunamAvlis/XR-Heritage-Framework-Toolkit.git?path=/Unity_Package
   ```

### 1-Click Experience Setup Wizard
1. Open **Tools > XR Heritage Framework > Experience Setup Wizard**.
2. Select the `.json` file generated from Track 1 and click **🚀 1. Import JSON & Check Required SDKs**.
   - Automatically prompts to install official Unity dependencies (**AR Foundation** for mobile tablets/phones or **OpenXR** for VR/MR headsets).
3. Click **🎨 2. Build Complete Playable Scene & Prefabs**:
   - Generates the camera rig (`XR_Origin_Mobile_AR_Rig` or `XR_Origin_VR_MR_Rig`).
   - Generates 3D physical artwork stand and places color-coded 3D **Hotspot Pins** mapped directly from normalized coordinates.
   - Sets up the interactive **Museum UI Canvas** (Chapter drawer, perspective tabs, pagination dots).
   - Generates the in-situ **TAM Visitor Survey Modal** that automatically logs visitor feedback to `TAM_Visitor_Results.csv`.
4. Press **Play (▶)** to test the entire experience immediately!

---

## 📂 Repository Contents

```
├── docs/                                   # GitHub Pages Online Web Studio
│   └── index.html                          # Zero-install browser co-creation studio
├── Web_Companion/                          # Offline local source for the Web Studio
│   └── index.html
├── AI_Skills/                              # Modular Persona Prompts
│   ├── Master_Framework_System_Prompt.md   # Adaptive persona system prompt
│   ├── Skill_1_Intake_and_Profiling.md     # Mandatory intake diagnostic (5 parameters)
│   ├── Skill_2_MultiPerspective_Mediation.md # Curatorial/Artist co-design rules
│   ├── Skill_3_XR_Affordance_Engine.md     # Hardware & spatial affordance engine
│   └── Skill_4_TAM_Evaluation_Generator.md # Validated TAM questionnaires
├── ALL_IN_ONE_PROMPT.txt                   # Single copy-paste master prompt for any LLM
├── Schemas/                                # JSON Schemas & Tested Case Studies
│   ├── heritage-experience.schema.json     # Formal Draft-07 specification schema
│   ├── sample_photography_experience.json  # Serralves case study (Carlos Lobo)
│   ├── sample_sculpture_experience.json    # Archaeological 3D sculpture case study
│   └── mares_da_memoria.json               # AR painting demo case study
├── Unity_Package/                          # Verified UPM Package (`com.heritage.xr-framework`)
│   ├── package.json
│   ├── Runtime/
│   │   ├── Scripts/ArtworkExperienceData.cs# ScriptableObject data model
│   │   ├── Scripts/ModernContentManager.cs # Responsive UI pagination & chapters
│   │   ├── Scripts/ArtworkLayerToggle.cs   # 3D Hotspot & audio layer toggling
│   │   └── Scripts/InAppTamSurveyManager.cs# In-situ TAM evaluation & CSV logger
│   └── Editor/
│       ├── XRHeritageSpecImporter.cs       # 3-Step Guided Experience Setup Wizard
│       ├── XRHeritageSceneBuilder.cs       # 1-Click Playable Scene & Prefab Builder
│       └── XRHeritageSDKManager.cs         # Automated SDK installer (AR / OpenXR)
├── LICENSE                                 # MIT License
└── README.md
```

---

## 🤝 Open Source & Contributions

This framework is open-source under the [MIT License](LICENSE). We welcome contributions from researchers, creative coders, and cultural institutions!
- Found a bug or want to suggest an affordance? Open an [Issue](https://github.com/LeunamAvlis/XR-Heritage-Framework-Toolkit/issues).
- Want to contribute code or new case studies? Fork the repository and submit a Pull Request.

---

## 🎓 Academic Citation

If you use this framework or toolkit in academic research, exhibitions, or cultural projects, please cite:

```bibtex
@phdthesis{silva2026xrheritage,
  author       = {Manuel Silva},
  title        = {Intelligent XR Framework for Museum Communication and Multi-Perspective Cultural Heritage Mediation},
  school       = {School of Arts, Universidade Cat{\'o}lica Portuguesa},
  year         = {2026},
  address      = {Porto, Portugal},
  advisor      = {Prof. Lu{\'i}s Teixeira}
}
```
