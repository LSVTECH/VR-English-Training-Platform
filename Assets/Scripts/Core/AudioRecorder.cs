using UnityEngine;

public class AudioRecorder : MonoBehaviour 
{ 
    public float silenceThreshold = 0.02f;  // ajustar según el entorno 
    public float silenceDuration = 1.2f;    // segundos de silencio para cortar 
    public int sampleRate = 16000;           // Whisper trabaja a 16kHz 
 
    private AudioClip micClip; 
    private bool isRecording = false; 
    private float silenceTimer = 0f; 
 
    public System.Action<AudioClip> OnRecordingComplete; 
 
    public void StartListening() 
    { 
        micClip = Microphone.Start(null, true, 30, sampleRate); 
        isRecording = true; 
        silenceTimer = 0f; 
    } 
 
    void Update() 
    { 
        if (!isRecording) return; 
        float[] samples = new float[128]; 
        int pos = Microphone.GetPosition(null) - 128; 
        if (pos < 0) return; 
        micClip.GetData(samples, pos); 
        float energy = 0f; 
        foreach (var s in samples) energy += s * s; 
        energy = Mathf.Sqrt(energy / samples.Length); 
        if (energy < silenceThreshold) 
        { 
            silenceTimer += Time.deltaTime; 
            if (silenceTimer >= silenceDuration) 
            { 
                StopAndSend(); 
            } 
        } 
        else silenceTimer = 0f; 
    } 
    
    void StopAndSend() 
    { 
        isRecording = false; 
        Microphone.End(null); 
        OnRecordingComplete?.Invoke(micClip); 
    } 
}
