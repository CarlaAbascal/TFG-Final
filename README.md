# TFG-Final

## 1. Presentació

Aquest repositori conté el conjunt de projectes desenvolupats per al Treball de Fi de Grau.

El projecte està organitzat en diferents carpetes, separant els demostradors individuals de l’aplicació final. Cada demostrador implementa una funcionalitat concreta relacionada amb el control i la supervisió d’un dron, com ara la comunicació MQTT, el streaming de vídeo, el reconeixement de veu, el reconeixement de gestos i la detecció d’objectes.

L’aplicació final integra les funcionalitats principals en una única interfície desenvolupada en C# amb Windows Forms.

## 2. Estructura del repositori

El repositori està organitzat de la manera següent:

```text
TFG-Final/
├── AplicacioFinal/
├── Demostradors/
│   ├── MQTT/
│   ├── ReconeixementGestosObjectes/
│   ├── ReconeixementVeu/
│   └── StreamingVideo/
└── README.md
```

Cada carpeta inclou el seu propi `README.md`, on s’expliquen els requisits, la instal·lació, la ubicació dels fitxers principals, l’execució i els possibles problemes de cada projecte.

## 3. Descripció de les carpetes

### AplicacioFinal

Conté l’aplicació final del projecte.

Aquesta aplicació integra les funcionalitats principals desenvolupades durant el Treball de Fi de Grau: control del dron, telemetria, streaming de vídeo, reconeixement de gestos, detecció d’objectes, reconeixement de veu, captura d’imatges i gravació de vídeo.

### Demostradors/MQTT

Conté la documentació relacionada amb l’ús de MQTT dins del projecte.

MQTT s’utilitza com a mecanisme de comunicació entre alguns scripts auxiliars en Python i les aplicacions desenvolupades en C#.

### Demostradors/ReconeixementGestosObjectes

Conté el demostrador de reconeixement de gestos i detecció d’objectes.

El reconeixement de gestos es realitza amb MediaPipe i la detecció d’objectes amb YOLO. Els resultats es comuniquen a l’aplicació mitjançant MQTT.

### Demostradors/ReconeixementVeu

Conté el demostrador de reconeixement de veu.

Aquest demostrador permet interpretar comandes vocals per executar accions bàsiques sobre el dron, com connectar, enlairar, aterrar, avançar o girar.

### Demostradors/StreamingVideo

Conté el demostrador de streaming de vídeo en temps real.

L’aplicació està desenvolupada en C# amb Windows Forms i utilitza WebView2 per visualitzar el vídeo rebut mitjançant WebRTC.

## 4. Execució

Per executar qualsevol part del projecte, primer cal clonar el repositori:

```bash
git clone https://github.com/CarlaAbascal/TFG-Final.git
```

Després, cal entrar a la carpeta corresponent i seguir les instruccions del seu `README.md`.

## 5. Notes

Aquest repositori està pensat per agrupar tots els demostradors i l’aplicació final del Treball de Fi de Grau.

No cal descarregar cap altre repositori extern, ja que cada carpeta conté els fitxers necessaris per executar la funcionalitat corresponent.
