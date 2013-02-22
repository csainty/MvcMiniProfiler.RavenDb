using System;
using System.Collections.Concurrent;
using Raven.Abstractions.Connection;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;
using StackExchange.Profiling;

namespace MvcMiniProfiler.RavenDb
{
    public class Profiler
    {
        private static ConcurrentDictionary<string, IDisposable> _Requests = new ConcurrentDictionary<string, IDisposable>();

        public static void AttachTo(DocumentStore store)
        {
            if (store == null)
                return;
            store.SessionCreatedInternal += TrackSession;
            store.AfterDispose += AfterDispose;

            if (store.JsonRequestFactory != null)
            {
                store.JsonRequestFactory.ConfigureRequest += BeginRequest;
                store.JsonRequestFactory.LogRequest += EndRequest;
            }
        }

        private static void TrackSession(InMemoryDocumentSessionOperations obj)
        {
            using (MiniProfiler.Current.Step("RavenDb: Created Session")) { }
        }

        private static void BeginRequest(object sender, WebRequestEventArgs e)
        {
            var profiler = MiniProfiler.Current;
            if (profiler != null && e.Request != null)
                _Requests.TryAdd(e.Request.RequestUri.PathAndQuery, profiler.Step("RavenDb: Query - " + e.Request.RequestUri.PathAndQuery));
        }

        private static void EndRequest(object sender, RequestResultArgs e)
        {
            IDisposable request;
            if (_Requests.TryRemove(e.Url, out request))
                if (request != null) request.Dispose();
        }

        private static void AfterDispose(object sender, EventArgs e)
        {
            var store = sender as DocumentStore;
            if (store != null)
            {
                store.SessionCreatedInternal -= TrackSession;
                store.AfterDispose -= AfterDispose;

                if (store.JsonRequestFactory != null)
                {
                    store.JsonRequestFactory.ConfigureRequest -= BeginRequest;
                    store.JsonRequestFactory.LogRequest -= EndRequest;
                }
            }
        }
    }
}