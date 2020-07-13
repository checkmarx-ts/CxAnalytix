FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY ./ App/
WORKDIR /App
RUN ln -s dotnet.exe.config dotnet.config
ENTRYPOINT ["dotnet", "CxAnalytixDaemon.dll"]