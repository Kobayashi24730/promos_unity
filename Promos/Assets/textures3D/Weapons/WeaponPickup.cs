using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Configurações")]
        [Tooltip("Arraste aqui o osso da mão do personagem (RightHand) onde a arma vai ficar.")]
        public Transform weaponHolder;
        [Tooltip("Tecla para apanhar a arma.")]
        public KeyCode pickupKey = KeyCode.E;
        [Header("Mensagem de Interação (Estilo Skyrim)")]
        [Tooltip("Texto que aparecerá na HUD quando chegar perto.")]
        public string promptMessage = "Pressione [E] para pegar Faca";
        private bool _isPlayerNearby = false;
        private GameObject _player;
        public Vector3 possitionOffset = Vector3.zero;
        public Vector3 rotationOffset =  Vector3.zero;
        public Vector3 localScale = new Vector3(1f, 1f, 1f);

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<ThirdPersonController>() != null)
            {
                _isPlayerNearby = true;
                if (InteractionPrompt.Instance != null)
                {
                    InteractionPrompt.Instance.Show(promptMessage);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<ThirdPersonController>() != null)
            {
                _isPlayerNearby = false;
                if (InteractionPrompt.Instance!= null)
                {
                    InteractionPrompt.Instance.Hide();
                }
            }
        }

        private void Update()
        {
            if (_isPlayerNearby)
            {
                bool pressed = false;

                #if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null) {
                    if (pickupKey == KeyCode.E && Keyboard.current.eKey.wasPressedThisFrame)
                        pressed = true;
                    else if (pickupKey == KeyCode.F && Keyboard.current.fKey.wasPressedThisFrame)
                        pressed = true;
                }
                #else
                if (Input.GetKeyDown(pickupKey)) {
                    pressed = true;
                }
                #endif

                if (pressed) {
                    PickUp();
                }
            }
        }

        private void PickUp()
        {
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.Hide();
            }
            if (weaponHolder != null)
            {
                transform.SetParent(weaponHolder, false);
                transform.localScale = localScale;
                transform.localPosition = possitionOffset;
                transform.localRotation = Quaternion.Euler(rotationOffset);

                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                enabled = false;
                Debug.Log("Arma apanhada com sucesso!");
            }
            else
            {
                Debug.LogWarning("O 'WeaponHolder' (mão do personagem) não foi definido no inspetor!");
            }
        }
    }
}