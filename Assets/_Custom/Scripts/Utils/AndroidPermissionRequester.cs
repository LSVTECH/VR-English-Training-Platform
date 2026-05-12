using UnityEngine;
using UnityEngine.Android;

namespace VREnglish.Utils
{
    /// <summary>
    /// Asegura que el usuario acepte los permisos de micrófono en Android/Quest.
    /// </summary>
    public class AndroidPermissionRequester : MonoBehaviour
    {
        void Start()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#endif
        }
    }
}
