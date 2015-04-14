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
- You'll need to register in the correct order of dependencies

If you're in need of a full featured IoC container I would recommend [Autofac](http://autofac.org/) or [StructureMap](http://docs.structuremap.net/).

(A manual for using it is on the way.)
