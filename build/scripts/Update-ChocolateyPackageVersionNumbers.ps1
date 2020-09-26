
<#
.SYNOPSIS
Updates the version number in nuspec files.

.DESCRIPTION
The script finds the latest version of the chocolatey package from the repo and then updates the nuspec file to use the latest version.

.PARAMETER nuspecFileName
The resource group to deploy to.

.PARAMETER chocoSource
The chocolatey repo source url.

.EXAMPLE
.\Update-ChocolateyPackageVersionNumbers -nuspecFileName "..\chocolatey\biztalkmigrator\biztalkmigrator.nuspec" -chocoSource "https://chocolatey.org/packages/"
#>
[CmdletBinding(SupportsShouldProcess = $true)]
Param(
    [parameter(Mandatory = $true)]
    [string] $nuspecFileName,
    [parameter(Mandatory = $true)]
    [string] $chocoSource
)
$nuspecFullName = Join-Path -Path $PSScriptRoot -ChildPath $nuspecFileName

[xml]$xml = Get-Content $nuspecFullName

Write-Verbose $xml.InnerXml

# Get the dependencies from the nuspec file.
$dependencies = $xml.package.metadata.dependencies

foreach ($dependency in $dependencies.ChildNodes) {
    
    # Get the package name.
    $packageName = $dependency.id

    Write-Verbose "Updating dependency $packageName"
    
    # Get the latest package version from the repo.
    $latestPackageVersion = cmd.exe /c choco search $packageName -s $chocoSource --all --pre | Out-String -Stream | Select-String -Pattern "^$packageName.*$" | Sort-Object -Descending | Select-Object -First 1
    
    Write-Verbose "Latest package: $latestPackageVersion"

    # Get the version number from the package.
    $versionNumber = $latestPackageVersion.Line.Split(" ")[1]

    Write-Verbose "Version number: $versionNumber"

    # Set the version number
    $dependency.SetAttribute("version", $versionNumber)

    Write-Verbose "Updated dependency"
}

# Save the xml document.
$xml.Save($nuspecFullName)

Write-Verbose "File saved: $nuspecFullName"