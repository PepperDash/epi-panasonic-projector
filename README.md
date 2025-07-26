# Panasonic Projector Essentials Plugin (c) 2021

## Overview

This plugin is designed to work with Panasonic projectors controlled via TCP/IP or RS-232. For config information, see the [config snippets](##Configuration)

## Configuration

### RS-232

```json
{
  "key": "projector-1",
  "type": "panasonicProjector",
  "group": "display",
  "name": "Projector",
  "properties": {
    "control": {
      "method": "com",
      "comParams": {
        "protocol": "RS232",
        "baudRate": 9600,
        "dataBits": 8,
        "stopBits": 1,
        "parity": "None",
        "softwareHandshake": "None",
        "hardwareHandshake": "None",
        "pacing": 0
      },
      "controlPortDevKey": "processor",
      "controlPortNumber": 1
    },
    "id": "01",
    "warmupTimeInSeconds":1,
    "cooldownTimeInSeconds":2
  }
}
```

#### Notes

- Valid `id` values are ZZ, 01 to 64, and 0A to 0Z
- `warmupTimeInSeconds` and `cooldownTimeInSeconds` are optional. If they are omitted, the plugin will default to 1 second and 2 seconds respectively.

### TCP/IP

```json
{
  "key": "projector-1",
  "type": "panasonicProjector",
  "group": "display",
  "name": "Projector",
  "properties": {
    "control": {
      "method": "tcpIp",
      "tcpSshProperties": {
        "port": 22,
        "address": "0.0.0.0",
        "username": "",
        "password": "",
        "autoReconnect": true,
        "autoReconnectIntervalMs": 5000,
        "bufferSize": 32768
      }
    }
  }
}
```

#### Notes

- The `username` and `password` are required only if the projector has security enabled

## License

Provided under [MIT License](LICENSE.md)

# Contributing

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They are referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

To install dependencies once nuget.exe is installed, run the following command from the root directory of your repository:
`nuget install .\packages.config -OutputDirectory .\packages -excludeVersion`.
To verify that the packages installed correctly, open the plugin solution in your repo and make sure that all references are found, then try and build it.

### Installing Different versions of PepperDash Core

If you need a different version of PepperDash Core, use the command `nuget install .\packages.config -OutputDirectory .\packages -excludeVersion -Version {versionToGet}`. Omitting the `-Version` option will pull the version indicated in the packages.config file.
