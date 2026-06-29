# MQTT

## 1. Presentació

Aquest apartat forma part del Treball de Fi de Grau i documenta l’ús de MQTT dins del projecte.

MQTT no correspon a un demostrador executable de manera independent, sinó que s’utilitza com a mecanisme de comunicació entre alguns scripts auxiliars desenvolupats en Python i les aplicacions desenvolupades en C# amb Windows Forms.

En aquest projecte, MQTT s’utilitza principalment dins del demostrador de **reconeixement de gestos i detecció d’objectes**. Els scripts de Python publiquen missatges amb el gest reconegut o l’objecte detectat, i l’aplicació C# rep aquests missatges per mostrar la informació a la interfície o executar una acció associada al dron.

MQTT també s’utilitza dins de l’**aplicació final**, on les funcionalitats de gestos i objectes estan integrades en una única interfície.

## 2. Instal·lació

Per utilitzar MQTT en aquest projecte cal tenir instal·lat un broker MQTT en local. Es pot utilitzar **Mosquitto**.

El broker ha d’estar actiu a l’adreça:

```text
127.0.0.1
```

I al port:

```text
1883
```

Els scripts de Python utilitzen la llibreria:

```text
paho-mqtt
```

Aquesta dependència s’instal·la a través dels fitxers de dependències dels projectes que utilitzen MQTT, com ara:

```text
requirements_mp.txt
requirements_gestos.txt
```

En els projectes desenvolupats en C#, la comunicació MQTT es gestiona mitjançant la llibreria:

```text
MQTTnet
```

## 3. Ubicació dels fitxers principals

Aquest apartat no conté una aplicació independent. Els fitxers que utilitzen MQTT es troben dins dels projectes que fan servir aquesta comunicació.

En el demostrador de reconeixement de gestos i detecció d’objectes, els scripts de Python que publiquen missatges MQTT es troben a:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/detectar_mano_mp.py
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/detectarObjetos.py
```

L’aplicació C# que rep els missatges MQTT es troba dins de:

```text
TFG-Final/Demostradors/ReconeixementGestosObjectes/WindowsFormsApp1/WindowsFormsApp1/
```

En l’aplicació final, la comunicació MQTT també s’utilitza dins de:

```text
TFG-Final/AplicacioFinal/WindowsFormsApp1/WindowsFormsApp1/
```

## 4. Execució

MQTT no s’executa com un demostrador independent. Per utilitzar-lo, cal executar un dels projectes que depenen d’aquesta comunicació, com ara:

```text
Demostradors/ReconeixementGestosObjectes
AplicacioFinal
```

Abans d’executar aquests projectes, cal tenir el broker MQTT actiu.

Si s’utilitza Mosquitto, es pot comprovar si el port 1883 està actiu amb la comanda:

```bash
netstat -ano | findstr :1883
```

Un cop el broker MQTT està funcionant, es pot executar el demostrador corresponent. Els scripts de Python publicaran els missatges MQTT i l’aplicació C# els rebrà automàticament.

## 5. Possibles problemes

Si l’aplicació no rep missatges MQTT, comprovar que:

* El broker MQTT està en execució.
* El port `1883` està lliure.
* L’adreça configurada és `127.0.0.1`.
* No hi ha cap tallafoc bloquejant la connexió local.
* Els scripts de Python s’estan executant correctament.
* La llibreria `paho-mqtt` està instal·lada.
* L’aplicació C# està subscrita al tema MQTT correcte.
* Els scripts de Python publiquen al mateix tema MQTT.

Si el broker MQTT no s’inicia, comprovar que:

* Mosquitto està instal·lat correctament.
* El servei de Mosquitto està actiu.
* El port `1883` no està ocupat per un altre procés.
* La configuració del broker permet connexions locals.

## 6. Notes

Aquest apartat serveix com a documentació del funcionament de MQTT dins del projecte.

MQTT no té un repositori ni una aplicació pròpia, ja que s’utilitza com a part interna d’altres projectes.

La comunicació MQTT s’ha utilitzat principalment per connectar els scripts de reconeixement de gestos i detecció d’objectes amb l’aplicació desenvolupada en C#.

No cal executar cap projecte des d’aquesta carpeta. Per provar MQTT, cal executar el demostrador de reconeixement de gestos i detecció d’objectes o l’aplicació final.
