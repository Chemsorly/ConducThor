FROM microsoft/dotnet:1.1.4-sdk-jessie as builder
SHELL ["/bin/bash", "-c"]

COPY . '/root/build'
WORKDIR '/root/build'
RUN dotnet restore
RUN dotnet publish 'ConducThor_Client/ConducThor_Client.csproj'

FROM chemsorly/keras-cntk:1.0.1-ubuntu-py2-cpu
SHELL ["/bin/bash", "-c"]

ARG CONDUCTHOR_VERSION
ENV CONDUCTHOR_VERSION ${CONDUCTHOR_VERSION}
ENV CONDUCTHOR_OS ubuntu
ENV CONDUCTHOR_TYPE cpu
ENV CONDUCTHOR_HOST ""

# Install .NET Core
RUN apt-get update && apt-get -y install apt-transport-https curl
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
RUN mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
RUN sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-trusty-prod trusty main" > /etc/apt/sources.list.d/dotnetdev.list'
RUN apt-get update && apt-get -y install dotnet-dev-1.1.4

# run app
COPY --from=builder 'root/build/ConducThor_Client/bin/Debug/netcoreapp1.1/publish/' '/root/app'
WORKDIR '/root/app'

ENTRYPOINT dotnet ConducThor_Client.dll $CONDUCTHOR_HOST