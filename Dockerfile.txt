FROM microsoft/aspnetcore-build:2 as publish

COPY . /src
WORKDIR /src/XmlCombiner.Web
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

FROM microsoft/aspnetcore:2

COPY --from=publish /publish /app
WORKDIR /app

ENTRYPOINT ["dotnet", "XmlCombiner.Web.dll"]