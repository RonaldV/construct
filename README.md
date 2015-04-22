# Construct
Is a lightweight and minimal .NET IoC container. It is inspired by TinyIoC, but with the added benefit of being fully PCL compliant. Construct was created out of the need for a lightweight IoC container in mobile applications using .NET. But that doesn't mean it can't be used in orther kinds of applications.
I've had great success using it in Xamarin projects as wel as others.

## Why didn't I just use TinyIoC?
I had a lot of issues getting TinyIoC to work out of the box in PCL projects. It has problems with some PCL profiles.

See for example the following issues:
- https://github.com/grumpydev/TinyIoC/issues/75
- https://github.com/grumpydev/TinyIoC/pull/31

## Advantages
- lightweight
- easy to understand
- easy to change
- fluent component registration

## Disadvantages (at the moment)
- Only basic features
  - Single instance registration
  - Instance per dependency (also known as transient) registration
  - Only constructor injection
- You'll need to register in the correct order of dependencies

If you're in need of a full featured IoC container I would recommend [Autofac](http://autofac.org/) or [StructureMap](http://docs.structuremap.net/).

## How to use

You can copy paste the Construct.cs file in the project you want to use it or in the future there will be a nuget package available.

### Code example

    // Declare the container in the class that starts the application.
    // For example in a console application you should declare it in the Program class.
    // In Xamarin Forms you should declare it in the App class.
    container = new Container ().Configure ((builder, context) => {
        // Register an instance per dependency
        builder.Register <SomeViewModel> ();
        // Register an instance per dependency by its interface
        builder.Register <SomeService> ().As<ISomeService> ();
        // Register a single instance
        builder.Register <SingleService> ().SingleInstance ();
        // Register a single instance by its interface
        builder.Register <Single2Service> ().As<ISomeService> ().SingleInstance ();
        // For registering a single instance or an instance per dependency with a parameter not registered in the container
        builder.Register (() => new SomeOtherService ("A setting")); // For further configuration see previous examples
    }).Build ();
