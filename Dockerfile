FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
ARG CHECKMARX_STATE_PATH=/var/cxanalytix
COPY ./ App/
WORKDIR /App

RUN  mkdir -p /etc/cxanalytix &&
	mkdir -p /var/cxanalytix &&
	mv dotnet.config /etc/cxanalytix &&
	ln -s /etc/cxanalytix/dotnet.config dotnet.config
	
ENTRYPOINT ["dotnet", "CxAnalytixDaemon.dll"]