using UnityEngine;
using UnityEngine.InputSystem;

namespace VREnglish.Input
{
    /// <summary>
    /// Lee el botón A del mando VR derecho (OpenXR).
    /// Mantener pulsado = grabar. Soltar = enviar audio.
    /// </summary>
    public class VRRecordButton : MonoBehaviour
    {
        [Header("Input Action")]
        [SerializeField] private InputActionReference recordAction;

        [Header("References")]
        [SerializeField] private VREnglish.Core.ConversationManager conversationManager;

        private bool isHolding = false;

        private void OnEnable()
        {
            if (recordAction != null && recordAction.action != null)
            {
                recordAction.action.Enable();
                recordAction.action.started += OnRecordPressed;
                recordAction.action.canceled += OnRecordReleased;
            }
        }

        private void OnDisable()
        {
            if (recordAction != null && recordAction.action != null)
            {
                recordAction.action.started -= OnRecordPressed;
                recordAction.action.canceled -= OnRecordReleased;
                recordAction.action.Disable();
            }
        }

        private void OnRecordPressed(InputAction.CallbackContext ctx)
        {
            if (isHolding) return;
            isHolding = true;
            conversationManager.StartRecording();
            Debug.Log("<color=green>[VRRecordButton]</color> Botón A mantenido → Grabando...");
        }

        private void OnRecordReleased(InputAction.CallbackContext ctx)
        {
            if (!isHolding) return;
            isHolding = false;
            conversationManager.StopRecording();
            Debug.Log("<color=red>[VRRecordButton]</color> Botón A soltado → Enviando audio...");
        }
    }
}
