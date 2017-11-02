FROM chemsorly/keras-cntk:latest-windows-py2-cpu
SHELL ["powershell"]

ARG CONDUCTHOR_VERSION
ENV CONDUCTHOR_VERSION=${CONDUCTHOR_VERSION}
ENV CONDUCTHOR_OS="windows"
ENV CONDUCTHOR_TYPE="cpu"
ARG CONDUCTHOR_HOST=""

# Install .NET Core
ENV DOTNET_VERSION 2.0.0
ENV DOTNET_DOWNLOAD_URL https://download.microsoft.com/download/5/F/0/5F0362BD-7D0A-4A9D-9BF9-022C6B15B04D/dotnet-runtime-2.0.0-win-x64.zip

RUN Invoke-WebRequest $Env:DOTNET_DOWNLOAD_URL -OutFile dotnet.zip; \
    Expand-Archive dotnet.zip -DestinationPath $Env:ProgramFiles\dotnet; \
    Remove-Item -Force dotnet.zip

RUN setx /M PATH $($Env:PATH + ';' + $Env:ProgramFiles + '\dotnet')

# run app
COPY '.\ConducThor_Client\bin\Debug\netcoreapp2.0\publish\' 'C:\\app\\' 
WORKDIR 'C:\\app\\'

ENTRYPOINT dotnet .\ConducThor_Client.dll $Env:CONDUCTHOR_HOST