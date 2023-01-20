param env string
param version string = '1.0.0.0'
param name string

module bri 'bri.bicep' = {
    name: 'bri${env}${version}'
    params: {
      name: name
    }
}
