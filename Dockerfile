FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG CHECKMARX_STATE_PATH=/var/cxanalytix

ENV CHECKMARX_STATE_PATH=$CHECKMARX_STATE_PATH

COPY ./ App/
WORKDIR /App

RUN apt-get update && apt-get install -y xmlstarlet && \
	mkdir -p /etc/cxanalytix && \
	mkdir -p /var/cxanalytix && \
	mkdir -p /var/log/cxanalytix && \
	mv dotnet.config /etc/cxanalytix && \
	mv CxAnalytixDaemon.log4net /etc/cxanalytix && \
	ln -s /etc/cxanalytix/dotnet.config dotnet.config && \
	ln -s /etc/cxanalytix/CxAnalytixDaemon.log4net CxAnalytixDaemon.log4net && \
	xmlstarlet ed -d "//root/appender-ref[@ref='RollingFile']" /etc/cxanalytix/CxAnalytixDaemon.log4net > /etc/cxanalytix/logtmp.xml && \
	rm -f /etc/cxanalytix/CxAnalytixDaemon.log4net && \
	mv /etc/cxanalytix/logtmp.xml /etc/cxanalytix/CxAnalytixDaemon.log4net && \
	for v in $(xmlstarlet sel -T -t -v "//appender/file[contains(@value, 'logs')]/@value" /etc/cxanalytix/CxAnalytixDaemon.log4net); do export newv=$(echo $v | sed "s/logs\//\/var\/logs\/cxanalytix\//g"); sed -i "s.$v.$newv.g" /etc/cxanalytix/CxAnalytixDaemon.log4net; done && \
	apt-get clean


	
ENTRYPOINT ["dotnet", "CxAnalytixDaemon.dll"]