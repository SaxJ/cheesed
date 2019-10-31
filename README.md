# Cheesed
A stupid application that will count the number of times somebody has been
  cheesed leaving their computer unlocked and unattended.
  That is, just counting the number of times somebody has logged in with Google.

## Development

Start a development server with:

```
stack exec -- yesod devel
```

As your code changes, your site will be automatically recompiled and redeployed
to localhost:3000.

## Tests

```
stack test --flag cheesed:library-only --flag cheesed:dev
```

## Building
To deploy a new production executable, just `stack build`.
