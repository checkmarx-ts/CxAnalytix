FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY bin/Release/netcoreapp3.1/publish/ App/
WORKDIR /App
RUN ln -s dotnet.exe.config dotnet.config
ENTRYPOINT ["dotnet", "CxAnalytixDaemon.dll"]