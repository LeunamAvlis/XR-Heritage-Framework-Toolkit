using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeritageXR.Framework
{
    public class ModernContentManager : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private ArtworkExperienceData experienceData;

        [Header("UI Display Components")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text narrativeBodyText;
        [SerializeField] private Image mediaDisplayImage;
        [SerializeField] private Text perspectiveBadgeText;

        [Header("Navigation Controls")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private RectTransform dotsContainer;
        [SerializeField] private GameObject dotIndicatorPrefab;

        [Header("Perspective Filter Tabs (Optional)")]
        [SerializeField] private Button showAllTab;
        [SerializeField] private Button showArtistTab;
        [SerializeField] private Button showCuratorTab;

        [Header("Ergonomics & Behavior")]
        [SerializeField] private bool autoAdvanceTimer = false;
        [SerializeField] private float autoAdvanceIntervalSeconds = 8f;
        [SerializeField] private float swipeThresholdPixels = 60f;

        // Runtime state
        private List<ContentChapter> activeFilteredChapters = new List<ContentChapter>();
        private List<Image> spawnedDots = new List<Image>();
        private int currentChapterIndex = 0;
        private Vector2 touchStartPos;
        private float chapterTimer = 0f;

        // Events
        public event Action<int, ContentChapter> OnChapterChanged;
        public event Action OnExperienceCompleted;

        private void Awake()
        {
            if (nextButton != null) nextButton.onClick.AddListener(NextChapter);
            if (prevButton != null) prevButton.onClick.AddListener(PreviousChapter);

            if (showAllTab != null) showAllTab.onClick.AddListener(() => SetPerspectiveFilter(null));
            if (showArtistTab != null) showArtistTab.onClick.AddListener(() => SetPerspectiveFilter(PerspectiveType.ArtistVoice));
            if (showCuratorTab != null) showCuratorTab.onClick.AddListener(() => SetPerspectiveFilter(PerspectiveType.CuratorialLens));
        }

        private void Start()
        {
            if (experienceData != null)
            {
                Initialize(experienceData);
            }
        }

        public void Initialize(ArtworkExperienceData data)
        {
            experienceData = data;
            SetPerspectiveFilter(null); // Show all by default
        }

        public void SetPerspectiveFilter(PerspectiveType? filter)
        {
            activeFilteredChapters.Clear();

            if (experienceData == null || experienceData.chapters == null) return;

            foreach (var chapter in experienceData.chapters)
            {
                if (filter == null || chapter.perspective == filter.Value)
                {
                    activeFilteredChapters.Add(chapter);
                }
            }

            currentChapterIndex = 0;
            RebuildDots();
            DisplayCurrentChapter();
        }

        private void RebuildDots()
        {
            if (dotsContainer == null || dotIndicatorPrefab == null) return;

            foreach (Transform child in dotsContainer)
            {
                Destroy(child.gameObject);
            }
            spawnedDots.Clear();

            for (int i = 0; i < activeFilteredChapters.Count; i++)
            {
                GameObject dotObj = Instantiate(dotIndicatorPrefab, dotsContainer);
                Image dotImg = dotObj.GetComponent<Image>();
                if (dotImg != null)
                {
                    spawnedDots.Add(dotImg);
                }
                int capturedIndex = i;
                Button dotBtn = dotObj.GetComponent<Button>();
                if (dotBtn != null)
                {
                    dotBtn.onClick.AddListener(() => JumpToChapter(capturedIndex));
                }
            }
            UpdateDotsVisual();
        }

        private void UpdateDotsVisual()
        {
            for (int i = 0; i < spawnedDots.Count; i++)
            {
                if (spawnedDots[i] != null)
                {
                    spawnedDots[i].color = (i == currentChapterIndex) ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                    spawnedDots[i].transform.localScale = (i == currentChapterIndex) ? Vector3.one * 1.25f : Vector3.one;
                }
            }
        }

        public void JumpToChapter(int index)
        {
            if (activeFilteredChapters.Count == 0) return;
            currentChapterIndex = Mathf.Clamp(index, 0, activeFilteredChapters.Count - 1);
            chapterTimer = 0f;
            DisplayCurrentChapter();
        }

        public void NextChapter()
        {
            if (activeFilteredChapters.Count == 0) return;

            if (currentChapterIndex < activeFilteredChapters.Count - 1)
            {
                currentChapterIndex++;
                chapterTimer = 0f;
                DisplayCurrentChapter();
            }
            else
            {
                // Reached the end
                OnExperienceCompleted?.Invoke();
            }
        }

        public void PreviousChapter()
        {
            if (activeFilteredChapters.Count == 0) return;

            if (currentChapterIndex > 0)
            {
                currentChapterIndex--;
                chapterTimer = 0f;
                DisplayCurrentChapter();
            }
        }

        private void DisplayCurrentChapter()
        {
            if (activeFilteredChapters.Count == 0 || currentChapterIndex >= activeFilteredChapters.Count) return;

            var chapter = activeFilteredChapters[currentChapterIndex];

            if (titleText != null) titleText.text = chapter.chapterTitle;
            if (narrativeBodyText != null) narrativeBodyText.text = chapter.narrativeText;

            if (perspectiveBadgeText != null)
            {
                switch (chapter.perspective)
                {
                    case PerspectiveType.ArtistVoice:
                        perspectiveBadgeText.text = "🎨 ARTIST'S PERSPECTIVE";
                        perspectiveBadgeText.color = new Color(0.95f, 0.65f, 0.2f);
                        break;
                    case PerspectiveType.CuratorialLens:
                        perspectiveBadgeText.text = "🏛️ CURATORIAL CONTEXT";
                        perspectiveBadgeText.color = new Color(0.35f, 0.75f, 0.95f);
                        break;
                    default:
                        perspectiveBadgeText.text = "👁️ ARTWORK OVERVIEW";
                        perspectiveBadgeText.color = Color.white;
                        break;
                }
            }

            UpdateDotsVisual();
            OnChapterChanged?.Invoke(currentChapterIndex, chapter);
        }

        private void Update()
        {
            HandleSwipeGestures();

            if (autoAdvanceTimer && activeFilteredChapters.Count > 0)
            {
                chapterTimer += Time.deltaTime;
                if (chapterTimer >= autoAdvanceIntervalSeconds)
                {
                    chapterTimer = 0f;
                    NextChapter();
                }
            }
        }

        private void HandleSwipeGestures()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 delta = touch.position - touchStartPos;
                    if (Mathf.Abs(delta.x) > swipeThresholdPixels && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x < 0) NextChapter();
                        else PreviousChapter();
                    }
                }
            }
        }
    }
}
