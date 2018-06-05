FROM microsoft/dotnet:1.1.4-sdk-jessie as builder
SHELL ["/bin/bash", "-c"]

COPY . '/root/build'
WORKDIR '/root/build'
RUN dotnet restore
RUN dotnet publish 'ConducThor_Client/ConducThor_Client.csproj'

FROM chemsorly/keras-tensorflow:latest-ubuntu-py2-cpu
SHELL ["/bin/bash", "-c"]

ARG CONDUCTHOR_VERSION
ENV CONDUCTHOR_VERSION ${CONDUCTHOR_VERSION}
ENV CONDUCTHOR_OS ubuntu
ENV CONDUCTHOR_TYPE cpu
ENV CONDUCTHOR_HOST ""

# Install .NET Core
RUN apt-get update && apt-get -y upgrade && apt-get -y install apt-transport-https curl gnupg
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        \
    # .NET Core dependencies
        libc6 \
        libcurl3 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu52 \
        liblttng-ust0 \
        libssl1.0.0 \
        libstdc++6 \
        libunwind8 \
        libuuid1 \
        zlib1g \
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
ENV DOTNET_VERSION 1.1.8
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-debian-x64.$DOTNET_VERSION.tar.gz

RUN curl -SL $DOTNET_DOWNLOAD_URL --output dotnet.tar.gz \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# run app
COPY --from=builder 'root/build/ConducThor_Client/bin/Debug/netcoreapp1.1/publish/' '/root/app'
WORKDIR '/root/app'

ENTRYPOINT dotnet ConducThor_Client.dll $CONDUCTHOR_HOST