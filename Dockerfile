FROM mcr.microsoft.com/dotnet/core/sdk:3.0-bionic

RUN mkdir /artifacts/
WORKDIR /opt
COPY . .
RUN ls -ls
WORKDIR /opt

RUN sed -i 's/\\/\//g' Neo.Performance.Primitives.sln
RUN dotnet test -c Release -v n
RUN dotnet publish -c Release

WORKDIR /opt/Neo.Performance.Primitives/bin/Release
RUN cp *.nupkg /artifacts/