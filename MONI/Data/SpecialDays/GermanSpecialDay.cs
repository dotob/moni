using System;

namespace MONI.Data.SpecialDays {
    public class GermanSpecialDay : ISpecialDay, IComparable<ISpecialDay> {
        public GermanSpecialDay(string name, DateTime date) {
            this.Name = name;
            this.Date = date;
        }

        /// <summary>
        /// Der Name des speziellen Tags
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Das Datum des speziellen Tags
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Vergleicht ein GermanSpecialDay-Objekt mit dem aktuellen
        /// </summary>
        /// <param name="otherSpecialDay">Das andere GermanSpecialDay-Objekt</param>
        public int CompareTo(ISpecialDay otherSpecialDay) {
            return this.Date.CompareTo(otherSpecialDay.Date);
        }
    }
}