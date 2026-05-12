using System;
using System.IO;
using UnityEngine;

namespace VREnglish.Audio
{
    /// <summary>
    /// Captura el audio del micrófono y lo convierte a formato WAV para enviarlo al backend.
    /// </summary>
    public class AudioRecorder : MonoBehaviour
    {
        public event Action<byte[]> OnSpeechRecorded;

        private AudioClip recording;
        private string micName;
        private bool isRecording = false;

        private void Start()
        {
            if (Microphone.devices.Length > 0)
            {
                micName = Microphone.devices[0];
                Debug.Log($"[AudioRecorder] Usando micrófono: {micName}");
            }
            else
            {
                Debug.LogError("[AudioRecorder] No se detectó ningún micrófono.");
            }
        }

        public void StartListening()
        {
            if (string.IsNullOrEmpty(micName)) return;
            
            isRecording = true;
            recording = Microphone.Start(micName, false, 10, 16000);
            Debug.Log("[AudioRecorder] Grabación iniciada...");
        }

        public void StopListening()
        {
            if (!isRecording) return;

            isRecording = false;
            int lastPos = Microphone.GetPosition(micName);
            Microphone.End(micName);

            if (lastPos > 0)
            {
                byte[] wavData = ConvertToWav(recording, lastPos);
                OnSpeechRecorded?.Invoke(wavData);
                Debug.Log($"[AudioRecorder] Grabación finalizada. Enviando {wavData.Length} bytes.");
            }
        }

        private byte[] ConvertToWav(AudioClip clip, int lengthSamples)
        {
            float[] samples = new float[lengthSamples * clip.channels];
            clip.GetData(samples, 0);

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    writer.Write(36 + samples.Length * 2);
                    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                    writer.Write(16);
                    writer.Write((short)1);
                    writer.Write((short)clip.channels);
                    writer.Write(16000);
                    writer.Write(16000 * clip.channels * 2);
                    writer.Write((short)(clip.channels * 2));
                    writer.Write((short)16);
                    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples)
                    {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return stream.ToArray();
            }
        }
    }
}
