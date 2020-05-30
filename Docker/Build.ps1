dotnet tool restore                            `
	--configfile                               `
    ../.config/dotnet-tools.json
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

docker build --target chrome -t alta/chrome .
docker build --target firefox -t alta/firefox .