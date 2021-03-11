FROM mcr.microsoft.com/dotnet/sdk:5.0.201-nanoserver-20H2 AS publish

WORKDIR /source
COPY *.fsproj .
# Copy csproj and restore as distinct layers
RUN dotnet restore

COPY * .
RUN dotnet publish -c Release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0.4-nanoserver-20H2 as runtime

ENV servers ""
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "WebApi"]