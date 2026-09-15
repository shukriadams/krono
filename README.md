# Krono

A simple cron-like runner that works in a Docker container with zero interaction. Available as a single drop-and-run binary with no dependencies, as well as in a Docker container. 

Why? It is unnecessarily complicated to run `cron` in a container. Krono lets you run cronjobs on a system, using a static config file, without having to change the host machine's cron state.

## Install

Download a binary for your system from the releases page, make it executable and run it directly.

To start krono in daemon mode create a config file and run

    krono --daemon --settings /path/to/config.yml 

## Docker

Krono is made to be run in a docker container. You can build it into your own image (see the project's own [Dockerfile](./build/Dockerfile) to see how to set it up), or run the project's own image(`shukriadams/krono`), here's an example compose 

    services:
    kronos:
        image: shukriadams/krono:0.0.1
        volumes:
        - ./settings.yml:/opt/krono/settings.yml
        - ./logs:/var/log/krono:rw

Note that we're passing in a settings yml file, and a directory for Krono to write its logs to. Make sure that the logs directory is owned by user 1001 
    
    chown -R 1001 ./logs

## Config

Jobs are configured in a single Yaml file with the following basic format

    Jobs:
    -   Name: Job 1
        Mask: "* * * * *"
        Command: echo "some command here"
    -   Name: List disk root every 10 minutes
        Mask: "*/10 * * * *"
        Command: ls / -lh

See [config.md](./docs/CONFIG.md) for more detailed documentation.

## License

GPL3 (see license file for more information)