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



## 2. Instal·lació



Per executar aquest projecte cal tenir instal·lat:



1\. **Visual Studio 2019 o Visual Studio 2022**

&#x20;  Durant la instal·lació cal seleccionar la càrrega de treball:



```text

Desenvolupament d’escriptori amb .NET

```



2\. **.NET Framework 4.7.2 Developer Pack**

&#x20;  El projecte utilitza .NET Framework 4.7.2, per tant cal instal·lar aquest paquet per poder compilar-lo correctament.



3\. **Micròfon funcional**

&#x20;  L’ordinador ha de tenir un micròfon connectat i configurat correctament.



4\. **Reconeixement de veu de Windows**

&#x20;  Cal comprovar que Windows té activat el reconeixement de veu i que el micròfon té permisos d’ús.



5\. **Llibreria `csDronLink.dll`**

&#x20;  Aquesta llibreria ha d’estar dins de la carpeta del projecte:



```text
TFG-Reconeixement-Veu/csDronLink.dll
```



## 3. Execució



Per executar el projecte:



1\. Descarregar o clonar el repositori principal `TFG-Final`.



```bash
git clone https://github.com/CarlaAbascal/TFG-Final.git
```



2\. Entrar a la carpeta del projecte de veu:



```text
TFG-Final/Demostradors/ReconeixementVeu/
```



3\. Obrir amb Visual Studio el fitxer de solució:



```text
WindowsFormsApp1/WindowsFormsApp1.sln
```



4\. Compilar la solució:



```text
Compilar > Compilar solució
```



5\. Executar l’aplicació des de Visual Studio amb el botó **Iniciar** o amb la tecla **F5**.



## 4. Possibles problemes



Si Visual Studio mostra un error relacionat amb `.NET Framework 4.7.2`, cal instal·lar el Developer Pack corresponent.



Si apareix un error relacionat amb `csDronLink.dll`, cal comprovar que el fitxer es troba a la carpeta indicada i que no s’ha mogut.



Si l’aplicació no reconeix la veu, cal comprovar que el micròfon funciona correctament i que Windows té permisos per utilitzar-lo.



## 5. Notes



Aquest projecte està pensat per executar-se de manera independent dins del repositori `TFG-Final`.



No cal descarregar cap altre repositori extern, ja que tots els fitxers necessaris per executar aquesta part estan inclosos dins de la carpeta `ReconeixementVeu`.



