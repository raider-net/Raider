using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Validation
{
	public class ValidationManager
	{
		private readonly Dictionary<Type, Dictionary<Type, IValidator>> _rulesRegister = new Dictionary<Type, Dictionary<Type, IValidator>>(); //Dictionary<TDto, Dictionary<TCommand, Validator<TDto>>>

		public Dictionary<Type, IValidator>? GetRulesFor<T>()
			=> GetRulesFor(typeof(T));

		public Dictionary<Type, IValidator>? GetRulesFor(Type objectType)
		{
			if (!_rulesRegister.TryGetValue(objectType, out Dictionary<Type, IValidator>? commandRules))
				return null;

			return commandRules.ToDictionary(k => k.Key, v => v.Value);
		}

		public IValidator? GetRulesFor<T, TCommand>()
			=> GetRulesFor(typeof(T), typeof(TCommand));

		public IValidator? GetRulesFor(Type objectType, Type commandType)
		{
			if (!_rulesRegister.TryGetValue(objectType, out Dictionary<Type, IValidator>? commandRules))
				return null;

			if (!commandRules.TryGetValue(commandType, out IValidator? validator))
				return null;

			return validator;
		}

		public Dictionary<Type, IValidationDescriptor>? GetValidationDescriptorsFor<T>()
			=> GetValidationDescriptorsFor(typeof(T));

		public Dictionary<Type, IValidationDescriptor>? GetValidationDescriptorsFor(Type objectType)
		{
			if (!_rulesRegister.TryGetValue(objectType, out Dictionary<Type, IValidator>? commandRules))
				return null;

			return commandRules.ToDictionary(k => k.Key, v => v.Value.ToDescriptor());
		}

		public IValidationDescriptor? GetValidationDescriptorsFor<T, TCommand>()
			=> GetValidationDescriptorsFor(typeof(T), typeof(TCommand));

		public IValidationDescriptor? GetValidationDescriptorsFor(Type objectType, Type commandType)
		{
			if (!_rulesRegister.TryGetValue(objectType, out Dictionary<Type, IValidator>? commandRules))
				return null;

			if (!commandRules.TryGetValue(commandType, out IValidator? validator))
				return null;

			return validator.ToDescriptor();
		}

		public void RegisterRulesFor<T, TCommand>(Validator<T> validator)
		{
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			var ttype = typeof(T);
			if (!_rulesRegister.TryGetValue(ttype, out Dictionary<Type, IValidator>? commandRules))
			{
				commandRules = new Dictionary<Type, IValidator>();
				_rulesRegister.Add(ttype, commandRules);
			}

			commandRules[typeof(TCommand)] = validator.SoftClone(typeof(TCommand));
		}
	}
}
