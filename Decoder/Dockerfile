FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /Sln

COPY . .
RUN dotnet restore

#######################################################
# RUN dotnet publish -c Release -r linux-x64 -o out /p:ShowLinkerSizeComparison=true
WORKDIR /Sln/Decoder
# RUN dotnet add package ILLink.Tasks -v 0.1.5-preview-1461378 -s https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
# RUN dotnet publish -c Release -r linux-x64 -o out /p:LinkDuringPublish=true /p:ShowLinkerSizeComparison=true
RUN dotnet publish -c Release -r linux-x64 -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0 AS runtime

WORKDIR /App

COPY --from=build-env /Sln/Decoder/out ./

ENTRYPOINT ["/App/Decoder"]
