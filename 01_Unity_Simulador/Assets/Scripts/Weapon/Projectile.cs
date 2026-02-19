using UnityEngine;
using PoligonoVirtual.World; // Namespace for ReactiveTarget

namespace PoligonoVirtual.Weapon
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("Balística")]
        public float speed = 15f; // VELOCIDAD DEBUG (Lenta para ver trayectoria)
        public float lifetime = 5f; // Auto-destrucción tras 5s si no choca

        [Header("Efectos")]
        public GameObject impactEffectPrefab; // Opcional: Chispa/Impacto

        private Rigidbody _rb;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            
            // --- DEBUG VISUAL: TRAIL RENDERER ---
            TrailRenderer tr = gameObject.AddComponent<TrailRenderer>();
            tr.time = 0.5f;
            tr.startWidth = 0.05f;
            tr.endWidth = 0.01f;
            tr.material = new Material(Shader.Find("Sprites/Default"));
            tr.startColor = Color.yellow;
            tr.endColor = Color.red;
            // ------------------------------------

            // Aplicar velocidad inicial
            // Usamos ForceMode.VelocityChange para ignorar la masa
            // Nota: En Unity 6 usamos linearVelocity, mantenemos compatibilidad con velocity si es necesario.
            // _rb.velocity = transform.forward * speed; 
             _rb.linearVelocity = transform.forward * speed;

            // Auto-cleanup para no llenar la jerarquía de balas perdidas
            Destroy(gameObject, lifetime);
        }

        void OnCollisionEnter(Collision collision)
        {
            // --- LOG VITAL DE DEPURACIÓN ---
            Debug.Log($"Bala chocó contra: {collision.gameObject.name} | Tag: {collision.gameObject.tag}");
            // -------------------------------

            // 1. Verificar si golpeamos un blanco reactivo
            ReactiveTarget target = collision.gameObject.GetComponent<ReactiveTarget>();
            
            if (target != null)
            {
                // Calcular fuerza del impacto: Velocidad * Masa
                Vector3 impactForce = _rb.linearVelocity * _rb.mass; 
                
                // Punto de contacto
                Vector3 impactPoint = collision.contacts[0].point;

                target.Hit(impactPoint, impactForce);
            }

            // 2. Efecto visual (Opcional)
            if (impactEffectPrefab != null)
            {
                // Instanciar efecto en el punto exacto y rotación de la normal
                ContactPoint contact = collision.contacts[0];
                Instantiate(impactEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
            }

            // 3. Destruir la bala
            Destroy(gameObject);
        }
    }
}
