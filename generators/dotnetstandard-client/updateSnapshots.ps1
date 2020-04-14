Push-Location

cd $PSScriptRoot
cd java
../../../tools/gradlew jar
cd ../npm
npm test -- -u

Pop-Location
