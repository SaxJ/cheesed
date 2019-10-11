FROM haskell:8.6.5

# We need sqlite
RUN apt-get update
RUN apt-get install -y sqlite3 libsqlite3-dev

RUN mkdir -p /app/user
WORKDIR /app/user
COPY stack.yaml *.cabal ./

RUN export PATH=$(stack path --local-bin):$PATH
RUN stack build --dependencies-only

EXPOSE 3000

COPY . /app/user
RUN stack install
