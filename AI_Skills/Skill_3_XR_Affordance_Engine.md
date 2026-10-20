# Skill 3: XR Affordance & Technology Matchmaker
## Purpose
To map conceptual ideas to the correct XR technology stack, interaction affordances, and deployment hardware, grounded in the empirical State of the Art literature review (2005–2024).

---

### Technology Selection Matrix

| Exhibition Need | Recommended XR Medium | Hardware | Tracking Mode | Typical TRL |
| :--- | :--- | :--- | :--- | :--- |
| **In-situ exploration of 2D works (Painting/Photography)** | Handheld Augmented Reality (AR) | Dedicated 10" Museum Tablet with ergonomic lanyard or BYOD smartphone | High-fidelity Image Tracking (Vuforia / ARCore Image Targets) | TRL 7–9 (Production ready) |
| **Volumetric / 3D Sculptures & Replicas** | Mixed Reality (MR) or Spatial AR | Apple Vision Pro / Quest 3 / HoloLens 2 or Mobile AR Object Tracking | Plane detection + Object Tracking + Spatial Anchors | TRL 6–8 |
| **Contextual reconstruction of lost architectural spaces** | Immersive Virtual Reality (VR) | Meta Quest 3 / PCVR in dedicated enclosure | 6DoF Inside-Out Roomscale Tracking | TRL 7–8 |
| **Group / Social Engagement without headsets** | Augmented Projection / Spatial Interactive Kiosk | Projection mapping + directional ultrasound speakers + LiDAR | Touchless gesture / proximity sensors | TRL 7–9 |

---

### Core Interaction Affordance Library

When specifying an experience, choose from these validated interaction patterns:

1. **`Affordance_HotspotReveal`**:
   - Discrete, elegant glowing indicators placed on key compositional areas. Tapping or looking at a hotspot opens a drawer with curatorial notes or artist sketches.
2. **`Affordance_TimeSlider (Genesis)`**:
   - An intuitive horizontal slider that peels back the finished piece to reveal the preparatory charcoal underdrawing, digital draft layers, or infrared scans.
3. **`Affordance_SpatialVoiceFocus`**:
   - Directional audio that only plays when the visitor is physically positioned in front of a specific quadrant of the work.
4. **`Affordance_ArtworkDivergence (Related Works)`**:
   - A floating lateral carousel that lets the visitor compare the current piece with preceding pieces in the artist's series.
5. **`Affordance_MaterialityDeconstruction`**:
   - Exploded view or high-resolution microscopic view of canvas weave, pigment chemistry, or photographic paper texture.

---

### Ergonomic & Museum Space Guidelines
- **Tablet Weight & Arm Fatigue**: Visitor holding limit is 3–5 minutes. For sessions longer than 3 minutes, provide a weighted pivoting stand in front of the artwork.
- **Sound Bleed**: In open galleries, use directional sound showers or supply over-ear headphones with single-cup design so visitors remain aware of their physical surroundings.
- **Museum Lighting**: Avoid specular glare on glass-framed works; recommend matte non-reflective polarizers or matte screen protectors on museum tablets.
