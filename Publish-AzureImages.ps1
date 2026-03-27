[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('All', 'Auth', 'Profile', 'Catalogs', 'Orders')]
    [string[]]$Service = @('All'),

    [string]$Registry = 'lavanderiapro.azurecr.io',

    [string]$ImageTag = 'latest'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$rootPath = Split-Path -Parent $PSCommandPath
$serviceConfig = [ordered]@{
    Auth = @{
        ImageName = 'auth-service'
        Path = Join-Path $rootPath 'services\Auth'
    }
    Profile = @{
        ImageName = 'profile-service'
        Path = Join-Path $rootPath 'services\Profile'
    }
    Catalogs = @{
        ImageName = 'catalogs-service'
        Path = Join-Path $rootPath 'services\Catalogs'
    }
    Orders = @{
        ImageName = 'orders-service'
        Path = Join-Path $rootPath 'services\Orders'
    }
}

[string[]]$selectedServices = if ($Service -contains 'All') {
    $serviceConfig.Keys
}
else {
    $Service | Select-Object -Unique
}

foreach ($serviceName in $selectedServices) {
    $servicePath = $serviceConfig[$serviceName].Path

    if (-not (Test-Path -LiteralPath $servicePath)) {
        throw "No se encontro la ruta del servicio '$serviceName': $servicePath"
    }
}

foreach ($serviceName in $selectedServices) {
    $config = $serviceConfig[$serviceName]
    $localImage = $config.ImageName
    $remoteImage = '{0}/{1}:{2}' -f $Registry, $config.ImageName, $ImageTag

    if (-not $PSCmdlet.ShouldProcess($serviceName, "docker build/tag/push -> $remoteImage")) {
        continue
    }

    Write-Host "Procesando $serviceName..."
    Push-Location -LiteralPath $config.Path
    try {
        Write-Host "  docker build -t $localImage ."
        docker build -t $localImage .
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo el build de $serviceName."
        }

        Write-Host "  docker tag $localImage $remoteImage"
        docker tag $localImage $remoteImage
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo el tag de $serviceName."
        }

        Write-Host "  docker push $remoteImage"
        docker push $remoteImage
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo el push de $serviceName."
        }
    }
    finally {
        Pop-Location
    }
}
