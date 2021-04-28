param(
[String]$OutLoc="artifacts",
[String]$BuildConfig="Release",
[String]$Version="",
[String]$Build="",
[Switch]$Docker,
[Switch]$Clean
)

$dt = Get-Date

if ($Version -eq "")
{
	$Version = '{0:yyyy.MM.dd}' -f $dt

}

if ($Build -eq "")
{
	$Build = '{0:Hms}' -f $dt
}

$dir = Split-Path $OutLoc

if ($dir -eq "" )
{
	$OutLoc = (Get-Location).Path + "\" + $OutLoc
}

$OutLoc = $OutLoc + "\" + $BuildConfig

if ($Clean -AND (Test-Path $OutLoc) )
{
	Remove-Item -path $OutLoc -recurse
	dotnet clean .\CxAnalytix.sln
}

Write-Host "Output Directory: $OutLoc"


if ((-NOT $Docker) -AND (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) )
{
	Write-Host "Building [$BuildConfig] using .Net Core"
	dotnet publish .\CxAnalytix.sln -p:VersionPrefix=${Version} --version-suffix $Build -o $OutLoc -c $BuildConfig --no-self-contained
}
elseif (Get-Command "docker.exe" -ErrorAction SilentlyContinue)
{
	Write-Host "Building [$BuildConfig] using .Net Core on Docker"
	if (-NOT (Test-Path $OutLoc) )
	{
		New-Item -Path $OutLoc -Type "dir" | Out-Null
	}
	
	docker pull mcr.microsoft.com/dotnet/core/sdk
	docker run --mount type=bind,src=${OutLoc},target=/artifacts --mount type=bind,src=${pwd},target=/mnt -it mcr.microsoft.com/dotnet/core/sdk bash -c "cd /mnt && dotnet test"
	docker run --mount type=bind,src=${OutLoc},target=/artifacts --mount type=bind,src=${pwd},target=/mnt -it mcr.microsoft.com/dotnet/core/sdk bash -c "cd /mnt && dotnet publish CxAnalytix.sln -p:VersionPrefix=${Version} --version-suffix ${Build} -o /artifacts -c ${BuildConfig} --no-self-contained"
	
	

}
else
{
  Write-Error ".Net Core or Docker Desktop is not installed, can not build."
}