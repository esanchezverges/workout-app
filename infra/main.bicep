@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Full container image reference, for example ghcr.io/owner/workout-app:sha.')
param image string

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'workout-env'
  location: location
  properties: {}
}

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'workout-app'
  location: location
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
      // Scale to zero keeps it free when idle. SQLite means no more than one replica.
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output url string = 'https://${app.properties.configuration.ingress.fqdn}'
