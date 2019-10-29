FROM ubuntu:19.10

# We need sqlite
RUN apt-get update
RUN apt-get install -y sqlite3 libsqlite3-dev ca-certificates

COPY ./.stack-work/dist/x86_64-linux-tinfo6/Cabal-2.4.0.1/build/cheesed/cheesed /cheesed
COPY ./static /static
COPY ./config /config

EXPOSE 3000

CMD /cheesed
