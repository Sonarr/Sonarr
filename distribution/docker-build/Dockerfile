FROM ubuntu:xenial AS builder

ENV DEBIAN_FRONTEND noninteractive 
ENV MONO_VERSION 5.14

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb http://download.mono-project.com/repo/debian stable-xenial/snapshots/$MONO_VERSION main" > /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && apt-get install -y \ 
        devscripts build-essential tofrodos \
        dh-make dh-systemd \
        cli-common-dev \
        mono-complete \
        sqlite3 libcurl3 mediainfo

RUN apt-cache policy mono-complete
RUN apt-cache policy cli-common-dev

COPY debian-start.sh /debian-start.sh
RUN  fromdos /debian-start.sh

WORKDIR /data
VOLUME [ "/data/sonarr_bin", "/data/build", "/data/output" ]
CMD /debian-start.sh
