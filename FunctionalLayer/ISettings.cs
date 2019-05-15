namespace FunctionalLayer
{
	public interface ISettings
	{
		bool TimerEnabled { get; }
		int TimerInterval { get; }
	}
}