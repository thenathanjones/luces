param($NuGet="..\NuGet.exe")

$ProjectRoot = Split-Path -parent $MyInvocation.MyCommand.Definition

$PackageConfigs = Get-ChildItem -Path $ProjectRoot -Filter packages.config -Recurse

foreach ($Config in $PackageConfigs) {
	&$Nuget i $Config.FullName -o "$ProjectRoot/packages"
}