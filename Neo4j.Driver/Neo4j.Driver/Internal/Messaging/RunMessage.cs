﻿// Copyright (c) 2002-2017 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Collections.Generic;

namespace Neo4j.Driver.Internal.Messaging
{
    internal class RunMessage : IRequestMessage
    {
        private readonly string _statement;
        private readonly IDictionary<string, object> _statementParameters;

        public RunMessage(string statement, IDictionary<string, object> statementParameters = null)
        {
            _statement = statement;
            _statementParameters = statementParameters;
        }

        public void Dispatch(IMessageRequestHandler messageRequestHandler)
        {
            messageRequestHandler.HandleRunMessage( _statement, _statementParameters );
        }

        public override string ToString()
        {
            return $"RUN `{_statement}` {_statementParameters.ValueToString()}";
        }
    }
}