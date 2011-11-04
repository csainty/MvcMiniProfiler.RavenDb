using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcMiniProfiler;
using Raven.Client.Connection;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;

namespace MvcMiniProfiler.RavenDb
{
	public class Profiler
	{
		private static Dictionary<string, IDisposable> _Requests = new Dictionary<string, IDisposable>();

		public static void AttachTo(DocumentStore store) {
			store.SessionCreatedInternal += TrackSession;
			store.JsonRequestFactory.ConfigureRequest += BeginRequest;
			store.JsonRequestFactory.LogRequest += EndREquest;
		}

		private static void TrackSession(InMemoryDocumentSessionOperations obj) {
			MvcMiniProfiler.MiniProfiler.Current.Step("RavenDb: Created Session").Dispose();
		}

		private static void BeginRequest(object sender, WebRequestEventArgs e) {
			_Requests.Add(e.Request.RequestUri.PathAndQuery, MvcMiniProfiler.MiniProfiler.Current.Step("RavenDb: Query - " + e.Request.RequestUri.PathAndQuery));
		}

		private static void EndREquest(object sender, RequestResultArgs e) {
			IDisposable request;
			if (_Requests.TryGetValue(e.Url, out request))
				request.Dispose();
		}
	}
}