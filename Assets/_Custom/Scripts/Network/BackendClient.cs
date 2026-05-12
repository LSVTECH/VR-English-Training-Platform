using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace VREnglish.Network
{
    /// <summary>
    /// Encargada de comunicarse con FastAPI por HTTP.
    /// Envía audio grabado y recibe respuestas (texto + audio url).
    /// </summary>
    public class BackendClient : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private string backendUrl = "http://localhost:8080";
        [SerializeField] private string sessionId = "test_user_session";

        public event Action<string> OnSubtitleReceived;
        public event Action<string> OnAudioUrlReceived;
        public event Action<float, string> OnScoreReceived;
        public event Action<string> OnError;

        public void SendAudioAsync(byte[] wavData)
        {
            StartCoroutine(UploadAudioCoroutine(wavData));
        }

        private IEnumerator UploadAudioCoroutine(byte[] wavData)
        {
            string url = $"{backendUrl.TrimEnd('/')}/api/v1/conversation";

            WWWForm form = new WWWForm();
            form.AddBinaryData("audio", wavData, "speech.wav", "audio/wav");
            form.AddField("session_id", sessionId);
            form.AddField("turn_number", 1); // Incrementar en el manager

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[BackendClient] Error: {www.error}");
                    OnError?.Invoke(www.error);
                }
                else
                {
                    Debug.Log($"[BackendClient] Respuesta recibida: {www.downloadHandler.text}");
                    ProcessResponse(www.downloadHandler.text);
                }
            }
        }

        private void ProcessResponse(string json)
        {
            try
            {
                var response = JsonUtility.FromJson<BackendResponse>(json);
                OnSubtitleReceived?.Invoke(response.ai_response_text);
                OnAudioUrlReceived?.Invoke(response.audio_url);
                OnScoreReceived?.Invoke(response.score, response.feedback);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BackendClient] Error parsing response: {e.Message}");
            }
        }

        [Serializable]
        private class BackendResponse
        {
            public string transcription;
            public string ai_response_text;
            public string audio_url;
            public float score;
            public string feedback;
        }
    }
}
