namespace Raider.Generator
{
	public class GeneratorError
	{
		public int? Column { get; set; }
		public string? ErrorNumber { get; set; }
		public string? ErrorText { get; set; }
		public string? FileName { get; set; }
		public bool IsWarning { get; set; }
		public int? Line { get; set; }
	}
}
