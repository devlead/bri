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
@allowed([
  1
  3
  25
  50
  99
])
param age int = 3

param animal object = {
  name: name
  age: age
  species: 'Ape'
  eats: 'Banana'
  alive: alive
}

@secure()
@description('Very secret object')
param topSecret object

var eatsFruit = contains(fruit, animal.eats)
var uniqueName = uniqueString(name)
var base64Encoded = reduce(
                      items(topSecret),
                      '',
                      (result, item) => '${result}\r\n${item.key}:${base64(item.value)}'
                    )

@description('Unique name of animal')
output uniqueName string = uniqueName
output animal object = animal
@description('Flag if animal eats fruit.')
output eatsFruit bool = eatsFruit
output parent string = parent
output base64Encoded string = base64Encoded
