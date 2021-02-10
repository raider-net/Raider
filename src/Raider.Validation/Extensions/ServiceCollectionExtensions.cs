using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Exceptions;
using Raider.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.Validation.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderValidation<TSearchBaseAssembly>(this IServiceCollection services)
			=> AddRaiderValidation(services, typeof(TSearchBaseAssembly).Assembly);

		public static IServiceCollection AddRaiderValidation(this IServiceCollection services, params Assembly[] assemblies)
		{
			if (!assemblies.Any())
				throw new ArgumentNullException(nameof(assemblies), "At least one assembly is requred to scan for handlers.");

			var validationManager = new ValidationManager();

			var validationDescriptorBuilderType = typeof(IValidationDescriptorBuilder);

			var typesToScan =
				assemblies
					.Distinct()
					.SelectMany(a => a.DefinedTypes)
					.Where(type =>
						!type.IsInterface
						&& !type.IsAbstract
						&& validationDescriptorBuilderType.IsAssignableFrom(type));

			bool found = false;
			foreach (var descriptorBuilderTypeInfo in typesToScan)
			{
				var attribute = descriptorBuilderTypeInfo.GetCustomAttribute<ValidationRegisterAttribute>();
				if (0 < attribute?.CommandTypes?.Length)
				{
					foreach (var commandType in attribute.CommandTypes)
					{
						IValidationDescriptorBuilder? validationDescriptorBuilder = null;

						var defaultCtor = descriptorBuilderTypeInfo.GetDefaultConstructor();
						if (defaultCtor == null)
						{
							validationDescriptorBuilder = (IValidationDescriptorBuilder)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(descriptorBuilderTypeInfo);
							if (validationDescriptorBuilder == null)
								throw new InvalidOperationException($"Cannot create instance of {descriptorBuilderTypeInfo}");
						}
						else
						{
							validationDescriptorBuilder = (IValidationDescriptorBuilder)defaultCtor.Invoke(null);
							if (validationDescriptorBuilder == null)
								throw new InvalidOperationException($"Cannot create instance of {descriptorBuilderTypeInfo}");
						}

						IValidationDescriptor? descriptor = null;
						try
						{
							descriptor = validationDescriptorBuilder.ToDescriptor();
						}
						catch (Exception ex)
						{
							throw new ConfigurationException($"Cannot create {nameof(IValidationDescriptor)}. Call {nameof(validationDescriptorBuilder.ToDescriptor)} on instance of {descriptorBuilderTypeInfo.FullName} created with {(defaultCtor == null ? "uninitialized object." : "default constructor.")}", ex);
						}

						if (descriptor == null)
							throw new InvalidOperationException($"{nameof(validationDescriptorBuilder.ToDescriptor)} on instance of {descriptorBuilderTypeInfo.FullName} returns null.");

						found = validationManager.RegisterValidationDescriptorFor(descriptor.ObjectType, commandType, descriptor) || found;
					}
				}
			}

			if (!found)
				throw new ConfigurationException("No validator was found.");

			services.TryAddSingleton<IValidationManager>(validationManager);

			return services;
		}
	}
}
