FROM microsoft/dotnet:2.0-sdk-jessie AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY nuget.config ./
COPY . ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out ./Vostok.Contrails.Api
COPY ./Vostok.Contrails.Api/appsettings.json /app/appsettings.json


# build runtime image
FROM microsoft/aspnetcore:2.0-jessie

WORKDIR /app
COPY --from=build-env /app ./

ENTRYPOINT ["dotnet", "./Vostok.Contrails.Api/out/Vostok.Contrails.Api.dll"]
