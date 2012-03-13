MvcMiniProfiler.RavenDb is a RavenDb profiling plugin for MvcMiniProfiler.

###Install
To setup, all you need to do is attach the profiler to your DocumentStore instance when created.  
**Note:** EmbeddableDocumentStore is not supported due to it's lack of profiling support generally.

```
var store = new DocumentStore();  
store.Initialize();  
MvcMiniProfiler.RavenDb.Profiler.AttachTo(store);  
```

###Learn More

RavenDb - http://www.ravendb.net  
MvcMiniProfiler - http://code.google.com/p/mvc-mini-profiler/  
Chris Sainty - http://blog.csainty.com | [@csainty](http://www.twitter.com/csainty/)

