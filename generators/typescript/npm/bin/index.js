#!/usr/bin/env node

const { join } = require('path');

const dllPath = join(__dirname, '../dotnet/PrincipleStudios.OpenApiCodegen.Client.TypeScript.dll');

const [arg0,arg1, ...args] = process.argv;
process.argv = [arg0,arg1, dllPath, ...args];

require('dotnet-3.1/dist/call');
