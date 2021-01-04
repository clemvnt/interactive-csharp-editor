namespace InteractiveEditor.Abstractions
{
  /// <summary>
  /// ViewModel.
  /// </summary>
  public class ViewModel
  {
    public string Name { get; set; }

    public ViewModel(string name)
    {
      Name = name;
    }
  }
}
