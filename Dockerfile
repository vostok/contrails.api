FROM mono:5.8.0.108 AS build-env

RUN apt-get update
RUN apt-get --yes install curl libunwind8 gettext apt-transport-https unzip git-core
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
RUN mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
RUN sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-jessie-prod jessie main" > /etc/apt/sources.list.d/dotnetdev.list'
RUN apt-get update
RUN apt-get --yes install dotnet-sdk-2.0.0 

WORKDIR /
RUN curl https://github.com/skbkontur/cement/releases/download/v1.0.22/e6257f9699a456f4d1626424ab90d2cb27337188.zip -L > cement.zip
RUN mkdir ./cement
RUN unzip -o cement.zip -d ./cement
RUN mono ../cement/dotnet/cm.exe init
RUN curl https://raw.githubusercontent.com/vostok/cement-modules/master/settings -L > ~/.cement/settings

WORKDIR /app
COPY . ./
RUN mono ../cement/dotnet/cm.exe update-deps
RUN mono ../cement/dotnet/cm.exe build-deps -v
RUN mono ../cement/dotnet/cm.exe build -v

RUN dotnet publish -c Release -o out ./Vostok.Contrails.Api
COPY ./Vostok.Contrails.Api/appsettings.json /app/appsettings.json


# build runtime image
FROM microsoft/aspnetcore:2.0-jessie

WORKDIR /app
COPY --from=build-env /app ./

ENTRYPOINT ["dotnet", "./Vostok.Contrails.Api/out/Vostok.Contrails.Api.dll"]
