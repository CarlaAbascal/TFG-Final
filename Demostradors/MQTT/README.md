# MQTT

## 1. Presentació

Aquest projecte forma part del Treball de Fi de Grau i documenta l’ús de MQTT dins del sistema de control i supervisió del dron.

MQTT s’utilitza com a protocol de comunicació lleuger entre alguns scripts auxiliars desenvolupats en Python i les aplicacions desenvolupades en C# amb Windows Forms.

En aquest projecte, MQTT s’utilitza principalment per enviar informació generada pels mòduls de visió per computador cap a l’aplicació C#. Per exemple, els scripts de reconeixement de gestos i detecció d’objectes poden publicar missatges MQTT amb el gest reconegut o l’objecte detectat, i l’aplicació C# pot rebre aquests missatges per mostrar-los a la interfície o executar una acció associada.

## 2. Instal·lació

Per utilitzar MQTT en aquest projecte cal tenir instal·lat:

### 2.1 Broker MQTT

Cal tenir un broker MQTT actiu en local. Es pot utilitzar **Mosquitto**.

El broker ha d’escoltar a l’adreça local:

```text
127.0.0.1:1883
```

### 2.2 Client MQTT per a Python

Els scripts de Python utilitzen la llibreria `paho-mqtt`.

Es pot instal·lar amb:

```bash
pip install paho-mqtt
```

Si el projecte utilitza un fitxer de dependències, aquesta llibreria pot estar inclosa dins de:

```text
requirements_mp.txt
requirements_gestos.txt
```

### 2.3 Llibreria MQTT per a C#

Els projectes desenvolupats en C# utilitzen paquets NuGet per gestionar les dependències.

La llibreria principal utilitzada per MQTT és:

```text
MQTTnet
```

En obrir la solució amb Visual Studio, normalment els paquets NuGet es restauren automàticament.

Si no es restauren automàticament, cal fer clic dret sobre la solució dins del **Solution Explorer** i seleccionar:

```text
Restore NuGet Packages
```

## 3. Ubicació dels fitxers principals

Aquest apartat no conté una aplicació independent, sinó la documentació de la comunicació MQTT utilitzada pels diferents demostradors.

Els scripts que publiquen missatges MQTT es troben principalment als projectes de reconeixement de gestos i detecció d’objectes:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/detectar_mano_mp.py
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/detectarObjetos.py
```

L’aplicació C# que rep els missatges MQTT es troba dins del projecte corresponent:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/
```

En l’aplicació final, la comunicació MQTT també s’utilitza dins de:

```text
TFG-Final/AplicacioFinal/WindowsFormsApp1/WindowsFormsApp1/
```

## 4. Execució

Per utilitzar MQTT en els demostradors cal seguir aquests passos:

### 4.1 Iniciar el broker MQTT

Abans d’executar l’aplicació, cal tenir el broker MQTT actiu.

Si s’utilitza Mosquitto, es pot comprovar que el servei està funcionant des de Windows.

També es pot iniciar manualment des d’una terminal, segons la instal·lació realitzada.

### 4.2 Comprovar el port MQTT

El port utilitzat és:

```text
1883
```

Per comprovar si el port està ocupat o si hi ha un procés escoltant en aquest port, es pot executar:

```bash
netstat -ano | findstr :1883
```

### 4.3 Executar el demostrador corresponent

Un cop el broker MQTT està actiu, es pot executar el demostrador que utilitza MQTT, com ara:

```text
ReconeixementGestosObjectes
AplicacioFinal
```

En aquests projectes, l’aplicació C# es connecta al broker MQTT i els scripts de Python publiquen els resultats detectats.

### 4.4 Publicació i recepció de missatges

Els scripts de Python publiquen missatges MQTT amb la informació detectada.

L’aplicació C# es subscriu als temes corresponents i rep aquests missatges per actualitzar la interfície o executar accions relacionades amb el control del dron.

## 5. Possibles problemes

### 5.1 L’aplicació no connecta amb MQTT

Comprovar que:

* El broker MQTT està en execució.
* El port `1883` està lliure.
* L’adreça configurada és `127.0.0.1`.
* No hi ha cap tallafoc bloquejant la connexió local.
* El paquet NuGet `MQTTnet` està instal·lat correctament.

### 5.2 Els scripts de Python no publiquen missatges

Comprovar que:

* La llibreria `paho-mqtt` està instal·lada.
* El broker MQTT està actiu.
* Els scripts de Python s’estan executant correctament.
* La càmera o el sistema de detecció funciona correctament.
* No apareixen errors al terminal o al registre de l’aplicació.

### 5.3 El broker MQTT no s’inicia

Comprovar que:

* Mosquitto està instal·lat correctament.
* El port `1883` no està ocupat per un altre procés.
* El servei de Mosquitto està actiu.
* La configuració del broker permet connexions locals.

### 5.4 No es reben missatges a l’aplicació C#

Comprovar que:

* L’aplicació C# està subscrita al tema correcte.
* Els scripts de Python publiquen al mateix tema.
* El broker MQTT està funcionant.
* No hi ha errors de connexió al registre de l’aplicació.

## 6. Notes

Aquest apartat serveix com a documentació del funcionament de MQTT dins del projecte.

MQTT no s’executa com un demostrador independent, sinó que dona suport a altres funcionalitats, especialment al reconeixement de gestos i a la detecció d’objectes.

El broker MQTT s’ha d’iniciar abans d’executar els projectes que depenen d’aquesta comunicació.

No cal pujar cap entorn virtual ni fitxers generats automàticament al repositori.
