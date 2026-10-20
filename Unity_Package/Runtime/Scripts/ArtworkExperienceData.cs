using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeritageXR.Framework
{
    [Serializable]
    public enum PerspectiveType
    {
        PhysicalArtwork,
        ArtistVoice,
        CuratorialLens,
        ComparativeAnalysis
    }

    [Serializable]
    public enum MediaContentType
    {
        Image,
        AudioVoiceover,
        VideoNarrative,
        Model3D,
        InteractiveComparisonSlider
    }

    [Serializable]
    public class HotspotItem
    {
        public string id;
        public string title;
        [TextArea(2, 5)]
        public string description;
        public Vector3 localNormalizedPosition; // Position relative to artwork canvas (0..1)
        public PerspectiveType perspective;
        public MediaContentType contentType;
        public string mediaAssetPath;
        public string audioClipPath;
    }

    [Serializable]
    public class ContentChapter
    {
        public string chapterId;
        public string chapterTitle;
        public PerspectiveType perspective;
        [TextArea(3, 8)]
        public string narrativeText;
        public string mediaUrlOrPath;
        public float suggestedDwellTimeSeconds = 15f;
        public List<HotspotItem> hotspots = new List<HotspotItem>();
    }

    [Serializable]
    public class ArtworkMetadata
    {
        public string title;
        public string artistName;
        public string creationYear;
        public string medium;
        public string physicalDimensions;
        public string museumOrCollection;
        public string curatorName;
    }

    [CreateAssetMenu(fileName = "NewArtworkExperience", menuName = "XR Heritage/Artwork Experience Data", order = 1)]
    public class ArtworkExperienceData : ScriptableObject
    {
        public string experienceId;
        public ArtworkMetadata metadata = new ArtworkMetadata();
        
        [Header("Exhibition Constraints")]
        public string exhibitionLocation;
        public bool requireHeadphones = false;
        public float maxRecommendedSessionMinutes = 5f;

        [Header("Structured Multi-Perspective Chapters")]
        public List<ContentChapter> chapters = new List<ContentChapter>();

        [Header("In-Situ TAM Evaluation")]
        public bool enablePostExperienceSurvey = true;
        public List<string> customTamQuestions = new List<string>();
    }
}
