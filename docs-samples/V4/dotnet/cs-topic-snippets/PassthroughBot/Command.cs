using System;
using System.Collections.Generic;

namespace ContainerLib
{
    /// <summary>Represents a command, a meta-level instruction for controling the selection behavior.</summary>
    public class Command : IEquatable<Command>, IEquatable<string>
    {
        /// <summary>The Help command: provide help, repeat the current dialog.</summary>
        public static Command Help { get; } = new Command("help");

        /// <summary>The Back command: exit the current dialog, if not already at the top.</summary>
        public static Command Back { get; } = new Command("back");

        /// <summary>The Reset command: clear the stack and start again from the top.</summary>
        public static Command Reset { get; } = new Command("reset");

        /// <summary>All of the defined commands, as a list.</summary>
        public static IReadOnlyList<Command> Commands
            = new List<Command> { Help, Back, Reset };

        public Command(string name)
        {
            Name = !string.IsNullOrWhiteSpace(name)
                ? name.Trim().ToLowerInvariant()
                : throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public bool Equals(Command other)
        {
            return Name.Equals(other?.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Equals(string other)
        {
            return Name.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
