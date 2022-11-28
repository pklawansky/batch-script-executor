// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Diagnostics;

var directory = GetDirectory();
var files = LoadBatchScripts(directory);
PromptBatchFileExecution(files, directory);









string GetDirectory()
{
    var appsettingsJson = File.ReadAllText("appsettings.json");
    var appsettings = JsonConvert.DeserializeObject<dynamic>(appsettingsJson)!;
    return appsettings.directory;
}
void AppendText(string text)
{
    Console.WriteLine(text);
}

List<string> LoadBatchScripts(string directory)
{
    var files = Directory.EnumerateFiles(directory)
        .Where(x => x.ToLower().EndsWith(".bat"))
        .Select(x => Path.GetFileName(x)).ToList();
    return files;
}

void PromptBatchFileExecution(List<string> files, string directory)
{
    PrintBatchScriptList(files, directory);
    AppendText($"Type the index of the script you want to execute, (x) to exit:");
    var key = Console.ReadLine()!;
    if (!int.TryParse(key, out int index) || index > files.Count - 1)
    {
        if (key.ToLower() == "x")
        {
            return;
        }
        AppendText($"Incorrect Key Pressed, try again.");
        PromptBatchFileExecution(files, directory);
    }

    ExecuteAsAdmin(files[index], directory);
    //ExecuteBatchScript(files[index], directory);
    PromptBatchFileExecution(files, directory);
}

void PrintBatchScriptList(List<string> files, string directory)
{
    AppendText($"""Found the following batch files in "{directory}":""");
    for (var i = 0; i < files.Count; i++)
    {
        AppendText($"{i}. {files[i]}");
    }
}

void ExecuteBatchScript(string fileName, string directory)
{
    ProcessStartInfo startinfo = new ProcessStartInfo();
    startinfo.FileName = "cmd";
    startinfo.WorkingDirectory = directory;
    startinfo.RedirectStandardOutput = true;
    startinfo.CreateNoWindow = true;
    Process process = new Process();
    process.StartInfo = startinfo;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardInput = true;
    process.Start();

    process.StandardInput.WriteLine(fileName);
    process.StandardInput.WriteLine("exit");

    process.StartInfo.RedirectStandardOutput = true;

    while (!process.StandardOutput.EndOfStream)
    {
        var line = process.StandardOutput.ReadLine();
        AppendText(line ?? "");
    }

    process.WaitForExit();
}

void ExecuteAsAdmin(string fileName, string directory)
{
    Process proc = new Process();
    proc.StartInfo.FileName = Path.Combine(directory, fileName);
    proc.StartInfo.UseShellExecute = true;
    proc.StartInfo.Verb = "runas";
    proc.Start();
}