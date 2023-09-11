namespace TREAgent.Repositories;

public class TeskAudit
{
    public int id { get; set; }
    public string message { get; set; }
    public string teskid { get; set; }
    public DateTime dated { get; set; } = DateTime.Now;
}