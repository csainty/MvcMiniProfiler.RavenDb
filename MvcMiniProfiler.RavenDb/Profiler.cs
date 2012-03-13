using System;
using System.Collections.Concurrent;
using Raven.Client.Connection;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;

namespace MvcMiniProfiler.RavenDb
{
    public class Profiler
    {
        private static ConcurrentDictionary<string, IDisposable> _Requests = new ConcurrentDictionary<string, IDisposable>();

        public static void AttachTo(DocumentStore store)
        {
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
            var profiler = MvcMiniProfiler.MiniProfiler.Current;
            if (profiler != null)
            {
                using (profiler.Step("RavenDb: Created Session")) { }
            }
        }

        private static void BeginRequest(object sender, WebRequestEventArgs e)
        {
            var profiler = MvcMiniProfiler.MiniProfiler.Current;
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