# Reconeixement de veu



## 1. Presentació



Aquest projecte forma part del Treball de Fi de Grau i implementa un demostrador de reconeixement de veu per al control d’un dron.



L’aplicació està desenvolupada en C# amb Windows Forms i permet interpretar comandes de veu per executar accions bàsiques com connectar, enlairar, aterrar, avançar una distància determinada o girar un nombre concret de graus.



Les comandes reconegudes estan definides en castellà, per exemple:



```text
conectar
despegar
aterrizar
avanzar 5 metros
girar 90 grados a la derecha
girar 45 grados a la izquierda
```

Aquest demostrador permet validar de manera independent el funcionament del reconeixement de veu abans d’integrar-lo dins de l’aplicació final.

## 2. Instal·lació

Per executar aquest projecte cal tenir instal·lat:

1\. **Visual Studio 2019 o Visual Studio 2022**

Durant la instal·lació cal seleccionar la càrrega de treball:

```text
Desenvolupament d’escriptori amb .NET
```

2\. **.NET Framework 4.7.2 Developer Pack**

El projecte utilitza .NET Framework 4.7.2, per tant cal instal·lar aquest paquet per poder compilar-lo correctament.

3\. **Micròfon funcional**

L’ordinador ha de tenir un micròfon connectat i configurat correctament.

4\. **Reconeixement de veu de Windows**

Cal comprovar que Windows té activat el reconeixement de veu i que el micròfon té permisos d’ús.

5\. **Llibreria `csDronLink.dll`**

Aquesta llibreria ha d’estar dins de la carpeta del projecte:

```text
TFG-Final/Demostradors/ReconeixementVeu/csDronLink.dll
```
## 3. Ubicació dels fitxers principals

Els fitxers principals del demostrador han d’estar situats a les carpetes següents.

La llibreria del dron ha d’estar a:
```
TFG-Final/Demostradors/ReconeixementVeu/csDronLink.dll
```

El fitxer de solució de Visual Studio ha d’estar a:
```
TFG-Final/Demostradors/ReconeixementVeu/WindowsFormsApp1/WindowsFormsApp1.sln
```

Els fitxers principals del projecte C# han d’estar dins de:
```
TFG-Final/Demostradors/ReconeixementVeu/WindowsFormsApp1/WindowsFormsApp1/
```

Els fitxers principals de l’aplicació són:
```text
TFG-Final/Demostradors/ReconeixementVeu/WindowsFormsApp1/WindowsFormsApp1/Form1.cs
TFG-Final/Demostradors/ReconeixementVeu/WindowsFormsApp1/WindowsFormsApp1/conversacion.cs
TFG-Final/Demostradors/ReconeixementVeu/WindowsFormsApp1/WindowsFormsApp1/Program.cs
```


## 4. Execució

Per executar el projecte:

### 1. Descarregar o clonar el repositori principal `TFG-Final`.

```bash
git clone https://github.com/CarlaAbascal/TFG-Final.git
```

### 2. Entrar a la carpeta del projecte de veu:

```text
TFG-Final/Demostradors/ReconeixementVeu/
```

### 3. Obrir amb Visual Studio el fitxer de solució:

```text
WindowsFormsApp1/WindowsFormsApp1.sln
```
### 4. Comprovar el micròfon

Abans d’executar l’aplicació, comprovar que el micròfon està connectat i funciona correctament.

### 5. Compilar la solució:

```text
Compilar > Compilar solució
```

### 6. Executar l’aplicació 
Des de Visual Studio amb el botó **Iniciar** o amb la tecla **F5**.

### 7. Utilitzar les comandes de veu

Un cop l’aplicació està en execució, es poden utilitzar comandes de veu en castellà per controlar el dron.

Algunes comandes possibles són:

```text
conectar
despegar
aterrizar
avanzar cinco metros
girar noventa grados a la derecha
girar cuarenta y cinco grados a la izquierda
```

Si una comanda necessita un valor concret, com una distància o uns graus, l’aplicació pot demanar informació addicional.


## 5. Possibles problemes

Si l’aplicació no compila

Comprovar que:

* El projecte s’ha obert des del fitxer `WindowsFormsApp1.sln`.
* Està instal·lat el `.NET Framework 4.7.2 Developer Pack`.
* Els paquets NuGet s’han restaurat correctament.
* El fitxer `csDronLink.dll` es troba a la carpeta principal del demostrador.
* Visual Studio no mostra errors de referències.


Si no es reconeix la veu

Comprovar que:

* El micròfon està connectat.
* El micròfon funciona correctament.
* Windows té permisos per utilitzar el micròfon.
* El reconeixement de veu de Windows està disponible.
* L’idioma configurat és compatible amb les comandes definides.
* No hi ha massa soroll ambiental.


Si les comandes no s’interpreten correctament

Comprovar que:

* Les comandes s’estan pronunciant de manera clara.
* Les comandes utilitzades coincideixen amb les definides al projecte.
* Els valors de distància o graus són vàlids.
* El micròfon no està massa lluny de l’usuari.

  
## 5. Notes

Aquest projecte està pensat per executar-se de manera independent dins del repositori `TFG-Final`.

No cal descarregar cap altre repositori extern, ja que tots els fitxers necessaris per executar aquesta part estan inclosos dins de la carpeta `ReconeixementVeu`.
