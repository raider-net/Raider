namespace Raider.Messaging
{
	public interface IComponent
	{
		int IdComponent { get; }
		bool Initialized { get; }
		string Name { get; }
		int IdScenario { get; }
		ComponentState State { get; }
	}
}
