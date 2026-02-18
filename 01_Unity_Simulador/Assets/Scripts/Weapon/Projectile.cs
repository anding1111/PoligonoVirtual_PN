using UnityEngine;

namespace PoligonoVirtual.Weapon
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("Balística")]
        [SerializeField] private float speed = 350f; // m/s (aprox 9mm)
        [SerializeField] private float lifetime = 5f;

        [Header("Impacto")]
        [SerializeField] private GameObject impactEffectPrefab;

        private Rigidbody _rb;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            
            // Impulso inicial
            // ForceMode.VelocityChange ignora la masa, útil para proyectiles rápidos
            _rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
            
            // Destrucción por tiempo (si no golpea nada)
            Destroy(gameObject, lifetime);
        }

        void OnCollisionEnter(Collision collision)
        {
            // Instanciar efecto si existe
            if (impactEffectPrefab != null)
            {
                // Un pequeño ajuste en la normal para que el efecto no quede dentro de la geometría
                ContactPoint contact = collision.contacts[0];
                Quaternion rot = Quaternion.LookRotation(contact.normal);
                Instantiate(impactEffectPrefab, contact.point, rot);
            }

            // Destruir la bala
            Destroy(gameObject);
        }
    }
}
