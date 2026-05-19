using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using AK.Wwise;

namespace Gamekit3D
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioPlayer : MonoBehaviour
    {
        // =============================
        // AUDIO BACKEND CONTROL
        // =============================

        [Header("Audio Backend")]
        [Tooltip("If true, Wwise will be used instead of Unity AudioSource.")]
        public bool useWwise = false;

        // =============================
        // UNITY AUDIO STRUCTURES
        // =============================

        [Serializable]
        public class SoundBank
        {
            public string name;
            public AudioClip[] clips;
        }

        [Serializable]
        public class MaterialAudioOverride
        {
            public Material[] materials;

            [Header("Unity Audio")]
            public SoundBank[] banks;

            [Header("Wwise Audio")]
            public AK.Wwise.Event[] wwiseEvents;
            public AK.Wwise.Switch wwiseSwitch;
        }

        [Header("Unity Audio Settings")]
        public bool randomizePitch = true;
        public float pitchRandomRange = 0.2f;
        public float playDelay = 0;
        public SoundBank defaultBank = new SoundBank();

        [Header("Wwise Default Settings")]
        public AK.Wwise.Event[] defaultWwiseEvents;
        public AK.Wwise.Switch defaultSwitch;

        [Header("Material Overrides")]
        public MaterialAudioOverride[] materialOverrides;

        [Header("Debug")]
        public bool debug = false;

        [HideInInspector] public bool playing;
        [HideInInspector] public bool canPlay;

        protected AudioSource m_AudioSource;

        protected Dictionary<Material, SoundBank[]> m_UnityLookup =
            new Dictionary<Material, SoundBank[]>();

        protected Dictionary<Material, AK.Wwise.Event[]> m_WwiseEventLookup =
            new Dictionary<Material, AK.Wwise.Event[]>();

        protected Dictionary<Material, AK.Wwise.Switch> m_WwiseSwitchLookup =
            new Dictionary<Material, AK.Wwise.Switch>();

        protected Dictionary<string, SoundBank[]> m_UnityNameLookup =
            new Dictionary<string, SoundBank[]>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, AK.Wwise.Event[]> m_WwiseEventNameLookup =
            new Dictionary<string, AK.Wwise.Event[]>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, AK.Wwise.Switch> m_WwiseSwitchNameLookup =
            new Dictionary<string, AK.Wwise.Switch>(StringComparer.OrdinalIgnoreCase);

        public AudioSource audioSource => m_AudioSource;
        public AudioClip clip { get; private set; }

        // =============================
        // INITIALIZATION
        // =============================

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();

            BuildMaterialLookups();

            // If using Wwise, disable AudioSource to avoid double playback
            if (useWwise && m_AudioSource != null)
                m_AudioSource.enabled = false;
        }

        void BuildMaterialLookups()
        {
            m_UnityLookup.Clear();
            m_WwiseEventLookup.Clear();
            m_WwiseSwitchLookup.Clear();
            m_UnityNameLookup.Clear();
            m_WwiseEventNameLookup.Clear();
            m_WwiseSwitchNameLookup.Clear();

            foreach (var entry in materialOverrides)
            {
                if (entry.materials == null) continue;

                foreach (var mat in entry.materials)
                {
                    if (mat == null) continue;

                    string cleanName = mat.name.Replace(" (Instance)", "").Trim();

                    if (entry.banks != null && entry.banks.Length > 0)
                    {
                        m_UnityLookup[mat] = entry.banks;
                        m_UnityNameLookup[cleanName] = entry.banks;
                    }

                    if (entry.wwiseEvents != null && entry.wwiseEvents.Length > 0)
                    {
                        m_WwiseEventLookup[mat] = entry.wwiseEvents;
                        m_WwiseEventNameLookup[cleanName] = entry.wwiseEvents;
                    }

                    if (entry.wwiseSwitch != null)
                    {
                        m_WwiseSwitchLookup[mat] = entry.wwiseSwitch;
                        m_WwiseSwitchNameLookup[cleanName] = entry.wwiseSwitch;
                    }
                }
            }
        }

        // ============================================================
        // PUBLIC PLAY FUNCTIONS (Game Kit calls these everywhere)
        // ============================================================

        public void PlayRandomClip()
        {
            PlayRandomClip(null, 0);
        }

        public AudioClip PlayRandomClip(Material overrideMaterial, int bankId = 0)
        {
            if (useWwise)
            {
                PlayWwise(overrideMaterial, bankId, gameObject);
                return null;
            }

            return PlayUnity(overrideMaterial, bankId);
        }

        // ============================================================
        // UNITY BACKEND
        // ============================================================

        private AudioClip PlayUnity(Material overrideMaterial, int bankId)
        {
            SoundBank bank = defaultBank;
            bool matchedOverride = false;

            if (overrideMaterial != null)
            {
                if (m_UnityLookup.TryGetValue(overrideMaterial, out var banks))
                {
                    if (bankId >= 0 && bankId < banks.Length)
                    {
                        bank = banks[bankId];
                        matchedOverride = true;
                    }
                }
                else
                {
                    string cleanName = overrideMaterial.name.Replace(" (Instance)", "").Trim();
                    if (m_UnityNameLookup.TryGetValue(cleanName, out banks))
                    {
                        if (bankId >= 0 && bankId < banks.Length)
                        {
                            bank = banks[bankId];
                            matchedOverride = true;
                        }
                    }
                }
            }

            if (debug)
            {
                Debug.Log($"[{name}] Unity Play. Material: {(overrideMaterial != null ? overrideMaterial.name : "None")} | Matched Override: {matchedOverride} | Bank: {bank.name}");
            }

            if (bank.clips == null || bank.clips.Length == 0)
                return null;

            var chosenClip = bank.clips[Random.Range(0, bank.clips.Length)];
            if (chosenClip == null)
                return null;

            if (m_AudioSource == null)
                return null;

            m_AudioSource.pitch = randomizePitch
                ? Random.Range(1f - pitchRandomRange, 1f + pitchRandomRange)
                : 1f;

            m_AudioSource.clip = chosenClip;
            m_AudioSource.PlayDelayed(playDelay);

            clip = chosenClip;

            if (debug)
                Debug.Log($"{name} playing Unity clip: {chosenClip.name}");

            return chosenClip;
        }

        // ============================================================
        // WWISE BACKEND
        // ============================================================

        private void PlayWwise(Material overrideMaterial, int bankId, GameObject target)
        {
            AK.Wwise.Event[] eventsToUse = defaultWwiseEvents;
            AK.Wwise.Switch switchToUse = defaultSwitch;
            bool matchedEvents = false;
            bool matchedSwitch = false;

            if (overrideMaterial != null)
            {
                // Match Event
                if (m_WwiseEventLookup.TryGetValue(overrideMaterial, out var matEvents))
                {
                    eventsToUse = matEvents;
                    matchedEvents = true;
                }
                else
                {
                    string cleanName = overrideMaterial.name.Replace(" (Instance)", "").Trim();
                    if (m_WwiseEventNameLookup.TryGetValue(cleanName, out matEvents))
                    {
                        eventsToUse = matEvents;
                        matchedEvents = true;
                    }
                }

                // Match Switch
                if (m_WwiseSwitchLookup.TryGetValue(overrideMaterial, out var matSwitch))
                {
                    switchToUse = matSwitch;
                    matchedSwitch = true;
                }
                else
                {
                    string cleanName = overrideMaterial.name.Replace(" (Instance)", "").Trim();
                    if (m_WwiseSwitchNameLookup.TryGetValue(cleanName, out matSwitch))
                    {
                        switchToUse = matSwitch;
                        matchedSwitch = true;
                    }
                }
            }

            if (debug)
            {
                Debug.Log($"[{name}] Wwise Play. Material: {(overrideMaterial != null ? overrideMaterial.name : "None")} | Matched Events: {matchedEvents} | Matched Switch: {matchedSwitch}");
            }

            if (eventsToUse == null || eventsToUse.Length == 0)
            {
                if (debug)
                    Debug.LogWarning($"{name}: No Wwise events assigned.");
                return;
            }

            if (bankId < 0 || bankId >= eventsToUse.Length)
                bankId = 0;

            if (switchToUse != null)
                switchToUse.SetValue(target);

            eventsToUse[bankId].Post(target);

            if (debug)
                Debug.Log($"{name} posted Wwise event: {eventsToUse[bankId].Name}");
        }

        // ============================================================
        // OPTIONAL EXPLICIT WWISE CALLS
        // ============================================================

        public void WwiseEventPlay(int bankId)
        {
            PlayWwise(null, bankId, gameObject);
        }

        public void WwiseEventPlay(Material mat, int bankId)
        {
            PlayWwise(mat, bankId, gameObject);
        }

        public void WwiseEventPlay(GameObject target)
        {
            PlayWwise(null, 0, target);
        }
    }
}