using System.Reflection;
using System.Text;
using NuGet.Versioning;

var defaultArgs = new[]
                  {
                      "--name",
                      "--fullVersion",
                      "--version",
                      "--major",
                      "--minor",
                      "--patch",
                      "--release",
                      "--build",
                      "--fileVersion",
                      "--configuration",
                      "--description"
                  };

if (args.Length == 0)
{
    Console.WriteLine(DisplayHelp());

    return;
}

var filePath = args.FirstOrDefault(t => !t.StartsWith("-") && File.Exists(t));

if (string.IsNullOrWhiteSpace(filePath))
{
    Console.WriteLine(DisplayHelp());

    return;
}

filePath = Path.GetFullPath(filePath);

try
{
    var assembly = Assembly.LoadFile(filePath);
    var assemblyName = assembly.GetName().Name;
    var attributes = assembly.GetCustomAttributes().ToArray();
    var attrVersion = attributes.OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
    var attrFileVersion = attributes.OfType<AssemblyFileVersionAttribute>().FirstOrDefault();
    var attrConfiguration = attributes.OfType<AssemblyConfigurationAttribute>().FirstOrDefault();
    var attrDescription = attributes.OfType<AssemblyDescriptionAttribute>().FirstOrDefault();

    SemanticVersion.TryParse(attrVersion?.InformationalVersion, out var semVer);

    var simple = args.Any(t => t.ToUpper() == "-S" || t.ToUpper() == "--SIMPLE");

    var infoArgs = args.Where(t => t.ToUpper() != "-S" && t.ToUpper() != "--SIMPLE")
                       .Skip(1)
                       .ToArray();

    if (infoArgs.Length == 0)
    {
        infoArgs = defaultArgs;
    }

    var sb = new StringBuilder();

    foreach (var arg in infoArgs)
    {
        var info = arg.ToUpper() switch
                   {
                       "-N" => ("Name", assemblyName),
                       "--NAME" => ("Name", assemblyName),
                       "--FULLVERSION" => ("FullVersion", semVer.ToFullString()),
                       "-V" => ("Version", $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}"),
                       "--VERSION" => ("Version", $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}"),
                       "--MAJOR" => ("Major", semVer.Major.ToString()),
                       "--MINOR" => ("Minor", semVer.Minor.ToString()),
                       "--PATCH" => ("Patch", semVer.Patch.ToString()),
                       "--RELEASE" => ("Release", semVer.Release),
                       "--BUILD" => ("Build", semVer.Metadata),
                       "-FV" => ("FileVersion", attrFileVersion?.Version),
                       "--FILEVERSION" => ("FileVersion", attrFileVersion?.Version),
                       "-C" => ("Configuration", attrConfiguration?.Configuration),
                       "--CONFIGURATION" => ("Configuration", attrConfiguration?.Configuration),
                       "-D" => ("Description", attrDescription?.Description),
                       "--DESCRIPTION" => ("Description", attrDescription?.Description),
                       _ => (null, null)
                   };

        if (info.Item1 == null) continue;

        if (simple)
        {
            sb.Append(info.Item2 + "\t");
        }
        else
        {
            sb.AppendLine(info.Item1 + ": " + info.Item2);
        }
    }

    var output = sb.ToString().Trim();

    if (!string.IsNullOrWhiteSpace(output)) Console.WriteLine(output);
}
catch (Exception e)
{
    Console.Error.WriteLine(e.ToString());
}

string DisplayHelp()
{
    var sbHelp = new StringBuilder();
    sbHelp.AppendLine();
    sbHelp.AppendLine("Usage: dotnet asm-info PATH-TO-ASSEMBLY");
    sbHelp.AppendLine("Usage: dotnet asm-info PATH-TO-ASSEMBLY [OPTIONS]");
    sbHelp.AppendLine();
    sbHelp.AppendLine("path-to-assembly:");
    sbHelp.AppendLine("  The path to an application .dll file to execute.");
    sbHelp.AppendLine();
    sbHelp.AppendLine("Options:");
    sbHelp.AppendLine("  -n,     --name           Display the assembly name.");
    sbHelp.AppendLine("  -v,     --version        Display the assembly version.");
    sbHelp.AppendLine("          --fullVersion    Display the assembly full version.");
    sbHelp.AppendLine("          --major          Display the assembly major version.");
    sbHelp.AppendLine("          --minor          Display the assembly minor version.");
    sbHelp.AppendLine("          --patch          Display the assembly patch version.");
    sbHelp.AppendLine("          --release        Display the assembly release version.");
    sbHelp.AppendLine("          --build          Display the assembly build metadata.");
    sbHelp.AppendLine("  -fv,    --fileVersion    Display the assembly file version.");
    sbHelp.AppendLine("  -c,     --configuration  Display the assembly configuration.");
    sbHelp.AppendLine("  -d,     --description    Display the assembly description.");
    sbHelp.AppendLine("  -s,     --simple         Display with simple format (without field name, tab-delimited string).");
    sbHelp.AppendLine("  -?, -h, --help           Display help.");

    return sbHelp.ToString();
}