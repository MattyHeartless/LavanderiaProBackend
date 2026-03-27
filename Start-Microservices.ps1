[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('All', 'Auth', 'Profile', 'Catalogs', 'Orders')]
    [string[]]$Service = @('All'),

    [switch]$InCurrentWindow
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$environmentName = 'Development'

function Quote-WtValue {
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    '"' + ($Value.Replace('"', '\"')) + '"'
}

function ConvertTo-PwshEncodedCommand {
    param(
        [Parameter(Mandatory)]
        [string]$Script
    )

    [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($Script))
}

$rootPath = Split-Path -Parent $PSCommandPath
$windowsTerminal = Get-Command wt -ErrorAction SilentlyContinue
$servicePaths = [ordered]@{
    Auth     = Join-Path $rootPath 'services\Auth\Auth.API'
    Profile  = Join-Path $rootPath 'services\Profile\Profile.API'
    Catalogs = Join-Path $rootPath 'services\Catalogs\Catalogs.API'
    Orders   = Join-Path $rootPath 'services\Orders\Orders.API'
}

[string[]]$selectedServices = if ($Service -contains 'All') {
    $servicePaths.Keys
}
else {
    $Service | Select-Object -Unique
}

if ($InCurrentWindow -and $selectedServices.Count -ne 1) {
    throw 'Usa -InCurrentWindow con un solo servicio.'
}

foreach ($serviceName in $selectedServices) {
    $servicePath = $servicePaths[$serviceName]

    if (-not (Test-Path -LiteralPath $servicePath)) {
        throw "No se encontro la ruta del servicio '$serviceName': $servicePath"
    }
}

if ($InCurrentWindow) {
    $serviceName = $selectedServices[0]
    $servicePath = $servicePaths[$serviceName]

    if ($PSCmdlet.ShouldProcess($serviceName, "Ejecutar dotnet run en $servicePath")) {
        Write-Host "Iniciando $serviceName en la ventana actual con entorno $environmentName..."
        Push-Location -LiteralPath $servicePath
        try {
            $env:ASPNETCORE_ENVIRONMENT = $environmentName
            $env:DOTNET_ENVIRONMENT = $environmentName
            dotnet run
        }
        finally {
            Pop-Location
        }
    }

    return
}

if ($null -ne $windowsTerminal) {
    $wtSegments = @()

    if ($env:WT_SESSION) {
        $wtSegments += '-w 0'
    }

    foreach ($serviceName in $selectedServices) {
        $servicePath = $servicePaths[$serviceName]
        $quotedTitle = Quote-WtValue -Value $serviceName
        $quotedPath = Quote-WtValue -Value $servicePath
        $escapedPath = $servicePath.Replace("'", "''")
        $pwshScript = "`$env:ASPNETCORE_ENVIRONMENT='$environmentName'; `$env:DOTNET_ENVIRONMENT='$environmentName'; Set-Location -LiteralPath '$escapedPath'; dotnet run"
        $encodedCommand = ConvertTo-PwshEncodedCommand -Script $pwshScript
        $wtSegments += "new-tab --title $quotedTitle -d $quotedPath pwsh -NoExit -EncodedCommand $encodedCommand"
    }

    if ($PSCmdlet.ShouldProcess(($selectedServices -join ', '), 'Abrir los servicios en pestañas de Windows Terminal')) {
        Write-Host "Iniciando servicios en pestañas de Windows Terminal..."
        Start-Process -FilePath $windowsTerminal.Source -ArgumentList ($wtSegments -join ' ; ') | Out-Null
    }

    return
}

foreach ($serviceName in $selectedServices) {
    $servicePath = $servicePaths[$serviceName]
    $escapedPath = $servicePath.Replace("'", "''")
    $command = "`$env:ASPNETCORE_ENVIRONMENT='$environmentName'; `$env:DOTNET_ENVIRONMENT='$environmentName'; Set-Location -LiteralPath '$escapedPath'; dotnet run"

    if ($PSCmdlet.ShouldProcess($serviceName, "Abrir una nueva ventana y ejecutar dotnet run en $servicePath")) {
        Write-Host "Iniciando $serviceName en una nueva ventana con entorno $environmentName..."
        Start-Process -FilePath 'pwsh' -ArgumentList @('-NoExit', '-Command', $command) -WorkingDirectory $servicePath | Out-Null
    }
}
