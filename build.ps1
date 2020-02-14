param(
[String]$OutLoc="artifacts",
[String]$BuildConfig="Release",
[Switch]$Docker,
[Switch]$Clean
)

$dir = Split-Path $OutLoc

if ($dir -eq "" )
{
	$OutLoc = (Get-Location).Path + "\" + $OutLoc
}

$OutLoc = $OutLoc + "\" + $BuildConfig

if ($Clean -AND (Test-Path $OutLoc) )
{
	Remove-Item -path $OutLoc -recurse
}

Write-Host "Output Directory: $OutLoc"


if ((-NOT $Docker) -AND (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) )
{
	Write-Host "Building [$BuildConfig] using .Net Core"
	dotnet publish .\CxAnalytix.sln -o $OutLoc -c $BuildConfig
}
elseif (Get-Command "docker.exe" -ErrorAction SilentlyContinue)
{
	Write-Host "Building [$BuildConfig] using .Net Core on Docker"
	if (-NOT (Test-Path $OutLoc) )
	{
		New-Item -Path $OutLoc -Type "dir" | Out-Null
	}
	
	docker run --mount type=bind,src=${OutLoc},target=/artifacts --mount type=bind,src=${pwd},target=/mnt -it mcr.microsoft.com/dotnet/core/sdk bash -c "cd /mnt && dotnet publish CxAnalytix.sln -o /artifacts -c ${BuildConfig}"
	
	

}
else
{
  Write-Error ".Net Core or Docker Desktop is not installed, can not build."
}