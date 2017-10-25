FROM chemsorly/msbuilder:latest
SHELL ["powershell"]

COPY . 'C:\\build\\'  
WORKDIR 'C:\\build\\'

ARG MSBUILD_PROJECT=""
ARG MSBUILD_TARGET=""
ARG MSBUILD_ARGS=""

RUN ["nuget.exe", "restore"]  
RUN & 'C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe' $env:MSBUILD_PROJECT $env:MSBUILD_TARGET $env:MSBUILD_ARGS