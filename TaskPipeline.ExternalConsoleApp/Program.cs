using System.CommandLine;

// a little hack to return a non-zero exit code when an app throws an exception.
// The info about the exception is provided in stderr, the code - in process.ExitCode.
static void ConsoleUnhandledExceptionWithExitCode(object sender, UnhandledExceptionEventArgs e)
{
	Console.Error.WriteLine(e.ExceptionObject);
	Environment.Exit(1);
}
AppDomain.CurrentDomain.UnhandledException += ConsoleUnhandledExceptionWithExitCode;


var delayOption = new Option<int>
	(name: "--delay",
	description: "Execution time in seconds, int.",
	getDefaultValue: () => 1);

var shouldThrowOption = new Option<bool>
	(name: "--throw",
	description: "An option to define if external command should throw an exception, bool.",
	getDefaultValue: () => false);


var rootCommand = new RootCommand("External command line app");
rootCommand.AddOption(delayOption);
rootCommand.AddOption(shouldThrowOption);

rootCommand.SetHandler(async (delayOptionValue, shouldThrowOptionValue) =>
{
	Console.WriteLine($"External console app start with arguments:");
	Console.WriteLine($"--delay = {delayOptionValue}");
	Console.WriteLine($"--throw = {shouldThrowOptionValue}");
	
	await Task.Delay(delayOptionValue * 1000);

	if (shouldThrowOptionValue)
		throw new ApplicationException("External console app throws an exception as requested.");

	Console.WriteLine("External console app finishes successfully.");
}, delayOption, shouldThrowOption);

await rootCommand.InvokeAsync(args);

return 0;
