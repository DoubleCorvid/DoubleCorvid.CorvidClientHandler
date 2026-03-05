# Corvid Client Handler
An easily extensible wrapper for .NET's Http client.

## Description
Corvid Client Handler is a wrapper extension for .NET's Http client that hides the boiler-plate needed to setup http clients. It is designed to be extensible, with the default implementations being easily inherited and overridden. Dependencies are lite, currently only depending on `Microsoft.Extensions.Http`.

## Current Features
* Configurable rate limiting
* Request retry with exponential back-off

## Planned Features
* Logging hooks

## Dependecies
* Microsoft.Extensions.Http, 9.0.7 or greater

## Installation
Install the package via your prefered package manager, then reference as you would any other package.

### Nuget Package Manager
Search for and install the package as you would normally.

### Dotnet CLI
Release builds (currently unavaible) can be installed with the following command:

```sh
dotnet add pacakge DoubleCorvid.CorvidClientHandler DoubleCorvid.CorvidClientHandler.Framework
```

Pre-release builds can be installed with the `--prerelease` option:

```sh
dotnet add pacakge --prerelease DoubleCorvid.CorvidClientHandler DoubleCorvid.CorvidClientHandler.Framework
```

## Usage
Default usage is as follows:
```cs
```
## Authors
* DoubleCorvid
#### Copyright © 2026 DoubleCorvid