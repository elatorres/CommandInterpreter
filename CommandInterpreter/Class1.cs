namespace CommandInterpreter;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

public abstract class Command
{
    public string Keyword { get; }
    public string Description { get; }

    protected Command(string keyword, string description)
    {
        Keyword = keyword.ToLower();
        Description = description;
    }

    public abstract void Execute(string parameterString);

    /// <summary>
    /// Parses a string into arguments, respecting double quotes for strings with spaces.
    /// Example: 'copy "C:\My Folder" D:\Backup' -> ["copy", "C:\My Folder", "D:\Backup"]
    /// </summary>
    protected string[] ParseArguments(string parameterString)
    {
        if (string.IsNullOrWhiteSpace(parameterString)) return Array.Empty<string>();

        // This Regex looks for:
        // 1. Content inside double quotes: "([^"]*)"
        // 2. OR non-whitespace sequences: [^\s]+
        var matches = Regex.Matches(parameterString, @"\""([^""]*)\""|([^\s]+)");

        return matches.Select(m => 
            m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value
        ).ToArray();
    }
}

public class CommandInterpreter
{
    private readonly Dictionary<string, Command> _commands = new();
    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private bool _isRunning = false;

    public void RegisterCommand(Command command)
    {
        _commands[command.Keyword] = command;
    }
    
    public bool UnregisterCommand(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return false;
        string key = keyword.ToLower();
        // Optional: Prevent unregistering core system commands
        if (key == "help" || key == "exit")
        {
            Console.WriteLine($"\nSystem Error: Cannot unregister core command '{key}'.");
            return false;
        }

        if (_commands.ContainsKey(key))
        {
            _commands.Remove(key);
            return true;
        }
        return false;
    }
    // usage
    // var shell = new CommandInterpreter();
    // var secretCmd = new SecretCommand();
    // // Add it
    // shell.RegisterCommand(secretCmd);
    // // ... later in your logic ...
    // // Remove it
    // if (shell.UnregisterCommand("secret"))
    // { Console.WriteLine("Secret command has been disabled."); }
    
    public CommandInterpreter()
    {
        // Standard internal commands
        RegisterCommand(new HelpCommand(_commands));
        RegisterCommand(new ExitCommand(this));
    }
    
    public void Stop()
    {
        _isRunning = false;
    }
    
    public void Run()
    {
        _isRunning = true;
        Console.WriteLine("Interpreter started. Type 'help' for commands or 'exit' to quit.");

        while (_isRunning)        {
            Console.Write("\n> ");
            string input = ReadLineCustom();
            
            // Check if the loop was stopped during ReadLine (e.g., via a signal)
            if (!_isRunning) break;
            
            if (string.IsNullOrWhiteSpace(input)) continue;

            _history.Add(input);
            _historyIndex = _history.Count;

            ProcessInput(input);
        }
        Console.WriteLine("Interpreter session ended.");
    }

    private void ProcessInput(string input)
    {
        var parts = input.Split(' ', 2);
        string key = parts[0].ToLower();
        string args = parts.Length > 1 ? parts[1] : "";

        if (_commands.TryGetValue(key, out var command))
        {
            command.Execute(args);
        }
        else
        {
            Console.WriteLine($"\nUnknown command: {key}");
        }
    }

    private string ReadLineCustom()
    {
        StringBuilder buffer = new StringBuilder();
        int cursor = 0;
        int promptOffset = 2; // Length of "> "

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return buffer.ToString();
                
                case ConsoleKey.Home:
                    cursor = 0;
                    Console.CursorLeft = promptOffset;
                    break;

                case ConsoleKey.End:
                    cursor = buffer.Length;
                    Console.CursorLeft = promptOffset + buffer.Length;
                    break;
                
                case ConsoleKey.LeftArrow:
                    if (cursor > 0) { cursor--; Console.CursorLeft--; }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursor < buffer.Length) { cursor++; Console.CursorLeft++; }
                    break;

                case ConsoleKey.Backspace:
                    if (cursor > 0)
                    {
                        buffer.Remove(cursor - 1, 1);
                        cursor--;
                        RewriteLine(buffer.ToString(), cursor);
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursor < buffer.Length)
                    {
                        buffer.Remove(cursor, 1);
                        RewriteLine(buffer.ToString(), cursor);
                    }
                    break;

                case ConsoleKey.UpArrow:
                    // HandleHistoryNavigation(buffer, ref cursor, true);
                    if (_history.Count > 0 && _historyIndex > 0)
                    {
                        _historyIndex--;
                        buffer.Clear().Append(_history[_historyIndex]);
                        cursor = buffer.Length;
                        RewriteLine(buffer.ToString(), cursor);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    // HandleHistoryNavigation(buffer, ref cursor, false);
                    if (_historyIndex < _history.Count - 1)
                    {
                        _historyIndex++;
                        buffer.Clear().Append(_history[_historyIndex]);
                    }
                    else
                    {
                        _historyIndex = _history.Count;
                        buffer.Clear();
                    }
                    cursor = buffer.Length;
                    RewriteLine(buffer.ToString(), cursor);
                    break;
                
                default:
                    // Handle typing and pasting (if the character is valid)
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        buffer.Insert(cursor, keyInfo.KeyChar);
                        cursor++;
                        RewriteLine(buffer.ToString(), cursor);
                    }
                    break;
            }
        }
    }

    private void RewriteLine(string text, int cursor)
    {
        int currentLine = Console.CursorTop;
        Console.SetCursorPosition(2, currentLine); // Adjust '2' based on prompt length "> "
        Console.Write(new string(' ', Console.WindowWidth - 3)); // Clear line
        Console.SetCursorPosition(2, currentLine);
        Console.Write(text);
        Console.SetCursorPosition(2 + cursor, currentLine);
    }
}

// Help Command
public class HelpCommand : Command
{
    private readonly Dictionary<string, Command> _registry;

    // We pass the registry from the interpreter so Help can "see" other commands
    public HelpCommand(Dictionary<string, Command> registry) 
        : base("help", "Displays a list of commands or details of a specific command.")
    {
        _registry = registry;
    }

    public override void Execute(string parameterString)
    {
        var args = ParseArguments(parameterString);

        // Scenario: "help" (list everything)
        if (args.Length == 0)
        {
            Console.WriteLine("\nAvailable Commands (Alphabetical):");
            Console.WriteLine(new string('-', 40));

            // Sort the commands by Keyword before printing
            var sortedCommands = _registry.Values.OrderBy(c => c.Keyword);

            foreach (var cmd in sortedCommands)
            {
                // PadRight(12) ensures the descriptions align vertically
                Console.WriteLine($"- {cmd.Keyword.PadRight(12)} : {cmd.Description}");
            }
        }
        // Scenario: "help add" (show specific description)
        else
        {
            string target = args[0].ToLower();
            if (_registry.TryGetValue(target, out var cmd))
            {
                Console.WriteLine($"\nCommand: {cmd.Keyword}");
                Console.WriteLine($"Description: {cmd.Description}");
            }
            else
            {
                Console.WriteLine($"\nError: Command '{target}' not found.");
            }
        }
    }
}

// Exit command
public class ExitCommand : Command
{
    private readonly CommandInterpreter _interpreter;

    public ExitCommand(CommandInterpreter interpreter) 
        : base("exit", "Terminates the command interpreter and closes the session.")
    {
        _interpreter = interpreter;
    }

    public override void Execute(string parameterString)
    {
        Console.WriteLine("Exiting interpreter...");
        _interpreter.Stop(); // Signals the loop to end
    }
}


