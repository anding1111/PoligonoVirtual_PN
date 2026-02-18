/*
 * SENTINEL-X1 FIRMWARE v1.0 (MVP)
 * Plataforma: ESP32-S3 DevKit
 * Sensores: BNO085 (I2C) + Piezoeléctrico (Analog)
 * Comunicación: UDP Wi-Fi (Baja Latencia)
 */

#include <WiFi.h>
#include <WiFiUdp.h>
#include <Wire.h>
#include <Adafruit_BNO08x.h>

// --- CONFIGURACIÓN DE RED (EDITAR ESTO) ---
const char* ssid = "NOMBRE_DE_TU_WIFI";      // Tu red Wi-Fi
const char* password = "TU_CONTRASEÑA";      // Tu contraseña
const char* pcIp = "192.168.1.XX";           // LA IP DE TU COMPUTADOR (Windows/Mac)
const int udpPort = 4242;                    // Puerto que escucha Unity

// --- CONFIGURACIÓN DE HARDWARE ---
// Pines I2C para ESP32-S3 (Revisar pinout de tu placa específica)
#define I2C_SDA 8
#define I2C_SCL 9

// Pin del Piezo (Gatillo)
#define PIN_PIEZO 4       // ADC Pin
#define PIEZO_THRESHOLD 500 // Sensibilidad (0-4095). Ajustar según pruebas.

// --- OBJETOS ---
WiFiUDP udp;
Adafruit_BNO08x bno08x(-1);
sh2_SensorValue_t sensorValue;

// --- ESTRUCTURA DE DATOS (Debe ser idéntica al Struct en Unity C#) ---
struct __attribute__((packed)) SentinelData {
    float qW;
    float qX;
    float qY;
    float qZ;
    bool trigger;
};

SentinelData dataPacket;
bool triggerState = false;
unsigned long lastTriggerTime = 0;

void setup() {
  Serial.begin(115200);
  
  // 1. Configurar Piezo
  pinMode(PIN_PIEZO, INPUT); // Lectura analógica

  // 2. Conexión Wi-Fi Rápida
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  // Desactiva el ahorro de energía del WiFi para reducir latencia
  esp_wifi_set_ps(WIFI_PS_NONE); 
  
  Serial.print("Conectando a WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(100);
    Serial.print(".");
  }
  Serial.println("\nConectado! IP: " + WiFi.localIP().toString());
  
  udp.begin(udpPort);

  // 3. Iniciar IMU BNO085
  Wire.begin(I2C_SDA, I2C_SCL);
  if (!bno08x.begin_I2C()) {
    Serial.println("ERROR CRÍTICO: No se detecta BNO085 Check wiring!");
    while (1) { delay(100); } // Bucle infinito de error
  }

  // Activar reporte de rotación (Game Rotation Vector - Sin magnetómetro)
  // 2500 microsegundos = 400Hz
  bno08x.enableReport(SH2_GAME_ROTATION_VECTOR, 2500);
  
  Serial.println("Sentinel-X1 LISTO Y DISPARANDO DATOS.");
}

void loop() {
  // A. LECTURA DEL GATILLO (PIEZO)
  int piezoVal = analogRead(PIN_PIEZO);
  
  // Lógica simple de disparo (Debounce de 200ms)
  if (piezoVal > PIEZO_THRESHOLD && (millis() - lastTriggerTime > 200)) {
    triggerState = true;
    lastTriggerTime = millis();
    Serial.println("BANG! - Disparo detectado");
  } else {
    triggerState = false;
  }

  // B. LECTURA DE ROTACIÓN Y ENVÍO
  if (bno08x.getSensorEvent(&sensorValue)) {
    if (sensorValue.sensorId == SH2_GAME_ROTATION_VECTOR) {
      
      // Llenar el paquete
      dataPacket.qW = sensorValue.un.gameRotationVector.real;
      dataPacket.qX = sensorValue.un.gameRotationVector.i;
      dataPacket.qY = sensorValue.un.gameRotationVector.j;
      dataPacket.qZ = sensorValue.un.gameRotationVector.k;
      dataPacket.trigger = triggerState;

      // Enviar UDP
      udp.beginPacket(pcIp, udpPort);
      udp.write((const uint8_t*)&dataPacket, sizeof(SentinelData));
      udp.endPacket();
    }
  }
}