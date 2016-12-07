using System;

namespace MONI.Data.SpecialDays
{
    public interface ISpecialDay
    {
        string Name { get; set; }
        DateTime Date { get; set; }
    }
}