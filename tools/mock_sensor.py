import socket
import json
import time
import math

UDP_IP = "127.0.0.1"
UDP_PORT = 4242

print(f"Iniciando Mock Sensor Sentinel-X1 en {UDP_IP}:{UDP_PORT}")

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

start_time = time.time()
last_fire_time = start_time

try:
    while True:
        current_time = time.time()
        elapsed = current_time - start_time
        
        # Simulación de movimiento oscilante (yaw)
        # Rotación alrededor del eje Y (Up)
        # Quaternion alrededor de Y: [0, sin(theta/2), 0, cos(theta/2)] order X, Y, Z, W for Unity usually? 
        # Standard: q = w + xi + yj + zk
        # Unity: x, y, z, w
        
        # Oscilamos entre -45 y 45 grados
        angle_rad = math.sin(elapsed * 2.0) * (math.pi / 4.0) 
        
        # Fórmula de rotación eje-ángulo para eje Y (0,1,0)
        # qx = ax * sin(angle/2) = 0
        # qy = ay * sin(angle/2) = 1 * sin(angle/2)
        # qz = az * sin(angle/2) = 0
        # qw = cos(angle/2)
        
        half_angle = angle_rad / 2.0
        qw = math.cos(half_angle)
        qx = 0.0
        qy = math.sin(half_angle)
        qz = 0.0
        
        # Simulación de disparo cada 3 segundos
        trigger = False
        if current_time - last_fire_time > 3.0:
            trigger = True
            # Mantenemos el trigger true solo un frame/ciclo simulado
            last_fire_time = current_time
            print(">>> DISPARO (TRIGGER TRUE)")
            
        data = {
            "quatW": qw,
            "quatX": qx,
            "quatY": qy, 
            "quatZ": qz,
            "trigger": trigger
        }
        
        message = json.dumps(data).encode('utf-8')
        sock.sendto(message, (UDP_IP, UDP_PORT))
        
        # Feedback visual reducido
        if not trigger:
            # Imprimir cada ~0.5s para no saturar consola
            if int(elapsed * 10) % 5 == 0:
                print(f"Enviando: Rot Y={math.degrees(angle_rad):.1f}° | Trigger={trigger}")
        
        # Simular 60Hz aprox
        time.sleep(1.0/60.0)
        
except KeyboardInterrupt:
    print("\nDeteniendo simulación.")
    sock.close()
