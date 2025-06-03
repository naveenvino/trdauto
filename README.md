# NewRepo

This repository contains a .NET application.

To install the .NET SDK required to build the project, run the setup script:

```bash
./setup.sh
```

## Configuration

The application reads Dhan API settings from configuration. The default `appsettings.json` file contains placeholder values. Provide real credentials either via environment variables or an `appsettings.Development.json` file which is not committed to source control.

Environment variable names follow the ASP.NET Core convention using `__` for nested settings. For example:

```bash
export DhanApiSettings__AccessToken="<your token>"
export DhanApiSettings__ClientId="<your client id>"
export DhanApiSettings__TradingViewWebhookPassphrase="<passphrase>"
```

Alternatively, create an `appsettings.Development.json` next to `appsettings.json` with your secrets. This file is loaded automatically when running in the `Development` environment.
