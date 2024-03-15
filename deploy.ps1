$IP_DEPLOY = $args[0]
$Origin = $args[1]
$USERNAME_DEPLOY = $args[2]
$PASSWORD_DEPLOY = $args[3]
$DIRECTORYDESTINY = $args[4]
$AppPoolName = $args[5]

write-host "There are a total of $($args.count) arguments"

for ($i = 0; $i -lt $args.Length; $i++)
{
    # Output the current item
    Write-Host $args[$i]
}


Get-ChildItem -Path $Origin
Write-Output "ServicePath: " + $ServicePath
Write-Output "ServicePath: " + $ServiceName
Write-Output "Destino:" + $Destination


Set-Item wsman:\localhost\client\TrustedHosts -Value $IP_DEPLOY -Force
get-Item WSMan:\localhost\Client\TrustedHosts

$User = "$($IP_DEPLOY)\$($USERNAME_DEPLOY)"
Write-Output "User:" + $User
$PWord = ConvertTo-SecureString -String $PASSWORD_DEPLOY -AsPlainText -Force
$Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User, $PWord
$Session = New-PSSession -ComputerName $IP_DEPLOY -Credential $Credential


$Destination = "$DIRECTORYDESTINY"
Write-Output "Destination:" + $Destination

Test-NetConnection $IP_DEPLOY -Port 5985

# Verificar que la carpeta de origen exista
if (!(Test-Path $Origin -PathType Container)) {
    Write-Host "La carpeta de origen '$Origin' no existe."
    exit
}

# Obtener el contenido de la carpeta local Ws_OLS
$resultQueryOrigin = Get-ChildItem -Path $Origin -ErrorAction SilentlyContinue

Write-Output $resultQueryOrigin

# Detener el sitio web en el servidor remoto
Invoke-Command -Session $Session -ScriptBlock {
    param($AppPoolName)
    Import-Module WebAdministration
    Stop-Website -Name "$AppPoolName"
} -ArgumentList $AppPoolName

Write-Host "Sitio detenido..."
# Verificar la existencia de la carpeta de destino en el servidor remoto
$resultQueryDestiny = Invoke-Command -Session $Session -ScriptBlock {
    param($Destination)
    Get-ChildItem -Path $Destination -ErrorAction SilentlyContinue
} -ArgumentList $Destination

if ($resultQueryDestiny -eq $null) {
    Write-Host "La carpeta de destino '$Destination' no existe en el servidor remoto."
}

# Eliminar todos los archivos y carpetas dentro del destino, excluyendo 'aspnet_client'
Invoke-Command -Session $Session -ScriptBlock {
    param($Destination)
    Get-ChildItem -Path $Destination | Where-Object { $_.Name -ne 'aspnet_client' } | Remove-Item -Recurse -Force

    # Crear la carpeta de destino nuevamente si es necesario
    if (!(Test-Path $Destination)) {
        New-Item -Path $Destination -ItemType Directory -Force
    }
} -ArgumentList $Destination

Write-Host "Eliminados archivos anteriores"
# Copiar el contenido de la carpeta local Ws_OLS en el servidor remoto
Copy-Item -Path "$Origin\*" -Destination $Destination -ToSession $Session -Recurse -Force
Write-Host "Se copian archivos a ruta destino"
# Iniciar el sitio web en el servidor remoto

Invoke-Command -Session $Session -ScriptBlock {
    param($AppPoolName)
    Import-Module WebAdministration
    Start-Website -Name "$AppPoolName"
} -ArgumentList $AppPoolName

Write-Host "Se inicia el IIS"