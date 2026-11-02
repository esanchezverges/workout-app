@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Full container image reference, for example ghcr.io/owner/workout-app:sha.')
param image string

@description('Region for the database. Spain Central does not offer the Azure SQL free limit.')
param sqlLocation string = 'francecentral'

@description('Entra object ID of the database administrator.')
param sqlAdminObjectId string

@description('Display name of the database administrator.')
param sqlAdminLogin string

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'workout-id'
  location: location
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'workout-sql-${uniqueString(resourceGroup().id)}'
  location: sqlLocation
  properties: {
    minimalTlsVersion: '1.2'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: tenant().tenantId
      azureADOnlyAuthentication: true
    }
  }
}

// The 0.0.0.0 rule means "any Azure service". Login still requires Entra.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'workout'
  location: sqlLocation
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
  }
  properties: {
    // Free offer: when the monthly allowance runs out the database pauses instead of billing.
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    minCapacity: json('0.5')
    maxSizeBytes: 34359738368
    requestedBackupStorageRedundancy: 'Local'
  }
}

var containerEnv = [
  {
    name: 'ConnectionStrings__Workout'
    // Holds no secret: the managed identity is the credential.
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${database.name};Authentication=Active Directory Managed Identity;User Id=${identity.properties.clientId};Encrypt=True;Connect Timeout=90'
  }
]

var userAssignedIdentity = {
  type: 'UserAssigned'
  userAssignedIdentities: {
    '${identity.id}': {}
  }
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'workout-env'
  location: location
  properties: {}
}

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'workout-app'
  location: location
  identity: userAssignedIdentity
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'workout-app'
          image: image
          env: containerEnv
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/healthz'
                port: 8080
              }
            }
          ]
        }
      ]
      // Scale to zero keeps it free when idle.
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

resource migrateJob 'Microsoft.App/jobs@2024-03-01' = {
  name: 'workout-migrate'
  location: location
  identity: userAssignedIdentity
  properties: {
    environmentId: environment.id
    configuration: {
      triggerType: 'Manual'
      replicaTimeout: 600
      replicaRetryLimit: 0
      manualTriggerConfig: {
        parallelism: 1
        replicaCompletionCount: 1
      }
    }
    template: {
      containers: [
        {
          name: 'migrate'
          image: image
          args: ['--migrate']
          env: containerEnv
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
    }
  }
}

output url string = 'https://${app.properties.configuration.ingress.fqdn}'
