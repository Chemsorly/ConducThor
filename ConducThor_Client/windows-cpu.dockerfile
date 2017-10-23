FROM chemsorly/keras-cntk:latest-windows-py2-cpu
SHELL ["powershell"]

COPY . 'C:\\app\\'  
WORKDIR 'C:\\app\\'


ARG CONDUCTHOR_VERSION="dev"

ENTRYPOINT dotnet run