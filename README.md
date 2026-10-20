# Intelligent XR Heritage Framework & Unity Toolkit (v2.0)

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Research: UCP Porto](https://img.shields.io/badge/Research-School_of_Arts_UCP-green.svg)](https://artes.porto.ucp.pt)

An open-source, doctoral-research-backed framework bridging **AI-assisted co-design**, **multi-perspective curatorial mediation**, and **production-ready XR implementation** for museums and cultural heritage institutions.

Developed by **Manuel Silva** and **Prof. Luís Teixeira** at the School of Arts, Portuguese Catholic University (*Universidade Católica Portuguesa - UCP*), Porto.

---

## 🏛️ Why This Framework?

Museum XR experiences often suffer from a disconnect between curatorial intent, artist vision, and technical execution:
- **Curators** worry about technological gimmicks overshadowing authentic artworks.
- **Artists** need their autonomy and creative genesis protected.
- **Developers & Designers** need clear affordances and rapid prototyping tools instead of abstract text.
- **Researchers** need validated empirical instruments (Technology Acceptance Model - TAM) to evaluate impact.

This repository provides an **end-to-end operational pipeline** resolving these challenges:

```
┌─────────────────────────────────┐
│  1. AI CONCEPTION LAYER         │  Modular AI skills & intake diagnostic
│     (Antigravity / GPT / Claude)│  Balances artist voice, curatorial lens & space constraints
└────────────────┬────────────────┘
                 │
                 ▼ Outputs standard JSON Spec
┌─────────────────────────────────┐
│  2. HERITAGE SCHEMA BRIDGE      │  `heritage-experience.schema.json`
│     (Data Interchange)          │  Validated exchange format
└────────────────┬────────────────┘
                 │
                 ▼ 1-Click Unity Importer
┌─────────────────────────────────┐
│  3. UNITY XR PRODUCTION ENGINE  │  `com.heritage.xr-framework` (UPM)
│     (Interactive Runtime & TAM) │  Auto-generates UI, layers, and visitor survey logging
└─────────────────────────────────┘
```

---

## 📦 How to Install in Unity (via Package Manager)

You can install this toolkit directly into any Unity project using the **Unity Package Manager (UPM)** via Git:

### Method 1: Git URL (Recommended)
1. In Unity, open **Window > Package Manager**.
2. Click the **`+`** (plus) icon in the top-left corner and select **Add package from git URL...**
3. Paste the following URL and click **Add**:
   ```
   https://github.com/LeunamAvlis/xr-heritage-framework.git?path=/Unity_Package
   ```
   *(Replace `LeunamAvlis/xr-heritage-framework` with your repository URL).*

### Method 2: Manual / Local Installation
1. Download or clone this repository.
2. In Unity Package Manager, click **`+` > Add package from disk...**
3. Select `XR_Intelligent_Framework/Unity_Package/package.json`.

---

## 🚀 Quick Start Guide

### Step 1: Conceptualize with the AI Framework
1. Copy the contents of [`AI_Skills/Master_Framework_System_Prompt.md`](AI_Skills/Master_Framework_System_Prompt.md) into your preferred AI platform (ChatGPT Custom GPT, Claude Project, Perplexity, or Antigravity).
2. Run the diagnostic intake questionnaire from [`AI_Skills/Skill_1_Intake_and_Profiling.md`](AI_Skills/Skill_1_Intake_and_Profiling.md).
3. The AI will output a completed, validated **Heritage Experience Specification (JSON)**.

### Step 2: Import into Unity
1. In Unity, click the top menu: **Tools > XR Heritage Framework > Import Experience Spec (JSON)**.
2. Select your AI-generated JSON file (or use one of the samples in `Schemas/`).
3. Click **Import & Generate ScriptableObject**:
   - Unity parses the JSON and creates an `ArtworkExperienceData` asset in `Assets/XRHeritageData/`.
4. Click **Setup Scene with Heritage UI Rig**:
   - Unity instantiates the complete GameObject hierarchy: `ModernContentManager`, `ArtworkLayerToggle`, and `InAppTamSurveyManager`.

### Step 3: Test & Collect Visitor Data
- The UI provides interactive touch pagination, swipe gestures, and perspective filters (*All*, *Artist Voice*, *Curatorial Lens*).
- When the visitor completes the experience, the built-in TAM micro-survey records anonymous Likert scores and feedback directly to `TAM_Visitor_Results.csv` on the device.

---

## 📂 Repository Contents

```
├── AI_Skills/
│   ├── Master_Framework_System_Prompt.md   # Master instruction for AI personas
│   ├── Skill_1_Intake_and_Profiling.md     # Mandatory intake diagnostic (medium, space, role)
│   ├── Skill_2_MultiPerspective_Mediation.md # Curatorial/Artist co-design & gimmick filter
│   ├── Skill_3_XR_Affordance_Engine.md     # Hardware & affordances (2005–2024 benchmark)
│   └── Skill_4_TAM_Evaluation_Generator.md # Validated TAM questionnaires & micro-surveys
├── Schemas/
│   ├── heritage-experience.schema.json     # Formal JSON Schema (Draft-07)
│   ├── sample_photography_experience.json  # Serralves case study (Carlos Lobo)
│   └── sample_sculpture_experience.json    # Archaeological 3D sculpture case study
├── Unity_Package/                          # UPM Package (`com.heritage.xr-framework`)
│   ├── package.json
│   ├── Runtime/
│   │   ├── HeritageXR.Framework.asmdef
│   │   └── Scripts/
│   │       ├── ArtworkExperienceData.cs    # ScriptableObject data model
│   │       ├── ModernContentManager.cs     # Responsive UI pagination & swipe
│   │       ├── ArtworkLayerToggle.cs       # Physical/Artist/Curator visibility
│   │       └── InAppTamSurveyManager.cs    # In-situ TAM survey CSV logger
│   └── Editor/
│       ├── HeritageXR.Framework.Editor.asmdef
│       └── XRHeritageSpecImporter.cs       # 1-Click Unity JSON importer tool
├── LICENSE                                 # MIT License
└── README.md
```

---

## 🎓 Academic Citation

If you use this framework or toolkit in academic research or museum projects, please cite:

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

---

## 📄 License
This project is licensed under the [MIT License](LICENSE) - feel free to use, modify, and distribute for academic, cultural, and commercial projects.
