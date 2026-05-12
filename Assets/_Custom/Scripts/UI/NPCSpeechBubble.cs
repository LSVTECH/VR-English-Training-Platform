using UnityEngine;
using TMPro;

namespace VREnglish.UI
{
    /// <summary>
    /// Nube de texto flotante que muestra lo que dice la IA.
    /// Se adjunta a un Canvas World Space cerca del NPC.
    /// </summary>
    public class NPCSpeechBubble : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI speechText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private float fadeSpeed = 3f;
        [SerializeField] private float displayDuration = 8f;
        [SerializeField] private bool lookAtCamera = true;

        private float hideTimer = 0f;
        private bool isShowing = false;
        private Transform cameraTransform;

        private void Start()
        {
            cameraTransform = Camera.main?.transform;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            // Billboard: que el canvas siempre mire al jugador
            if (lookAtCamera && cameraTransform != null)
            {
                Vector3 dirToCamera = cameraTransform.position - transform.position;
                dirToCamera.y = 0; // Mantener vertical
                if (dirToCamera != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-dirToCamera);
                }
            }

            // Fade in/out
            if (canvasGroup != null)
            {
                float targetAlpha = isShowing ? 1f : 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }

            // Auto-hide timer
            if (isShowing)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0f)
                {
                    isShowing = false;
                }
            }
        }

        /// <summary>
        /// Muestra un mensaje en la burbuja de texto del NPC.
        /// Llamado por el ConversationManager cuando recibe respuesta de la IA.
        /// </summary>
        public void ShowMessage(string message)
        {
            if (speechText != null)
            {
                speechText.text = message;
            }
            isShowing = true;
            hideTimer = displayDuration;
        }

        /// <summary>
        /// Oculta la burbuja inmediatamente.
        /// </summary>
        public void Hide()
        {
            isShowing = false;
        }
    }
}
