using System;
using System.Reflection;

namespace Butjok {

    public static class CommandsExtensions {

        private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void RegisterFrom(this Commands commands, Assembly assembly) {
            foreach (var type in assembly.GetTypes())
                RegisterFrom(commands, type);
        }
        public static void RegisterFrom(this Commands commands, Type type) {
            RegisterFrom(commands, type, null, Static);
        }
        public static void RegisterFrom(this Commands commands, object targetObject) {
            RegisterFrom(commands, targetObject.GetType(), targetObject, Instance);
        }
        private static void RegisterFrom(this Commands commands, Type type, object targetObject,
            BindingFlags bindingFlags) {
            Assert.That(commands != null);
            Assert.That(type != null);

            foreach (var fieldInfo in type.GetFields(bindingFlags))
            foreach (var attribute in fieldInfo.GetCustomAttributes<CommandAttribute>()) {
                commands.Add(name: attribute.Name, fieldInfo: fieldInfo, targetObject: targetObject,
                    helpText: attribute.HelpText);
            }

            foreach (var propertyInfo in type.GetProperties(bindingFlags))
            foreach (var attribute in propertyInfo.GetCustomAttributes<CommandAttribute>()) {
                commands.Add(name: attribute.Name, propertyInfo: propertyInfo, targetObject: targetObject,
                    helpText: attribute.HelpText);
            }

            foreach (var methodInfo in type.GetMethods(bindingFlags))
            foreach (var attribute in methodInfo.GetCustomAttributes<CommandAttribute>()) {
                commands.Add(name: attribute.Name, arguments: attribute.Arguments, methodInfo: methodInfo,
                    targetObject: targetObject, helpText: attribute.HelpText);
            }
        }
    }
}