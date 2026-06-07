import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import { WatchlistProvider } from './WatchlistContext.jsx'

ReactDOM.createRoot(document.getElementById('react-app-root')).render(
    <React.StrictMode>
        <WatchlistProvider>
            <App />
        </WatchlistProvider>
    </React.StrictMode>,
)