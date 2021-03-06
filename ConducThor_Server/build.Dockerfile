FROM chemsorly/msbuilder:2.0.0
SHELL ["powershell"]

COPY . 'C:\\build\\'  
WORKDIR 'C:\\build\\'

ARG MSBUILD_PROJECT=""
ARG MSBUILD_TARGET=""
ARG MSBUILD_ARGS=""

RUN ["nuget.exe", "restore"]  
RUN & msbuild $env:MSBUILD_PROJECT $env:MSBUILD_TARGET $env:MSBUILD_ARGS