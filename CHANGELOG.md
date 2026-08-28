# Changelog

## 1.0.0

- Production diagnostics SDK for .NET MAUI on iOS and Android
- Capture crashes, ANR / UI freezes, unhandled exceptions, network and API failures
- Device, OS, app version, memory, storage, battery, and connectivity snapshots
- Session context: last screen, last user action, last successful API call
- Persistent "What happened before the crash?" timeline
- `MauiDiagnostics.TrackException`, `TrackEvent`, and `GenerateReportAsync`
- Optional `HttpClient` handler for automatic API breadcrumbs
