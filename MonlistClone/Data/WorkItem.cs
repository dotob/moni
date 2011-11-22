namespace MonlistClone.Data {
  public class WorkItem {
    public WorkItem(TimeItem start, TimeItem end, string project, string position) {
      this.Start = start;
      this.End = end;
      this.Project = project;
      this.Position = position;
    }

    public TimeItem Start { get; set; }
    public TimeItem End { get; set; }
    public string Project { get; set; }
    public string Position { get; set; }

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