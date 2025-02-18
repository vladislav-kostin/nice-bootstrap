using System;

// namespace VK.Bootstrap
// {
[AttributeUsage(AttributeTargets.Class)]
public class BootstrapBeforeAttribute : Attribute
{
	public BootstrapBeforeAttribute(Type type)
	{
		Type = type;
	}

	public Type Type { get; private set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class BootstrapAfterAttribute : Attribute
{
	public BootstrapAfterAttribute(Type type)
	{
		Type = type;
	}

	public Type Type { get; private set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class BootstrapOrderAttribute : Attribute
{
	public BootstrapOrderAttribute(int order)
	{
		Order = order;
	}

	public int Order { get; private set; }
}
// }