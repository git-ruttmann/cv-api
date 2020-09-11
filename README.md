# CV API

A dotnet REST API to provide topics for an online CV.
The GUI part is handled by Angular [cv-angular](https://github.com/git-ruttmann/cv-angular)

## API path

* api/v1/authenticate: authenticate codes
* api/v1/vita: read the vita entries
* api/v1/track: track user interactions
* api/v1/health: provide health status
* api/v1/reload: reload the contents

## Architecture

* The API is build upon dotnet core. 
* The data can be provided by Azure BLOB storage or disk files.
* The login codes and groups are listed in the file codes.
  Files are configured in appsettings.json.
* Services are tested in Vita.Test.
* The data provider is mocked by an in memory file system.

## Deployment

* The API is hosted as Azure web app.
* The Angular Frontend is hosted by Azure blob storage.
* The frontend is built and deployed by [Azure DevOps](https://dev.azure.com/az102/cv-angular).
* The frontend [cv.ruttmann.name](https://cv.ruttmann.name) is handled by an Azure Application Gateway, including SSL.
* The raw data is provided by an Azure blob storage.
* The access to the raw data is secured by Azure idendity.

