# moni - der bessere weg stunden zu erfassen #

## die idee ##

die idee bei moni ist, dass es bei der stundenerfassung nicht so sehr auf exakte uhrzeiten ankommt, sondern auf die stunden anzahl. deshalb bietet moni eine einfache möglichkeit stundenaufwände einzugeben und trotzdem start und endzeiten aufzuzeichnen, sowie automatische pauseneinträge zu erzeugen. die eingabe besteht aus einem bestimmten format, dass unten beschrieben ist. am anfang ist es ein bisschen gewöhnungsbedüftig, aber man kann es schnell lernen. 

außerdem bietet moni weitere features, die die eingabe vereinfachen:

- wochenweise ansicht
- automatische pauseneintragung
- abkürzungen für projekte
- abkürzungen für ganze tage
- anzeige stunden pro project/abkürzung
- prognose arbeitstunden
- keyboardnavigation


## das interface ##

![moni-interface](coming-soon)

## keyboad shortcuts ##

### navigation ####

- **tab**: in eingabefeld des vorigen tages gehen
- **shift+tab**: in eingabefeld des folgenden tages gehen
- **strg+cursor_links**: vorige woche anzeigen
- **strg+cursor_rechts**: nächste woche anzeigen

### andere ###
- **strg+u**: eingabe des letzten tages hierhin kopieren

# dokumentation der eingabe #

## ganz einfach: ein eintrag ##

eingabe: 

**8,8;12345-000**

ausgabe: 

- 8:00-16:00 12345-000

erläuterung: 

erste zahl vor dem komma ist die startzeit, gefolgt von der anzahl der stunden und project-position

## mehrere einträge ##

eingabe: 

**8,4;12345-000,4;54321-000**

ausgabe: 

- 8:00-12:00 12345-000
- 12:00-16:00 54321-000

erläuterung: 

kommasepariert werde die einträge aneinandergereiht

## teilstunden ##

eingabe: 

**8:30,4.25;12345-000,3.75;54321-000**

ausgabe: 

- 8:30-12:45 12345-000
- 12:45-16:30 54321-000

erläuterung: 

uhrzeiten werden im format **stunde:minute** angegeben. stunden mit **punkt** getrennt


## beschreibungen ##

eingabe: 

**8:30,4.25;12345-000(fehlerbehebung),3.75;54321-000(support)**

ausgabe: 

- 8:30-12:45 12345-000  fehlerbehebung
- 12:45-16:30 54321-000  support

erläuterung: 

beschreibungen können pro eintrag in **klammern** angegeben werden

## abkürzungen ##

konfiguration:

**ctb => 12345-000**
**ktl => 54321-000(spezifikation)**

eingabe: 

**8:30,4.25;ctb,3.75;ktl**

ausgabe: 

- 8:30-12:45 12345-000
- 12:45-16:30 54321-000  spezifikation


## abkürzungen und beschreibungen ##

konfiguration:

- **ctb => 12345-000**
- **ktl => 54321-000(spezifikation)**

eingabe: 

**8:30,4.25;ctb(support),2.75;ktl(spezifikation schnittstelle),1;ktl(+ bahn)**

ausgabe: 

- 8:30-12:45 12345-000  support
- 12:45-15:30 54321-000  spezifikation schnittstelle
- 15:30-17:30 54321-000  spezifikation schnittstelle bahn

erläuterung: 

abkürzungen können auch beschreibungen enthalten. wird die abkürzung mit beschreibung eingegeben wird diese angefügt ("(+ )") oder ersetzt ("( )") die beschreibung der abkürzung

## abkürzungen für einen ganzen tag ##

konfiguration:

- **krank => 8,8;12345-000(krank oder doc)** (WholeDayExpansion  = true, siehe settings.json erläuterung)

eingabe: 

**krank**

ausgabe: 

- 8:00-16:00 12345-000  krank oder doc

## manuelle pause ##

eingabe: 

**8:00,4;12345-000,1!,4;12345-000**

ausgabe: 

- 8:00-12:00 12345-000
- 13:00-17:00 12345-000

erläuterung: 

endet ein eintrag mit **!** dann wird die folgende stundenzahl als pause eingefügt

## automatische pause ##

konfiguration:

- **pause einfügen**
- **pause um 12:00**
- **pausenlänge 30min**

eingabe: 

**8:00,8;12345-000**

ausgabe: 

- 8:00-12:00 12345-000
- 12:30-16:30 12345-000

erläuterung: 

es wird automatisch ein pause um 12:00 eingefügt

## endzeit statt stunden anzahl ##

eingabe: 

**8:00,-16;12345-000**

ausgabe: 

- 8:00-16:00 12345-000

erläuterung: 

statt der stunden anzahl kann auch eine zeit eingegeben werden, diese muß dann mit **-** prefixed werden. automatische pausen werden dabei ebenfalls berücksichtigt




