# NewRepo

This repository contains a .NET 8 Web API.

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/)

Running tests also requires the .NET 8 SDK. After installing the SDK, you can verify
the setup by executing:

```bash
dotnet test
```

You can run `setup.sh` to install the required SDK and add the Microsoft
package repository:

```bash
./setup.sh
```

`setup.sh` downloads packages from the internet, so it requires network access
and may fail in an offline environment.

### Manual installation

If you prefer to install the SDK manually, execute the following commands on an
Ubuntu system:

```bash
sudo apt-get update
sudo apt-get install -y wget apt-transport-https
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

## Build

Restore dependencies and build the solution:

```bash
dotnet restore
dotnet build
```

## Run the API

Start the web API from the project directory:

```bash
cd DhanAlgoTrading
dotnet run
```

The API will run using the configuration in `appsettings.json`.
