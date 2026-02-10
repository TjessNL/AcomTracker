FROM sdk

WORKDIR /src
COPY .. /

RUN dotnet restore

ENTRYPOINT ["dotnet", "DotNet.Docker.dll"]