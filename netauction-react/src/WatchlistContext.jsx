import { createContext, useState, useEffect } from 'react';
import toast from 'react-hot-toast';

export const WatchlistContext = createContext();

export function WatchlistProvider({ children }) {
    const [watchlist, setWatchlist] = useState(() => {
        // Citim favoritele salvate în browser la pornire
        const saved = localStorage.getItem('netauction_watchlist');
        return saved ? JSON.parse(saved) : [];
    });

    useEffect(() => {
        localStorage.setItem('netauction_watchlist', JSON.stringify(watchlist));
    }, [watchlist]);

    const toggleFavorite = (id, titlu) => {
        setWatchlist(prev => {
            const exists = prev.includes(id);
            if (exists) {
                toast.success(`Eliminat din favorite: ${titlu}`, { icon: '🗑️' });
                return prev.filter(item => item !== id);
            } else {
                toast.success(`Adăugat la favorite: ${titlu}`, { icon: '❤️' });
                return [...prev, id];
            }
        });
    };

    return (
        <WatchlistContext.Provider value={{ watchlist, toggleFavorite }}>
            {children}
        </WatchlistContext.Provider>
    );
}