using System;
using UnityEngine;

namespace VREnglish.Core
{
    /// <summary>
    /// Orquestador central de la conversación. 
    /// Mantiene la separación de preocupaciones entre la captura de audio, la red y la respuesta del NPC.
    /// </summary>
    [DefaultExecutionOrder(-50)] // Asegura que se inicialice antes que los sistemas dependientes
    public class ConversationManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private VREnglish.Audio.AudioRecorder audioRecorder;
        [SerializeField] private VREnglish.Network.BackendClient backendClient;
        
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
                backendClient.OnTTSAudioReceived += HandleTTSAudioReceived;
            }
        }

        private void OnDisable()
        {
            if (audioRecorder != null) audioRecorder.OnSpeechRecorded -= HandlePlayerSpeech;
            if (backendClient != null) 
            {
                backendClient.OnSubtitleReceived -= HandleSubtitleReceived;
                backendClient.OnTTSAudioReceived -= HandleTTSAudioReceived;
            }
        }

        private void HandlePlayerSpeech(byte[] audioData)
        {
            if (currentState == ConversationState.NPCSpeaking) return;
            
            ChangeState(ConversationState.ProcessingBackend);
            backendClient.SendAudioStreamAsync(audioData);
        }

        private void HandleSubtitleReceived(string aiText)
        {
            // Implementar lógica de UI para subtítulos aquí
            Debug.Log($"[Subtitle] {aiText}");
        }

        private void HandleTTSAudioReceived(AudioClip ttsClip)
        {
            ChangeState(ConversationState.NPCSpeaking);
            // Implementar lógica del NPC hablando aquí
            Debug.Log("[ConversationManager] Audio recibido, el NPC debería hablar ahora.");
            
            // Simular que el NPC termina de hablar después de la duración del clip
            Invoke(nameof(OnNPCSpeechFinished), ttsClip.length);
        }

        private void OnNPCSpeechFinished()
        {
            ChangeState(ConversationState.Idle);
            if (audioRecorder != null) audioRecorder.StartListening();
        }

        private void ChangeState(ConversationState newState)
        {
            currentState = newState;
            Debug.Log($"[ConversationManager] State changed to: {newState}");
        }
    }
}
