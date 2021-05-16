FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /Sln

COPY . .
RUN dotnet restore

# RUN dotnet publish -c Release -r linux-x64 -o out /p:LinkDuringPublish=false /p:ShowLinkerSizeComparison=true
WORKDIR /Sln/KlsnReceiver
RUN dotnet publish -c Release -r linux-x64 -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0 AS runtime

WORKDIR /App

COPY --from=build-env /Sln/KlsnReceiver/out ./

ENTRYPOINT ["/App/KlsnReceiver"]
