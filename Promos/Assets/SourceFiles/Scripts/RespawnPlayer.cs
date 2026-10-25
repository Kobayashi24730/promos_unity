using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class RespawnPlayer : MonoBehaviour
    {
        [Tooltip("The Y position threshold at which the player will respawn.")]
        public float yThreshold = -5f; 

        private Vector3 _startingPosition;
        private Quaternion _startingRotation;
        private CharacterController _characterController;
        public CinemachineCamera vCam;
        private ThirdPersonController _thirdPersonController;
        
        [Header("Áudio")]
        public AudioClip respawnSound;

        private void Start()
        {
            // Salva a posição e rotação iniciais
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;

            // Obtém o CharacterController
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                Debug.LogError("CharacterController component is required for RespawnPlayer script!");
            }

            // Obtém o ThirdPersonController
            _thirdPersonController = GetComponent<ThirdPersonController>();
            if (_thirdPersonController == null)
            {
                Debug.LogError("ThirdPersonController component is required for RespawnPlayer!");
            }
        }

        private void Update()
        {
            // Verifica se o jogador caiu abaixo do limite
            if (transform.position.y < yThreshold)
            {
                Respawn();
            }
        }

        private void Respawn()
        {
            // Desativa o CharacterController para permitir alterar a posição sem conflitos de física
            if (_characterController != null)
            {
                _characterController.enabled = false;
            }

            // Reposiciona o jogador
            transform.position = _startingPosition;
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Reativa o CharacterController
            if (_characterController != null)
            {
                _characterController.enabled = true;
            }

            // Reseta a velocidade e a rotação da câmara de forma limpa através do controlador
            if (_thirdPersonController != null)
            {
                _thirdPersonController.ResetVerticalVelocity();
                _thirdPersonController.ResetCameraRotation(90f);
            }

            // Toca o som apenas se ele estiver atribuído no Inspector
            if (respawnSound != null)
            {
                AudioSource.PlayClipAtPoint(respawnSound, transform.position);
            }
        }
    }
}