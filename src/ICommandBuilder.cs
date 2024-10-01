namespace PanasonicProjectorEpi
{
    public interface ICommandBuilder
    {
        string Delimiter { get; }
        string GetCommand(string cmd, string parameter);
        string GetCommand(string cmd);
    }
}