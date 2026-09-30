using Eco.Core.Sessions;
using NUnit.Framework;

namespace Eco.Core.Tests
{
    public class KioskIdleWatcherTests
    {
        [Test]
        public void Tick_BelowTimeout_DoesNotFire()
        {
            var watcher = new KioskIdleWatcher(10);
            var fired = false;
            watcher.OnIdleTimeout += () => fired = true;

            watcher.Tick(9);

            Assert.IsFalse(fired);
        }

        [Test]
        public void Tick_ReachesTimeout_Fires()
        {
            var watcher = new KioskIdleWatcher(10);
            var fired = false;
            watcher.OnIdleTimeout += () => fired = true;

            watcher.Tick(6);
            watcher.Tick(5);

            Assert.IsTrue(fired);
        }

        [Test]
        public void Reset_ClearsAccumulatedTime()
        {
            var watcher = new KioskIdleWatcher(10);
            var fired = false;
            watcher.OnIdleTimeout += () => fired = true;

            watcher.Tick(9);
            watcher.Reset();
            watcher.Tick(9);

            Assert.IsFalse(fired);
        }
    }
}
