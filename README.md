# Moni - Der schnellere Weg Stunden zu erfassen #

## Die Idee ##

Die Idee bei Moni ist, dass es bei der Stundenerfassung nicht so sehr auf exakte Uhrzeiten ankommt, sondern auf die Stunden Anzahl. Deshalb bietet Moni eine einfache Möglichkeit Stundenaufwände einzugeben und trotzdem Start und Endzeiten aufzuzeichnen, sowie automatische Pauseneinträge zu erzeugen. die Eingabe besteht aus einem bestimmten Format, das unten beschrieben ist. Am Anfang ist es ein bisschen gewöhnungsbedüftig, aber man kann es schnell lernen. 

Außerdem bietet Moni weitere Features, die die Eingabe vereinfachen:

- Wochenweise Ansicht
- Automatische Pauseneintragung (und Deaktivierung für einzelne Tage)
- Abkürzungen für Projekte und Beschreibung
- Abkürzungen für ganze Tage
- Anzeige Stunden pro Projekt/Abkürzung
- Prognose Arbeitsstunden
- Monatsstatistik
- Keyboardnavigation
- Projektnummernsuche
- Positionsnummernsuche
- Hitlisten für meistgenutze Projekt- und Positionsnummern
- Monatsseitenleiste


## Das Interface ##

![Hauptansicht](doc/moni_mainwindow.png)

1. Prognose Arbeitstunden
2. Hervorhebung Feiertage
3. Warnung wenn nicht 8h pro Tag verbraucht
4. Automatische Pauseneintragung
5. Mehrzeilige Einträge
6. Beschreibungen von Abkürzungen erweiteren
7. Beschreibungen von Abkürzungen ersetzen
8. Abkürzungen für ganze Tage
9. Hervorhebung Wochenende
10. Abkürzungen
11. Monatliche Nutzungsstatistik (Tagesgenau und Summe)
12. Hervorhebung von Abkürzungen für ganze Tage
13. Monlist starten und Daten importieren
14. Link zu Github
15. Einstellungen

![Shortcut bearbeiten](doc/moni_shortcut.png)
![Einstellungen bearbeiten](doc/moni_settings.png)
![Projektnummern-Suche](doc/moni_pnsearch.png)

## Installation ##

1. Aktuelle Versionen [hier](https://github.com/dotob/moni/releases) runterladen.
2. Zip entpacken
3. Moni.exe starten
4. Es wird beim Start ein **data** Verzeichnis angelegt.

## Admin ##

Beim ersten starten möchte moni gern loggen, eine Settings-Datei und ein Daten-Verzeichnis anlegen. Dabei wird das Verzeichnis in dem gestartet wurde bevorzugt. Ist dieses nicht schreibbar, dann wird in das ApplicationData-Verzeichnis (ab Win 7 etwa hier: c:\Users\deinusername\AppData\Roaming\) geschrieben. 

Für die Settings-Datei wird eine Standardversion genommen, falls im Startverzeichnis keine **settings.json.skeleton** liegt. Sonst wird diese als vorlage genutzt. 

In allen Pfadangaben kann **#{appdata}** als Platzhalter für das ApplicationData-Verzeichnis (+/moni) genutzt werden.
In allen Pfadangaben kann **#{userhome}** als Platzhalter für das Benutzer-Verzeichnis genutzt werden.


<!--- shorthelp: ab hier wird in moni angezeigt --->
## Keyboad Shortcuts ##

### Navigation ####

- **tab**: In Eingabefeld des folgenden Tages gehen (nur innerhalb einer Woche)
- **shift+tab**: In Eingabefeld des vorigen Tages gehen (nur innerhalb einer Woche)
- **strg+cursor_links**: Vorige Woche anzeigen
- **strg+cursor_rechts**: Nächste Woche anzeigen
- **shift+strg+cursor_links**: Gleichen Tag im vorigen Monat anzeigen
- **shift+strg+cursor_rechts**: Gleichen Tag im nächsten Monat anzeigen
- **escape**: Gehe zu Heute

### Andere Helferlein ###
- **F1**: Diese Hilfe
- **strg+u**: Eingabe des vorigen Tages in aktuellen Tag kopieren
- **strg+n**: Neuen Eintrag mit Endzeit jetzt im aktuellen Tag erstellen
- **strg+f**: Projektnummernsuche öffnen
- **strg++**: Hängt "(+ )", an den aktuellen Eintrag an
- **strg+q**: MONI beenden
- **bild hoch**: Aktuellen Eintrag um 15min erhöhen bzw. erweitern
- **bild runter**: Aktuellen Eintrag um 15min erniedrigen bzw. verkürzen
- **strg+1**: Bereitet die aktuelle Eingabe für einen neuen Eintrag der Länge 1 Stunde vor (hängt also ",1;" an). Geht genauso auch für die anderen Zahlen von 2-9.
- **click mit links auf einen Eintrag unter Textbox**: Selektiert den dafür verantwortlichen Text und kopiert ihn in die Zwischenablage
- **click mit rechts auf einen Eintrag unter Textbox**: Selektiert den dafür verantwortlichen Text (ohne Zeit) und kopiert ihn in die Zwischenablage
- **click auf Eintrag in Hitlisten**: Fügt die Nummer der aktuellen Textbox hinzu

# Dokumentation der Eingabe #

## Uhrzeiten eingeben ##

Korrekte Eingaben (Beispiele):

- 8
- 8:00
- 800
- 8:30
- 830
- 85 => 8:05

## Stunden ##

**Trennzeichen ist hier der Dezimalpunkt!!**

- 1
- 1.5
- 1.75

## Ganz einfach: Ein Eintrag ##

Eingabe: 

**8,8;12345-000**

Ausgabe: 

- 8:00-16:00 12345-000

Erläuterung: 

Erste Zahl vor dem Komma ist die Startzeit, gefolgt von der Anzahl der Stunden und Projekt-Position

## Mehrere Einträge ##

Eingabe: 

**8,4;12345-000,4;54321-000**

Ausgabe: 

- 8:00-12:00 12345-000
- 12:00-16:00 54321-000

Erläuterung: 

Kommasepariert werden die Einträge aneinandergereiht

## Teilstunden ##

Eingabe: 

**8:30,4.25;12345-000,3.75;54321-000**

Ausgabe: 

- 8:30-12:45 12345-000
- 12:45-16:30 54321-000

Erläuterung: 

Uhrzeiten werden im Format **Stunde:Minute** angegeben. Stunden mit **Punkt** getrennt


## Beschreibungen ##

Eingabe: 

**8:30,4.25;12345-000(fehlerbehebung),3.75;54321-000(support)**

Ausgabe: 

- 8:30-12:45 12345-000  fehlerbehebung
- 12:45-16:30 54321-000  support

Erläuterung: 

Beschreibungen können pro Eintrag in **Klammern** angegeben werden. Es können auch Klammern und ,:; in Beschreibungen angegeben werden.

## Abkürzungen ##

Konfiguration:

- **proj1 => 12345-000**
- **proj2 => 54321-000(spezifikation)**

Eingabe: 

**8:30,4.25;proj1,3.75;proj2**

Ausgabe: 

- 8:30-12:45 12345-000
- 12:45-16:30 54321-000  spezifikation

## Abkürzungen, Ersetzung der Positionsnummer##

Konfiguration:

- **proj1 => 12345-000**
- **proj2 => 54321-000(spezifikation)**

Eingabe: 

**8:30,4.25;proj1,3.75;proj2-123**

Ausgabe: 

- 8:30-12:45 12345-000
- 12:45-16:30 54321-123  spezifikation

Erläuterung:

Die in der Abkürzung gespeicherte Positionsnummer kann von Fall zu Fall überschrieben werden. Gezählt wird der Eintrag aber weiterhin zur ursprünglichen Abkürzung.

## Abkürzungen und Beschreibungen ##

Konfiguration:

- **proj1 => 12345-000**
- **proj2 => 54321-000(spezifikation)**

Eingabe: 

**8:30,4.25;proj1(support),2.75;proj2(schnittstelle spezifiziert),1;proj2(+ bahnschnittstelle)**

Ausgabe: 

- 8:30-12:45 12345-000  support
- 12:45-15:30 54321-000  schnittstelle spezifiziert
- 15:30-17:30 54321-000  spezifikation testschnittstelle

Erläuterung: 

Abkürzungen können auch Beschreibungen enthalten. Wird die Abkürzung mit Beschreibung eingegeben wird diese angefügt ("(+ )") oder ersetzt ("( )") die Beschreibung der Abkürzung

## Abkürzungen für einen ganzen Tag ##

Konfiguration:

- **krank => 8,8;12345-000(krank oder doc)** (Beim Anlegen eines Shortcut angeben, dass er einen ganzen Tag ersetzt!)

Eingabe: 

**krank**

Ausgabe: 

- 8:00-16:00 12345-000  krank oder doc

## Manuelle Pause mit Stundenzahl ##

Eingabe: 

**8:00,4;12345-000,1!,4;12345-000**

Ausgabe: 

- 8:00-12:00 12345-000
- 13:00-17:00 12345-000

Erläuterung: 

Endet ein Eintrag mit **!** dann wird die Stundenzahl als Pause eingefügt
Vor dem **!** kann geklammert ein Kommentar stehen: **8:00,4;12345-000,1(werkstatt)!,4;12345-000**

## Manuelle Pause mit Endzeit ##

Eingabe: 

**8:00,4;12345-000,-14!,3;12345-000**

Ausgabe: 

- 8:00-12:00 12345-000
- 14:00-17:00 12345-000

Erläuterung: 

Endet ein Eintrag mit **!** dann wird eine Pause bis zur Endzeit eingefügt
Vor dem **!** kann geklammert ein Kommentar stehen: **8:00,4;12345-000,-14(werkstatt)!,3;12345-000**

## Automatische Pause ##

Konfiguration:

- **Pause einfügen**
- **Pause um 12:00 Uhr**
- **Pausenlänge 30 min**

Eingabe: 

**8:00,8;12345-000**

Ausgabe: 

- 8:00-12:00 12345-000
- 12:30-16:30 12345-000

Erläuterung: 

Es wird automatisch eine Pause um 12:00 eingefügt

## Automatische Pause für einen Tag deaktivieren ##

Konfiguration:

- **Pause einfügen**
- **Pause um 12:00 Uhr**
- **Pausenlänge 30 min**

Eingabe: 

**//8:00,8;12345-000**

Ausgabe: 

- 8:00-16:30 12345-000

Erläuterung:

mit "//" kann das Einfügen einer automatischen Pause für diesen Tag ausgeschaltet werden

## Endzeit statt Stundenanzahl ##

Eingabe: 

**8:00,-16;12345-000**

Ausgabe: 

- 8:00-16:00 12345-000

Erläuterung: 

Statt der Stunden Anzahl kann auch eine Zeit eingegeben werden, diese muß dann mit **-** prefixed werden. Automatische Pausen werden dabei ebenfalls berücksichtigt.

## Geleistete Arbeitszeit eines Tages auf 0 h festsetzen ##

Eingabe:

**!**

Erläuterung: 

Dieser Tag wird mit 0 geleisteten Stunden bei der prognostizierten Arbeitszeit eingerechnet.



