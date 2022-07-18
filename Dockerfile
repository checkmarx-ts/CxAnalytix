FROM debian:stable-slim

ARG CHECKMARX_STATE_PATH=/var/cxanalytix

ENV CHECKMARX_STATE_PATH=$CHECKMARX_STATE_PATH

COPY ./ App/
WORKDIR /App

RUN apt update && apt install -y xmlstarlet && \
	mkdir -p /etc/cxanalytix && \
	mkdir -p /var/cxanalytix && \
	mkdir -p /var/log/cxanalytix && \
	mv cxanalytix.config /etc/cxanalytix && \
	mv cxanalytix.log4net /etc/cxanalytix && \
	ln -s /etc/cxanalytix/cxanalytix.log4net cxanalytix.log4net && \
	xmlstarlet ed -d "//root/appender-ref[@ref='RollingFile']" /etc/cxanalytix/cxanalytix.log4net > /etc/cxanalytix/logtmp.xml && \
	rm -f /etc/cxanalytix/cxanalytix.log4net && \
	mv /etc/cxanalytix/logtmp.xml /etc/cxanalytix/cxanalytix.log4net && \
	for v in $(xmlstarlet sel -T -t -v "//appender/file[contains(@value, 'logs')]/@value" /etc/cxanalytix/cxanalytix.log4net); do export newv=$(echo $v | sed "s/logs\//\/var\/logs\/cxanalytix\//g"); sed -i "s.$v.$newv.g" /etc/cxanalytix/cxanalytix.log4net; done && \
	apt remove -y xmlstarlet && \
	apt clean

	
ENTRYPOINT ["CxAnalytixDaemon"]