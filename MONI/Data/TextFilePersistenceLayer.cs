using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MONI.Util;
using NLog;

namespace MONI.Data
{
    public class TextFilePersistenceLayer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string dataDirectory;
        private List<WorkDayPersistenceData> workDaysData = new List<WorkDayPersistenceData>();

        public TextFilePersistenceLayer(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
            // check for dir
            if (!Directory.Exists(dataDirectory))
            {
                try
                {
                    logger.Debug("try to create data dir: {0}", dataDirectory);
                    Directory.CreateDirectory(dataDirectory);
                }
                catch (Exception e)
                {
                    logger.Error("failed to create data dir: {0} with: {1}", dataDirectory, e);
                }
            }
        }

        public IEnumerable<WorkDayPersistenceData> WorkDaysData
        {
            get { return this.workDaysData; }
        }

        public int EntryCount
        {
            get { return this.workDaysData.Count; }
        }

        public DateTime FirstEntryDate
        {
            get
            {
                var workDayPersistenceData = this.workDaysData.FirstOrDefault();
                if (workDayPersistenceData != null)
                {
                    return workDayPersistenceData.Date;
                }

                return DateTime.Today;
            }
        }

        public ReadWriteResult ReadData()
        {
            ReadWriteResult ret = new ReadWriteResult {Success = true};
            try
            {
                var dataFiles = Directory.GetFiles(this.dataDirectory, "*md", SearchOption.TopDirectoryOnly);
                foreach (var dataFile in dataFiles)
                {
                    logger.Debug("start reading data from file: {0}", dataFile);
                    if (File.Exists(dataFile))
                    {
                        var readAllLines = File.ReadAllLines(dataFile);
                        int i = 0;
                        foreach (var wdLine in readAllLines)
                        {
                            if (!string.IsNullOrWhiteSpace(wdLine.Replace("\0", "")))
                            {
                                var wdDateData = wdLine.Token("|", 1);
                                var wdDateParts = wdDateData.Split(',').Select(s => Convert.ToInt32(s)).ToList();
                                var data = new WorkDayPersistenceData
                                {
                                    Year = wdDateParts.ElementAt(0),
                                    Month = wdDateParts.ElementAt(1),
                                    Day = wdDateParts.ElementAt(2),
                                    LineNumber = i,
                                    FileName = dataFile
                                };

                                var wdStringData = wdLine.Token("|", 2);
                                data.OriginalString = wdStringData.Replace("&#x007C;", "|").Replace("<br />", Environment.NewLine);

                                this.workDaysData.Add(data);
                                i++;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ret.Success = false;
                ret.Error = exception.Message;
                logger.Error(exception, "readdata failed");
            }

            return ret;
        }

        public async Task<ReadWriteResult> SaveDataAsync(WorkYear year)
        {
            Dictionary<string, List<string>> changedFiles = year.Months
                .Where(m => m.Days.Any(wd => wd.IsChanged))
                .ToDictionary(
                    m => Path.Combine(this.dataDirectory, $"{year.Year}_{m.Month:00}.md"),
                    m => m.Days
                        .Where(wd => wd.IsChanged && !wd.IsEmpty() || !wd.IsEmpty())
                        .Select(wd => $"{wd.Year},{wd.Month},{wd.Day}|{wd.OriginalString.Replace(Environment.NewLine, "<br />").Replace("|", "&#x007C;")}")
                        .ToList());

            foreach (var fileInfo in changedFiles.Where(f => f.Value.Any()))
            {
                using (var stream = new FileStream(fileInfo.Key, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true))
                using (var sw = new StreamWriter(stream))
                {
                    foreach (var line in fileInfo.Value)
                    {
                        await sw.WriteLineAsync(line);
                    }

                    await sw.FlushAsync();
                    await stream.FlushAsync();
                }
            }

            return new ReadWriteResult {Success = true};
        }

        public ReadWriteResult SetDataOfYear(WorkYear workYear)
        {
            var ret = ReadData();
            if (ret.Success)
            {
                foreach (WorkDayPersistenceData data in this.WorkDaysData.Where(wdpd => wdpd.Year == workYear.Year).ToList())
                {
                    var workDay = workYear.GetDay(data.Month, data.Day);
                    WorkDay errorDay = workDay;
                    try
                    {
                        workDay.SetData(data.OriginalString);
                    }
                    catch (Exception exception)
                    {
                        ret.ErrorCount++;
                        // only remember first error for now
                        if (ret.Success)
                        {
                            ret.Success = false;
                            string errorMessage = exception.Message;
                            if (errorDay != null)
                            {
                                errorMessage = string.Format("Beim Einlesen von {0} in der Datei {1} trat in Zeile {2} folgender Fehler auf: {3}", errorDay, data.FileName, data.LineNumber, exception.Message);
                                logger.Error(errorMessage);
                            }

                            ret.Error = errorMessage;
                        }
                    }
                }

                if (ret.ErrorCount > 1)
                {
                    ret.Error += string.Format("\n\n\nEs liegen noch {0} weitere Fehler vor.", ret.ErrorCount);
                }
            }

            return ret;
        }
    }

    public class WorkDayPersistenceData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string OriginalString { get; set; }
        public int LineNumber { get; set; }
        public string FileName { get; set; }

        public DateTime Date
        {
            get { return new DateTime(this.Year, this.Month, this.Day); }
        }
    }

    public class ReadWriteResult
    {
        public string Error { get; set; }
        public bool Success { get; set; }
        public int ErrorCount { get; set; }
    }
}