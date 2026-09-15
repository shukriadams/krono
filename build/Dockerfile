FROM ubuntu:26.04

RUN apt update \
    && apt install ssmtp -y \
    && useradd -u 1001 krono \
    && mkdir -p /opt/krono \
    && mkdir -p /var/log/krono \
    && chown -R krono /opt/krono \
    && chown -R krono /var/log/krono 

ENV PATH="$PATH:/opt/krono"

COPY ./publish/Krono /opt/krono/krono

RUN chmod +x /opt/krono/krono

USER krono

WORKDIR /opt/krono

CMD sh -c 'krono --daemon --settings /opt/krono/settings.yml'