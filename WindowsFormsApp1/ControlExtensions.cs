using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class EventStreamBuilder<TEventArgs>
    {
        private TaskCompletionSource<TEventArgs> taskCompletionSource = new (TaskCreationOptions.RunContinuationsAsynchronously);  

        public void Handle(object _, TEventArgs args)
        {
            taskCompletionSource.SetResult(args);
            taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public async IAsyncEnumerable<TEventArgs> Build(Action subscribe, Action unsubscribe, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            subscribe();
            do
            {
                yield return await taskCompletionSource.Task;
            }
            while (!cancellationToken.IsCancellationRequested);
            unsubscribe();
        }
    }
}
