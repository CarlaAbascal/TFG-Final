# Aplicació final

## 1. Presentació

Aquest projecte forma part del Treball de Fi de Grau i implementa l’aplicació final del sistema de control del dron.

L’aplicació està desenvolupada en **C# amb Windows Forms** i integra en una única interfície les diferents funcionalitats desenvolupades durant el projecte: control manual del dron, connexió amb la llibreria `csDronLink.dll`, visualització de vídeo en temps real, reconeixement de gestos, detecció d’objectes, reconeixement de veu, telemetria, captura d’imatges i gravació de vídeo.

El vídeo es mostra dins del formulari mitjançant **WebView2**, mentre que les funcionalitats de visió per computador i streaming es gestionen amb scripts auxiliars en **Python**. La comunicació entre els scripts i l’aplicació es realitza mitjançant **MQTT**.

## 2. Instal·lació

Per executar aquest projecte cal tenir instal·lat:

### 1. Visual Studio 2019 o Visual Studio 2022

Durant la instal·lació cal seleccionar la càrrega de treball:

```text
Desenvolupament d’escriptori amb .NET
```

### 2. .NET Framework 4.7.2 Developer Pack

El projecte està desenvolupat amb **.NET Framework 4.7.2**.

### 3. Python 3.10

Es recomana utilitzar **Python 3.10**, especialment per al reconeixement de gestos amb MediaPipe.

Per comprovar la versió instal·lada:

```bash
py -3.10 --version
```

### 2.4 Microsoft Edge WebView2 Runtime

Necessari perquè l’aplicació C# pugui mostrar el vídeo dins del formulari.

### 2.5 Broker MQTT

L’aplicació utilitza MQTT per rebre la informació dels gestos i dels objectes detectats.

Cal tenir un broker MQTT actiu en local, per exemple **Mosquitto**, escoltant al port:

```text
127.0.0.1:1883
```

### 2.6 Càmera o webcam funcional

La càmera s’utilitza per capturar el vídeo, detectar gestos i detectar objectes.

### 2.7 Micròfon funcional

Necessari per utilitzar el reconeixement de veu.

### 2.8 Llibreria csDronLink.dll

La llibreria `csDronLink.dll` ha d’estar situada a la carpeta principal de l’aplicació final:

```text
TFG-Final/AplicacioFinal/csDronLink.dll
```

### 2.9 Paquets NuGet

En obrir la solució amb Visual Studio, cal restaurar els paquets NuGet del projecte.

Les dependències principals del projecte C# són:

```text
Microsoft.Web.WebView2
MQTTnet
OpenCvSharp
```

Normalment Visual Studio restaura aquests paquets automàticament en obrir la solució.

## 3. Preparació dels entorns virtuals

Aquest projecte utilitza dos entorns virtuals de Python:

```text
mp_env          → servidor WebRTC, publisher de vídeo i detecció d’objectes
gestos_env310  → reconeixement de gestos amb MediaPipe
```

Els dos entorns s’han de crear dins de la carpeta principal de l’aplicació final:

```text
TFG-Final/AplicacioFinal/
```

El projecte inclou dos fitxers de dependències:

```text
requirements_mp.txt
requirements_gestos.txt
```

### 3.1 Crear l’entorn virtual mp_env

Aquest entorn s’utilitza per executar el servidor WebRTC, el publisher de vídeo i la detecció d’objectes.

Obrir una terminal dins de la carpeta principal de l’aplicació final:

```bash
cd TFG-Final\AplicacioFinal
```

Crear l’entorn virtual:

```bash
py -3.10 -m venv mp_env
```

Activar l’entorn virtual:

```bash
mp_env\Scripts\activate
```

Actualitzar `pip`:

```bash
python -m pip install --upgrade pip
```

Instal·lar les dependències:

```bash
pip install -r requirements_mp.txt
```

### 3.2 Crear l’entorn virtual gestos_env310

Aquest entorn s’utilitza per executar el reconeixement de gestos amb MediaPipe.

Des de la carpeta principal de l’aplicació final:

```bash
cd TFG-Final\AplicacioFinal
```

Crear l’entorn virtual:

```bash
py -3.10 -m venv gestos_env310
```

Activar l’entorn virtual:

```bash
gestos_env310\Scripts\activate
```

Actualitzar `pip`:

```bash
python -m pip install --upgrade pip
```

Instal·lar les dependències:

```bash
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

```bash
python -c "import mediapipe as mp; print(mp.__version__); print(hasattr(mp,'solutions'))"
```

El resultat correcte hauria de ser semblant a:

```text
0.10.14
True
```

## 4. Execució

Per executar el projecte cal seguir aquests passos:

### 4.1 Clonar el repositori

Descarregar o clonar el repositori principal `TFG-Final`:

```bash
git clone https://github.com/CarlaAbascal/TFG-Final.git
```

### 4.2 Entrar a la carpeta de l’aplicació final

```bash
cd TFG-Final\AplicacioFinal
```

### 4.3 Preparar els entorns virtuals

Crear els entorns virtuals `mp_env` i `gestos_env310` seguint els passos de l’apartat 3.

### 4.4 Obrir la solució amb Visual Studio

Obrir amb Visual Studio el fitxer de solució:

```text
WindowsFormsApp1/WindowsFormsApp1.sln
```

### 4.5 Restaurar els paquets NuGet

Si Visual Studio no els restaura automàticament, restaurar els paquets NuGet del projecte.

### 4.6 Comprovar el broker MQTT

Abans d’executar l’aplicació, comprovar que el broker MQTT està actiu en local:

```text
127.0.0.1:1883
```

### 4.7 Compilar i executar l’aplicació

Compilar i executar el projecte des de Visual Studio.

### 4.8 Connectar el sistema

Dins de l’aplicació, prémer el botó **Conectar**.

Aquest botó connecta el dron en mode simulació, inicia la telemetria i arrenca automàticament els scripts necessaris per al vídeo en temps real:

```text
server.py
script_publisher.py
```

Quan la connexió s’ha iniciat correctament, el vídeo capturat per la càmera es mostra dins del formulari mitjançant WebView2.

### 4.9 Activar el reconeixement de gestos

Per activar el reconeixement de gestos, prémer el botó **Gestos**.

Els gestos reconeguts s’envien a l’aplicació mitjançant MQTT i s’associen a les accions següents:

```text
Palm  → Despegar
Puño  → Aterrizar
Uno   → Avanzar
Dos   → Girar a la derecha
Tres  → Girar a la izquierda
```

### 4.10 Activar la detecció d’objectes

Per activar la detecció d’objectes, prémer el botó **Objetos**.

El sistema executa el script de detecció d’objectes i envia a l’aplicació la informació dels objectes detectats mitjançant MQTT.

### 4.11 Activar el reconeixement de veu

Per activar el reconeixement de veu, prémer el botó **Activar voz**.

L’aplicació permet interpretar comandes vocals com:

```text
conéctate
despega
aterriza
avanza
gira a la derecha
gira a la izquierda
sube
baja
activa gestos
activa objetos
detener
```

En alguns casos, si falta algun paràmetre, l’aplicació pot demanar informació addicional. Per exemple, si l’usuari diu “avanza”, el sistema pot demanar quants metres ha d’avançar.

### 4.12 Capturar imatges i gravar vídeo

L’aplicació també permet:

```text
Capturar imatges en format JPG
Gravar vídeo en format WEBM
```

Els fitxers generats es desen a la carpeta de descàrregues de l’usuari.

### 4.13 Aturar els processos auxiliars

Per aturar els processos auxiliars, prémer el botó **Detener**.

Aquest botó atura els scripts de gestos, objectes, servidor WebRTC i publisher de vídeo.

## 5. Possibles problemes

### 5.1 L’aplicació no compila

Comprovar que:

* El projecte s’ha obert des del fitxer `WindowsFormsApp1.sln`.
* Està instal·lat el `.NET Framework 4.7.2 Developer Pack`.
* Els paquets NuGet s’han restaurat correctament.
* El fitxer `csDronLink.dll` es troba a `TFG-Final/AplicacioFinal/csDronLink.dll`.
* Visual Studio no mostra errors de referències.
* No falten fitxers del projecte C#.

### 5.2 El vídeo no es mostra

Comprovar que:

* S’ha premut primer el botó **Conectar**.
* La càmera està connectada.
* La càmera no està sent utilitzada per una altra aplicació.
* El port `8080` no està ocupat per un altre procés.
* L’entorn virtual `mp_env` existeix dins de `AplicacioFinal/`.
* Les dependències de `requirements_mp.txt` estan instal·lades.
* WebView2 Runtime està instal·lat.
* Els scripts `server.py` i `script_publisher.py` s’han iniciat correctament.

Per comprovar si el port 8080 està ocupat:

```bash
netstat -ano | findstr :8080
```

Si apareix un procés utilitzant aquest port, es pot finalitzar amb:

```bash
taskkill /PID <PID> /F
```

### 5.3 El reconeixement de gestos no funciona

Comprovar que:

* S’ha premut primer el botó **Conectar**.
* S’ha premut el botó **Gestos**.
* L’entorn virtual `gestos_env310` existeix dins de `AplicacioFinal/`.
* Les dependències de `requirements_gestos.txt` estan instal·lades.
* MediaPipe està instal·lat amb la versió correcta.
* El fitxer `detectar_mano_mp.py` està inclòs al projecte.
* El fitxer `hand_landmarker.task` està inclòs al projecte.
* El broker MQTT està actiu.
* La càmera funciona correctament.

### 5.4 La detecció d’objectes no funciona

Comprovar que:

* S’ha premut primer el botó **Conectar**.
* S’ha premut el botó **Objetos**.
* L’entorn virtual `mp_env` existeix dins de `AplicacioFinal/`.
* Les dependències de `requirements_mp.txt` estan instal·lades.
* El fitxer `detectarObjetos.py` està inclòs al projecte.
* El fitxer `yolov8n.pt` està inclòs al projecte.
* El broker MQTT està actiu.
* La càmera funciona correctament.

### 5.5 El reconeixement de veu no funciona

Comprovar que:

* El micròfon està connectat.
* El micròfon funciona correctament.
* El reconeixement de veu de Windows està disponible.
* L’idioma de reconeixement és compatible amb les comandes definides.
* No hi ha massa soroll ambiental.

### 5.6 MQTT no connecta

Comprovar que:

* El broker MQTT està en execució.
* El port `1883` està lliure.
* L’adreça configurada és `127.0.0.1`.
* No hi ha cap tallafoc bloquejant la connexió local.

### 5.7 Els scripts de Python no s’executen

Comprovar que:

* Els entorns virtuals estan creats dins de `AplicacioFinal/`.
* Els noms dels entorns són exactament:

```text
mp_env
gestos_env310
```

* Python 3.10 està instal·lat.
* Les dependències s’han instal·lat correctament.
* Els fitxers `.py` estan ubicats a la carpeta correcta.
* Els scripts tenen permisos d’execució.

## 6. Notes

Aquest projecte està pensat per executar-se de manera independent dins del repositori `TFG-Final`.

No cal descarregar cap altre repositori extern, ja que tots els fitxers necessaris per executar l’aplicació final han d’estar inclosos dins de la carpeta `AplicacioFinal`.

Els entorns virtuals de Python no s’han de pujar al repositori. Cada usuari els ha de crear localment seguint els passos d’instal·lació.

Tampoc s’han de pujar al repositori les carpetes generades automàticament per Visual Studio o Python, com ara:

```text
.vs/
bin/
obj/
packages/
mp_env/
gestos_env310/
__pycache__/
```

Les captures d’imatge i les gravacions de vídeo generades per l’aplicació es desen a la carpeta de descàrregues de l’usuari.
