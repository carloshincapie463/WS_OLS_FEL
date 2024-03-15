$filePath = $args[0].ToString()
$Token = $args[1].ToString()
$EnvioJSON = $args[2].ToString()
$EnvioJSONAnulacion = $args[3].ToString()
$RevisaLinea = $args[4].ToString()
$InfoFactura = $args[5].ToString()
$Usuario = $args[6].ToString()
$PasswordHead = $args[7].ToString()
$UserName = $args[8].ToString()
$Password = $args[9].ToString()
$IdCompany = $args[10].ToString()
$Ambiente = $args[11].ToString()
$IPES = $args[12].ToString()
$IPES_Sala = $args[13].ToString()


write-host "There are a total of $($args.count) arguments"

for ($i = 0; $i -lt $args.Length; $i++)
{
    # Output the current item
    Write-Host $args[$i]
}

# Write-Output $filePath

Get-Content -Path $filePath

#Write-Output $urlApiColas
# file not found, nothing to do
if (-Not (Test-Path $filePath))
{
	Write-Output "File not found: $filePath"
	return
}

# read the config in xml
[System.Xml.XmlDocument]$doc = new-object System.Xml.XmlDocument
$doc.Load($filePath)

# find the connection string 
foreach($item in $doc.get_DocumentElement().appSettings.add)
{
	# use your name here
	Write-Output "$($item.key):$($item.value)"
	switch($item.key)
	{
		"Token" {$item.value = $Token}
		"EnvioJSON" {$item.value = $EnvioJSON}
		"EnvioJSONAnulacion" {$item.value = $EnvioJSONAnulacion}
		"RevisaLinea" {$item.value = $RevisaLinea}
		"InfoFactura" {$item.value = $InfoFactura}
		"Usuario" {$item.value = $Usuario}
		"PasswordHead" {$item.value = $PasswordHead}
		"userName" {$item.value = $UserName}
		"password" {$item.value = $Password}
		"idCompany" {$item.value = $IdCompany}
		"ambiente" {$item.value = $Ambiente}
		"IPES" {$item.value = $IPES}
		"IPES_Sala" {$item.value = $IPES_Sala}
	}        
}

# save the new file
$doc.Save($filePath)
