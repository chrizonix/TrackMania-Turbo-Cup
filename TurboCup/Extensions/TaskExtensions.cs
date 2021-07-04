using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TurboCup.Extensions {

public static class TaskExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NoWarning(this Task t) { }
}
}
