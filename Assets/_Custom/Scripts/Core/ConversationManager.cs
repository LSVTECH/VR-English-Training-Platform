using System;
using UnityEngine;
using UnityEngine.Networking;

namespace VREnglish.Core
{
    /// <summary>
    /// Orquestador central de la conversación. 
    /// Mantiene la separación de preocupaciones entre la captura de audio, la red y la respuesta del NPC.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ConversationManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private VREnglish.Audio.AudioRecorder audioRecorder;
        [SerializeField] private VREnglish.Network.BackendClient backendClient;
        [SerializeField] private AudioSource npcAudioSource;

        [Header("UI References")]
        [SerializeField] private VREnglish.UI.NPCSpeechBubble speechBubble;
        
        public enum ConversationState
        {
            Idle,
            ListeningToPlayer,
            ProcessingBackend,
            NPCSpeaking
        }

        [Header("State Debug")]
        [SerializeField] private ConversationState currentState = ConversationState.Idle;

        private void OnEnable()
        {
            if (audioRecorder != null) audioRecorder.OnSpeechRecorded += HandlePlayerSpeech;
            if (backendClient != null) 
            {
                backendClient.OnSubtitleReceived += HandleSubtitleReceived;
                backendClient.OnAudioUrlReceived += HandleAudioUrlReceived;
                backendClient.OnError += HandleNetworkError;
            }
        }

        private void OnDisable()
        {
            if (audioRecorder != null) audioRecorder.OnSpeechRecorded -= HandlePlayerSpeech;
            if (backendClient != null) 
            {
                backendClient.OnSubtitleReceived -= HandleSubtitleReceived;
                backendClient.OnAudioUrlReceived -= HandleAudioUrlReceived;
                backendClient.OnError -= HandleNetworkError;
            }
        }

        private void HandlePlayerSpeech(byte[] audioData)
        {
            if (currentState == ConversationState.NPCSpeaking) return;
            
            ChangeState(ConversationState.ProcessingBackend);
            backendClient.SendAudioAsync(audioData);
        }

        private void HandleSubtitleReceived(string aiText)
        {
            Debug.Log($"<color=cyan>[NPC Subtitle]</color> {aiText}");
            if (speechBubble != null) speechBubble.ShowMessage(aiText);
        }

        private void HandleAudioUrlReceived(string audioUrl)
        {
            string fullUrl = audioUrl.StartsWith("http") ? audioUrl : "http://localhost:8080" + audioUrl;
            StartCoroutine(DownloadAndPlayAudio(fullUrl));
        }

        private System.Collections.IEnumerator DownloadAndPlayAudio(string url)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (npcAudioSource != null)
                    {
                        ChangeState(ConversationState.NPCSpeaking);
                        npcAudioSource.clip = clip;
                        npcAudioSource.Play();
                        Invoke(nameof(OnNPCSpeechFinished), clip.length);
                    }
                }
                else
                {
                    Debug.LogError($"[ConversationManager] Error downloading TTS: {www.error}");
                    ChangeState(ConversationState.Idle);
                }
            }
        }

        private void HandleNetworkError(string error)
        {
            Debug.LogError($"[ConversationManager] Network Error: {error}");
            ChangeState(ConversationState.Idle);
        }

        private void OnNPCSpeechFinished()
        {
            ChangeState(ConversationState.Idle);
        }

        private void ChangeState(ConversationState newState)
        {
            currentState = newState;
            Debug.Log($"[ConversationManager] State: <color=yellow>{newState}</color>");
        }

        // --- Métodos públicos para VRRecordButton (hold-to-talk) ---

        /// <summary>
        /// Llamado cuando el usuario PULSA el botón A del mando VR.
        /// </summary>
        public void StartRecording()
        {
            if (currentState != ConversationState.Idle) return;
            audioRecorder.StartListening();
            ChangeState(ConversationState.ListeningToPlayer);
        }

        /// <summary>
        /// Llamado cuando el usuario SUELTA el botón A del mando VR.
        /// </summary>
        public void StopRecording()
        {
            if (currentState != ConversationState.ListeningToPlayer) return;
            audioRecorder.StopListening();
            // El audio se enviará automáticamente via el evento OnSpeechRecorded
        }
    }
}
