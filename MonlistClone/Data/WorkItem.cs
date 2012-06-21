namespace MonlistClone.Data {
  public class WorkItem {
    public WorkItem(TimeItem start, TimeItem end, string project, string position) {
      this.Start = start;
      this.End = end;
      this.Project = project;
      this.Position = position;
    }

    public WorkItem(TimeItem start, TimeItem end, string project, string position, string description) {
      this.Start = start;
      this.End = end;
      this.Project = project;
      this.Position = position;
      this.Description = description;
    }

    public WorkItem(TimeItem start, TimeItem end) {
      Start = start;
      End = end;
    }

    public TimeItem Start { get; set; }
    public TimeItem End { get; set; }
    public string Project { get; set; }
    public string Position { get; set; }
    public string Description { get; set; }

    public string ProjectPosition { get { return string.Format("{0}-{1}", Project, Position); } }

    public double HoursDuration {
      get { return this.End - this.Start; }
    }

    public override bool Equals(object obj) {
      WorkItem ti = obj as WorkItem;
      if (ti != null) {
        return Equals(this.Start, ti.Start) && Equals(this.End, ti.End) && Equals(this.Project, ti.Project) && Equals(this.Position, ti.Position);
      }
      return false;
    }


    public override string ToString() {
      return string.Format("{0}-{1} ; {2}:{3}", this.Start, this.End, this.Project, this.Position);
    }
  }
}