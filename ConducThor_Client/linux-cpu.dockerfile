FROM chemsorly/keras-cntk:latest-ubuntu-py2-cpu
SHELL ["/bin/bash", "-c"]

COPY . 'root/app'  
WORKDIR 'root/app'


ARG CONDUCTHOR_VERSION="dev"

ENTRYPOINT dotnet run