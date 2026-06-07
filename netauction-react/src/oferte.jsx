import React, { useContext, useEffect } from 'react'
import ReactDOM from 'react-dom/client'
import OferteApp from './OferteApp.jsx'
import { WatchlistProvider, WatchlistContext } from './WatchlistContext.jsx'

// Mini-componentă care va updata badge-ul de favorite din Navbar-ul .NET (dacă există un element cu ID-ul respectiv)
function NavbarSync() {
    const { watchlist } = useContext(WatchlistContext);

    useEffect(() => {
        const navbarBadge = document.getElementById('watchlist-navbar-count');
        if (navbarBadge) {
            navbarBadge.innerText = watchlist.length;
            navbarBadge.style.display = watchlist.length > 0 ? 'inline-block' : 'none';
        }
    }, [watchlist]);

    return null;
}

ReactDOM.createRoot(document.getElementById('react-oferte-root')).render(
    <React.StrictMode>
        <WatchlistProvider>
            <NavbarSync />
            <OferteApp />
        </WatchlistProvider>
    </React.StrictMode>,
)