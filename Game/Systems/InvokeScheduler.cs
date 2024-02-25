#nullable enable
using Game.Events;
using System;
using System.Collections.Generic;

namespace Game.Systems
{
    public class InvokeScheduler : IDisposable, IListener<UpdateEvent>
    {
        private readonly List<float> actionExpireTimes = new();
        private readonly List<Action> actions = new();

        private float time;

        public void InvokeAfter(float delay, Action action)
        {
            float expireTime = time + delay;
            actionExpireTimes.Add(expireTime);
            actions.Add(action);
        }

        void IListener<UpdateEvent>.Receive(VirtualMachine vm, ref UpdateEvent e)
        {
            time += e.delta;
            Poll();
        }

        private void Poll()
        {
            int actionCount = actions.Count;
            for (int i = actionCount - 1; i >= 0; i--)
            {
                Action action = actions[i];
                float expireTime = actionExpireTimes[i];
                if (time >= expireTime)
                {
                    actionExpireTimes.RemoveAt(i);
                    actions.RemoveAt(i);
                    action();
                }
            }
        }

        public void Dispose()
        {
            actionExpireTimes.Clear();
            actions.Clear();

            //todo: implement cancellations
        }
    }
}