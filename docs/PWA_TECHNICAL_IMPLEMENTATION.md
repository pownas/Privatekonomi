# PWA Implementation - Technical Documentation

## Overview

Privatekonomi implements a full Progressive Web App (PWA) with offline support, background sync, push notifications, and installability. This document describes the technical implementation for developers.

## Architecture

### Components

1. **Service Worker** (`wwwroot/service-worker.js`)
   - Handles caching strategies
   - Manages offline queue (IndexedDB)
   - Background sync
   - Push notifications

2. **PWA Manager** (`wwwroot/app.js` - `window.pwaManager`)
   - Service worker registration
   - Install prompt management
   - Online/offline monitoring
   - IndexedDB operations

3. **Blazor Components** (`Components/PWA/`)
   - `OfflineIndicator.razor` - Shows offline status
   - `InstallPwaPrompt.razor` - Installation banner
   - `UpdateNotification.razor` - New version alerts

4. **Manifest** (`wwwroot/manifest.json`)
   - App metadata
   - Icons and theme
   - Display configuration

## Service Worker Implementation

### Cache Strategy

The service worker uses a **network-first, cache-fallback** strategy:

```javascript
// For dynamic content (API calls, pages)
async function networkFirst(request) {
  try {
    const networkResponse = await fetch(request);
    if (networkResponse && networkResponse.status === 200) {
      const cache = await caches.open(DYNAMIC_CACHE_NAME);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    const cachedResponse = await caches.match(request);
    if (cachedResponse) return cachedResponse;
    throw error;
  }
}

// For static resources (CSS, JS, images)
async function cacheFirst(request) {
  const cachedResponse = await caches.match(request);
  if (cachedResponse) return cachedResponse;
  
  const networkResponse = await fetch(request);
  if (networkResponse && networkResponse.status === 200) {
    const cache = await caches.open(DYNAMIC_CACHE_NAME);
    cache.put(request, networkResponse.clone());
  }
  return networkResponse;
}
```

### Cache Versioning

```javascript
const CACHE_VERSION = 'v2';
const STATIC_CACHE_NAME = `privatekonomi-static-${CACHE_VERSION}`;
const DYNAMIC_CACHE_NAME = `privatekonomi-dynamic-${CACHE_VERSION}`;
```

Old caches are automatically cleaned up on activation.

### IndexedDB for Offline Queue

```javascript
const DB_NAME = 'PrivatekonomyOfflineDB';
const DB_VERSION = 1;
const TRANSACTION_STORE = 'pendingTransactions';

// Schema
{
  id: AutoIncrement,
  data: Object,      // Transaction data
  timestamp: String  // ISO timestamp
}
```

## Background Sync

### Registration (Client-side)

```javascript
// In pwaManager.onOnline()
if ('serviceWorker' in navigator && 'sync' in navigator.serviceWorker) {
  navigator.serviceWorker.ready.then(registration => {
    return registration.sync.register('sync-transactions');
  });
}
```

### Sync Handler (Service Worker)

```javascript
self.addEventListener('sync', (event) => {
  if (event.tag === 'sync-transactions') {
    event.waitUntil(syncPendingTransactions());
  }
});

async function syncPendingTransactions() {
  const db = await openDatabase();
  const transactions = await getAllPendingTransactions(db);
  
  for (const transaction of transactions) {
    const response = await fetch('/api/transactions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(transaction.data)
    });
    
    if (response.ok) {
      await deletePendingTransaction(db, transaction.id);
      await notifyClients({
        type: 'TRANSACTION_SYNCED',
        transactionId: transaction.id
      });
    }
  }
  db.close();
}
```

## Push Notifications

### Server-side (Future Implementation)

```csharp
// TODO: Add to backend
public class PushNotificationService
{
    public async Task SendNotification(string userId, NotificationData data)
    {
        // Get user's push subscription
        var subscription = await GetUserPushSubscription(userId);
        
        // Send notification via Web Push protocol
        await webPushClient.SendNotificationAsync(
            subscription,
            JsonSerializer.Serialize(data),
            vapidDetails
        );
    }
}
```

### Client-side Subscription

```javascript
// In pwaManager
async subscribeToPush() {
  const registration = await navigator.serviceWorker.ready;
  const subscription = await registration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: urlBase64ToUint8Array(VAPID_PUBLIC_KEY)
  });
  
  // Send subscription to backend
  await fetch('/api/push/subscribe', {
    method: 'POST',
    body: JSON.stringify(subscription)
  });
  
  return subscription;
}
```

### Notification Handler (Service Worker)

```javascript
self.addEventListener('push', (event) => {
  const data = event.data.json();
  const options = {
    body: data.body,
    icon: '/icon-192x192.png',
    badge: '/favicon.png',
    vibrate: [200, 100, 200],
    data: data,
    actions: data.actions || []
  };
  
  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});
```

## Blazor-JavaScript Interop

### JavaScript to Blazor

```javascript
// Setup callbacks in pwaManager
window.blazorPwaCallbacks = {
  onOnline: () => {
    blazorPwaCallbacks.offlineIndicatorRef
      .invokeMethodAsync('OnOnlineStatusChanged', true);
  },
  onOffline: () => {
    blazorPwaCallbacks.offlineIndicatorRef
      .invokeMethodAsync('OnOnlineStatusChanged', false);
  },
  onInstallAvailable: () => {
    blazorPwaCallbacks.installPromptRef
      .invokeMethodAsync('OnInstallAvailable');
  }
};
```

### Blazor Component

```csharp
@code {
    private DotNetObjectReference<OfflineIndicator>? objRef;
    
    protected override async Task OnInitializedAsync()
    {
        objRef = DotNetObjectReference.Create(this);
        
        await JSRuntime.InvokeVoidAsync("eval", @"
            window.blazorPwaCallbacks.offlineIndicatorRef = arguments[0];
        ", objRef);
    }
    
    [JSInvokable]
    public async Task OnOnlineStatusChanged(bool isOnline)
    {
        IsOnline = isOnline;
        await InvokeAsync(StateHasChanged);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (objRef != null)
        {
            objRef.Dispose();
        }
    }
}
```

## Installation Flow

### BeforeInstallPrompt Event

```javascript
window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault();
  pwaManager.deferredPrompt = e;
  
  // Notify Blazor
  blazorPwaCallbacks.onInstallAvailable();
});
```

### Show Install Prompt

```javascript
async showInstallPrompt() {
  if (!this.deferredPrompt) return false;
  
  this.deferredPrompt.prompt();
  const { outcome } = await this.deferredPrompt.userChoice;
  
  this.deferredPrompt = null;
  return outcome === 'accepted';
}
```

### Installation Detection

```javascript
isRunningAsPWA() {
  return window.matchMedia('(display-mode: standalone)').matches ||
         window.navigator.standalone === true ||
         document.referrer.includes('android-app://');
}
```

## Update Flow

### Service Worker Update Detection

```javascript
registration.addEventListener('updatefound', () => {
  const newWorker = registration.installing;
  
  newWorker.addEventListener('statechange', () => {
    if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
      // New version available
      blazorPwaCallbacks.onUpdateAvailable();
    }
  });
});
```

### Apply Update

```javascript
applyUpdate() {
  navigator.serviceWorker.ready.then(registration => {
    if (registration.waiting) {
      registration.waiting.postMessage({ type: 'SKIP_WAITING' });
    }
  });
  
  setTimeout(() => window.location.reload(), 100);
}
```

## Testing

### Manual Testing

1. **Service Worker Registration**
   ```javascript
   // DevTools Console
   navigator.serviceWorker.getRegistration()
   ```

2. **Cache Inspection**
   - DevTools → Application → Cache Storage
   - Verify static and dynamic caches

3. **IndexedDB**
   - DevTools → Application → IndexedDB
   - Check PrivatekonomyOfflineDB

4. **Offline Mode**
   - DevTools → Network → Offline checkbox
   - Verify offline page and cached resources

### Automated Testing

```javascript
// Playwright test example
test('PWA should be installable', async ({ page }) => {
  await page.goto('/');
  
  const isInstallable = await page.evaluate(() => {
    return 'serviceWorker' in navigator;
  });
  
  expect(isInstallable).toBe(true);
});

test('Service worker should be registered', async ({ page }) => {
  await page.goto('/');
  
  const swRegistered = await page.evaluate(async () => {
    const reg = await navigator.serviceWorker.getRegistration();
    return reg !== undefined;
  });
  
  expect(swRegistered).toBe(true);
});
```

## Lighthouse Audit

To validate PWA compliance:

```bash
# Install Lighthouse
npm install -g lighthouse

# Run audit
lighthouse https://your-app.com --view --preset=desktop

# Check PWA score (should be > 90)
```

### PWA Checklist

- [x] Registers a service worker
- [x] Responds with 200 when offline
- [x] Has a web app manifest
- [x] Configures viewport
- [x] Has an icon (192x192)
- [x] Has an icon (512x512)
- [x] Theme color meta tag
- [x] Apple touch icon
- [x] HTTPS
- [x] Redirects HTTP to HTTPS

## Debugging

### Service Worker Console

```javascript
// In service worker context
console.log('[Service Worker] Message');
```

View in DevTools → Application → Service Workers → Console

### Message Passing

```javascript
// From page to service worker
navigator.serviceWorker.controller.postMessage({
  type: 'CACHE_URLS',
  urls: ['/new-page']
});

// From service worker to page
self.clients.matchAll().then(clients => {
  clients.forEach(client => {
    client.postMessage({ type: 'UPDATE_AVAILABLE' });
  });
});
```

### Common Issues

1. **Service Worker not updating**
   - Solution: Clear cache, unregister, reload
   ```javascript
   navigator.serviceWorker.getRegistrations().then(regs => {
     regs.forEach(reg => reg.unregister());
   });
   ```

2. **Offline page not showing**
   - Verify offline.html is in static assets
   - Check fetch event handler

3. **Background sync not working**
   - Requires HTTPS
   - Not supported in all browsers (check caniuse.com)

## Future Enhancements

### Periodic Background Sync

```javascript
// Request permission
const status = await navigator.permissions.query({
  name: 'periodic-background-sync'
});

// Register periodic sync
const registration = await navigator.serviceWorker.ready;
await registration.periodicSync.register('update-transactions', {
  minInterval: 24 * 60 * 60 * 1000 // Once per day
});
```

### Share Target API

```json
// In manifest.json
{
  "share_target": {
    "action": "/share",
    "method": "POST",
    "enctype": "multipart/form-data",
    "params": {
      "title": "title",
      "text": "text",
      "url": "url",
      "files": [{
        "name": "receipt",
        "accept": ["image/*", "application/pdf"]
      }]
    }
  }
}
```

### File Handling API

```json
// In manifest.json
{
  "file_handlers": [{
    "action": "/import",
    "accept": {
      "text/csv": [".csv"],
      "application/json": [".json"]
    }
  }]
}
```

## Resources

- [Service Workers API - MDN](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [Background Sync API](https://web.dev/periodic-background-sync/)
- [Push API](https://developer.mozilla.org/en-US/docs/Web/API/Push_API)
- [Web App Manifest](https://web.dev/add-manifest/)
- [IndexedDB API](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
