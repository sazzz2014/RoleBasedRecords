[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $repoRoot ".env"

function New-RandomBytes {
    param([Parameter(Mandatory = $true)][int]$Length)

    $bytes = New-Object byte[] $Length
    $randomNumberGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $randomNumberGenerator.GetBytes($bytes)
    }
    finally {
        $randomNumberGenerator.Dispose()
    }

    return ,$bytes
}

function New-RandomHex {
    param([Parameter(Mandatory = $true)][int]$ByteLength)

    return -join ((New-RandomBytes -Length $ByteLength) | ForEach-Object { $_.ToString("x2") })
}

function New-LocalEnvironment {
    $databasePassword = "rbr_db_$(New-RandomHex -ByteLength 16)"
    $adminPassword = "Rbr_Admin_$(New-RandomHex -ByteLength 16)"
    $userPassword = "Rbr_User_$(New-RandomHex -ByteLength 16)"
    $jwtSigningKey = [Convert]::ToBase64String((New-RandomBytes -Length 32))

    $lines = @(
        "POSTGRES_DB=role_based_records"
        "POSTGRES_USER=role_based_records"
        "POSTGRES_PASSWORD=$databasePassword"
        "API_PORT=5279"
        "Jwt__SigningKeyBase64=$jwtSigningKey"
        "Seed__AdminEmail=admin@example.com"
        "Seed__AdminPassword=$adminPassword"
        "Seed__UserEmail=user@example.com"
        "Seed__UserPassword=$userPassword"
    )

    Set-Content -LiteralPath $envFile -Value $lines -Encoding ASCII
    Write-Host "Created .env with random local secrets." -ForegroundColor Green
}

function Import-LocalEnvironment {
    foreach ($line in Get-Content -LiteralPath $envFile -Encoding UTF8) {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0 -or $trimmed.StartsWith("#")) {
            continue
        }

        $separator = $trimmed.IndexOf("=")
        if ($separator -le 0) {
            throw "Invalid line in .env: $trimmed"
        }

        $name = $trimmed.Substring(0, $separator).Trim()
        $value = $trimmed.Substring($separator + 1).Trim()
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }

    if ([string]::IsNullOrWhiteSpace($env:API_PORT)) {
        [Environment]::SetEnvironmentVariable("API_PORT", "5279", "Process")
    }

    $requiredVariables = @(
        "POSTGRES_DB"
        "POSTGRES_USER"
        "POSTGRES_PASSWORD"
        "API_PORT"
        "Jwt__SigningKeyBase64"
        "Seed__AdminEmail"
        "Seed__AdminPassword"
        "Seed__UserEmail"
        "Seed__UserPassword"
    )

    foreach ($name in $requiredVariables) {
        $value = [Environment]::GetEnvironmentVariable($name, "Process")
        if ([string]::IsNullOrWhiteSpace($value) -or $value.Contains("replace-with")) {
            throw "The .env value '$name' is missing or contains a placeholder. Delete .env to regenerate it."
        }
    }

    $apiPort = 0
    if (-not [int]::TryParse($env:API_PORT, [ref]$apiPort) -or $apiPort -lt 1 -or $apiPort -gt 65535) {
        throw "The .env value 'API_PORT' must be a number between 1 and 65535."
    }
}

function Assert-CommandSucceeded {
    param([Parameter(Mandatory = $true)][string]$Message)

    if ($LASTEXITCODE -ne 0) {
        throw $Message
    }
}

function Sync-DatabasePassword {
    $escapedUser = $env:POSTGRES_USER.Replace('"', '""')
    $escapedPassword = $env:POSTGRES_PASSWORD.Replace("'", "''")
    $sql = "ALTER ROLE `"$escapedUser`" WITH PASSWORD '$escapedPassword';"

    docker compose exec --no-TTY postgres psql `
        --username $env:POSTGRES_USER `
        --dbname $env:POSTGRES_DB `
        --command $sql | Out-Null
    Assert-CommandSucceeded -Message "The PostgreSQL password could not be synchronized with .env."
}

Push-Location $repoRoot
try {
    Write-Host "RoleBasedRecords local startup" -ForegroundColor Cyan

    if ($null -eq (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "Docker was not found. Install and start Docker Desktop."
    }

    docker compose version | Out-Null
    Assert-CommandSucceeded -Message "Docker Compose is unavailable. Start or update Docker Desktop."
    docker info | Out-Null
    Assert-CommandSucceeded -Message "Docker Engine is unavailable. Start Docker Desktop."

    if (-not (Test-Path -LiteralPath $envFile)) {
        New-LocalEnvironment
    }
    Import-LocalEnvironment

    docker compose config --quiet
    Assert-CommandSucceeded -Message "Docker Compose configuration is invalid."

    Write-Host "Starting PostgreSQL..." -ForegroundColor Cyan
    docker compose up --detach --wait postgres
    Assert-CommandSucceeded -Message "PostgreSQL could not start."
    Sync-DatabasePassword

    Write-Host "Building and starting the service. The first launch may take several minutes..." -ForegroundColor Cyan
    docker compose up --detach --build --wait --wait-timeout 240
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Container logs:" -ForegroundColor Yellow
        docker compose logs --tail 100
        throw "The service could not start. Ensure port $env:API_PORT is free."
    }

    $swaggerUrl = "http://localhost:$env:API_PORT/swagger"
    Write-Host ""
    Write-Host "Service started successfully." -ForegroundColor Green
    Write-Host "Swagger:        $swaggerUrl" -ForegroundColor Green
    Write-Host "Admin:          $env:Seed__AdminEmail"
    Write-Host "Admin password: $env:Seed__AdminPassword"
    Write-Host "User:           $env:Seed__UserEmail"
    Write-Host "User password:  $env:Seed__UserPassword"
    Write-Host ""
    Write-Host "You can close this window. Use STOP.cmd to stop the service." -ForegroundColor Yellow

    try {
        Start-Process $swaggerUrl
    }
    catch {
        Write-Host "The browser could not be opened automatically. Open the Swagger URL above." -ForegroundColor Yellow
    }
}
catch {
    Write-Host ""
    Write-Host "Startup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
