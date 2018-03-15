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
using System.Text;
using OpenTl.Netty.Socks.Utils;

namespace OpenTl.Netty.Socks.DecoderResults
{
    public sealed class DecoderResult
    {
        private static readonly Signal SignalUnfinished = Signal.ValueOf<DecoderResult>("UNFINISHED");

        private static readonly Signal SignalSuccess = Signal.ValueOf<DecoderResult>("SUCCESS");

        public static readonly DecoderResult Unfinished = new DecoderResult(SignalUnfinished);

        public static readonly DecoderResult Success = new DecoderResult(SignalSuccess);

        private readonly Exception _cause;

        private DecoderResult(Exception cause)
        {
            _cause = cause;
        }

        public Exception Cause
        {
            get
            {
                if (IsFailure()) return _cause;

                return null;
            }
        }

        public static DecoderResult Failure(Exception cause)
        {
            return new DecoderResult(cause);
        }

        public bool IsFailure()
        {
            return _cause != SignalSuccess && _cause != SignalUnfinished;
        }

        public bool IsFinished()
        {
            return _cause != SignalUnfinished;
        }

        public bool IsSuccess()
        {
            return _cause == SignalSuccess;
        }

        public override string ToString()
        {
            if (IsFinished())
            {
                if (IsSuccess()) return "success";

                var cause = Cause.ToString();
                return new StringBuilder(cause.Length + 17)
                    .Append("failure(")
                    .Append(cause)
                    .Append(')')
                    .ToString();
            }

            return "unfinished";
        }
    }
}