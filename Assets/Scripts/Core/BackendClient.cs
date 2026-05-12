using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class BackendClient : MonoBehaviour
{
    private string baseUrl = "http://localhost:8000/api/v1";

    public IEnumerator InitSession(string sessionId, string scenario, string level, Action<ConversationResponse> callback)
    {
        // Placeholder for session init
        Debug.Log($"Initializing session: {sessionId}");
        yield return null;
    }

    public IEnumerator SendAudio(string sessionId, AudioClip clip, Action<ConversationResponse> callback)
    {
        // Placeholder for sending audio
        Debug.Log($"Sending audio for session: {sessionId}");
        yield return null;
    }
}
