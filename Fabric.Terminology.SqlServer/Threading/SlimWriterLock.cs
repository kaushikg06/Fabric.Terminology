﻿namespace Fabric.Terminology.SqlServer.Threading
{
    using System;
    using System.Threading;

    internal class SlimWriterLock : IDisposable
    {
        private readonly ReaderWriterLockSlim locker;

        public SlimWriterLock(ReaderWriterLockSlim readWriteLock)
        {
            this.locker = readWriteLock;
            this.locker.EnterReadLock();
        }

        void IDisposable.Dispose()
        {
            this.locker.ExitReadLock();
        }
    }
}