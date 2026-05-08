using System;
using UnityEngine;
using System.Threading.Tasks;

namespace VREnglish.Network
{
    /// <summary>
    /// Capa de red agnóstica a Unity Engine en sus métodos principales.
    /// Encargada de comunicarse con FastAPI por WebSocket.
    /// </summary>
    public class BackendClient : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private string backendWebSocketUrl = "ws://localhost:8000/ws/conversation";

        // Eventos C# estándar.
        public event Action<string> OnSubtitleReceived;
        public event Action<AudioClip> OnTTSAudioReceived;

        private void Start()
        {
            _ = ConnectToServerAsync();
        }

        private async Task ConnectToServerAsync()
        {
            // TODO: Implementar lógica de conexión real con System.Net.WebSockets
            await Task.Yield();
            Debug.Log($"[BackendClient] Connected to {backendWebSocketUrl}");
        }

        public void SendAudioStreamAsync(byte[] pcmData)
        {
            // TODO: Enviar audio por WebSocket al backend
            Debug.Log($"[BackendClient] Enviando {pcmData.Length} bytes de audio al backend...");
        }

        // Método de simulación para ser llamado cuando se reciba texto del backend
        protected virtual void ReceiveTextFromServer(string text)
        {
            OnSubtitleReceived?.Invoke(text);
        }

        // Método de simulación para ser llamado cuando se reciba audio TTS del backend
        protected virtual void ReceiveTTSFromServer(AudioClip clip)
        {
            OnTTSAudioReceived?.Invoke(clip);
        }
    }
}
