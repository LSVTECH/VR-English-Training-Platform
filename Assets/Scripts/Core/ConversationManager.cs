using UnityEngine; 
using System.Collections; 
using System;

public class ConversationManager : MonoBehaviour 
{ 
    [Header("Referencias")] 
    public AudioRecorder audioRecorder; 
    public BackendClient backendClient; 
    public NPCController npcController; 
    public SubtitleDisplay subtitleDisplay; 
    public HUDManager hudManager; 
 
    [Header("Configuración de sesión")] 
    public string studentLevel = "B1"; // A2, B1, B2 
    public string scenario = "hotel_checkin"; 
 
    private string sessionId; 
    private bool isListening = false; 
 
    void Start() 
    { 
        sessionId = Guid.NewGuid().ToString(); 
        StartCoroutine(InitializeSession()); 
    } 
 
    IEnumerator InitializeSession() 
    { 
        // La IA saluda primero 
        yield return backendClient.InitSession( 
            sessionId, scenario, studentLevel, 
            OnAIResponse 
        ); 
    } 
 
    public void OnStudentFinishedSpeaking(AudioClip recording) 
    { 
        hudManager.ShowState("Procesando..."); 
        StartCoroutine(backendClient.SendAudio( 
            sessionId, recording, OnAIResponse 
        )); 
    } 
 
    void OnAIResponse(ConversationResponse response) 
    { 
        subtitleDisplay.ShowSubtitle(response.aiResponseText); 
        npcController.PlayAudioWithLipSync(response.audioClip); 
        hudManager.UpdateScore(response.scoring); 
    } 
}

[Serializable]
public class ConversationResponse
{
    public string transcription;
    public string aiResponseText;
    public string audio_url;
    public AudioClip audioClip; // Handled by BackendClient
    public ScoringData scoring;
    public ScenarioState scenario_state;
}

[Serializable]
public class ScoringData
{
    public float grammar_score;
    public float vocabulary_score;
    public bool task_completion;
}

[Serializable]
public class ScenarioState
{
    public string current;
    public bool complication_active;
    public int turns_remaining;
}
