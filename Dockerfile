FROM vstk/cement:latest AS build-env

WORKDIR /app
COPY . ./
RUN mono ../cement/dotnet/cm.exe update-deps
RUN mono ../cement/dotnet/cm.exe build-deps -v
RUN mono ../cement/dotnet/cm.exe build -v

RUN dotnet publish -c Release -o out ./Vstk.Contrails.Api
COPY ./Vstk.Contrails.Api/appsettings.json /app/appsettings.json


# build runtime image
FROM microsoft/aspnetcore:2.0-jessie

WORKDIR /app
COPY --from=build-env /app ./

ENTRYPOINT ["dotnet", "./Vstk.Contrails.Api/out/Vstk.Contrails.Api.dll"]
