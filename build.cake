#load nuget:?package=DevelopEngine.Cake


///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var projects = GetProjects(File("./src/ModulEngine.sln"), configuration);
var artifacts = "./dist/";
var frameworks = new List<string> { "netstandard2.0" };


///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////


Task("Default")
.IsDependentOn("Post-Build")
.IsDependentOn("NuGet");

RunTarget(target);
