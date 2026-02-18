# PROYECTO: SIMULADOR DE TIRO TÁCTICO VR (POLICÍA NACIONAL)

## 1. Visión General
Sistema de entrenamiento inmersivo (MilSim) usando Realidad Virtual, armas reales adaptadas con retroceso CO2 y sensórica propietaria.
- **Objetivo:** Precisión, velocidad de reacción y toma de decisiones (Shoot/Don't shoot).
- **Plataforma VR:** Meta Quest 3 (Standalone / PCVR Tethered).
- **Motor:** Unity 6 (URP).

## 2. Arquitectura de Hardware (Crítica)
### A. Arma (Sentinel-X1)
- **Arma:** Sig Sauer SP 2022 con kit CO2.
- **Sensor:** Módulo ESP32 + BNO085 (IMU 9-Ejes) montado en riel Picatinny.
- **Comunicación:** UDP sobre Wi-Fi. Envía cuaterniones (rotación) y trigger (disparo) a 400Hz.
- **Protocolo de Datos:** JSON/Binario vía UDP al puerto 4242 del PC.

### B. Visualización
- **Headset:** Meta Quest 3.
- **Tracking:** Fusión de datos (Rotación rápida por Sensor UDP + Corrección de posición por visión artificial del Quest).

## 3. Stack de Software
- **Cliente VR:** Unity 6 + C# + Meta XR SDK.
- **Networking Local:** UDP Socket en C# (para latencia <5ms).
- **Backend:** Laravel (API futura para guardar logs de sesiones).
- **Dashboard:** React (Lovable.dev) para visualización en tiempo real.

## 4. Reglas de Desarrollo para el Agente
1. **Rendimiento:** Todo script en Unity debe ser optimizado (evitar `Update` innecesarios).
2. **Latencia:** La prioridad es la recepción de datos UDP del sensor.
3. **Código:** Generar C# modular y comentado.
4. **Seguridad:** No usar munición viva, el sistema es solo simulación.