# DhanAlgoTrading

This repository contains an ASP.NET application targeting **.NET 8.0**.

## Development environment setup

Run the provided `setup.sh` script to install the required .NET SDK. The script
is intended for Debian/Ubuntu environments and requires `sudo` privileges.

```bash
./setup.sh
```

The script checks for an existing .NET 8 installation and installs it from the
Microsoft package repository when missing.

## Building the project

After installing the SDK, restore dependencies and build the solution:

```bash
dotnet restore

dotnet build
```

