namespace Raider.Commands
{
	public interface ICommand { }

	public interface ICommand<out TResult> : ICommand
	{ }
}
