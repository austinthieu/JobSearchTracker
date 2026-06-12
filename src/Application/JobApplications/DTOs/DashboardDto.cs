namespace Application.JobApplications.DTOs;

public class DashboardDto
{
  public int TotalApplications { get; set; }
  public int ActiveApplications { get; set; }       // not rejected/withdrawn/accepted
  public int InterviewsScheduled { get; set; }
  public int Offers { get; set; }
  public int Rejected { get; set; }
  public double? ResponseRate { get; set; }          // % that got a response (phone screen+)
  public double? InterviewToOfferRate { get; set; }  // % of interviews → offer
  public Dictionary<string, int> StatusBreakdown { get; set; } = [];
}
