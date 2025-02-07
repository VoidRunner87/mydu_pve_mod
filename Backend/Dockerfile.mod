FROM debian:bookworm AS nq_server_build

ADD https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb /tmp/packages-microsoft-prod.deb
RUN apt update && apt install -y ca-certificates
RUN dpkg -i /tmp/packages-microsoft-prod.deb
RUN apt update && apt install -y dotnet-sdk-7.0

COPY . /source
RUN cd /source && dotnet publish --self-contained \
    /nodeReuse:false -r linux-x64 \
    -p:UseSharedCompilation=false -c Release -o /install/Mod

FROM debian:bookworm AS release
RUN apt update && DEBIAN_FRONTEND=noninteractive apt install -y libcurl4 libgoogle-perftools4 \
  libhiredis0.14 libpq5 libicu72
LABEL nqcomponent orleans
COPY --from=nq_server_build /install/Mod /Mod
WORKDIR /Mod
ENTRYPOINT ["/Mod/Examples", "/config/dual.yaml"]
