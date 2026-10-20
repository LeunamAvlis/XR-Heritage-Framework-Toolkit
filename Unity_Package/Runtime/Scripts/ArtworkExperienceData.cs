using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeritageXR.Framework
{
    [Serializable]
    public enum PerspectiveType
    {
        PhysicalArtwork = 0,
        ArtistVoice = 1,
        CuratorialLens = 2,
        ComparativeAnalysis = 3
    }

    [Serializable]
    public enum MediaContentType
    {
        Image = 0,
        AudioVoiceover = 1,
        VideoNarrative = 2,
        Model3D = 3,
        InteractiveComparisonSlider = 4
    }

    [Serializable]
    public class HotspotItem
    {
        public string id;
        public string title;
        [TextArea(2, 5)]
        public string description;
        public PerspectiveType perspective;
        public MediaContentType contentType;
        public Vector3 normalizedCoordinates; // Position relative to artwork canvas (0..1)
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
        public string mediaAssetPath;
        public string audioClipPath;
        public float suggestedDwellTimeSeconds = 20f;
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

    [Serializable]
    public class ExhibitionConstraints
    {
        public string location;
        public string deviceTarget;
        public bool requireHeadphones;
        public float maxRecommendedSessionMinutes = 5f;
        [TextArea(2, 4)]
        public string artistAutonomyBoundary;
    }

    [Serializable]
    public class TamEvaluationSettings
    {
        public bool enabled = true;
        public List<string> quickSurveyQuestions = new List<string>();
    }

    [CreateAssetMenu(fileName = "NewArtworkExperience", menuName = "XR Heritage/Artwork Experience Data", order = 1)]
    public class ArtworkExperienceData : ScriptableObject
    {
        public string experienceId;
        public string version = "2.0";
        public ArtworkMetadata metadata = new ArtworkMetadata();
        public ExhibitionConstraints exhibitionConstraints = new ExhibitionConstraints();

        [Header("Structured Multi-Perspective Chapters")]
        public List<ContentChapter> chapters = new List<ContentChapter>();

        [Header("In-Situ TAM Evaluation")]
        public TamEvaluationSettings tamEvaluation = new TamEvaluationSettings();
    }
}
