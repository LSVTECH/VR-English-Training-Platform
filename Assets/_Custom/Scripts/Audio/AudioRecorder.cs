using System;
using UnityEngine;

namespace VREnglish.Audio
{
    /// <summary>
    /// Encargado de capturar el micrófono del headset VR.
    /// Implementa VAD (Voice Activity Detection) básico para enviar audio solo cuando el usuario habla.
    /// </summary>
    public class AudioRecorder : MonoBehaviour
    {
        public event Action<byte[]> OnSpeechRecorded;

        [Header("VAD Settings")]
        [SerializeField] private float silenceThreshold = 0.02f;
        [SerializeField] private float maxSilenceDuration = 1.5f;

        private bool isListening = false;
        
        public void StartListening()
        {
            isListening = true;
            Debug.Log("[AudioRecorder] Micrófono encendido, escuchando al usuario...");
            // TODO: Iniciar Microphone.Start()
        }

        public void StopListening()
        {
            isListening = false;
            Debug.Log("[AudioRecorder] Micrófono apagado.");
            // TODO: Detener Microphone.End()
        }

        // Simulación temporal para cuando el usuario termine de hablar
        public void SimulateSpeechEnd()
        {
            if (!isListening) return;
            
            StopListening();
            
            // Simular un payload de audio vacío para propósitos de prueba
            byte[] dummyAudioData = new byte[1024];
            OnSpeechRecorded?.Invoke(dummyAudioData);
        }
    }
}
