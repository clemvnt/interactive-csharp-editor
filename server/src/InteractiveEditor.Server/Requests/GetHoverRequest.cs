namespace InteractiveEditor.Server.Requests
{
  public class GetHoverRequest
  {
    public string Code { get; set; }
    public int LineNumber { get; set; }
    public int Column { get; set; }
  }
}
