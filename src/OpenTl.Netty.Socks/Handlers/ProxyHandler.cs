/*
 * Copyright 2014 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using log4net;
using NullGuard;

namespace OpenTl.Netty.Socks.Handlers
{
    public abstract class ProxyHandler : ChannelDuplexHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyHandler));

        /**
     * The default connect timeout: 10 seconds.
     */
        private static readonly long DefaultConnectTimeoutMillis = 10000;

        /**
     * A string that signifies 'no authentication' or 'anonymous'.
     */
        protected static readonly string AuthNone = "none";

        private readonly EndPoint _proxyAddress;

        private IChannelHandlerContext _ctx;
        private bool _finished;
        private bool _flushedPrematurely;
        private PendingWriteQueue _pendingWrites;

        private bool _suppressChannelReadComplete;

        protected ProxyHandler(EndPoint proxyAddress)
        {
            _proxyAddress = proxyAddress;
        }

        /**
     * Returns the connect timeout in millis.  If the connection attempt to the destination does not finish within
     * the timeout, the connection attempt will be failed.
     */
        public long ConnectTimeoutMillis { get; set; } = DefaultConnectTimeoutMillis;

        /**
     * Returns the name of the proxy protocol in use.
     */
        public abstract string Protocol { get; }

        /**
     * Returns the name of the authentication scheme in use.
     */
        public abstract string AuthScheme { get; }

        /**
     * Returns the address of the destination to connect to via the proxy server.
     */
        [AllowNull] public EndPoint DestinationAddress { get; private set; }

        /**
     * Returns the address of the proxy server.
     */
        public T ProxyAddress<T>() where T : EndPoint
        {
            return (T) _proxyAddress;
        }

        /**
     * Rerutns {@code true} if and only if the connection to the destination has been established successfully.
     */
        public bool IsConnected()
        {
            return _ctx.Channel.Active;
        }


        public override void HandlerAdded(IChannelHandlerContext context)
        {
            _ctx = context;
            AddCodec(context);

            if (context.Channel.Active) SendInitialMessage(context);
        }

        /**
     * Adds the codec handlers required to communicate with the proxy server.
     */
        protected abstract void AddCodec(IChannelHandlerContext ctx);

        /**
     * Removes the encoders added in {@link #addCodec(IChannelHandlerContext)}.
     */
        protected abstract void RemoveEncoder(IChannelHandlerContext ctx);

        /**
     * Removes the decoders added in {@link #addCodec(IChannelHandlerContext)}.
     */
        protected abstract void RemoveDecoder(IChannelHandlerContext ctx);

        public override async Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress,
            [AllowNull] EndPoint localAddress)
        {
            if (DestinationAddress != null) throw new InvalidOperationException();

            DestinationAddress = remoteAddress;
            await context.ConnectAsync(_proxyAddress, localAddress);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            SendInitialMessage(context);
            context.FireChannelActive();
        }

        /**
     * Sends the initial message to be sent to the proxy server. This method also starts a timeout task which marks
     * the {@link #connectPromise} as failure if the connection attempt does not success within the timeout.
     */
        private void SendInitialMessage(IChannelHandlerContext ctx)
        {
//        long connectTimeoutMillis = this.connectTimeoutMillis;
//        if (connectTimeoutMillis > 0) {
//            connectTimeoutFuture = ctx.executor().schedule(new Runnable() {
//                @Override
//                public void run() {
//                    if (!connectPromise.isDone()) {
//                        setConnectFailure(new ProxyConnectException(exceptionMessage("timeout")));
//                    }
//                }
//            }, connectTimeoutMillis, TimeUnit.MILLISECONDS);
//        }

            var initialMessage = NewInitialMessage(ctx);
            if (initialMessage != null) SendToProxyServer(initialMessage);

            ReadIfNeeded(ctx);
        }

        /**
     * Returns a new message that is sent at first time when the connection to the proxy server has been established.
     *
     * @return the initial message, or {@code null} if the proxy server is expected to send the first message instead
     */
        protected abstract object NewInitialMessage(IChannelHandlerContext ctx);

        /**
     * Sends the specified message to the proxy server.  Use this method to send a response to the proxy server in
     * {@link #handleResponse(IChannelHandlerContext, Object)}.
     */
        protected void SendToProxyServer(object msg)
        {
            _ctx.WriteAndFlushAsync(msg);
//            .addListener(writeListener);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            if (_finished)
                _ctx.FireChannelInactive();
            else
                SetConnectFailure(new ProxyConnectException(ExceptionMessage("disconnected"),
                    new InvalidOperationException()));
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (_finished)
                _ctx.FireExceptionCaught(exception);
            else
                SetConnectFailure(exception);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (_finished)
            {
                // Received a message after the connection has been established; pass through.
                _suppressChannelReadComplete = false;
                _ctx.FireChannelRead(message);
            }
            else
            {
                _suppressChannelReadComplete = true;
                Exception cause = null;
                try
                {
                    var done = HandleResponse(_ctx, message);
                    if (done) SetConnectSuccess();
                }
                catch (Exception t)
                {
                    cause = t;
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                    if (cause != null) SetConnectFailure(cause);
                }
            }
        }


        /**
     * Handles the message received from the proxy server.
     *
     * @return {@code true} if the connection to the destination has been established,
     *         {@code false} if the connection to the destination has not been established and more messages are
     *         expected from the proxy server
     */
        protected abstract bool HandleResponse(IChannelHandlerContext ctx, object response);

        private void SetConnectSuccess()
        {
            _finished = true;
            var removedCodec = true;

            removedCodec &= SafeRemoveEncoder();

            _ctx.FireUserEventTriggered(new ProxyConnectionEvent(Protocol, AuthScheme, _proxyAddress,
                DestinationAddress));

            removedCodec &= SafeRemoveDecoder();

            if (removedCodec)
            {
                WritePendingWrites();

                if (_flushedPrematurely) _ctx.Flush();
            }
            else
            {
                // We are at inconsistent state because we failed to remove all codec handlers.
                Exception cause = new ProxyConnectException(
                    "failed to remove all codec handlers added by the proxy handler; bug?",
                    new InvalidOperationException());
                FailPendingWrites(cause);
                _ctx.FireExceptionCaught(cause);
                _ctx.CloseAsync();
            }
        }

        private bool SafeRemoveDecoder()
        {
            try
            {
                RemoveDecoder(_ctx);
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("Failed to remove proxy decoders:", e);
            }

            return false;
        }

        private bool SafeRemoveEncoder()
        {
            try
            {
                RemoveEncoder(_ctx);
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("Failed to remove proxy encoders:", e);
            }

            return false;
        }

        private void SetConnectFailure(Exception cause)
        {
            _finished = true;

            if (!(cause is ProxyConnectException))
                cause = new ProxyConnectException(
                    ExceptionMessage(cause.ToString()), cause);

            SafeRemoveDecoder();
            SafeRemoveEncoder();

            FailPendingWrites(cause);
            _ctx.FireExceptionCaught(cause);
            _ctx.CloseAsync();
        }

        /**
         * Decorates the specified exception message with the common information such as the current protocol,
         * authentication scheme, proxy address, and destination address.
         */
        protected string ExceptionMessage(string msg)
        {
            if (msg == null) msg = "";

            var buf = new StringBuilder(128 + msg.Length)
                .Append(Protocol)
                .Append(", ")
                .Append(AuthScheme)
                .Append(", ")
                .Append(_proxyAddress)
                .Append(" => ")
                .Append(DestinationAddress);
            if (msg.Length != 0) buf.Append(", ").Append(msg);

            return buf.ToString();
        }


        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            if (_suppressChannelReadComplete)
            {
                _suppressChannelReadComplete = false;

                ReadIfNeeded(_ctx);
            }
            else
            {
                _ctx.FireChannelReadComplete();
            }
        }

        public override async Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (_finished)
            {
                WritePendingWrites();
                await _ctx.WriteAsync(message);
            }
            else
            {
                await AddPendingWrite(_ctx, message);
            }
        }

        public override void Flush(IChannelHandlerContext context)
        {
            if (_finished)
            {
                WritePendingWrites();
                _ctx.Flush();
            }
            else
            {
                _flushedPrematurely = true;
            }
        }

        private static void ReadIfNeeded(IChannelHandlerContext ctx)
        {
            if (!ctx.Channel.Configuration.AutoRead) ctx.Read();
        }

        private void WritePendingWrites()
        {
            if (_pendingWrites != null)
            {
                _pendingWrites.RemoveAndWriteAllAsync();
                _pendingWrites = null;
            }
        }

        private void FailPendingWrites(Exception cause)
        {
            if (_pendingWrites != null)
            {
                _pendingWrites.RemoveAndFailAll(cause);
                _pendingWrites = null;
            }
        }

        private async Task AddPendingWrite(IChannelHandlerContext ctx, object msg)
        {
            var pendingWrites = _pendingWrites;
            if (pendingWrites == null) _pendingWrites = pendingWrites = new PendingWriteQueue(ctx);
            await pendingWrites.Add(msg);
        }
    }
}