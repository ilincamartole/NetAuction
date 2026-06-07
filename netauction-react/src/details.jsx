import React from 'react'
import ReactDOM from 'react-dom/client'
import DetailsApp from './DetailsApp.jsx'
import { WatchlistProvider } from './WatchlistContext.jsx'

ReactDOM.createRoot(document.getElementById('react-details-root')).render(
    <React.StrictMode>
        <WatchlistProvider>
            {/* Acum DetailsApp poate citi și modifica favoritele fără să mai dea eroare */}
            <DetailsApp />
        </WatchlistProvider>
    </React.StrictMode>,
)