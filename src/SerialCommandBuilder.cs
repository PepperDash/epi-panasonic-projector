using System;

namespace PepperDash.Essentials.Plugins.Display.Panasonic.Projector
{
    public class SerialCommandBuilder : ICommandBuilder
    {
        public string Delimiter
        {
            get { return "\x03"; }
        }

        private readonly string _id;

        private const string CommandWithParameterFormat = "AD{0};{1}:{2}";
        private const string CommandWithoutParameterFormat = "AD{0};{1}";

        public SerialCommandBuilder(string id)
        {
            _id = id;
        }

        public string GetCommand(string cmd, string parameter)
        {
            var cmdToSend = String.Format("\x02{0}\x03", CommandWithParameterFormat);
            return String.Format(cmdToSend, _id, cmd, parameter);
        }

        public string GetCommand(string cmd)
        {
            var cmdToSend = String.Format("\x02{0}\x03", CommandWithoutParameterFormat);
            return String.Format(cmdToSend, _id, cmd);
        }
    }
}