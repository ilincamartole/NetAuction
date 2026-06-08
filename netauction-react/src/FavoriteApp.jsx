import { useState, useEffect, useContext } from 'react';
import { WatchlistContext } from './WatchlistContext.jsx';
import toast, { Toaster } from 'react-hot-toast';

// --- SUB-COMPONENTA PENTRU SKELETON LOADING (SHIMMER EFFECT) ---
function SkeletonCard() {
    return (
        <div className="col-md-6 col-lg-4 animate__animated animate__fadeIn">
            <div className="card h-100 border-0 shadow-sm rounded-4 overflow-hidden" style={{ background: '#ffffff' }}>
                <div className="bg-secondary bg-opacity-10 progress-bar-striped progress-bar-animated" style={{ height: '220px', animation: 'pulse 1.5s infinite ease-in-out' }}></div>
                <div className="card-body p-4">
                    <div className="bg-secondary bg-opacity-20 rounded-pill mb-2" style={{ width: '40%', height: '15px', animation: 'pulse 1.5s infinite ease-in-out' }}></div>
                    <div className="bg-secondary bg-opacity-20 rounded mb-3" style={{ width: '80%', height: '22px', animation: 'pulse 1.5s infinite ease-in-out' }}></div>
                    <div className="bg-secondary bg-opacity-10 p-3 rounded-3 mb-3" style={{ height: '60px', animation: 'pulse 1.5s infinite ease-in-out' }}></div>
                    <div className="d-flex gap-2">
                        <div className="bg-secondary bg-opacity-20 rounded-pill flex-grow-1" style={{ height: '35px' }}></div>
                        <div className="bg-secondary bg-opacity-20 rounded-pill" style={{ width: '40px', height: '35px' }}></div>
                    </div>
                </div>
            </div>
        </div>
    );
}

// --- COMPONENTA PRINCIPALA ---
export default function FavoriteApp() {
    const { watchlist, toggleFavorite } = useContext(WatchlistContext);
    const [produse, setProduse] = useState([]);
    const [loading, setLoading] = useState(true);
    const [selectedCategory, setSelectedCategory] = useState('Toate');

    useEffect(() => {
        if (watchlist.length === 0) {
            setProduse([]);
            setLoading(false);
            return;
        }

        const promises = watchlist.map(id =>
            fetch(`/Licitatii/GetLicitatieLiveDetails/${id}`).then(res => res.json())
        );

        Promise.all(promises)
            .then(results => {
                const produseValide = results.map(r => r.licitatie);
                setProduse(produseValide);
                setLoading(false);
            })
            .catch(err => {
                console.error("Eroare fetch favorite:", err);
                toast.error("Eroare la încărcarea produselor favorite.");
                setLoading(false);
            });
    }, [watchlist]);

    const categoriiDisponibile = ['Toate', ...new Set(produse.map(p => p.categorie || p.Categorie))];

    // --- LINIE CORECTATA COMPLET FARA TEXTE PARAZITE ---
    const produseFiltrate = produse.filter(p => {
        const catCurenta = p.categorie || p.Categorie;
        return selectedCategory === 'Toate' ? true : catCurenta === selectedCategory;
    });

    return (
        <div className="container py-5 px-lg-5" style={{ minHeight: '80vh' }}>
            <Toaster position="top-right" />

            {/* Header curat */}
            <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center mb-4 gap-3 animate__animated animate__fadeIn">
                <div>
                    <h2 className="fw-bolder text-dark mb-1">Produse Favorite</h2>
                    <p className="text-muted mb-0">Urmărești evoluția prețurilor pentru {watchlist.length} obiecte salvate</p>
                </div>
                <a href="/Licitatii/Index" className="btn btn-outline-primary rounded-pill px-4 fw-bold shadow-sm">
                    <i className="bi bi-plus-circle me-2"></i>Adaugă produse
                </a>
            </div>

            {/* Pastile de Filtrare Dinamică pe Categorie */}
            {produse.length > 0 && (
                <div className="d-flex flex-wrap gap-2 mb-5 animate__animated animate__fadeIn">
                    {categoriiDisponibile.map(cat => (
                        <button
                            key={cat}
                            onClick={() => setSelectedCategory(cat)}
                            className={`btn btn-sm rounded-pill px-3 fw-bold transition-all ${selectedCategory === cat ? 'btn-dark' : 'btn-white border bg-white text-secondary'}`}
                            style={{ transition: 'all 0.2s' }}
                        >
                            {cat}
                        </button>
                    ))}
                </div>
            )}

            {/* Render logic */}
            {loading ? (
                <div className="row g-4">
                    <SkeletonCard />
                    <SkeletonCard />
                    <SkeletonCard />
                </div>
            ) : produse.length === 0 ? (
                <div className="card border-0 shadow-lg rounded-5 p-5 text-center bg-white animate__animated animate__zoomIn">
                    <div className="card-body">
                        <i className="bi bi-heart text-muted display-1 mb-4 d-block opacity-50"></i>
                        <h3 className="fw-bolder text-dark">Lista ta este goală</h3>
                        <p className="text-muted fs-5 mb-0">Explorează piața și folosește inimioara de pe fișa produselor.</p>
                    </div>
                </div>
            ) : (
                <div className="row g-4">
                    {produseFiltrate.length > 0 ? (
                        produseFiltrate.map(produs => {
                            const id = produs.id || produs.Id;
                            const titlu = produs.titlu || produs.Titlu;
                            const imaginePath = produs.imaginePath || produs.ImaginePath;
                            const categorie = produs.categorie || produs.Categorie;
                            const pretCurent = produs.pretCurent !== undefined ? produs.pretCurent : produs.PretCurent;
                            const esteIncheiata = produs.esteIncheiata !== undefined ? produs.esteIncheiata : produs.EsteIncheiata;

                            return (
                                <div key={id} className="col-md-6 col-lg-4 animate__animated animate__fadeInUp">
                                    <div
                                        className="card h-100 border-0 shadow-sm rounded-4 overflow-hidden position-relative bg-white"
                                        style={{ transition: 'all 0.2s ease-in-out' }}
                                        onMouseEnter={(e) => e.currentTarget.style.transform = 'translateY(-5px)'}
                                        onMouseLeave={(e) => e.currentTarget.style.transform = 'none'}
                                    >
                                        <div className="bg-dark d-flex align-items-center justify-content-center overflow-hidden" style={{ height: '220px' }}>
                                            {imaginePath ? (
                                                <img src={`/images/${imaginePath}`} className="w-100 h-100" alt={titlu} style={{ objectFit: 'cover' }} />
                                            ) : (
                                                <i className="bi bi-image text-white-50 display-4"></i>
                                            )}
                                        </div>

                                        <div className="card-body p-4">
                                            <span className="badge bg-light text-primary border rounded-pill px-3 py-1 mb-2 fw-bold" style={{ fontSize: '0.75rem' }}>{categorie}</span>
                                            <h5 className="card-title fw-bold text-dark mb-3 text-truncate">{titlu}</h5>

                                            <div className="d-flex justify-content-between align-items-center bg-light p-3 rounded-3 mb-3">
                                                <div>
                                                    <small className="text-muted d-block text-uppercase fw-bold" style={{ fontSize: '0.65rem' }}>Miză Curentă</small>
                                                    <span className="fw-bold text-success fs-5">{pretCurent} RON</span>
                                                </div>
                                                <span className={`badge ${esteIncheiata ? 'bg-secondary' : 'bg-success'} rounded-pill px-2 py-1 small`}>
                                                    {esteIncheiata ? 'Finalizat' : 'Activ'}
                                                </span>
                                            </div>

                                            <div className="d-flex gap-2">
                                                <a href={`/Licitatii/Details/${id}`} className="btn btn-primary rounded-pill flex-grow-1 fw-bold btn-sm py-2 shadow-sm">
                                                    <i className="bi bi-eye-fill me-1"></i> Deschide
                                                </a>
                                                <button
                                                    onClick={() => {
                                                        toggleFavorite(id, titlu);
                                                        if (produseFiltrate.length <= 1) setSelectedCategory('Toate');
                                                    }}
                                                    className="btn btn-light text-danger border rounded-pill px-3 btn-sm py-2 shadow-sm"
                                                    title="Elimină din favorite"
                                                >
                                                    <i className="bi bi-trash3-fill"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            );
                        })
                    ) : (
                        <div className="col-12 text-center p-5 text-muted animate__animated animate__fadeIn">
                            <i className="bi bi-filter-circle display-5 d-block mb-2 opacity-50"></i>
                            Nu există produse favorite în categoria "{selectedCategory}".
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}