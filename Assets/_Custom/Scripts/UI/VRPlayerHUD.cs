using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VREnglish.UI
{
    /// <summary>
    /// Gestiona la interfaz de usuario que sigue al jugador (HUD).
    /// Muestra el estado del micrófono y el puntaje obtenido.
    /// </summary>
    public class VRPlayerHUD : MonoBehaviour
    {
        [Header("Microphone UI")]
        [SerializeField] private GameObject micIconRoot;
        [SerializeField] private Image micFillImage;

        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 5f;

        private float feedbackTimer = 0f;

        private void Start()
        {
            if (micIconRoot != null) micIconRoot.SetActive(false);
            if (scoreText != null) scoreText.text = "Score: 0";
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (feedbackText != null && feedbackText.gameObject.activeSelf)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0f)
                {
                    feedbackText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Activa/Desactiva el icono del micrófono en el HUD.
        /// </summary>
        public void SetMicActive(bool isActive)
        {
            if (micIconRoot != null)
            {
                micIconRoot.SetActive(isActive);
            }
        }

        /// <summary>
        /// Actualiza el puntaje y muestra un feedback temporal.
        /// </summary>
        public void UpdateScore(float score, string feedback = "")
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {Mathf.RoundToInt(score)}";
            }

            if (!string.IsNullOrEmpty(feedback) && feedbackText != null)
            {
                feedbackText.text = feedback;
                feedbackText.gameObject.SetActive(true);
                feedbackTimer = displayDuration;
            }
        }
    }
}
