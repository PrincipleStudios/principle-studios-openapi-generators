Push-Location
& ./dotnetcore-server-interfaces/build.ps1
Pop-Location

Push-Location
& ./dotnetstandard-client/build.ps1
Pop-Location

Push-Location
& ./typescript-axios/build.ps1
Pop-Location

Push-Location
& ./typescript-rxjs/build.ps1
Pop-Location
