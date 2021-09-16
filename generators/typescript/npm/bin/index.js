#!/usr/bin/env node

const { join } = require('path');

const dllPath = join(__dirname, '../dotnet/PrincipleStudios.OpenApiCodegen.Client.TypeScript.dll');

const [,, ...args] = process.argv;
require("child_process").
  spawn( `dotnet`, [dllPath, ...args] , {
    argv0:"dotnet" , stdio :'inherit'
  }).on('close' , code=> {
    process.exit(code);
  });
