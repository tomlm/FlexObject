# FlexObject
FlexObject is a hybrid object which combines the best of typed and dynamic in .NET

## It's pure dynamic
You can simply use it as a dynamic object

```C#
	dynamic obj = new FlexObject();
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


	Car car = new Car();
	car.Name = "test";
	car.Age = 15;
	if (car.Name == "test" && car["Age"] == 15)
	{
	}
```

## It can be both!
You can use it as a typed object AND a dynamic object at the same time!

```C#
	Car car = new Car();
	if (car.Name == null)
	{
		// use typed property
		car.Name = "Volvo";

		// assign a property via dictionary syntax
		car["x"] = 3;

		// use a property using dynamic keyword
		dynamic car2 = car;
		if (car.x == 3 && car.Name == "Volvo")
		{
			
		}
	}
```

You can also enumerate typed and untyped properties the same way.

```C#
	foreach(var property in car.GetProperties())
	{
		Debug.Print(car[property]);
	}
```

It also supports change notification
``` C#
	public class Car : FlexObject
	{
		private string _name;
		public string Name { get=>_name; set { _name = value; NotifyChanged();}}
	}
    Car car = new Car();
	car.PropertyChanged += (sender, args) => Debug.Print("Property {0} changed", args.PropertyName);
	car.Name = "Volvo";
	// => Property Name Changed
	car["x"] = 3; 
	// => Property x Changed
```
