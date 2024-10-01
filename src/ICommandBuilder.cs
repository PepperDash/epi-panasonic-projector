namespace PepperDash.Essentials.Plugins.Display.Panasonic.Projector
{
    public interface ICommandBuilder
    {
        string Delimiter { get; }
        string GetCommand(string cmd, string parameter);
        string GetCommand(string cmd);
    }
}