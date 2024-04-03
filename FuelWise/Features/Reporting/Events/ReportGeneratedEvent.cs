namespace FuelWise.Reporting;

public delegate void ReportGeneratedEventHandler(object sender, ReportGeneratedEventArgs e);

public sealed class ReportGeneratedEventArgs : EventArgs
{
    public ReportGeneratedEventArgs(Report report)
    {
        Report = report;
    }

    public Report Report { get; }
}