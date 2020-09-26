<#
.SYNOPSIS
  Install script run by chocolatey
.DESCRIPTION
  As part of the install of the BizTalk components, the appsettings json file for the tool is updated, for the dependent packages.
.EXAMPLE
   .\chocolateyinstall.ps1
#>

# Get the app settings file location.
$cliDir = Join-Path -path $env:ChocolateyInstall -ChildPath "lib\biztalkmigrator-cli\tools"
$cliAppSettingFileLocation = Join-Path -path $cliDir -ChildPath "appsettings.json"

# Load in the config.
$appSettings = Get-Content -Path $cliAppSettingFileLocation -Raw | ConvertFrom-Json

# Set the config for the BizTalk package.
$biztalkDir = Join-Path -path $env:ChocolateyInstall -ChildPath "lib\biztalkmigrator\tools"
$bizTalkBinDir = Join-Path -path $biztalkDir -ChildPath "bin"
$bizTalkConfigDir = Join-Path -path $biztalkDir -ChildPath "config"
$bizTalkTemplateDir = Join-Path -path $biztalkDir -ChildPath "templates"

$appSettings.AppConfig.FindPaths = @()
$appSettings.AppConfig.FindPaths += $bizTalkBinDir
$appSettings.AppConfig.FindPattern = "*StageRunners*.dll"
$appSettings.AppConfig.TemplateConfigPath = $bizTalkConfigDir
$appSettings.AppConfig.TemplatePaths = @()
$appSettings.AppConfig.TemplatePaths += $bizTalkTemplateDir

# Set the config for the Azure package.
$azureDir = Join-Path -path $env:ChocolateyInstall -ChildPath "lib\biztalkmigrator-azure\tools"
$azureTemplateDir = Join-Path -path $azureDir -ChildPath "templates"
$appSettings.AppConfig.TemplatePaths += $azureTemplateDir

# Save the config.
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $cliAppSettingFileLocation