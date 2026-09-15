# Configuring Krono

## Run

To run jobs, Krono must be started in daemon mode, with a path a jobs settings file. Use

    krono --settings /path/to/config.yml --daemon

You can also use the `KRONO_SETTINGS_PATH` environment variable to pass in the settings path.

## Environment variables

Krono lets you use environment variables in the commands you call. Pass vars to the Krono binary or container, then use as normal in settings yml

    Command: bash -c "some_command $SOME_ENV_VAR" 

## Email alerts

Krono supports sending email via sendmail. Sendmail should be pre-configured and working from the command line. You can test email sending directly by running

    krono --mailtest --receiver receiver@example.com

## Advanced config

The full settings Yaml spec is 

    Enabled: <bool> Set this to false to disable all cron jobs. Requires Krono
                    restart. Note that Krono will exit on start if no jobs are defined or enabled.
                    
    SenderAddress: <string> "From" email address for all email alerts. 

    ReceiverAddress: <string> Email address to send all alerts to. This value
                              can be overriden individually by each job.

    EmailNotifications: <bool> Set to true to enable email notifications. 
                               Default is false.

    LogRoot: <string> Path Krono writes all logs to. 
                      Default value is "/var/log/krono"

    Jobs:
        Name: <string> Name of cronjob. Must be unique per jobb and filesystem safe. Required.

        Mask: <string> Cronmask to control timer for executing this job. Required. 
                       Note that the asterisk character * commonly used in cronmarks breaks yaml, 
                       so always wrap this string in double quotes.

        Command: <string> Shell command to execute. Required.
        
        Enabled: <bool> Set to false to stop job from running. Requires Krono restart. Default is true.

        Verbose: <bool> If true, an email alert is sent whenever job runs, regardless of success or failure. 
                        Default is false.
        
        ReceiverAddress: <string> Email address to send alerts to. If not set defaults to global 
                                  ReceiverAddress value. If empty, no alerts will be sent for this job. 
                                  If this and global ReceiverAddress set, this will take precedence. Optional.
