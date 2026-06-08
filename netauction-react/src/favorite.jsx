import React from 'react'
import ReactDOM from 'react-dom/client'
import FavoriteApp from './FavoriteApp.jsx'
import { WatchlistProvider } from './WatchlistContext.jsx'

ReactDOM.createRoot(document.getElementById('react-favorite-root')).render(
    <React.StrictMode>
        <WatchlistProvider>
            <FavoriteApp />
        </WatchlistProvider>
    </React.StrictMode>,
)