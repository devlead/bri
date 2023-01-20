metadata documentation = {
  summary: 'A brief summary of the module.'
  description: '''A detailed description of the bicep module.
Used for the overview in documentation.
'''
}

@secure()
@description('Name of animal')
@minLength(1)
@maxLength(20)
@metadata({
  example: 'Pogo'
})
param name string

@allowed([
  'Adam'
  'Eve'
])
param parent string = 'Eve'

@description('Possible fruits.')
param fruit array = [
  'Banana'
  'Apple'
]

@description('Indicates if animal is alive')
param alive bool = true

@description('Age of animal')
@minValue(1)
@maxValue(99)
param age int = 3
param animal object = {
  name: name
  age: age
  species: 'Ape'
  eats: 'Banana'
  alive: alive
}

var eatsFruit = contains(fruit, animal.eats)
var uniqueName = uniqueString(name)

@description('Unique name of animal')
output uniqueName string = uniqueName
output animal object = animal
@description('Flag if animal eats fruit.')
output eatsFruit bool = eatsFruit
output parent string = parent