FROM microsoft/dotnet:1.1.4-sdk as builder
SHELL ["powershell"]

COPY . 'C:\\build\\'
WORKDIR 'C:\\build\\'
RUN dotnet restore
RUN dotnet publish .\ConducThor_Client\ConducThor_Client.csproj

FROM chemsorly/keras-cntk:1.0.1-windows-py2-cpu
SHELL ["powershell"]

ARG CONDUCTHOR_VERSION
ENV CONDUCTHOR_VERSION=${CONDUCTHOR_VERSION}
ENV CONDUCTHOR_OS="windows"
ENV CONDUCTHOR_TYPE="cpu"
ARG CONDUCTHOR_HOST=""

# Install .NET Core
ENV DOTNET_VERSION 1.1.4
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/release/1.1.0/Binaries/$DOTNET_VERSION/dotnet-win-x64.$DOTNET_VERSION.zip

RUN Invoke-WebRequest $Env:DOTNET_DOWNLOAD_URL -OutFile dotnet.zip; \
    Expand-Archive dotnet.zip -DestinationPath $Env:ProgramFiles\dotnet; \
    Remove-Item -Force dotnet.zip

RUN setx /M PATH $($Env:PATH + ';' + $Env:ProgramFiles + '\dotnet')

# run app
COPY --from=builder 'C:\build\ConducThor_Client\bin\Debug\netcoreapp1.1\publish\' 'C:\\app\\' 
WORKDIR 'C:\\app\\'

ENTRYPOINT dotnet .\ConducThor_Client.dll $Env:CONDUCTHOR_HOST