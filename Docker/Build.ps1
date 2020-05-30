cd .. ; dotnet tool restore ; cd Docker
dotnet paket restore                           `
	--project                                  `
    ../Alta/Alta.fsproj
dotnet publish                                 `
    --configuration Release                    `
    --self-contained                           `
    --runtime win-x64                          `
    --framework netcoreapp3.1                  `
    --output ./win-x64                         `
    ../Alta/Alta.fsproj

docker build --isolation=hyperv --target chrome -t alta/chrome .
docker build --isolation=hyperv --target firefox -t alta/firefox .