# Krono

A simple cron-like runner that can run in a Docker container. Does not require interactive crontab interaction. Single binary and easy
to install.

## Install

Download a binary for your system from the releases page, make it executable and run it directly. Krono
has no dependencies and is fully portable.

You can also run Krono directly as a Docker container.

## Run

To start krono in daemon mode run

    krono --settings /path/to/config.yml --daemon

You can also use the `KRONO_SETTINGS_PATH` environment variable to pass in the settings path.

## Config

Jobs are configured in a single Yaml file with the following basic format

    Jobs:
    -   Name: Job 1
        Mask: "* * * * *"
        Command: echo "some command here"
    -   Name: List disk root every 10 minutes
        Mask: "*/10 * * * *"
        Command: ls / -lh

### Environment variables

Krono supports whatever environment variables you pass to it or the container it runs in. Simply reference
these in `Command: <your calls here> $SOME_ENV_VAR` as you would any shell command.

### Email alerts

Krono supports sending email via sendmail. Sendmail should be pre-configured and working from the command line. You can test email sending directly by running

    krono --mailtest --receiver receiver@example.com

## License

GPL3 (see license file for more information)