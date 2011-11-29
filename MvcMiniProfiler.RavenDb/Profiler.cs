using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Raven.Client.Connection;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;

namespace MvcMiniProfiler.RavenDb
{
	public class Profiler
	{
		private static ConcurrentDictionary<string, IDisposable> _Requests = new ConcurrentDictionary<string, IDisposable>();

		public static void AttachTo(DocumentStore store) {
			store.SessionCreatedInternal += TrackSession;
			store.JsonRequestFactory.ConfigureRequest += BeginRequest;
			store.JsonRequestFactory.LogRequest += EndRequest;
		}

		private static void TrackSession(InMemoryDocumentSessionOperations obj) {
			MvcMiniProfiler.MiniProfiler.Current.Step("RavenDb: Created Session").Dispose();
		}

		private static void BeginRequest(object sender, WebRequestEventArgs e) {
			_Requests.TryAdd(e.Request.RequestUri.PathAndQuery, MvcMiniProfiler.MiniProfiler.Current.Step("RavenDb: Query - " + e.Request.RequestUri.PathAndQuery));
		}

		private static void EndRequest(object sender, RequestResultArgs e) {
			IDisposable request;
			if (_Requests.TryRemove(e.Url, out request))
				request.Dispose();
		}
	}
}