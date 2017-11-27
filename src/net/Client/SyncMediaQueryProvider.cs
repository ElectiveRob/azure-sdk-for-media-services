//-----------------------------------------------------------------------
// <copyright file="MediaQueryProvider.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.MediaServices.Client.DataServiceQuerySyncHelpers;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class SyncMediaQueryProvider<TData> : IQueryProvider
    {
        private IQueryProvider _inner;
        private MediaRetryPolicy _queryRetryPolicy;

        public SyncMediaQueryProvider(IQueryProvider inner, MediaRetryPolicy queryRetryPolicy)
        {
            _inner = inner;
            _queryRetryPolicy = queryRetryPolicy;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new SyncMediaQueryable<TElement, TData>(_inner.CreateQuery<TElement>(expression), _queryRetryPolicy);

        public IQueryable CreateQuery(Expression expression) => _inner.CreateQuery(expression);

        public object Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
        {
            MethodCallExpression m = expression as MethodCallExpression;
            if (!ReflectionUtil.TryIdentifySequenceMethod(m.Method, out var sequenceMethod))
                throw new NotSupportedException();

            switch (sequenceMethod)
            {
                case SequenceMethod.Count:
                case SequenceMethod.LongCount:
                    expression = m.Arguments[0];
                    break;
            }

            IQueryable<TResult> sourceQuery = _inner.CreateQuery<TResult>(expression);
            IQueryable<TResult> syncSource = new SyncMediaQueryable<TResult, TData>(sourceQuery, _queryRetryPolicy);
            IQueryable<TResult> source = new MediaQueryable<TResult, TData>(syncSource, _queryRetryPolicy);

            switch (sequenceMethod)
            {
                case SequenceMethod.First:
                    return source.AsEnumerable().First();
                case SequenceMethod.FirstOrDefault:
                    return source.AsEnumerable().FirstOrDefault();
                case SequenceMethod.Single:
                    return source.AsEnumerable().Single();
                case SequenceMethod.SingleOrDefault:
                    return source.AsEnumerable().SingleOrDefault();
                case SequenceMethod.Count:
                case SequenceMethod.LongCount:
                    return (TResult)Convert.ChangeType(source.Cast<object>().ToList().Count, typeof(TResult), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
