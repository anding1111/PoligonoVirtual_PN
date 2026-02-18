using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace PoligonoVirtual.Network
{
    /// <summary>
    /// Estructura simple para deserializar el JSON del Sentinel-X1.
    /// Recibimos la rotación completa (Cuaternión) y el estado del gatillo.
    /// </summary>
    [Serializable]
    public struct SentinelData
    {
        public float quatW;
        public float quatX;
        public float quatY;
        public float quatZ;
        public bool trigger;
    }

    /// <summary>
    /// Maneja la recepción de datos UDP en un hilo separado para evitar bloquear el Main Thread de Unity.
    /// Utiliza un ConcurrentQueue para pasar los datos de forma segura al Update().
    /// </summary>
    public class UDPReceiver : MonoBehaviour
    {
        [Header("Configuración UDP")]
        [SerializeField] private int port = 4242;
        [SerializeField] private bool showDebugLogs = true;

        // Cola concurrente para comunicación Hilo UDP -> Hilo Unity (Main Thread)
        // Usamos ConcurrentQueue porque es thread-safe y muy rápida (lock-free en muchos casos).
        private ConcurrentQueue<SentinelData> _dataQueue = new ConcurrentQueue<SentinelData>();

        private Thread _udpThread;
        private UdpClient _udpClient;
        private bool _isRunning = false;

        void Start()
        {
            StartUDPThread();
        }

        private void StartUDPThread()
        {
            _isRunning = true;
            _udpThread = new Thread(new ThreadStart(ReceiveData));
            _udpThread.IsBackground = true; // Asegura que el hilo muera si la app se cierra forzosamente
            _udpThread.Start();
            
            if (showDebugLogs) Debug.Log($"[UDPReceiver] Iniciado en puerto {port}");
        }

        /// <summary>
        /// Bucle principal del hilo de recepción UDP.
        /// </summary>
        private void ReceiveData()
        {
            try
            {
                _udpClient = new UdpClient(port);
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (_isRunning)
                {
                    try
                    {
                        // Bloqueante hasta recibir datos. 
                        // En producción real, se podría usar ReceiveAsync o configurar Timeout
                        // para comprobar _isRunning más frecuentemente si no llegan datos.
                        byte[] data = _udpClient.Receive(ref remoteEndPoint);

                        if (data.Length > 0)
                        {
                            string json = Encoding.UTF8.GetString(data);
                            
                            // Deserialización simple
                            // NOTA: JsonUtility es rápido pero genera algo de garbage. 
                            // Para optimización extrema (frecuencia > 100Hz), considerar parsing manual 
                            // o librerías Zero-Allocation si el GC se vuelve un problema.
                            SentinelData sentinelData = JsonUtility.FromJson<SentinelData>(json);

                            // Encolamos el dato más reciente
                            _dataQueue.Enqueue(sentinelData);
                        }
                    }
                    catch (SocketException sockEx)
                    {
                        // Ignorar error de interrupción al cerrar el socket
                        if (_isRunning) Debug.LogError($"[UDPReceiver] Socket Error: {sockEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[UDPReceiver] Error procesando datos: {ex.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDPReceiver] Error fatal en hilo UDP: {e.Message}");
            }
            finally
            {
                if (_udpClient != null)
                {
                    _udpClient.Close();
                    _udpClient = null;
                }
            }
        }

        /// <summary>
        /// Intenta obtener el último dato recibido del sensor.
        /// Debe ser llamado desde el Main Thread (Update).
        /// </summary>
        /// <param name="data">El dato si existe, o default si no.</param>
        /// <returns>True si se obtuvo un dato, False si la cola estaba vacía.</returns>
        public bool TryGetLatestData(out SentinelData data)
        {
            // Intentamos sacar el dato más antiguo de la cola (FIFO).
            // En un escenario de alta frecuencia, podríamos querer vaciar la cola
            // y quedarnos solo con el ÚLTIMO (LIFO behaviour simulado) para reducir latencia,
            // pero ConcurrentQueue no tiene TakeLast. 
            // Para latencia crítica, se podría implementar un buffer circular o una variable volátil con lock.
            // Por ahora, procesamos FIFO normal.
            return _dataQueue.TryDequeue(out data);
        }



        private void OnApplicationQuit()
        {
            _isRunning = false;
            
            // Forzamos cierre del cliente para desbloquear el Receive()
            if (_udpClient != null) 
            {
                _udpClient.Close();
                _udpClient = null;
            }

            // Esperamos a que el hilo termine (con timeout por seguridad)
            if (_udpThread != null && _udpThread.IsAlive)
            {
                _udpThread.Join(100); 
            }
            
            Debug.Log("[UDPReceiver] Finalizado correctamente.");
        }
    }
}
