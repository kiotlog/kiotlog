FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /Sln

COPY . .
RUN dotnet restore

# copy everything else and build
# COPY src ./src
# RUN dotnet publish -c Release -r linux-x64 -o out /p:ShowLinkerSizeComparison=true
WORKDIR /Sln/HttpReceiver
RUN dotnet publish -c Release -r linux-x64 -o out /p:LinkDuringPublish=false /p:ShowLinkerSizeComparison=true

# build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.0 AS runtime

WORKDIR /App

COPY --from=build-env /Sln/HttpReceiver/out ./

ENTRYPOINT ["/App/HttpReceiver" ]
