FROM ubuntu:xenial

ENV DEBIAN_FRONTEND noninteractive
ARG MONO_VERSION=5.20
ARG MONO_URL=stable-xenial/snapshots/$MONO_VERSION

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb http://download.mono-project.com/repo/debian $MONO_URL main" > /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 2009837CBFFD68F45BC180471F4F90DE2A9B4BF8 && \
    echo "deb http://apt.sonarr.tv/ubuntu xenial main" > /etc/apt/sources.list.d/sonarr.list && \
    apt-get update && apt-get install -y \ 
        tofrodos tzdata \
        sonarr \
        sqlite3 mediainfo \
        && rm -rf /var/lib/apt/lists/*

RUN apt-get update && apt-get install -y \ 
        libmono-system-runtime4.0-cil \
        libmono-system-net-http4.0-cil \
        && rm -rf /var/lib/apt/lists/*

COPY startup.sh /startup.sh
RUN  fromdos /startup.sh

WORKDIR /data/
VOLUME ["/data/_tests_linux", "/data/_output_linux", "/data/_tests_results"]

RUN groupadd sonarrtst -g 4020 && useradd sonarrtst -u 4021 -g 4020 -m -s /bin/bash
USER sonarrtst

CMD bash /startup.sh

