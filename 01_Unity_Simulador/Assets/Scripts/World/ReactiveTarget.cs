using UnityEngine;
using System.Collections;

namespace PoligonoVirtual.World
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    public class ReactiveTarget : MonoBehaviour
    {
        [Header("Configuración Blanco")]
        public float resetTime = 5.0f;          // Tiempo para levantarse
        
        [Header("Debug")]
        public AudioClip hitSound; // Audio opcional

        [Header("Estado")]
        private bool _isHit = false;
        private Quaternion _initialRotation;
        private Vector3 _initialPosition;
        private Rigidbody _rb;
        private AudioSource _audioSource;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>();
            
            // Guardar posición inicial
            _initialRotation = transform.localRotation;
            _initialPosition = transform.localPosition;
            
            // IMPORTANTE: Anclar el objeto al inicio para que no caiga por gravedad
            _rb.isKinematic = true;
        }

        public void Hit(Vector3 impactPoint, Vector3 impactForce)
        {
            if (_isHit) return; // Ya fue abatido

            _isHit = true;
            Debug.Log("DIANA ABATIDA: " + gameObject.name);

            // 1. Reproducir Sonido
            if (_audioSource != null)
            {
                // Usamos el clip asignado o el del AudioSource
                AudioClip clipToOneShot = hitSound != null ? hitSound : _audioSource.clip;
                if (clipToOneShot != null)
                {
                    _audioSource.PlayOneShot(clipToOneShot);
                }
            }

            // 2. Activar Física Real
            _rb.isKinematic = false; 

            // 3. Aplicar Impacto (Física realista basada en donde pegó la bala)
            _rb.AddForceAtPosition(impactForce, impactPoint, ForceMode.Impulse);

            // 4. Iniciar Reset
            StartCoroutine(ResetRoutine());
        }

        private IEnumerator ResetRoutine()
        {
            yield return new WaitForSeconds(resetTime);
            ResetTarget();
        }

        public void ResetTarget()
        {
            _isHit = false;

            // Detener movimiento y rotación
            // Nota: En Unity 6 'velocity' podría ser 'linearVelocity'
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            // Congelar físicas de nuevo
            _rb.isKinematic = true; 
            
            // Restaurar Transform original
            transform.localPosition = _initialPosition;
            transform.localRotation = _initialRotation;

            Debug.Log("Blanco Restaurado: " + gameObject.name);
        }
    }
}
