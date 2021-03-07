FROM ubuntu:focal AS builder

ENV DEBIAN_FRONTEND noninteractive 
ENV MONO_VERSION 5.18

RUN apt-get update && \
    apt-get -y -o Dpkg::Options::="--force-confold" install --no-install-recommends \
        apt-transport-https \
        wget dirmngr gpg gpg-agent \
        # add-apt-repository for PPAs
        software-properties-common && \
    rm -rf /var/lib/apt/lists/*
	
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb http://download.mono-project.com/repo/debian stable-focal main" > /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && apt-get install -y \ 
        devscripts build-essential tofrodos \
        dh-make dh-systemd \
        cli-common-dev \
        mono-complete \
        sqlite3 libcurl4 mediainfo
RUN apt-get upgrade -y

RUN apt-cache policy mono-complete
RUN apt-cache policy cli-common-dev

COPY debian-start.sh /debian-start.sh
RUN  fromdos /debian-start.sh

WORKDIR /data
VOLUME [ "/data/sonarr_bin", "/data/build", "/data/output" ]
CMD /debian-start.sh
