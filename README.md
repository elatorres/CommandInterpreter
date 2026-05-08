content = """# C# Command Line Interpreter (CLI)

A robust, extensible, and user-friendly command-line interpreter library for C#. 
This project provides a terminal-like experience within a standard console application, featuring real-time line editing, command history, and a plugin-based command architecture.

## Features

- **Advanced Line Editor**: 
  - **Real-time Editing**: Move the cursor freely with Left/Right arrow keys to insert or delete text at any position.
  - **Navigation**: Support for `Home` and `End` keys to jump to the start or end of the line.
  - **Clipboard Support**: Standard console paste operations are handled gracefully.
- **Command History**: Navigate through previous commands using `Up` and `Down` arrow keys.
- **Command Pattern Architecture**: Easily add or remove commands by inheriting from an abstract base class.
- **Smart Parameter Parsing**: 
  - Automatically handles arguments separated by spaces.
  - Supports **quoted strings** (e.g., `msg "Hello World" user1`), ensuring text with spaces is treated as a single parameter.
- **Self-Documenting**: 
  - Built-in `help` command that lists all registered commands sorted alphabetically.
  - Detailed help for specific commands (e.g., `help add`).
- **Graceful Shutdown**: Dedicated `exit` command to terminate the interpreter session cleanly.

## Getting Started

### 1. Define a Command
See MyProgram example.

Create a class that inherits from `Command`. Provide a keyword, a description, and implement the `Execute` logic.

```csharp
public class GreetCommand : Command
{
    public GreetCommand() : base("greet", "Greets a user. Usage: greet \"Full Name\"") { }

    public override void Execute(string parameterString)
    {
        var args = ParseArguments(parameterString);
        if (args.Length > 0)
        {
            Console.WriteLine($"Hello, {args[0]}!");
        }
        else
        {
            Console.WriteLine("Usage: greet \\"Name\\"");
        }
    }
}
```
### 2. Initialize the Interpreter
Register your commands and start the run loop.
```csharp
class Program
{
    static void Main()
    {
        var interpreter = new CommandInterpreter();
        
        // Register your custom commands
        interpreter.RegisterCommand(new GreetCommand());
        
        // Start the CLI
        interpreter.Run();
    }
}
```
## Key Components
### CommandInterpreter
The engine of the project. It intercepts raw keystrokes to manage the buffer and cursor, handles the command registry, and manages the execution loop.

### Command (Abstract Class)
The blueprint for all internal commands.
- **Keyword**: The string used to trigger the command.
- **Description**: Metadata used by the help system.
- **ParseArguments(string)**: A helper method using Regex to split input while respecting quotes.

### Controls
- **Key**              **Action**
  - **Enter**  Execute the current command
  - **Left / Right**  Move cursor within the current line
  - **Up / Down**  Scroll through command history
  - **Home / End**  Jump to start or end of the line
  - **Backspace / Delete**  Remove characters

License
This project is open-source and available under the Creative Commons Attribution license (CC BY 4.0).
Partly generated with Google Gemini
