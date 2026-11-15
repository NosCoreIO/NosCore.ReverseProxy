<p align="center">
  <img width="200" alt="NosCore" src="https://raw.githubusercontent.com/NosCoreIO/NosCore.Packets/master/icon.png"/>
</p>

<h1 align="center">NosCore.ReverseProxy</h1>

<p align="center">
  <a href="https://github.com/NosCoreIO/NosCore.ReverseProxy/actions/workflows/dotnet.yml"><img alt="Build" src="https://github.com/NosCoreIO/NosCore.ReverseProxy/actions/workflows/dotnet.yml/badge.svg?branch=master"/></a>
  <a href="https://discord.gg/Eu3ETSw"><img alt="Discord" src="https://img.shields.io/badge/Discord-join-5865F2?style=flat-square&logo=discord&logoColor=white"/></a>
  <a href="LICENSE"><img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue?style=flat-square"/></a>
</p>

A TCP proxy to put in front of the NosCore login and world servers. Each configured channel listens on a local port
and forwards to a server; when the login server does not answer in time, the proxy replies with the client's own
maintenance message instead of leaving it hanging.

## Configuration

[configuration/reverse-proxy.yml](configuration/reverse-proxy.yml):

```yaml
Timeout: 5000
Language: en
Channels:
  - RemotePort: 4001
    LocalPort: 4000
    RemoteHost: 127.0.0.1
    ServerType: LoginServer
  - RemotePort: 4010
    LocalPort: 1337
    RemoteHost: 127.0.0.1
    ServerType: WorldServer
```

## How it works

<p align="center">
  <img alt="Packet flow: a packet is forwarded to the login server; on timeout a maintenance packet is sent back" src="documentation.png"/>
</p>

## Running

```bash
dotnet build
dotnet build/net11.0/NosCore.ReverseProxy.dll
```

or `docker compose up`, which mounts `configuration/` into the container.

## Building

```bash
dotnet build
dotnet test
```

## Contributing

Questions, ideas and pull requests are welcome; most of the discussion happens on [Discord](https://discord.gg/Eu3ETSw).
Any editor that handles .NET 11 works: `dotnet build` and `dotnet test` run on Windows, Linux and macOS.
In Visual Studio, [Roslynator](https://github.com/dotnet/roslynator) and
[ReSharper](https://www.jetbrains.com/resharper/) help keep to the code style.

## Supporting NosCore

NosCore is a spare-time open source project. Sponsoring buys time spent on the emulator and the packages around it,
and gives the **Supporter** role on Discord.

<p>
  <a href="https://github.com/sponsors/0Lucifer0"><img alt="Sponsor on GitHub" src="https://img.shields.io/badge/GitHub-sponsor-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white"/></a>
  <a href="https://ko-fi.com/A3562BQV"><img alt="Support on Ko-fi" src="https://img.shields.io/badge/Ko--fi-support-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white"/></a>
  <a href="https://www.patreon.com/bePatron?u=6503887"><img alt="Become a patron" src="https://img.shields.io/badge/Patreon-become_a_patron-F96854?style=for-the-badge&logo=patreon&logoColor=white"/></a>
</p>

Thanks to [Navicat](https://www.navicat.com) for supporting the project.

<a href="https://www.navicat.com"><img height="60" alt="Navicat" src="https://user-images.githubusercontent.com/35202750/207230064-dcf23adc-9e96-4481-9a53-cd212f5bd60e.png"/></a>

## Legal

NosCore is an independent, unofficial project for educational use. It is not affiliated with, authorized, maintained,
sponsored or endorsed by Gameforge or any of its affiliates; NosTale is a trademark of Gameforge. Using it may be
against the terms of service of NosTale servers.

Released under the [MIT License](LICENSE).
