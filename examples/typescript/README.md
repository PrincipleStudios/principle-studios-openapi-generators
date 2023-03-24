FIXME: This example builds and runs, but does not properly demonstrate the TypeScript capabilities of these generators.

First, build the TypeScript library using the build script in this repo at `/generators/typescript/build.ps1`.

This project assumes you have a petstore server running at `https://localhost:5001/` with CORS allowing localhost connections.

    npm install
    npm run generate-api
    npm run start
