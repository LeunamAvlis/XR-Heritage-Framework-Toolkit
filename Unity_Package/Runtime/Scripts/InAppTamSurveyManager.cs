using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HeritageXR.Framework
{
    [Serializable]
    public class TamSurveySubmission
    {
        public string timestamp;
        public string experienceId;
        public int scorePerceivedUsefulness; // 1 to 5
        public int scoreEaseOfUse;           // 1 to 5
        public int scoreContentRelevance;    // 1 to 5
        public string qualitativeFeedback;
        public string deviceModel;
    }

    public class InAppTamSurveyManager : MonoBehaviour
    {
        [Header("UI Dialog Root")]
        [SerializeField] private GameObject surveyPanelRoot;

        [Header("Question 1: Perceived Usefulness (1-5)")]
        [SerializeField] private Text question1Label;
        [SerializeField] private Slider question1Slider;

        [Header("Question 2: Ease of Use (1-5)")]
        [SerializeField] private Text question2Label;
        [SerializeField] private Slider question2Slider;

        [Header("Question 3: Content Relevance (1-5)")]
        [SerializeField] private Text question3Label;
        [SerializeField] private Slider question3Slider;

        [Header("Qualitative Feedback")]
        [SerializeField] private InputField commentInputField;

        [Header("Action Buttons")]
        [SerializeField] private Button submitButton;
        [SerializeField] private Button skipButton;

        [Header("State")]
        [SerializeField] private string currentExperienceId = "DefaultExperience";

        private string logFilePath;

        private void Awake()
        {
            logFilePath = Path.Combine(Application.persistentDataPath, "TAM_Visitor_Results.csv");
            EnsureCsvHeaderExists();

            if (submitButton != null) submitButton.onClick.AddListener(SubmitSurvey);
            if (skipButton != null) skipButton.onClick.AddListener(DismissSurvey);

            if (surveyPanelRoot != null) surveyPanelRoot.SetActive(false);
        }

        public void PromptSurvey(string experienceId)
        {
            currentExperienceId = experienceId;
            if (surveyPanelRoot != null)
            {
                surveyPanelRoot.SetActive(true);
            }
        }

        public void DismissSurvey()
        {
            if (surveyPanelRoot != null)
            {
                surveyPanelRoot.SetActive(false);
            }
        }

        public void SubmitSurvey()
        {
            var submission = new TamSurveySubmission
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                experienceId = currentExperienceId,
                scorePerceivedUsefulness = question1Slider != null ? Mathf.RoundToInt(question1Slider.value) : 5,
                scoreEaseOfUse = question2Slider != null ? Mathf.RoundToInt(question2Slider.value) : 5,
                scoreContentRelevance = question3Slider != null ? Mathf.RoundToInt(question3Slider.value) : 5,
                qualitativeFeedback = commentInputField != null ? commentInputField.text.Replace(";", ",").Replace("\n", " ") : "",
                deviceModel = SystemInfo.deviceModel
            };

            SaveSubmissionToCsv(submission);
            DismissSurvey();
            Debug.Log($"[TAM Framework] Survey recorded successfully to: {logFilePath}");
        }

        private void EnsureCsvHeaderExists()
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    string header = "Timestamp;ExperienceId;PerceivedUsefulness;EaseOfUse;ContentRelevance;QualitativeFeedback;DeviceModel\n";
                    File.WriteAllText(logFilePath, header, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TAM Framework] Failed to initialize CSV: {e.Message}");
            }
        }

        private void SaveSubmissionToCsv(TamSurveySubmission s)
        {
            try
            {
                string line = $"{s.timestamp};{s.experienceId};{s.scorePerceivedUsefulness};{s.scoreEaseOfUse};{s.scoreContentRelevance};\"{s.qualitativeFeedback}\";\"{s.deviceModel}\"\n";
                File.AppendAllText(logFilePath, line, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError($"[TAM Framework] Failed to append survey data: {e.Message}");
            }
        }
    }
}
