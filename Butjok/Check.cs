using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable ExplicitCallerInfoArgument

namespace Butjok {
    public class CheckException : Exception {

        public CheckException(string message = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = null)
            : base(
                $"{Path.GetFileName(filePath)}:{lineNumber}: {memberName}(): {File.ReadLines(filePath).Skip(lineNumber - 1).Take(1).First().Trim()}\n{message}\n") {}
    }
    
    public static class Check {
        
        public static void That(bool condition, Func<string> message = null,
            [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = null) {

            if (condition)
                return;
            
            throw new CheckException(message?.Invoke(), filePath, lineNumber, memberName);
        }
    }
}