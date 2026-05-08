using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

namespace VREnglish.EditorTools
{
    /// <summary>
    /// Ventana personalizada del Editor para configurar y testear la conexión al Backend de IA.
    /// </summary>
    public class VREnglishSetupWindow : EditorWindow
    {
        private string backendUrl = "http://localhost:8000";
        private bool isConnected = false;
        private string aiName = "Ninguna";
        private bool isConnecting = false;

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
            backendUrl = EditorPrefs.GetString(PREF_URL_KEY, "http://localhost:8000");
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
            
            // Dibujar círculo de color
            Color previousColor = GUI.color;
            if (isConnecting) GUI.color = Color.yellow;
            else if (isConnected) GUI.color = Color.green;
            else GUI.color = Color.red;
            
            GUILayout.Label("●", new GUIStyle(EditorStyles.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter }, GUILayout.Width(20));
            GUI.color = previousColor;
            
            // Texto de estado
            string statusText = isConnecting ? "Conectando..." : (isConnected ? "Conectado" : "Desconectado");
            GUILayout.Label(statusText, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // --- MOSTRAR NOMBRE DE LA IA ---
            if (isConnected)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox($"IA Asignada exitosamente:\n{aiName}", MessageType.Info);
            }

            GUILayout.Space(20);

            // Botón de prueba (se deshabilita mientras carga)
            EditorGUI.BeginDisabledGroup(isConnecting);
            if (GUILayout.Button("Probar Conexión con IA", GUILayout.Height(35)))
            {
                _ = TestConnectionAsync();
            }
            EditorGUI.EndDisabledGroup();
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Nota: Esta ventana intenta hacer un ping GET a tu servidor FastAPI en la ruta '/api/status'. Si el servidor no está encendido, fallará.", MessageType.Warning);
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
    }
}
