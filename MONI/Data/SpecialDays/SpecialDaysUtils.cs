using System;

namespace MONI.Data.SpecialDays
{
  public class SpecialDaysUtils
  {
    /// <summary>
    /// Berechnet das Datum des Ostersonntags des übergebenen Jahres (Das C# 2010 Codebook)
    /// </summary>
    /// <param name="year">Das Jahr, für das das Ostersonntag-Datum berechnet werden soll</param>
    /// <returns>Die das Datum des Ostersonntags des übergebenen Jahres zurück</returns>
    /// <remarks>
    /// Die Berechnung erfolgt nach dem Original von Ronald W. Mallen (www.assa.org.au/edm.html)
    /// </remarks>
    public static DateTime GetEasterSundayDate(int year) {
      // Überprüfen, ob das Jahr für die Osterberechnung gültig ist
      if (year < 1583 || year > 4099) {
        throw new Exception("Das Jahr muss zwischen 1583 und 4099 liegen");
      }

      int firstDigits, remaining19, temp; // Zwischenergebnisse
      int tA, tB, tC, tD, tE; // Tabellenergebnisse A bis E

      firstDigits = year / 100; // die ersten zwei Ziffern
      remaining19 = year % 19; // Rest von year / 19  

      // PFM-Datum berechnen
      temp = (firstDigits - 15) / 2 + 202 - 11 * remaining19;

      switch (firstDigits) {
        case 21:
        case 24:
        case 25:
        case 27:
        case 28:
        case 29:
        case 30:
        case 31:
        case 32:
        case 34:
        case 35:
        case 38:
          temp -= 1;
          break;
        case 33:
        case 36:
        case 37:
        case 39:
        case 40:
          temp -= 2;
          break;
      }

      temp = temp % 30;

      tA = temp + 21;
      if (temp == 29) {
        tA = tA - 1;
      }
      if (temp == 28 && remaining19 > 10) {
        tA = tA - 1;
      }

      // Nächsten Sonntag ermitteln
      tB = (tA - 19) % 7;

      tC = (40 - firstDigits) % 4;
      if (tC == 3) {
        tC = tC + 1;
      }
      if (tC > 1) {
        tC = tC + 1;
      }

      temp = year % 100;
      tD = (temp + temp / 4) % 7;

      tE = ((20 - tB - tC - tD) % 7) + 1;

      // Das Datum ermitteln und zurückgeben
      var day = tA + tE;
      var month = 0;

      if (day > 31) {
        day -= 31;
        month = 4;
      } else {
        month = 3;
      }

      return new DateTime(year, month, day);
    }

    /// <summary>
    /// Berechnet die speziellen Tage für Deutschland (Nordrhein-Westfalen)
    /// </summary>
    /// <param name="year">Das Jahr, für das die speziellen Tage berechnet werden sollen</param>
    /// <returns>Gibt eine GermanSpecialDays-Auflistung mit den Daten der wichtigsten speziellen Tage für Deutschland zurück</returns>
    public static GermanSpecialDays GetGermanSpecialDays(int year) {
      var gsd = new GermanSpecialDays(year);

      // Die festen besonderen Tage eintragen
      gsd.Add("Neujahr", new DateTime(year, 1, 1));
      gsd.Add("Maifeiertag", new DateTime(year, 5, 1));
      gsd.Add("Tag der Deutschen Einheit", new DateTime(year, 10, 3));
      gsd.Add("Allerheiligen", new DateTime(year, 11, 1));
      gsd.Add("Heiliger Abend", new DateTime(year, 12, 24));
      gsd.Add("1. Weihnachtstag", new DateTime(year, 12, 25));
      gsd.Add("2. Weihnachtstag", new DateTime(year, 12, 26));
      gsd.Add("Silvester", new DateTime(year, 12, 31));

      // Datum des Ostersonntags berechnen
      var easterSunday = GetEasterSundayDate(year);

      // Die beweglichen besonderen Tage ermitteln, die sich auf Ostern beziehen
      gsd.Add("Rosenmontag", easterSunday.AddDays(-48));
      gsd.Add("Karfreitag", easterSunday.AddDays(-2));
      gsd.Add("Ostermontag", easterSunday.AddDays(1));
      gsd.Add("Ostersonntag", easterSunday);
      gsd.Add("Christi Himmelfahrt", easterSunday.AddDays(39));
      gsd.Add("Pfingstsonntag", easterSunday.AddDays(49));
      gsd.Add("Pfingstmontag", easterSunday.AddDays(50));
      gsd.Add("Fronleichnam", easterSunday.AddDays(60));

      return gsd;
    }
  }
}