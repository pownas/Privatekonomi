// Download file function for CSV exports
window.downloadFile = function(filename, contentType, base64Content) {
    const blob = base64ToBlob(base64Content, contentType);
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Helper function to convert base64 to Blob
function base64ToBlob(base64, contentType) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: contentType });
}

// Theme management functions
window.themeManager = {
    getTheme: function() {
        return localStorage.getItem('darkMode') === 'true';
    },
    setTheme: function(isDarkMode) {
        localStorage.setItem('darkMode', isDarkMode.toString());
        // Also apply the theme class immediately
        if (isDarkMode) {
            document.documentElement.classList.add('mud-theme-dark');
        } else {
            document.documentElement.classList.remove('mud-theme-dark');
        }
    },
    hasPreference: function() {
        return localStorage.getItem('darkMode') !== null;
    },
    // Get the initial theme state (what was pre-applied in the head script)
    // NOTE: This logic mirrors the inline script in App.razor <head>
    // Both must stay in sync to prevent theme flash on page load
    getInitialTheme: function() {
        var darkMode = localStorage.getItem('darkMode');
        if (darkMode === 'true') {
            return true;
        } else if (darkMode === 'false') {
            return false;
        } else {
            // No preference saved, check system preference
            return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
    }
};

// View density management functions
window.viewDensityManager = {
    getViewDensity: function() {
        return localStorage.getItem('viewDensity') === 'compact' ? 'compact' : 'spacious';
    },
    setViewDensity: function(isCompact) {
        const density = isCompact ? 'compact' : 'spacious';
        localStorage.setItem('viewDensity', density);
        // Apply the view density class immediately
        document.documentElement.classList.remove('view-density-compact', 'view-density-spacious');
        document.documentElement.classList.add('view-density-' + density);
    },
    hasPreference: function() {
        return localStorage.getItem('viewDensity') !== null;
    },
    // Get the initial view density state (what was pre-applied in the head script)
    getInitialViewDensity: function() {
        var viewDensity = localStorage.getItem('viewDensity');
        if (viewDensity === 'compact') {
            return true; // true = compact
        } else {
            return false; // false = spacious (default)
        }
    },
    isSmallScreen: function() {
        if (window.matchMedia) {
            return window.matchMedia('(max-width: 640px)').matches;
        }

        return window.innerWidth <= 640;
    }
};

// Keyboard shortcuts manager
window.keyboardShortcuts = {
    init: function(dotNetHelper) {
        // Store reference to .NET helper
        this.dotNetHelper = dotNetHelper;
        
        // Add keyboard event listener
        document.addEventListener('keydown', (e) => {
            // Ignore if user is typing in an input field
            if (e.target.tagName === 'INPUT' || 
                e.target.tagName === 'TEXTAREA' || 
                e.target.isContentEditable) {
                return;
            }
            
            // Ctrl/Cmd + N: New Transaction
            if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                e.preventDefault();
                this.navigate('/transactions/new');
            }
            // Ctrl/Cmd + B: Budgets
            else if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
                e.preventDefault();
                this.navigate('/budgets');
            }
            // Ctrl/Cmd + T: Transactions
            else if ((e.ctrlKey || e.metaKey) && e.key === 't') {
                e.preventDefault();
                this.navigate('/transactions');
            }
            // Ctrl/Cmd + H: Home/Dashboard
            else if ((e.ctrlKey || e.metaKey) && e.key === 'h') {
                e.preventDefault();
                this.navigate('/');
            }
            // Ctrl/Cmd + G: Goals
            else if ((e.ctrlKey || e.metaKey) && e.key === 'g') {
                e.preventDefault();
                this.navigate('/goals');
            }
            // Ctrl/Cmd + I: Investments
            else if ((e.ctrlKey || e.metaKey) && e.key === 'i') {
                e.preventDefault();
                this.navigate('/investments');
            }
            // Ctrl/Cmd + K: Calendar
            else if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                this.navigate('/transactions/calendar');
            }
            // Ctrl/Cmd + L: Tags
            else if ((e.ctrlKey || e.metaKey) && e.key === 'l') {
                e.preventDefault();
                this.navigate('/tags');
            }
            // Ctrl/Cmd + /: Show keyboard shortcuts help
            else if ((e.ctrlKey || e.metaKey) && e.key === '/') {
                e.preventDefault();
                if (this.dotNetHelper) {
                    this.dotNetHelper.invokeMethodAsync('ShowKeyboardShortcutsHelp');
                }
            }
        });
    },
    
    // Navigate while preserving theme to prevent flash
    navigate: function(url) {
        // Ensure theme class persists during navigation
        var isDark = document.documentElement.classList.contains('mud-theme-dark');
        if (isDark) {
            // Add a temporary attribute to ensure theme persists
            document.documentElement.setAttribute('data-force-dark', 'true');
        }
        window.location.href = url;
    },
    
    dispose: function() {
        // Remove event listener if needed
        this.dotNetHelper = null;
    }
};

// PWA Service Worker registration and management
window.pwaManager = {
    deferredPrompt: null,
    isOnline: navigator.onLine,
    pendingTransactions: [],
    
    // Initialize PWA features
    init: function() {
        this.register();
        this.setupInstallPrompt();
        this.setupOnlineStatusMonitoring();
        this.setupServiceWorkerMessages();
        this.checkPendingTransactions();
    },
    
    // Register service worker
    register: function() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/service-worker.js')
                .then((registration) => {
                    console.log('[PWA] Service Worker registered:', registration);
                    
                    // Check for updates
                    registration.addEventListener('updatefound', () => {
                        const newWorker = registration.installing;
                        console.log('[PWA] New service worker found');
                        
                        newWorker.addEventListener('statechange', () => {
                            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                                // New service worker available
                                console.log('[PWA] New version available');
                                this.showUpdateNotification();
                            }
                        });
                    });
                    
                    // Periodic update check (every hour)
                    setInterval(() => {
                        registration.update();
                    }, 60 * 60 * 1000);
                })
                .catch((error) => {
                    console.error('[PWA] Service Worker registration failed:', error);
                });
        }
    },
    
    // Setup install prompt capture
    setupInstallPrompt: function() {
        window.addEventListener('beforeinstallprompt', (e) => {
            console.log('[PWA] Install prompt available');
            e.preventDefault();
            this.deferredPrompt = e;
            
            // Notify app that install is available
            if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onInstallAvailable) {
                window.blazorPwaCallbacks.onInstallAvailable();
            }
        });
        
        window.addEventListener('appinstalled', () => {
            console.log('[PWA] App installed');
            this.deferredPrompt = null;
            
            // Notify app of successful installation
            if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onInstalled) {
                window.blazorPwaCallbacks.onInstalled();
            }
        });
    },
    
    // Show install prompt
    showInstallPrompt: async function() {
        if (!this.deferredPrompt) {
            console.log('[PWA] Install prompt not available');
            return false;
        }
        
        this.deferredPrompt.prompt();
        const { outcome } = await this.deferredPrompt.userChoice;
        console.log(`[PWA] User response to install prompt: ${outcome}`);
        
        this.deferredPrompt = null;
        return outcome === 'accepted';
    },
    
    // Check if app can be installed
    canInstall: function() {
        return this.deferredPrompt !== null;
    },
    
    // Monitor online/offline status
    setupOnlineStatusMonitoring: function() {
        window.addEventListener('online', () => {
            console.log('[PWA] Browser is online');
            this.isOnline = true;
            this.onOnline();
        });
        
        window.addEventListener('offline', () => {
            console.log('[PWA] Browser is offline');
            this.isOnline = false;
            this.onOffline();
        });
    },
    
    // Handle online event
    onOnline: function() {
        // Notify Blazor app
        if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onOnline) {
            window.blazorPwaCallbacks.onOnline();
        }
        
        // Trigger background sync if supported
        if ('serviceWorker' in navigator && 'sync' in navigator.serviceWorker) {
            navigator.serviceWorker.ready.then(registration => {
                return registration.sync.register('sync-transactions');
            }).then(() => {
                console.log('[PWA] Background sync registered');
            }).catch(err => {
                console.error('[PWA] Background sync failed:', err);
            });
        }
    },
    
    // Handle offline event
    onOffline: function() {
        // Notify Blazor app
        if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onOffline) {
            window.blazorPwaCallbacks.onOffline();
        }
    },
    
    // Setup service worker message handling
    setupServiceWorkerMessages: function() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.addEventListener('message', (event) => {
                console.log('[PWA] Message from service worker:', event.data);
                
                if (event.data && event.data.type === 'TRANSACTION_SYNCED') {
                    // Notify Blazor app about synced transaction
                    if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onTransactionSynced) {
                        window.blazorPwaCallbacks.onTransactionSynced(event.data.transactionId);
                    }
                }
            });
        }
    },
    
    // Queue transaction for offline sync
    queueTransaction: async function(transactionData) {
        if (!('indexedDB' in window)) {
            console.error('[PWA] IndexedDB not supported');
            return false;
        }
        
        try {
            const db = await this.openDatabase();
            const transaction = db.transaction(['pendingTransactions'], 'readwrite');
            const store = transaction.objectStore('pendingTransactions');
            
            const item = {
                data: transactionData,
                timestamp: new Date().toISOString()
            };
            
            await new Promise((resolve, reject) => {
                const request = store.add(item);
                request.onsuccess = () => resolve(request.result);
                request.onerror = () => reject(request.error);
            });
            
            console.log('[PWA] Transaction queued for sync');
            
            // Update pending count
            await this.checkPendingTransactions();
            
            db.close();
            return true;
        } catch (error) {
            console.error('[PWA] Failed to queue transaction:', error);
            return false;
        }
    },
    
    // Get pending transactions count
    checkPendingTransactions: async function() {
        if (!('indexedDB' in window)) {
            return 0;
        }
        
        try {
            const db = await this.openDatabase();
            const transaction = db.transaction(['pendingTransactions'], 'readonly');
            const store = transaction.objectStore('pendingTransactions');
            
            const count = await new Promise((resolve, reject) => {
                const request = store.count();
                request.onsuccess = () => resolve(request.result);
                request.onerror = () => reject(request.error);
            });
            
            db.close();
            
            // Notify Blazor app
            if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onPendingCountChanged) {
                window.blazorPwaCallbacks.onPendingCountChanged(count);
            }
            
            return count;
        } catch (error) {
            console.error('[PWA] Failed to check pending transactions:', error);
            return 0;
        }
    },
    
    // Open IndexedDB
    openDatabase: function() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('PrivatekonomyOfflineDB', 1);
            
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
            
            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                
                if (!db.objectStoreNames.contains('pendingTransactions')) {
                    const store = db.createObjectStore('pendingTransactions', { 
                        keyPath: 'id', 
                        autoIncrement: true 
                    });
                    store.createIndex('timestamp', 'timestamp', { unique: false });
                }
            };
        });
    },
    
    // Show update notification
    showUpdateNotification: function() {
        if (window.blazorPwaCallbacks && window.blazorPwaCallbacks.onUpdateAvailable) {
            window.blazorPwaCallbacks.onUpdateAvailable();
        }
    },
    
    // Apply update (reload page with new service worker)
    applyUpdate: function() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.ready.then(registration => {
                if (registration.waiting) {
                    registration.waiting.postMessage({ type: 'SKIP_WAITING' });
                }
            });
            
            // Reload page after a short delay
            setTimeout(() => {
                window.location.reload();
            }, 100);
        }
    },
    
    // Request notification permission
    requestNotificationPermission: async function() {
        if (!('Notification' in window)) {
            console.log('[PWA] Notifications not supported');
            return false;
        }
        
        const permission = await Notification.requestPermission();
        console.log('[PWA] Notification permission:', permission);
        return permission === 'granted';
    },
    
    // Subscribe to push notifications
    subscribeToPush: async function() {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            console.log('[PWA] Push notifications not supported');
            return null;
        }
        
        try {
            const registration = await navigator.serviceWorker.ready;
            
            // Check if already subscribed
            let subscription = await registration.pushManager.getSubscription();
            
            if (!subscription) {
                // Request permission first
                const hasPermission = await this.requestNotificationPermission();
                if (!hasPermission) {
                    return null;
                }
                
                // Subscribe to push
                subscription = await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: this.urlBase64ToUint8Array(
                        // TODO: Replace with actual VAPID public key from server
                        'BEl62iUYgUivxIkv69yViEuiBIa-Ib37J8xQmrpcPBblQjBIL1WsJ3-eN6_JG-eL5E2QdN3qZPTaC-lJQJqG1XY'
                    )
                });
            }
            
            return subscription;
        } catch (error) {
            console.error('[PWA] Failed to subscribe to push:', error);
            return null;
        }
    },
    
    // Helper to convert VAPID key
    urlBase64ToUint8Array: function(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, '+')
            .replace(/_/g, '/');
        
        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);
        
        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    },
    
    // Check if app is running as PWA
    isRunningAsPWA: function() {
        return window.matchMedia('(display-mode: standalone)').matches ||
               window.navigator.standalone === true ||
               document.referrer.includes('android-app://');
    }
};