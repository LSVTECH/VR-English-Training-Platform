using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;

namespace VREnglish.EditorTools
{
    /// <summary>
    /// Ventana personalizada del Editor para configurar y testear la conexión al Backend de IA.
    /// </summary>
    public class VREnglishSetupWindow : EditorWindow
    {
        private string backendUrl = "http://localhost:8080";
        private bool isConnected = false;
        private string aiName = "Ninguna";
        private bool isConnecting = false;
    private string chatInput = "";
    private string chatLog = "";
    private string lastAudioUrl = "";

        private const string PREF_URL_KEY = "VREnglish_BackendUrl";

        // Creamos la pestaña principal en el menú superior de Unity
        [MenuItem("EnglishTrainingPlatform/Connection Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<VREnglishSetupWindow>("AI Connection");
            window.minSize = new Vector2(400, 250);
            window.Show();
        }

        private void OnEnable()
        {
            // Cargar la URL guardada de sesiones anteriores
            backendUrl = EditorPrefs.GetString(PREF_URL_KEY, "http://localhost:8080");
        }

        private void OnGUI()
        {
            GUILayout.Space(15);
            GUILayout.Label("Configuración del Servidor Backend (FastAPI)", EditorStyles.boldLabel);

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            backendUrl = EditorGUILayout.TextField("Backend API URL", backendUrl);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(PREF_URL_KEY, backendUrl);
                isConnected = false; // Resetear estado si cambia la url
            }

            GUILayout.Space(20);

            // --- DIBUJAR INDICADOR DE ESTADO ---
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Estado de Conexión:", GUILayout.Width(130));
            Color previousColor = GUI.color;
            if (isConnecting) GUI.color = Color.yellow;
            else if (isConnected) GUI.color = Color.green;
            else GUI.color = Color.red;
            GUILayout.Label("●", new GUIStyle(EditorStyles.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter }, GUILayout.Width(20));
            GUI.color = previousColor;
            string statusText = isConnecting ? "Conectando..." : (isConnected ? "Conectado" : "Desconectado");
            GUILayout.Label(statusText, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            if (isConnected)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox($"IA Asignada exitosamente:\n{aiName}", MessageType.Info);
            }

            GUILayout.Space(20);

            // Botón de prueba de conexión
            EditorGUI.BeginDisabledGroup(isConnecting);
            if (GUILayout.Button("Probar Conexión con IA", GUILayout.Height(35)))
            {
                _ = TestConnectionAsync();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Nota: Esta ventana intenta hacer un ping GET a tu servidor FastAPI en la ruta '/api/status'. Si el servidor no está encendido, fallará.", MessageType.Warning);

            // --- CONTROL DEL SERVIDOR ---
            GUILayout.Space(20);
            GUILayout.Label("Control del Servidor", EditorStyles.boldLabel);
            if (GUILayout.Button("Encender Servidor Python (Backend)", GUILayout.Height(35)))
            {
                StartBackend();
            }

            // --- CHAT DE PRUEBA ---
            GUILayout.Space(30);
            GUILayout.Label("Chat de prueba (Demo)", EditorStyles.boldLabel);
            chatInput = EditorGUILayout.TextField("Entrada de texto", chatInput);
            if (GUILayout.Button("Enviar al Demo", GUILayout.Height(30)))
            {
                _ = SendDemoRequestAsync();
            }
            if (!string.IsNullOrEmpty(chatLog))
            {
                GUILayout.Label("Conversación:");
                EditorGUILayout.TextArea(chatLog, GUILayout.Height(100));
            }
            if (!string.IsNullOrEmpty(lastAudioUrl))
            {
                if (GUILayout.Button("Reproducir audio de respuesta", GUILayout.Height(30)))
                {
                    PlayAudio(lastAudioUrl);
                }
            }
        }

        private void StartBackend()
        {
            // Ruta del entorno enviada por el usuario
            string pythonPath = @"C:\Users\Usuario\Documents\ENV_Python\env\Scripts\python.exe";
            
            // Ruta de la carpeta backend relativa al proyecto
            string backendPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "backend"));

            if (!File.Exists(pythonPath))
            {
                Debug.LogError($"[VREnglish] No se encontró python en: {pythonPath}. Verifica la ruta del entorno virtual.");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = pythonPath;
            startInfo.Arguments = "-m uvicorn main:app --host 0.0.0.0 --port 8080 --reload";
            startInfo.WorkingDirectory = backendPath;
            startInfo.UseShellExecute = true; // Abre una ventana de comandos externa

            try
            {
                Process.Start(startInfo);
                Debug.Log("[VREnglish] Iniciando servidor FastAPI en ventana externa...");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[VREnglish] Error al iniciar el servidor: {ex.Message}");
            }
        }

        private async Task TestConnectionAsync()
        {
            isConnecting = true;
            isConnected = false;
            aiName = "Ninguna";
            Repaint();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = System.TimeSpan.FromSeconds(4);
                    
                    // Suponiendo que harás un endpoint en FastAPI: @app.get("/api/status")
                    string testUrl = backendUrl.TrimEnd('/') + "/api/status"; 
                    
                    HttpResponseMessage response = await client.GetAsync(testUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        isConnected = true;
                        
                        // Aquí idealmente leeríamos el JSON, por ejemplo: {"ai_name": "GPT-4o-mini", "status": "ok"}
                        // Como es un PoC, simulamos el string extraído si funciona:
                        aiName = "GPT-4o-mini (Perfil: Michael Brown)"; 
                    }
                    else
                    {
                        isConnected = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                isConnected = false;
                Debug.LogWarning($"[VREnglish Setup] No se pudo conectar a {backendUrl}. Error: {ex.Message}");
            }
            finally
            {
                isConnecting = false;
                Repaint();
            }
        }

        // ---------- Demo chat helpers ----------
        private async Task SendDemoRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(chatInput)) return;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{backendUrl.TrimEnd('/')}/demo";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) throw new System.Exception($"Status {response.StatusCode}");
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonUtility.FromJson<DemoResponse>(json);
                    chatLog = $"Yo: {chatInput}\nIA: {data.ai_response_text}\n";
                    lastAudioUrl = data.audio_url;
                    chatInput = "";
                    Repaint();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[VREnglish] Demo error: {ex.Message}");
            }
        }

        private void PlayAudio(string url)
        {
            string fullUrl = $"{backendUrl.TrimEnd('/')}{url}";
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullUrl, AudioType.MPEG);
            var asyncOp = www.SendWebRequest();
            asyncOp.completed += _ =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(www);
                    var source = GetOrCreateAudioSource();
                    source.clip = clip;
                    source.Play();
                }
                else
                {
                    Debug.LogError($"[VREnglish] No se pudo descargar audio: {www.error}");
                }
            };
        }

        private AudioSource GetOrCreateAudioSource()
        {
            var go = GameObject.Find("VREnglishAudioSource");
            if (go == null) go = new GameObject("VREnglishAudioSource");
            var src = go.GetComponent<AudioSource>();
            if (src == null) src = go.AddComponent<AudioSource>();
            return src;
        }

        [System.Serializable]
        private class DemoResponse
        {
            public string transcription;
            public string ai_response_text;
            public string audio_url;
            public string scenario_state;
            public Scoring scoring;
        }

        [System.Serializable]
        private class Scoring
        {
            public float grammar_score;
            public float vocabulary_score;
            public float professionalism_score;
            public string[] grammar_errors;
            public string suggested_phrase;
        }
    }
}
