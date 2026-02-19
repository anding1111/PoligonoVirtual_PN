using UnityEngine;
using PoligonoVirtual.Network;

namespace PoligonoVirtual.Weapon
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Conexión")]
        public UDPReceiver udpReceiver;

        [Header("Configuración Arma")]
        public GameObject projectilePrefab;
        public Transform muzzlePoint; // Punto de salida de la bala
        public float recoilForce = 0.1f; // Retroceso visual simple (opcional)

        [Header("Calibración")]
        // Offset manual (desde inspector)
        public Quaternion baseOffset = Quaternion.identity;
        // Estado anterior del gatillo
        private bool _lastTriggerState = false;
        // Offset dinámico (Zeroing) calculado en runtime
        private Quaternion _calibrationDelta = Quaternion.identity;

        [Header("Debug")]
        public bool showDebugLogs = true;
        
        // Cache del último dato válido para calibración
        private SentinelData _cachedSensorData;
        private bool _hasReceivedData = false;

        void Update()
        {
            // 1. Procesar datos del sensor (si hay nuevos)
            if (udpReceiver != null && udpReceiver.TryGetLatestData(out SentinelData data))
            {
                _cachedSensorData = data;
                _hasReceivedData = true;
                ProcessSensorData(data);
            }
            
            // 2. Input de Depuración (Editor)
#if UNITY_EDITOR
            HandleEditorInput();
#endif
        }

        private void ProcessSensorData(SentinelData data)
        {
            // Conversión de coordenadas Raw del sensor
            Quaternion sensorRaw = new Quaternion(data.quatX, data.quatY, data.quatZ, data.quatW);

            // Aplicamos rotación:  (Delta de Calibración * (Sensor Raw * Base Offset))
            // El orden de las rotaciones es crítico y depende de cómo queramos aplicar la corrección.
            // Opción robusta: 
            // 1. Aplicar BaseOffset para corregir la orientación física del sensor respecto al arma.
            // 2. Aplicar CalibrationDelta para corregir la dirección 'Adelante' global/local.
            
            // Enfoque simplificado: Rotación Final = CalibrationDelta * SensorRaw * BaseOffset
            // Probamos pre-multiplicando el delta (rotación global relativa)
            transform.localRotation = _calibrationDelta * (sensorRaw * baseOffset);

            // Detección de Disparo
            if (data.trigger && !_lastTriggerState)
            {
                Fire();
            }
            _lastTriggerState = data.trigger;
        }

        /// <summary>
        /// Instancia el proyectil y ejecuta efectos de disparo.
        /// </summary>
        private void Fire()
        {
            if (showDebugLogs) Debug.Log("BANG! Disparo detectado");

            // Validar referencias
            if (projectilePrefab != null && muzzlePoint != null)
            {
                // Instanciar proyectil con la rotación del muzzlePoint
                // (El script Projectile.cs aplicará la velocidad en Start usando transform.forward)
                Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("Falta referencia a ProjectilePrefab o MuzzlePoint en WeaponController.");
            }

            // Sonido
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null) audioSource.Play();
        }

        /// <summary>
        /// Calibra 'Adelante' (Identity) a la rotación actual del sensor.
        /// Es decir, hace que la dirección actual en la que apunta el arma sea el nuevo 'Centro'.
        /// </summary>
        public void CalibrateBoresight()
        {
            // Usamos el último dato conocido para calibrar
            if (_hasReceivedData)
            {
                Quaternion sensorRaw = new Quaternion(_cachedSensorData.quatX, _cachedSensorData.quatY, _cachedSensorData.quatZ, _cachedSensorData.quatW);
                Quaternion currentRotWithoutDelta = sensorRaw * baseOffset;
                
                // Calculamos el inverso para que al multiplicar resulte en Identity
                _calibrationDelta = Quaternion.Inverse(currentRotWithoutDelta);
                
                if (showDebugLogs) Debug.Log("Arma Calibrada (Boresight Zeroed).");
            }
            else
            {
                Debug.LogWarning("No se puede calibrar: No hay datos del sensor recibidos aún.");
            }
        }

        private void HandleEditorInput()
        {
            // 1. Simulación de Disparo
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Fire();
            }

            // 2. Simulación de Calibración
            if (Input.GetKeyDown(KeyCode.C))
            {
                CalibrateBoresight();
            }

            // --- ASEGÚRATE QUE ESTO ESTÉ AQUÍ ---
            // 3. Simulación de Rotación con Flechas
            float rotationSpeed = 100f * Time.deltaTime;
            Vector3 currentEuler = transform.localEulerAngles;

            if (Input.GetKey(KeyCode.LeftArrow)) currentEuler.y -= rotationSpeed;
            if (Input.GetKey(KeyCode.RightArrow)) currentEuler.y += rotationSpeed;
            if (Input.GetKey(KeyCode.UpArrow)) currentEuler.x -= rotationSpeed;
            if (Input.GetKey(KeyCode.DownArrow)) currentEuler.x += rotationSpeed;

            transform.localRotation = Quaternion.Euler(currentEuler);
            // ------------------------------------
        }
    }
}
