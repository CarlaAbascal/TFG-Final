# Streaming de vídeo



## 1. Presentació



Aquest projecte forma part del Treball de Fi de Grau i implementa un demostrador de streaming de vídeo en temps real.



L’aplicació està desenvolupada en C# amb Windows Forms i utilitza WebView2 per visualitzar el vídeo rebut mitjançant WebRTC. La transmissió de vídeo es gestiona amb scripts auxiliars en Python.



## 2. Instal·lació



Per executar aquest projecte cal tenir instal·lat:



1\. **Visual Studio 2019 o Visual Studio 2022**

&#x20;  Durant la instal·lació cal seleccionar la càrrega de treball:



```text
Desenvolupament d’escriptori amb .NET
```



2\. **.NET Framework 4.7.2 Developer Pack**

&#x20;  El projecte està desenvolupat amb .NET Framework 4.7.2.



3\. **Python 3**

&#x20;  Necessari per executar el servidor WebRTC i el publisher de vídeo.



4\. **Llibreries de Python**

&#x20;  Des de la carpeta `feature-webrtc`, instal·lar les dependències principals:



```bash
pip install aiohttp aiortc opencv-python av
```



5\. **Microsoft Edge WebView2 Runtime**

&#x20;  Necessari perquè l’aplicació C# pugui mostrar el vídeo dins del formulari.



6\. **Càmera o webcam funcional**

&#x20;  El publisher de vídeo utilitza la càmera de l’ordinador.



7\. **Llibreria `csDronLink.dll`**

&#x20;  Aquesta DLL ha d’estar situada a la carpeta principal del demostrador:



```text
TFG-Final/Demostradors/StreamingVideo/csDronLink.dll
```



## 3. Execució



Per executar el projecte cal seguir aquests passos:



1\. Descarregar o clonar el repositori principal `TFG-Final`.



```bash
git clone https://github.com/CarlaAbascal/TFG-Final.git
```



2\. Accedir a la carpeta del demostrador:



```text
TFG-Final/Demostradors/StreamingVideo/
```



3\. Obrir una terminal dins de la carpeta:



```text
feature-webrtc/
```



4\. Executar el servidor WebRTC:



```bash
python server.py
```



5\. Obrir una segona terminal dins de la mateixa carpeta i executar el publisher de vídeo:



```bash
python script_publisher.py
```



6\. Obrir amb Visual Studio el fitxer:



```text
WindowsFormsApp1/WindowsFormsApp1.sln
```



7\. Compilar i executar l’aplicació amb Visual Studio.



8\. Dins de l’aplicació, prémer el botó corresponent per iniciar el streaming. El vídeo es mostrarà dins del formulari mitjançant WebView2.



## 4. Possibles problemes



Si el vídeo no es mostra, comprovar que:



\* El servidor `server.py` està en execució.

\* El publisher `script_publisher.py` està en execució.

\* El port `8080` no està ocupat per un altre procés.

\* La càmera està connectada i no està sent utilitzada per una altra aplicació.

\* Les dependències de Python estan instal·lades.

\* WebView2 Runtime està instal·lat.

\* El fitxer `csDronLink.dll` es troba a la carpeta correcta.



## 5. Notes



Aquest projecte està pensat per executar-se de manera independent dins del repositori `TFG-Final`.



No cal descarregar cap altre repositori extern, ja que tots els fitxers necessaris per executar aquesta part estan inclosos dins de la carpeta `StreamingVideo`.



