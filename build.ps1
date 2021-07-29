param(
[String]$OutLoc="artifacts",
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

$OutLoc = $OutLoc + "\"

if ($Clean -AND (Test-Path $OutLoc) )
{
	Remove-Item -path $OutLoc -recurse
	dotnet clean .\CxAnalytix.sln
}

Write-Host "Output Directory: $OutLoc"


if ((-NOT $Docker) -AND (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) )
{
	Write-Host "Building using .Net Core on the local machine"
	dotnet publish .\CxAnalytix.sln -p:VersionPrefix=${Version} --version-suffix $Build -o $OutLoc/win-x64 -c ReleaseWindows -r win-x64
	dotnet publish .\CxAnalytix.sln -p:VersionPrefix=${Version} --version-suffix $Build -o $OutLoc/linux-x64 -c ReleaseLinux -r linux-x64
}
elseif (Get-Command "docker.exe" -ErrorAction SilentlyContinue)
{
	Write-Host "Building using .Net Core on Docker"
	if (-NOT (Test-Path $OutLoc) )
	{
		New-Item -Path $OutLoc -Type "dir" | Out-Null
	}
	
	docker pull mcr.microsoft.com/dotnet/sdk:3.1
	docker run --mount type=bind,src=${OutLoc},target=/artifacts --mount type=bind,src=${pwd},target=/mnt -it mcr.microsoft.com/dotnet/sdk:3.1 bash -c "cd /mnt && dotnet test"
	docker run --mount type=bind,src=${OutLoc},target=/artifacts --mount type=bind,src=${pwd},target=/mnt -it mcr.microsoft.com/dotnet/sdk:3.1 bash -c "cd /mnt && dotnet publish CxAnalytix.sln -p:VersionPrefix=${Version} --version-suffix ${Build} -o /artifacts/win-x64 -c ReleaseWindows -r win-x64"
	docker run --mount type=bind,src=${OutLoc},target=/artifacts --mount type=bind,src=${pwd},target=/mnt -it mcr.microsoft.com/dotnet/sdk:3.1 bash -c "cd /mnt && dotnet publish CxAnalytix.sln -p:VersionPrefix=${Version} --version-suffix ${Build} -o /artifacts/linux-x64 -c ReleaseLinux -r linux-x64"
	
	

}
else
{
  Write-Error ".Net Core or Docker Desktop is not installed, can not build."
}