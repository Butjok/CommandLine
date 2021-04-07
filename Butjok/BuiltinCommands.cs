using System.Text;
using UnityEngine;

namespace Butjok {

    public static class BuiltinCommands {

        private static readonly StringBuilder Sb = new StringBuilder();

        [Command]
        private static void Print(Arguments arguments) {
            Sb.Clear();
            foreach (var argument in arguments.Values)
                Sb.AppendLine(argument.ToString());
            Debug.Log(Sb.ToString());
        }


    }
}