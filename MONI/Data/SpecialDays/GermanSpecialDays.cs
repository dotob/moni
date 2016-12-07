using System;
using System.Collections.Generic;

namespace MONI.Data.SpecialDays
{
    public class GermanSpecialDays : Dictionary<DateTime, GermanSpecialDay>
    {
        private readonly int year;

        /// <summary>
        /// Gibt das Jahr zurück, für das diese speziellen Tage gelten
        /// </summary>
        public int Year
        {
            get { return this.year; }
        }

        internal GermanSpecialDays(int year)
        {
            this.year = year;
        }

        /// <summary>
        /// Fügt der Auflistung ein neues GermanSpecialDay-Objekt hinzu
        /// </summary>
        /// <param name="name">Der Name des Tags</param>
        /// <param name="date">Das Datum des Tags</param>
        internal void Add(string name, DateTime date)
        {
            base.Add(date, new GermanSpecialDay(name, date));
        }
    }
}