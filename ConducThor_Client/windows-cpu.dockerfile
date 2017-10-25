FROM chemsorly/keras-cntk:latest-windows-py2-cpu
SHELL ["powershell"]

ENV CONDUCTHOR_VERSION="dev"
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
COPY '.\ConducThor_Client\bin\Debug\netcoreapp1.1\publish\' 'C:\\app\\' 
WORKDIR 'C:\\app\\'

ENTRYPOINT dotnet .\ConducThor_Client.dll $Env:CONDUCTHOR_HOST