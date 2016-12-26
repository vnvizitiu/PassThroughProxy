﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.Cs.Tunnels
{
    public class TcpOneWayTunnel : IDisposable
    {
        private const int BufferSize = 8192;
        private CancellationTokenSource _cancellationTokenSource;

        public TcpOneWayTunnel()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Run(NetworkStream destination, NetworkStream source)
        {
            await Task.WhenAny(
                Tunnel(source, destination, _cancellationTokenSource.Token));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        private static async Task Tunnel(Stream source, Stream destination, CancellationToken token)
        {
            var buffer = new byte[BufferSize];

            try
            {
                int bytesRead;
                do
                {
                    bytesRead = await source.ReadAsync(buffer, 0, BufferSize, token);
                    await destination.WriteAsync(buffer, 0, bytesRead, token);
                } while (bytesRead > 0 && !token.IsCancellationRequested);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}