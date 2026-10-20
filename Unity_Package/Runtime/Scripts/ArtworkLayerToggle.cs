using System.Collections.Generic;
using UnityEngine;

namespace HeritageXR.Framework
{
    public class ArtworkLayerToggle : MonoBehaviour
    {
        [Header("Artwork Visual Layers")]
        [Tooltip("The parent GameObject containing curatorial markers/overlays")]
        [SerializeField] private GameObject curatorialLayerRoot;

        [Tooltip("The parent GameObject containing artist sketches/drafts")]
        [SerializeField] private GameObject artistLayerRoot;

        [Tooltip("Optional 3D reconstructed elements")]
        [SerializeField] private GameObject spatial3DReconstructionRoot;

        [Header("Audio Tracks")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip artistReflectionClip;
        [SerializeField] private AudioClip curatorialContextClip;

        public void SetLayerMode(PerspectiveType mode)
        {
            switch (mode)
            {
                case PerspectiveType.PhysicalArtwork:
                    SetVisibility(false, false, false);
                    StopAudio();
                    break;

                case PerspectiveType.ArtistVoice:
                    SetVisibility(true, false, false);
                    PlayAudio(artistReflectionClip);
                    break;

                case PerspectiveType.CuratorialLens:
                    SetVisibility(false, true, true);
                    PlayAudio(curatorialContextClip);
                    break;

                case PerspectiveType.ComparativeAnalysis:
                    SetVisibility(true, true, true);
                    break;
            }
        }

        private void SetVisibility(bool showArtist, bool showCurator, bool show3D)
        {
            if (artistLayerRoot != null) artistLayerRoot.SetActive(showArtist);
            if (curatorialLayerRoot != null) curatorialLayerRoot.SetActive(showCurator);
            if (spatial3DReconstructionRoot != null) spatial3DReconstructionRoot.SetActive(show3D);
        }

        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        private void StopAudio()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }
}
