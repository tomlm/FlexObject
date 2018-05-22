# FlexObject
FlexObject is a hybrid object which combines the best of typed and dynamic in .NET

## It's pure dynamic
You can simply use it as a dynamic object

```C#
	dynamic obj = JsonConvert.DeserializeObject<FlexObject>(json);
	obj.x = 3;
	obj.y = "yo";
```

## It's pure typed
You can use it as a typed object

```C#
	public class Car : FlexObject
	{
		public string Name { get;set;}
	}


	Car car = JsonConvert.DeserializeObject<Car>(json
	if (car.Name == null)
	{
	}
```

## It can be both!
You can use it as a typed object AND a dynamic object at the same time!

```C#
	public class Car : FlexObject
	{
		public string Name { get;set;}
	}


	Car car = JsonConvert.DeserializeObject<Car>(json
	if (car.Name == null)
	{
		// use typed property
		car.Name = "Volvo";

		// assign a property via dictionary syntax
		car["x"] = 3;

		// use a property using dynamic keyword
		dynamic car2 = car;
		if (car.x == 3)
		{
			
		}
	}
	// it serializes back
	json = JsonConvert.SerializeObject(var); 
```

You can also enumerate typed and untyped properties the same way.

```C#
	dynamic car = JsonConvert.DeserializeObject<Car>(json
	foreach(var property in car)
	{
		Debug.Print(property);
	}
```

