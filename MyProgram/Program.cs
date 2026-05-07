using CommandInterpreter;

// Program Entry Point
class Program
{
    // Add command
    public class MathCommand : Command
    {
        public MathCommand() : base("add", "Adds two numbers together. Usage: add <num1> <num2>") { }

        public override void Execute(string parameterString)
        {
            var args = ParseArguments(parameterString);
            if (args.Length == 2 && double.TryParse(args[0], out double a) && double.TryParse(args[1], out double b))
            {
                Console.WriteLine($"Result: {a + b}");
            }
            else
            {
                Console.WriteLine("Usage: add <num1> <num2>");
            }
        }
    }
    
    // Msg Command example
    public class MsgCommand : Command
    {
        public MsgCommand() : base("msg", "Sends a message. Usage: msg \"Your Message\" UserName") { }

        public override void Execute(string parameterString)
        {
            var args = ParseArguments(parameterString);

            if (args.Length >= 2)
            {
                string message = args[0]; // Content from inside the quotes
                string user = args[1];    // The next word
                Console.WriteLine($"Sending: '{message}' to {user}");
            }
            else
            {
                Console.WriteLine("Usage: msg \"text with spaces\" user");
            }
        }
    }
    
    // Main Program  ====================================
    static void Main()
    {
        // Add a command interpreter
        var shell = new CommandInterpreter.CommandInterpreter();
        
        // Add your custom business logic commands
        shell.RegisterCommand(new MathCommand());
        shell.RegisterCommand(new MsgCommand());
        
        // This will now block until the user types 'exit'
        shell.Run(); 

        Console.WriteLine("Press any key to close the window.");
        Console.ReadKey();
    }
}