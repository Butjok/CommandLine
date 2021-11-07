using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Butjok
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string name;
        public bool absolute;
        public CommandAttribute(string name = null, bool absolute = false) {
            this.name = name;
            this.absolute = absolute;
        }
    }
    [Serializable]
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException() { }
        public CommandNotFoundException(string message) : base(message) { }
        public CommandNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected CommandNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class NoObjectsException : Exception
    {
        public NoObjectsException() { }
        public NoObjectsException(string message) : base(message) { }
        public NoObjectsException(string message, Exception inner) : base(message, inner) { }
        protected NoObjectsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    public class MultipleObjectsException : Exception
    {
        public object[] targets;
        public object[] values;
        public MultipleObjectsException() { }
        public MultipleObjectsException(string message) : base(message) { }
        public MultipleObjectsException(string message, Exception inner) : base(message, inner) { }
        public MultipleObjectsException(string message, IEnumerable<object> targets, IEnumerable<object> values) : base(message) {
            this.targets = targets.ToArray();
            this.values = values.ToArray();
        }
        protected MultipleObjectsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
    public static class Commands
    {
        private static Dictionary<string, MemberInfo> commands;
        public static IEnumerable<string> Names => commands.Keys;

        public static void Add(string name, MemberInfo memberInfo)
            => commands[name] = memberInfo;
        public static bool IsVariable(string name) {
            if (!commands.TryGetValue(name, out var command))
                throw new CommandNotFoundException(name);
            return command is FieldInfo || command is PropertyInfo propertyInfo && propertyInfo.GetMethod != null;
        }

        //todo: add named arguments
        public static object Invoke(string name, IList<object> arguments, IEnumerable<KeyValuePair<string,object>>namedArguments) {
            if (!commands.TryGetValue(name, out var memberInfo))
                throw new CommandNotFoundException(name);
            switch (memberInfo) {
                case MethodInfo methodInfo: {
                    var targets = methodInfo.IsStatic ? new object[] { null } : UnityEngine.Object.FindObjectsOfType(methodInfo.DeclaringType);
                    var parameterInfos = methodInfo.GetParameters();
                    var defaultValues = new List<object>();
                    for (var i = arguments.Count; i < parameterInfos.Length; i++) {
                        if (!parameterInfos[i].IsOptional)
                            throw new TargetParameterCountException($"Parameter {parameterInfos[i]} is not optional, please provide a value for it.");
                        defaultValues.Add(parameterInfos[i].DefaultValue);
                    }
                    object returnValue = null;
                    try {
                        foreach (var target in targets)
                            returnValue = methodInfo.Invoke(target, arguments.Concat(defaultValues).ToArray());
                    }
                    catch (TargetParameterCountException e) {
                        throw new TargetParameterCountException($"Incorrect arguments number for the method {methodInfo}: expected {methodInfo.GetParameters().Length}, got {arguments.Count}.", e);
                    }
                    return returnValue;
                }
                case FieldInfo fieldInfo: {
                    var type = fieldInfo.DeclaringType;
                    var targets = fieldInfo.IsStatic ? new object[] { null } : UnityEngine.Object.FindObjectsOfType(type);
                    switch (arguments.Count) {
                        case 0:
                            if (targets.Length == 0)
                                throw new NoObjectsException($"No objects of type {type} found.");
                            if (targets.Length > 1)
                                throw new MultipleObjectsException($"Multiple objects of type {type} are present.", targets, targets.Select(t =>  fieldInfo.GetValue(t)).ToArray());
                            return fieldInfo.GetValue(targets.Last());
                        case 1:
                            foreach (var target in targets)
                                fieldInfo.SetValue(target, arguments[0]);
                            return null;
                        default:
                            throw new Exception($"Command {name} is a field {fieldInfo} and requires either one or no arguments.");
                    }
                }
                case PropertyInfo propertyInfo: {
                    var type = propertyInfo.DeclaringType;
                    var targets = propertyInfo.GetMethod?.IsStatic ?? propertyInfo.SetMethod.IsStatic ? new object[] { null } : UnityEngine.Object.FindObjectsOfType(type);
                    switch (arguments.Count) {
                        case 0:
                            if (targets.Length == 0)
                                throw new NoObjectsException($"No objects of type {type} found.");
                            if (targets.Length > 1)
                                throw new MultipleObjectsException($"Multiple objects of type {type} are present.", targets, targets.Select(t => propertyInfo.GetValue(t)).ToArray());
                            return propertyInfo.GetValue(targets.Last());
                        case 1:
                            foreach (var target in targets)
                                propertyInfo.SetValue(target, arguments[0]);
                            return null;
                        default:
                            throw new Exception($"Command {name} is a property {propertyInfo} and requires either one or no arguments.");
                    }
                }
                default: throw new ArgumentOutOfRangeException(memberInfo.ToString());
            }
        }

        public static void Initialize(bool includeNamespaceName = true) {
            commands = new Dictionary<string, MemberInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
                foreach (var methodInfo in type.GetMethods(bindingFlags))
                foreach (var attribute in methodInfo.GetCustomAttributes<CommandAttribute>())
                    commands[GenerateName(methodInfo, attribute, includeNamespaceName)] = methodInfo;
                foreach (var fieldInfo in type.GetFields(bindingFlags))
                foreach (var attribute in fieldInfo.GetCustomAttributes<CommandAttribute>())
                    commands[GenerateName(fieldInfo, attribute, includeNamespaceName)] = fieldInfo;
                foreach (var propertyInfo in type.GetProperties(bindingFlags))
                foreach (var attribute in propertyInfo.GetCustomAttributes<CommandAttribute>())
                    commands[GenerateName(propertyInfo, attribute, includeNamespaceName)] = propertyInfo;
            }
        }

        public static string GenerateName(MemberInfo memberInfo, CommandAttribute attribute = null, bool includeNamespaceName = true) {
            var classPrefix = (includeNamespaceName ? memberInfo.DeclaringType.FullName : memberInfo.DeclaringType.Name) + '.';
            return attribute?.name == null
                ? classPrefix + memberInfo.Name
                : attribute.absolute
                    ? attribute.name
                    : classPrefix + attribute.name;
        }
    }
}