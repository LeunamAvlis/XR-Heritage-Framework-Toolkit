#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace HeritageXR.Framework.Editor
{
    public static class XRHeritageSDKManager
    {
        // SDK Package IDs
        public const string PKG_AR_FOUNDATION = "com.unity.xr.arfoundation";
        public const string PKG_ARCORE = "com.unity.xr.arcore";
        public const string PKG_ARKIT = "com.unity.xr.arkit";

        public const string PKG_OPENXR = "com.unity.xr.openxr";
        public const string PKG_XR_MANAGEMENT = "com.unity.xr.management";
        public const string PKG_XR_INTERACTION = "com.unity.xr.interaction.toolkit";

        private static Queue<string> installQueue = new Queue<string>();
        private static AddRequest currentAddRequest;
        private static string currentlyInstallingPackage;

        public static void InstallMobileArSdk()
        {
            var packages = new List<string> { PKG_AR_FOUNDATION, PKG_ARCORE, PKG_ARKIT };
            QueuePackagesForInstallation(packages, "Mobile AR (AR Foundation + ARCore + ARKit)");
        }

        public static void InstallVrOpenXrSdk()
        {
            var packages = new List<string> { PKG_XR_MANAGEMENT, PKG_OPENXR, PKG_XR_INTERACTION };
            QueuePackagesForInstallation(packages, "VR / Spatial (OpenXR + XR Management + XRI)");
        }

        public static void InstallSpatialMrSdk()
        {
            var packages = new List<string> { PKG_XR_MANAGEMENT, PKG_OPENXR, PKG_AR_FOUNDATION };
            QueuePackagesForInstallation(packages, "Mixed Reality / Passthrough (OpenXR + AR Foundation)");
        }

        private static void QueuePackagesForInstallation(List<string> packages, string suiteName)
        {
            installQueue.Clear();
            foreach (var pkg in packages)
            {
                installQueue.Enqueue(pkg);
            }

            Debug.Log($"[XR Heritage Framework] Initiating installation of {suiteName}...");
            EditorApplication.update += ProcessInstallQueue;
            ProcessNextPackage();
        }

        private static void ProcessInstallQueue()
        {
            if (currentAddRequest == null) return;

            if (currentAddRequest.IsCompleted)
            {
                if (currentAddRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[XR Heritage Framework] Successfully installed: {currentlyInstallingPackage}");
                }
                else if (currentAddRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"[XR Heritage Framework] Failed to install {currentlyInstallingPackage}: {currentAddRequest.Error.message}");
                }

                currentAddRequest = null;
                EditorUtility.ClearProgressBar();

                if (installQueue.Count > 0)
                {
                    ProcessNextPackage();
                }
                else
                {
                    EditorApplication.update -= ProcessInstallQueue;
                    EditorUtility.DisplayDialog("SDK Installation Complete", 
                        "All required XR SDKs have been added to your Unity project!\n\nUnity will now recompile project assemblies.", 
                        "OK");
                }
            }
            else
            {
                EditorUtility.DisplayProgressBar("Installing XR SDK", $"Adding package: {currentlyInstallingPackage}...", 0.5f);
            }
        }

        private static void ProcessNextPackage()
        {
            if (installQueue.Count == 0) return;

            currentlyInstallingPackage = installQueue.Dequeue();
            Debug.Log($"[XR Heritage Framework] Requesting Package Manager to add: {currentlyInstallingPackage}");
            currentAddRequest = Client.Add(currentlyInstallingPackage);
        }

        /// <summary>
        /// Evaluates the imported experience device target and prompts for relevant SDK installation.
        /// </summary>
        public static void CheckAndPromptSdkForExperience(ArtworkExperienceData data)
        {
            if (data == null) return;

            string target = "";
            if (data.exhibitionConstraints != null && !string.IsNullOrEmpty(data.exhibitionConstraints.deviceTarget))
            {
                target = data.exhibitionConstraints.deviceTarget;
            }

            if (string.IsNullOrEmpty(target)) return;

            if (target.IndexOf("AR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                target.IndexOf("Tablet", StringComparison.OrdinalIgnoreCase) >= 0 ||
                target.IndexOf("Phone", StringComparison.OrdinalIgnoreCase) >= 0 ||
                target.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                bool install = EditorUtility.DisplayDialog(
                    "Install Mobile AR SDKs?",
                    $"This experience is configured for '{target}'.\n\nWould you like to install the required Unity Mobile AR SDKs (AR Foundation, ARCore, and ARKit) now?",
                    "Install AR SDKs",
                    "Skip for Now"
                );

                if (install)
                {
                    InstallMobileArSdk();
                }
            }
            else if (target.IndexOf("VR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     target.IndexOf("Headset", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     target.IndexOf("Immersive", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     target.IndexOf("OpenXR", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                bool install = EditorUtility.DisplayDialog(
                    "Install OpenXR SDKs?",
                    $"This experience is configured for '{target}'.\n\nWould you like to install the required OpenXR and XR Interaction Toolkit SDKs now?",
                    "Install OpenXR SDKs",
                    "Skip for Now"
                );

                if (install)
                {
                    InstallVrOpenXrSdk();
                }
            }
        }
    }
}
#endif
