using System;
using System.Threading.Tasks;

namespace AspNetCore.Authentication.Internal.Events
{
    public class InternalEvents
    {
        public Func<InternalAuthenticatedContext, Task> OnAuthenticated { get; set; } = context => Task.CompletedTask;
    }
}
