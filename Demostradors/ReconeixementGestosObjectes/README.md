# Reconeixement de gestos i objectes



## 1. Presentació



Aquest projecte forma part del Treball de Fi de Grau i implementa un demostrador de reconeixement de gestos i detecció d’objectes per al control i supervisió d’un dron.



L’aplicació està desenvolupada en C# amb Windows Forms i utilitza WebView2 per visualitzar el vídeo rebut mitjançant WebRTC. La captura i el processament del vídeo es gestionen amb scripts auxiliars en Python.



El demostrador integra dues funcionalitats principals:



```text
Reconeixement de gestos  → detecció de gestos de la mà amb MediaPipe
Reconeixement d’objectes → detecció d’objectes amb YOLO
```



Els gestos principals utilitzats són:



```text
Palma   → Despegar
Puño    → Aterrizar
Uno     → Avanzar
Dos     → Girar a la derecha
Tres    → Girar a la izquierda
```



El projecte també utilitza MQTT per enviar els gestos i objectes detectats cap a l’aplicació C#.



## 2. Instal·lació



Per executar aquest projecte cal tenir instal·lat:



1\. **Visual Studio 2019 o Visual Studio 2022**



Durant la instal·lació cal seleccionar la càrrega de treball:



```text
Desenvolupament d’escriptori amb .NET
```



2\. **.NET Framework 4.7.2 Developer Pack**



El projecte està desenvolupat amb .NET Framework 4.7.2.



3\. **Python 3.10**



És necessari per executar els scripts de reconeixement de gestos, detecció d’objectes, servidor WebRTC i publisher de vídeo.



Es recomana utilitzar Python 3.10 perquè és compatible amb la versió de MediaPipe utilitzada en aquest projecte.



4\. **Microsoft Edge WebView2 Runtime**



Necessari perquè l’aplicació C# pugui mostrar el vídeo dins del formulari.



5\. **Càmera o webcam funcional**



La càmera de l’ordinador s’utilitza per capturar el vídeo que després es processa amb MediaPipe i YOLO.



6\. **Broker MQTT**



El projecte utilitza MQTT per comunicar els resultats dels scripts de Python amb l’aplicació C#.



Es pot utilitzar Mosquitto com a broker local.



7\. **Llibreria `csDronLink.dll`**



Aquesta DLL ha d’estar situada a la carpeta principal del demostrador:



```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/csDronLink.dll
```



## 3. Preparació dels entorns virtuals


Aquest projecte utilitza dos entorns virtuals de Python:



```text
mp_env        → servidor WebRTC, publisher de vídeo i detecció d’objectes
gestos_env310 → reconeixement de gestos amb MediaPipe
```



Els dos entorns s’han de crear dins de la carpeta principal del demostrador:


```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/
```

El projecte inclou dos fitxers de dependències:

```text
requirements_mp.txt
requirements_gestos.txt
```


### 3.1 Crear l’entorn virtual `mp_env`

Aquest entorn s’utilitza per executar el servidor WebRTC, el publisher de vídeo i el reconeixement d’objectes.


Obrir una terminal dins de la carpeta principal del demostrador:


```bat
cd TFG-Final\Demostradors\ReconeixementGestosObjectes
```

Crear l’entorn virtual:

```bat
py -3.10 -m venv mp_env
```

Activar l’entorn virtual:

```bat
mp_env\Scripts\activate
```

Actualitzar `pip`:

```bat
python -m pip install --upgrade pip
```

Instal·lar les dependències:

```bat
pip install -r requirements_mp.txt
```

### 3.2 Crear l’entorn virtual `gestos_env310`

Aquest entorn s’utilitza per executar el reconeixement de gestos amb MediaPipe.

Des de la carpeta principal del demostrador:

```bat
cd TFG-Final\Demostradors\ReconeixementGestosObjectes
```

Crear l’entorn virtual:

```bat
py -3.10 -m venv gestos_env310
```

Activar l’entorn virtual:

```bat
gestos\_env310\\Scripts\\activate
```

Actualitzar `pip`:

```bat
python -m pip install --upgrade pip
```

Instal·lar les dependències:

```bat
pip install -r requirements_gestos.txt
```

El fitxer `requirements_gestos.txt` fixa la versió de MediaPipe:

```text
mediapipe==0.10.14
```

Aquesta versió és necessària perquè el script de gestos utilitza l’API clàssica de MediaPipe:

```python
mp.solutions.hands
```

Per comprovar que MediaPipe està instal·lat correctament, es pot executar:

```bat
python -c "import mediapipe as mp; print(mp.\_\_version\_\_); print(hasattr(mp,'solutions'))"
```

El resultat correcte hauria de ser semblant a:

```text
0.10.14
True
```

## 4. Ubicació dels fitxers principals


Els fitxers principals del demostrador han d’estar situats a les carpetes següents.

La llibreria del dron ha d’estar a:

```text
TFG-Final/Demostradors/ReconeixementGestos/csDronLink.dll
```

Els scripts del servidor WebRTC han d’estar a:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/feature-webrtc/server.py
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/feature-webrtc/script_publisher.py
```

Els scripts de gestos i objectes han d’estar a:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/detectar_mano_mp.py
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/detectarObjetos.py
```

També han d’estar disponibles els fitxers auxiliars necessaris per als models de detecció, com ara:

```text
hand_landmarker.task
yolov8n.pt
```

Aquests fitxers han d’estar dins de la carpeta del projecte C#:

```text
WindowsFormsApp1/WindowsFormsApp1/
```


## 5. Execució

Per executar el projecte cal seguir aquests passos:

### 1. Descarregar o clonar el repositori principal `TFG-Final`

```bash
git clone https://github.com/CarlaAbascal/TFG-Final.git
```

### 2. Entrar a la carpeta del demostrador

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/
```



### 3. Crear els entorns virtuals

Abans d’executar l’aplicació, cal haver creat els dos entorns virtuals explicats a l’apartat anterior:

```text
mp_env
gestos_env310
```

### 4. Obrir el projecte amb Visual Studio

Obrir amb Visual Studio el fitxer de solució:

```text
WindowsFormsApp1/WindowsFormsApp1.sln
```

### 5. Comprovar la ubicació de `csDronLink.dll`

El fitxer `csDronLink.dll` ha d’estar situat a:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/csDronLink.dll
```

### 6. Compilar i executar l’aplicació

Des de Visual Studio, compilar i executar el projecte.

### 7. Prémer el botó `Conectar`

Dins de l’aplicació, prémer el botó:

```text
Conectar
```

Aquest botó realitza les accions següents:

```text
1\. Connecta el dron en mode simulació.
2\. Sol·licita la telemetria.
3\. Inicia el servidor WebRTC.
4\. Inicia el publisher de vídeo.
5\. Deixa disponible el stream de vídeo.
```

No cal executar manualment `server.py` ni `script_publisher.py`, ja que l’aplicació els inicia automàticament.

Si tot funciona correctament, al registre de l’aplicació apareixeran missatges semblants a aquests:

```text
\[INFO] Iniciando servidor WebRTC y publisher de vídeo...
\[INFO] Iniciando server.py...
\[OK] server.py iniciado en puerto 8080.
\[INFO] Iniciando script_publisher.py...
\[OK] Stream WebRTC/MJPEG disponible.
\[OK] Server y publisher preparados.
```

### 8. Executar el reconeixement de gestos

Per iniciar el reconeixement de gestos, prémer el botó:

```text
Gestos
```

Aquest botó executa el script:

```text
detectar_mano_mp.py
```

amb l’entorn virtual:

```text
gestos_env310
```

Si funciona correctament, al registre apareixeran missatges semblants a:

```text
\[INFO] Cargando reconocimiento de gestos...
\[INFO] Iniciando detectar_mano_mp.py...
\[GESTOS] === SCRIPT GESTOS MEDIAPIPE INICIADO ===
\[GESTOS] \[OK] Conectado al broker MQTT (gestos)
```

El vídeo de gestos queda disponible a:

```text
http://127.0.0.1:8090/
```

### 9. Executar el reconeixement d’objectes

Per iniciar la detecció d’objectes, prémer el botó:

```text
Objetos
```

Aquest botó executa el script:

```text
detectarObjetos.py
```

amb l’entorn virtual:

```text
mp_env
```

El sistema detecta objectes amb YOLO i envia els resultats a l’aplicació mitjançant MQTT.

### 10. Visualització del vídeo

El vídeo principal del servidor WebRTC/MJPEG queda disponible a:

```text
http://127.0.0.1:8080/
```

El vídeo del reconeixement de gestos queda disponible a:

```text
http://127.0.0.1:8090/
```

## 6. Possibles problemes

Si el demostrador no funciona correctament, comprovar els punts següents.

### Error amb `mp_env`

Si apareix un error semblant a:

```text
\[ERROR] No existe python mp_env:
...\\ReconeixementGestosObjectes\\mp_env\\Scripts\\python.exe
```

vol dir que falta crear l’entorn virtual `mp_env`.

Solució:

```bat
cd TFG-Final\Demostradors\ReconeixementGestosObjectes 
py -3.10 -m 
venv mp_env mp_env\Scripts\activate 
python -m pip install --upgrade pip 
pip install -r requirements\_mp.txt
```

### Error amb `gestos_env310`

Si apareix un error indicant que no existeix l’entorn de gestos, cal crear-lo de nou:

```bat
cd TFG-Final\Demostradors\ReconeixementGestosObjectes
py -3.10 -m venv gestos_env310
gestos_env310\Scripts\activate
python -m pip install --upgrade pip
pip install -r requirements_gestos.txt
```

### Error amb `detectar_mano_mp.py` o `detectarObjetos.py`

Si apareix un error indicant que no existeix algun dels scripts:

```text
No existe detectar_mano_mp.py
No existe detectarObjetos.py
```

cal comprovar que es troben dins de la carpeta:

```text
WindowsFormsApp1/WindowsFormsApp1/
```

Concretament:

```text
WindowsFormsApp1/WindowsFormsApp1/detectar_mano_mp.py
WindowsFormsApp1/WindowsFormsApp1/detectarObjetos.py
```

### Error de MediaPipe

Si apareix aquest error:

```text
AttributeError: module 'mediapipe' has no attribute 'solutions'
```

vol dir que s’ha instal·lat una versió massa nova de MediaPipe.

Solució:

```bat
gestos_env310\Scripts\activate
pip uninstall mediapipe -y
pip install mediapipe==0.10.14
```

Després, comprovar la instal·lació:


```bat
python -c "import mediapipe as mp; print(mp.\_\_version\_\_); print(hasattr(mp,'solutions'))"
```

El resultat correcte ha de ser:

```text
0.10.14
True
```

### El servidor de gestos no respon

Si apareix aquest error:

```text
\[ERROR] El servidor de gestos no responde en http://127.0.0.1:8090/
```

normalment vol dir que el script `detectar_mano_mp.py` ha fallat abans d’aixecar el servidor.

Cal revisar els missatges que apareixen al registre amb l’etiqueta:

```text
\[GESTOS]
```

Els motius més habituals són:

```text
MediaPipe mal instal·lat
Càmera ocupada per una altra aplicació
Broker MQTT no iniciat
Falta algun fitxer auxiliar
```

### El port 8080 o 8090 està ocupat

Si algun port està ocupat, es pot comprovar amb:

```bat
netstat -ano | findstr :8080
```

o:

```bat
netstat -ano | findstr :8090
```

Per tancar el procés que està utilitzant el port:

```bat
taskkill /PID NUMERO\_PID /F
```
Substituint `NUMERO\_PID` pel número que apareix al resultat del `netstat`.

### La càmera no funciona

Si la càmera no s’obre, comprovar que:

\* La webcam està connectada.

\* La càmera no està sent utilitzada per una altra aplicació.

\* Python té permisos per accedir a la càmera.

\* No hi ha obertes aplicacions com Teams, Zoom, OBS o el navegador utilitzant la càmera.


### El vídeo no es mostra dins de l’aplicació

Si el vídeo no es mostra dins del formulari, comprovar que:

\* WebView2 Runtime està instal·lat.

\* `server.py` s’ha iniciat correctament.

\* `script_publisher.py` s’ha iniciat correctament.

\* El port 8080 no està ocupat.

\* La càmera està disponible.

\* L’aplicació s’ha connectat correctament abans de prémer `Gestos` o `Objetos`.



## 7. Notes



Aquest projecte està pensat per executar-se de manera independent dins del repositori `TFG-Final`.



No cal descarregar cap altre repositori extern, ja que tots els fitxers necessaris per executar aquesta part estan inclosos dins de la carpeta:



```text
ReconeixementGestosObjectes
```



El flux recomanat d’execució és:



```text
1. Obrir el projecte a Visual Studio.
2. Executar l’aplicació.
3. Prémer Conectar.
4. Prémer Gestos o Objetos segons la funcionalitat que es vulgui provar.
```



Els entorns virtuals han de mantenir exactament aquests noms:



```text
mp_env
gestos_env310
```



Aquests noms són importants perquè l’aplicació C# busca els executables de Python en aquestes rutes:



```text
ReconeixementGestosObjectes/mp_env/Scripts/python.exe
ReconeixementGestosObjectes/gestos_env310/Scripts/python.exe
```



Per evitar errors amb MediaPipe, el fitxer de dependències del reconeixement de gestos ha d’utilitzar la versió:



Aquests fitxers de requeriments permeten reconstruir els entorns virtuals fàcilment quan es descarrega el projecte en un altre ordinador. Tenen la següent estructura:



`requirements_mp.txt`



```text
opencv-python
paho-mqtt
aiohttp
aiortc
av
ultralytics
```



`requirements_gestos.txt`



```text
opencv-python
mediapipe==0.10.14
paho-mqtt
aiohttp
```



