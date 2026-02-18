#!/bin/bash
# Script de ayuda para crear el proyecto Unity URP
# Mueve el .gitignore temporalmente para permitir a Unity Hub crear la carpeta limpia.

echo "Preparando carpeta para proyecto Unity..."

if [ -f "01_Unity_Simulador/.gitignore" ]; then
    echo "Resguardando .gitignore..."
    mv 01_Unity_Simulador/.gitignore .gitignore_temp
fi

if [ -d "01_Unity_Simulador" ]; then
    # Solo borrar si está vacía
    rmdir 01_Unity_Simulador 2>/dev/null
    if [ $? -ne 0 ]; then
        echo "La carpeta 01_Unity_Simulador no está vacía. Por favor, asegúrate de que esté limpia salvo el .gitignore."
    else
        echo "Carpeta 01_Unity_Simulador eliminada temporalmente."
    fi
fi

echo ""
echo "=================================================================="
echo " INSTRUCCIONES MANUALES:"
echo "1. Abre UNITY HUB."
echo "2. Crea un NUEVO PROYECTO."
echo "3. Selecciona la plantilla: '3D (URP)' (Universal Render Pipeline)."
echo "4. Nombralo: '01_Unity_Simulador'"
echo "5. Ubicación: Selecciona la carpeta raíz de este repositorio:"
echo "   $(pwd)"
echo "6. Dale a 'Create Project'."
echo "=================================================================="
echo ""
read -p "Presiona [ENTER] cuando Unity haya terminado de crear los archivos..."

if [ -f ".gitignore_temp" ]; then
    echo "Restaurando .gitignore personalizado..."
    mv .gitignore_temp 01_Unity_Simulador/.gitignore
    echo "¡Hecho! .gitignore restaurado."
else
    echo "No se encontró el backup del .gitignore. Verifica manualmente."
fi
