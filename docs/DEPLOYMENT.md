# Deployment Guide

This guide covers different ways to deploy the AI-Ready REST API to Azure.

## Prerequisites

- Azure subscription
- Azure CLI installed (optional)
- Visual Studio 2022 or VS Code (optional)

## Option 1: Deploy from Visual Studio 2022

### Steps

1. **Open the Solution**:
   - Open `AIReadyRestApi.sln` in Visual Studio 2022

2. **Right-click the Project**:
   - In Solution Explorer, right-click on `AIReadyRestApi` project
   - Select `Publish...`

3. **Select Target**:
   - Choose `Azure`
   - Click `Next`

4. **Select Specific Target**:
   - Choose `Azure Function App (Windows)` or `Azure Function App (Linux)`
   - Click `Next`

5. **Select Function App**:
   - Choose existing Function App or create new one
   - If creating new:
     - Subscription: Select your subscription
     - Resource Group: Create new or use existing
     - Function App name: Enter unique name
     - Runtime stack: .NET 8 (Isolated)
     - Region: Select your preferred region
   - Click `Create` or `Finish`

6. **Publish**:
   - Click `Publish` button
   - Wait for deployment to complete

7. **Verify**:
   - Navigate to `https://<your-function-app>.azurewebsites.net/api/swagger/ui`
   - You should see the Swagger UI

## Option 2: Deploy using Azure CLI

### Create Resources

```bash
# Variables
RESOURCE_GROUP="rg-ai-ready-api"
LOCATION="eastus"
STORAGE_ACCOUNT="staireadyapi$(date +%s)"
FUNCTION_APP="func-ai-ready-api"
PLAN_NAME="plan-ai-ready-api"

# Login to Azure
az login

# Create Resource Group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Storage Account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --sku Standard_LRS

# Create Function App Plan (Consumption or Premium)
# For Consumption plan:
az functionapp plan create \
  --resource-group $RESOURCE_GROUP \
  --name $PLAN_NAME \
  --location $LOCATION \
  --sku Y1 \
  --is-linux false

# Or for Premium plan (better performance):
# az functionapp plan create \
#   --resource-group $RESOURCE_GROUP \
#   --name $PLAN_NAME \
#   --location $LOCATION \
#   --sku EP1 \
#   --is-linux false

# Create Function App
az functionapp create \
  --resource-group $RESOURCE_GROUP \
  --plan $PLAN_NAME \
  --name $FUNCTION_APP \
  --storage-account $STORAGE_ACCOUNT \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4

# Enable Application Insights (recommended)
az monitor app-insights component create \
  --app $FUNCTION_APP \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP

# Get Application Insights key
APPINSIGHTS_KEY=$(az monitor app-insights component show \
  --app $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey -o tsv)

# Configure Application Insights
az functionapp config appsettings set \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$APPINSIGHTS_KEY"
```

### Deploy the Code

```bash
# Build and publish
dotnet publish -c Release

# Create deployment package
cd bin/Release/net8.0/publish
zip -r ../../../deploy.zip .
cd ../../../

# Deploy to Azure
az functionapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $FUNCTION_APP \
  --src deploy.zip
```

### Verify Deployment

```bash
# Get Function App URL
FUNCTION_URL=$(az functionapp show \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --query defaultHostName -o tsv)

echo "Swagger UI: https://$FUNCTION_URL/api/swagger/ui"
echo "OpenAPI JSON: https://$FUNCTION_URL/api/openapi/v3.json"

# Test the API
curl https://$FUNCTION_URL/api/products
```

## Option 3: Deploy using GitHub Actions

### Setup

1. **Get Publish Profile**:
   ```bash
   az functionapp deployment list-publishing-profiles \
     --name $FUNCTION_APP \
     --resource-group $RESOURCE_GROUP \
     --xml
   ```

2. **Add Secret to GitHub**:
   - Go to your repository on GitHub
   - Settings > Secrets and variables > Actions
   - Click `New repository secret`
   - Name: `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
   - Value: Paste the publish profile XML
   - Click `Add secret`

3. **Create Workflow File**:
   Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure Functions

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_FUNCTIONAPP_NAME: 'func-ai-ready-api'
  AZURE_FUNCTIONAPP_PACKAGE_PATH: '.'
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: windows-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Publish
      run: dotnet publish --configuration Release --no-build --output ./output

    - name: Deploy to Azure Functions
      uses: Azure/functions-action@v1
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        package: ./output
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

4. **Push to GitHub**:
   ```bash
   git add .github/workflows/deploy.yml
   git commit -m "Add GitHub Actions deployment workflow"
   git push
   ```

## Option 4: Deploy using Azure DevOps

### Setup Pipeline

1. **Create Azure DevOps Pipeline**:
   Create `azure-pipelines.yml`:

```yaml
trigger:
  branches:
    include:
    - main

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'
  azureSubscription: 'your-service-connection'
  functionAppName: 'func-ai-ready-api'

steps:
- task: UseDotNet@2
  displayName: 'Install .NET SDK'
  inputs:
    version: '8.0.x'
    packageType: sdk

- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build project'
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Publish project'
  inputs:
    command: 'publish'
    publishWebProjects: false
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
    zipAfterPublish: true

- task: AzureFunctionApp@2
  displayName: 'Deploy to Azure Functions'
  inputs:
    azureSubscription: '$(azureSubscription)'
    appType: 'functionApp'
    appName: '$(functionAppName)'
    package: '$(Build.ArtifactStagingDirectory)/**/*.zip'
    deploymentMethod: 'auto'
```

## Configuration Settings

### Application Settings

Add these settings to your Function App:

```bash
az functionapp config appsettings set \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated" \
    "WEBSITE_RUN_FROM_PACKAGE=1"
```

### CORS Configuration

If you need to access the API from a web application:

```bash
az functionapp cors add \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --allowed-origins "https://your-web-app.com"

# Or allow all (not recommended for production):
# az functionapp cors add \
#   --name $FUNCTION_APP \
#   --resource-group $RESOURCE_GROUP \
#   --allowed-origins "*"
```

### Authentication

To enable authentication:

```bash
az functionapp auth update \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP \
  --enabled true \
  --action LoginWithAzureActiveDirectory
```

## Post-Deployment

### Update Authorization Levels

For production, update the authorization levels in your functions from `Anonymous` to `Function` or `Admin`:

```csharp
[HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")]
```

### Configure API Management (Optional)

For enterprise scenarios, consider using Azure API Management:

1. Create an API Management instance
2. Import the OpenAPI specification
3. Configure policies for:
   - Rate limiting
   - Caching
   - Request/response transformation
   - Authentication

```bash
# Create API Management instance
az apim create \
  --name "apim-ai-ready-api" \
  --resource-group $RESOURCE_GROUP \
  --publisher-email "admin@example.com" \
  --publisher-name "Your Company" \
  --sku-name Developer

# Import OpenAPI spec
az apim api import \
  --path /products \
  --resource-group $RESOURCE_GROUP \
  --service-name "apim-ai-ready-api" \
  --api-id products-api \
  --specification-format OpenApi \
  --specification-url "https://$FUNCTION_URL/api/openapi/v3.json"
```

### Monitoring

View metrics and logs:

```bash
# View live metrics
az monitor app-insights component show \
  --app $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP

# View logs
az functionapp log tail \
  --name $FUNCTION_APP \
  --resource-group $RESOURCE_GROUP
```

## Troubleshooting

### Common Issues

1. **Function App not starting**:
   - Check Application Insights logs
   - Verify runtime version is correct (.NET 8 Isolated)
   - Ensure all required packages are deployed

2. **OpenAPI not showing**:
   - Verify the OpenAPI extension is included in the deployment
   - Check that routes are correctly configured
   - Ensure the app is running

3. **Performance issues**:
   - Consider upgrading to Premium plan
   - Enable Application Insights profiling
   - Review and optimize function code

### Useful Commands

```bash
# View Function App logs
az functionapp log tail --name $FUNCTION_APP --resource-group $RESOURCE_GROUP

# Restart Function App
az functionapp restart --name $FUNCTION_APP --resource-group $RESOURCE_GROUP

# View configuration
az functionapp config appsettings list --name $FUNCTION_APP --resource-group $RESOURCE_GROUP

# Delete resources (when done testing)
az group delete --name $RESOURCE_GROUP --yes
```

## Cost Optimization

### Consumption Plan
- Pay only for execution time
- Automatically scales
- Best for sporadic workloads

### Premium Plan
- Pre-warmed instances (no cold start)
- Better performance
- VNet integration support
- Best for production workloads

### Dedicated Plan
- Run on dedicated VMs
- Predictable pricing
- Best when you have other App Service apps

## Security Best Practices

1. **Use Managed Identity** for accessing Azure resources
2. **Store secrets in Azure Key Vault**
3. **Enable HTTPS only**
4. **Implement proper authorization levels**
5. **Use Application Insights** for monitoring and security alerts
6. **Regular dependency updates** to patch vulnerabilities
7. **Network isolation** with VNet integration (Premium/Dedicated plans)

## Next Steps

After deployment:
1. Test all endpoints using the Swagger UI
2. Configure custom domain (if needed)
3. Set up monitoring alerts
4. Implement CI/CD pipeline
5. Configure backup and disaster recovery
6. Document any environment-specific configuration
