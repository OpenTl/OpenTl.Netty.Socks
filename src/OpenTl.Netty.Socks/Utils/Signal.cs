/*
 * Copyright 2012 The Netty Project
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
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Utils
{
    /**
 * A special {@link Error} which is used to signal some state or request by throwing it.
 * {@link Signal} has an empty stack trace and has no cause to save the instantiation overhead.
 */
    public sealed class Signal : Exception,
        IConstant
    {
        private static readonly long SerialVersionUid = -221145131122459977L;

        private static readonly SignalPool Pool = new SignalPool();

        private readonly SignalConstant _constant;

        /**
         * Creates a new {@link Signal} with the specified {@code name}.
         */
        private Signal(int id, string name)
        {
            _constant = new SignalConstant(id, name);
        }

        public int Id => _constant.Id;

        public string Name => _constant.Name;

        /**
         * Check if the given {@link Signal} is the same as this instance. If not an {@link IllegalStateException} will
         * be thrown.
         */
        public void Expect(Signal signal)
        {
            if (this != signal) throw new InvalidOperationException("unexpected signal: " + signal);
        }

        public override string ToString()
        {
            return Name;
        }

        /**
         * Returns the {@link Signal} of the specified name.
         */
        public static Signal ValueOf(string name)
        {
            return (Signal) Pool.ValueOf<Signal>(name);
        }

        /**
         * Returns the {@link Signal} of the specified name.
         */
        public static Signal ValueOf<TClass>(string name)
        {
            return ValueOf(typeof(TClass).Name + "#" + name);
        }

        private sealed class SignalPool : ConstantPool
        {
            protected override IConstant NewConstant<T>(int id, string name)
            {
                return new Signal(id, name);
            }
        }

        private sealed class SignalConstant : AbstractConstant<SignalConstant>
        {
            public SignalConstant(int id, string name) : base(id, name)
            {
            }
        }
    }
}