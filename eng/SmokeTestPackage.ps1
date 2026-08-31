param(
    [Parameter(Mandatory = $true)]
    [string]$PackageDirectory,

    [Parameter(Mandatory = $true)]
    [string]$PackageVersion
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts')) + [System.IO.Path]::DirectorySeparatorChar
$workDirectory = [System.IO.Path]::GetFullPath((Join-Path $artifactsRoot 'package-smoke'))

if (!$workDirectory.StartsWith($artifactsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Unsafe smoke-test directory: $workDirectory"
}

if (Test-Path -LiteralPath $workDirectory) {
    Remove-Item -LiteralPath $workDirectory -Recurse -Force
}

$env:NUGET_PACKAGES = Join-Path $workDirectory '.packages'

dotnet new wpf --name PackageSmoke --output $workDirectory --framework net10.0
if ($LASTEXITCODE -ne 0) { throw 'Unable to create the package smoke-test project.' }

dotnet add (Join-Path $workDirectory 'PackageSmoke.csproj') package WpfNotifications --version $PackageVersion --source ([System.IO.Path]::GetFullPath($PackageDirectory))
if ($LASTEXITCODE -ne 0) { throw 'Unable to install the generated WpfNotifications package.' }

$source = @'
using Notifications;
using Notifications.Enums;
using Notifications.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PackageSmoke;

internal static class PackageApiSmoke
{
    public static async Task ExerciseAsync(Window owner)
    {
        using var manager = new NotificationManager(new NotificationManagerOptions
        {
            ShowCloseButton = false,
            ShowCountdownBar = true,
            Overlay = new NotificationOverlayOptions
            {
                MaxItems = 3,
                Position = NotificationPosition.TopRight,
            },
        });

        var handle = await manager.ShowAsync(new NotificationRequest("package smoke test")
        {
            Target = NotificationTarget.Overlay(NotificationMonitor.Owner, owner),
            ExpirationTime = TimeSpan.FromSeconds(1),
            Tag = "smoke",
            DuplicateBehavior = NotificationDuplicateBehavior.UpdateExisting,
            ShowCloseButton = true,
            ShowCountdownBar = false,
        });

        await handle.UpdateAsync("updated");
        await handle.CloseAsync();
        _ = await handle.Completion;
        _ = await manager.ShowOverlayAsync("shortcut");

        var area = new Notifications.Controls.NotificationArea();
        _ = area.Show(
            "direct area API",
            TimeSpan.FromSeconds(1),
            new NotificationDisplayOptions
            {
                ShowCloseButton = false,
                ShowCountdownBar = false,
            });

        var notification = new Notifications.Controls.Notification();
        notification.ExpirationScheduled += (_, _) => { };
    }
}
'@

Set-Content -LiteralPath (Join-Path $workDirectory 'PackageApiSmoke.cs') -Value $source -Encoding utf8
dotnet build (Join-Path $workDirectory 'PackageSmoke.csproj') --configuration Release --no-restore -m:1 /nodeReuse:false
if ($LASTEXITCODE -ne 0) { throw 'The clean package consumer did not build.' }

Write-Host "Package smoke test passed: WpfNotifications $PackageVersion"
