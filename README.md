## Custom API Template

A .Net 8 Web Api project template featuring;
- PostgreSQL or SqlServer for database
- Redis for caching
- ElasticSearch, Kibana and Serilog for logging and visualization
- IdentityService for JWT Authentication and Refresh Tokens
- Excel creation with OpenXml for reporting
- Swagger, Response Compression, Fluent Validation, AutoMapper, Html Sanitizer

## Setup Guide
After cloning this project to your workstation, you can install the template by running dotnet install command.

    dotnet new install .

Then you can create a project with

    dotnet new customapitemplate -D "SqlServer" -C "Redis" -I true -Is true -Do "test-project" -o "TestProject"

Template options are:
    
    -D, --DatabaseProvider "SqlServer" or "PostgreSQL"
        Description: Database Provider for the project
        Default Value: "SqlServer"

    -C, --Cache "Redis" or "NoCache"
        Description: Response Cache
        Default Value: "NoCache"
    
    -I, --IsElasticSearchEnabled
        Description: Is Elastic Search and Kibana Enabled
        Default Value: false
    
    -Is, --IsDockerEnabled
        Description: Is Docker and Docker Compose Enabled
        Default Value: false
    
    -Do, --DockerProjectName
        Description: Docker and Docker Compose Project Name
        Default Value: "custom-api-template"
