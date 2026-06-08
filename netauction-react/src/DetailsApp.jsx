import { useState, useEffect, useRef, useContext } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import { WatchlistContext } from './WatchlistContext.jsx';

export default function DetailsApp() {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('descriere');
    const [sumaLicitata, setSumaLicitata] = useState('');
    const [timeLeft, setTimeLeft] = useState('Se calculează...');
    const [isUrgent, setIsUrgent] = useState(false);
    const [priceChanged, setPriceChanged] = useState(false);

    // Consumăm contextul global de favorite
    const { watchlist, toggleFavorite } = useContext(WatchlistContext);

    const licitatieId = window.currentLicitatieId;
    const currentUserId = window.currentUserId;
    const isAdmin = window.isUserAdmin;
    const prevPriceRef = useRef(null);

    const fetchLiveDetails = () => {
        if (!licitatieId) return;
        fetch(`/Licitatii/GetLicitatieLiveDetails/${licitatieId}`)
            .then(res => res.json())
            .then(resData => {
                if (prevPriceRef.current !== null && resData.licitatie.pretCurent !== prevPriceRef.current) {
                    setPriceChanged(true);
                    toast('Prețul acestei licitații s-a modificat live!', {
                        icon: '⚡',
                        style: { background: '#fff3cd', color: '#856404', fontWeight: 'bold' }
                    });
                    setTimeout(() => setPriceChanged(false), 1500);
                }
                prevPriceRef.current = resData.licitatie.pretCurent;
                setData(resData);
                setLoading(false);
            })
            .catch(err => {
                console.error(err);
                toast.error("Eroare la sincronizarea datelor live.");
            });
    };

    useEffect(() => {
        fetchLiveDetails();
        const interval = setInterval(fetchLiveDetails, 5000);
        return () => clearInterval(interval);
    }, []);

    useEffect(() => {
        if (!data || data.licitatie.esteIncheiata) return;

        const timer = setInterval(() => {
            const target = new Date(data.licitatie.dataFinalizare).getTime();
            const now = new Date().getTime();
            const distance = target - now;

            if (distance < 0) {
                clearInterval(timer);
                setTimeLeft('ÎNCHEIATĂ');
                return;
            }

            const days = Math.floor(distance / (1000 * 60 * 60 * 24));
            const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            const seconds = Math.floor((distance % (1000 * 60)) / 1000);

            if (distance < 3600000) setIsUrgent(true);

            setTimeLeft(`${days > 0 ? days + 'z ' : ''}${hours.toString().padStart(2, '0')}h ${minutes.toString().padStart(2, '0')}m ${seconds.toString().padStart(2, '0')}s`);
        }, 1000);

        return () => clearInterval(timer);
    }, [data]);

    const handleLicitare = async (e) => {
        e.preventDefault();
        const valoare = parseFloat(sumaLicitata);

        if (isNaN(valoare) || valoare <= data.licitatie.pretCurent) {
            toast.error(`Oferta trebuie să fie mai mare de ${data.licitatie.pretCurent} RON!`);
            return;
        }

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const formData = new FormData();
        formData.append('id', licitatieId);
        formData.append('sumaLicitata', valoare);
        if (token) formData.append('__RequestVerificationToken', token);

        const loadToast = toast.loading('Se procesează oferta...');

        try {
            const response = await fetch('/Licitatii/Licitare', { method: 'POST', body: formData });
            if (response.redirected) {
                toast.success('Ofertă plasată cu succes! Ești noul lider 👑', { id: loadToast });
                setSumaLicitata('');
                fetchLiveDetails();
            } else {
                throw new Error();
            }
        } catch (err) {
            toast.error('Tranzacție eșuată.', { id: loadToast });
        }
    };

    if (loading) {
        return (
            <div className="text-center p-5 animate__animated animate__fadeIn">
                <div className="spinner-border text-primary" role="status" style={{ width: '3rem', height: '3rem' }}></div>
                <p className="mt-3 text-muted fw-bold">Se încarcă datele panoului...</p>
            </div>
        );
    }

    const { licitatie, sellerName, winnerName, winnerFullName, winnerEmail, winnerAddress, istoricOferte } = data;
    const isFavorite = watchlist.includes(licitatie.id);

    return (
        <div className="row g-5 text-start animate__animated animate__fadeIn">
            <Toaster position="top-right" />
            <div className="col-lg-7">
                {/* Header Titlu Curățat + Buton Inimioară Interactiv */}
                <div className="d-flex justify-content-between align-items-center mb-4 bg-white p-4 rounded-4 shadow-sm border">
                    <div>
                        <h2 className="fw-bold text-dark mb-1">{licitatie.titlu}</h2>
                        <span className="badge bg-light text-secondary border rounded-pill px-3 py-2">ID Produs: #{licitatie.id}</span>
                    </div>
                    <button
                        onClick={() => toggleFavorite(licitatie.id, licitatie.titlu)}
                        className={`btn rounded-circle d-flex align-items-center justify-content-center border-0 shadow-sm transition-all`}
                        style={{ width: '55px', height: '55px', backgroundColor: isFavorite ? '#ffe5ec' : '#f8f9fa', transition: 'all 0.2s' }}
                    >
                        <i className={`bi ${isFavorite ? 'bi-heart-fill text-danger' : 'bi-heart text-secondary'} fs-3`}></i>
                    </button>
                </div>

                <div className="card border-0 shadow-lg rounded-5 overflow-hidden">
                    <div className="position-relative bg-dark d-flex align-items-center justify-content-center" style={{ minHeight: '550px' }}>
                        {licitatie.imaginePath ? (
                            <img src={`/images/${licitatie.imaginePath}`} className="img-fluid w-100" alt={licitatie.titlu} style={{ maxHeight: '650px', objectFit: 'contain' }} />
                        ) : (
                            <div className="text-white-50 text-center">
                                <i className="bi bi-image display-1"></i>
                                <p className="mt-3 fs-5">Imaginea nu a fost încărcată</p>
                            </div>
                        )}
                        <div className="position-absolute top-0 end-0 m-4">
                            <span className="badge bg-dark bg-opacity-50 text-white px-3 py-2 rounded-pill shadow-sm" style={{ backdropFilter: 'blur(10px)' }}>
                                <i className="bi bi-tag-fill me-1"></i> {licitatie.categorie}
                            </span>
                        </div>
                    </div>
                </div>

                <div className="mt-5">
                    <ul className="nav nav-pills mb-4 gap-2">
                        <li className="nav-item">
                            <button className={`nav-link rounded-pill px-4 fw-bold ${activeTab === 'descriere' ? 'active' : 'bg-white text-dark'}`} onClick={() => setActiveTab('descriere')}>Descriere</button>
                        </li>
                        <li className="nav-item">
                            <button className={`nav-link rounded-pill px-4 fw-bold ${activeTab === 'istoric' ? 'active' : 'bg-white text-dark'}`} onClick={() => setActiveTab('istoric')}>Istoric Oferte</button>
                        </li>
                    </ul>

                    <div className="tab-content">
                        {activeTab === 'descriere' && (
                            <div className="card border-0 shadow-sm rounded-5 p-4 animate__animated animate__fadeIn">
                                <h4 className="fw-bold mb-4 text-dark">Despre acest obiect</h4>
                                <p className="text-secondary fs-5" style={{ whiteSpace: 'pre-line', lineHeight: '1.8' }}>{licitatie.descriere}</p>
                            </div>
                        )}
                        {activeTab === 'istoric' && (
                            <div className="card border-0 shadow-sm rounded-5 p-4 animate__animated animate__fadeIn">
                                {istoricOferte.length > 0 ? (
                                    <div className="table-responsive">
                                        <table className="table table-hover align-middle border-0">
                                            <thead>
                                                <tr className="text-muted small text-uppercase">
                                                    <th className="border-0">Ofertant</th>
                                                    <th className="border-0">Sumă</th>
                                                    <th className="border-0 text-end">Dată</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {istoricOferte.map((bid, idx) => (
                                                    <tr key={bid.id} className={idx === 0 ? "table-success bg-opacity-10" : ""}>
                                                        <td className="border-0 fw-bold text-dark">{bid.username} {idx === 0 && !licitatie.esteIncheiata && '👑'}</td>
                                                        <td className="border-0"><span className={`badge ${idx === 0 ? 'bg-success' : 'bg-success-subtle text-success'} rounded-pill px-3`}>{bid.suma} RON</span></td>
                                                        <td className="border-0 text-end text-muted small">{bid.data}</td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    </div>
                                ) : (
                                    <p className="text-muted text-center my-3">Nu există nicio ofertă plasată.</p>
                                )}
                            </div>
                        )}
                    </div>
                </div>
            </div>

            <div className="col-lg-5">
                <div className="sticky-top" style={{ top: '2rem', zIndex: 10 }}>
                    <div className="card border-0 shadow-2-strong rounded-5 overflow-hidden">
                        <div className={`${licitatie.esteIncheiata ? 'bg-secondary' : isUrgent ? 'bg-danger' : 'bg-primary'} p-4 text-white text-center`}>
                            <small className="text-white-50 text-uppercase fw-bold d-block">{licitatie.esteIncheiata ? 'Licitație Finalizată' : 'Timpul Rămas:'}</small>
                            <div className={`display-6 fw-bolder mt-1 ${isUrgent && !licitatie.esteIncheiata ? 'animate__animated animate__pulse animate__infinite' : ''}`}>
                                {licitatie.esteIncheiata ? 'ÎNCHEIATĂ' : timeLeft}
                            </div>
                        </div>

                        <div className={`card-body p-4 p-xl-5 bg-white transition-all ${priceChanged ? 'bg-warning bg-opacity-25 animate__animated animate__shakeX' : ''}`} style={{ transition: 'all 0.4s ease' }}>
                            <div className="d-flex justify-content-between align-items-center mb-4">
                                <div>
                                    <small className="text-muted d-block fw-bold text-uppercase">Ofertă Curentă</small>
                                    <h2 className="text-success fw-bolder display-5 mb-0">{licitatie.pretCurent} <small className="fs-4">RON</small></h2>
                                </div>
                                <div className="text-end">
                                    <small className="text-muted d-block fw-bold text-uppercase">Preț Start</small>
                                    <span className="fw-bold text-dark fs-5">{licitatie.pretPornire} RON</span>
                                </div>
                            </div>

                            {licitatie.esteIncheiata ? (
                                <div className="alert alert-dark rounded-4 p-4 text-center border-0 shadow-sm mb-4">
                                    <i className="bi bi-trophy-fill display-5 text-warning mb-3 d-block"></i>
                                    <h5 className="fw-bold">Rezultat Final</h5>
                                    {winnerName ? (
                                        <>
                                            <p className="mb-0 fs-5">Câștigător: <span className="text-primary fw-bolder">{winnerName}</span></p>
                                            {isAdmin && (
                                                <div className="mt-4 p-3 bg-white rounded-4 border text-start shadow-sm small">
                                                    <h6 className="fw-bold text-primary mb-2"><i className="bi bi-shield-lock me-2"></i>Date Contact Securizate</h6>
                                                    <div>Nume: <strong>{winnerFullName}</strong></div>
                                                    <div>Email: <strong>{winnerEmail}</strong></div>
                                                    <div>Adresă: <strong>{winnerAddress}</strong></div>
                                                </div>
                                            )}
                                        </>
                                    ) : (
                                        <p className="mb-0 text-muted">Licitație încheiată fără oferte.</p>
                                    )}
                                </div>
                            ) : currentUserId === licitatie.seller_id ? (
                                <div className="alert alert-info rounded-5 p-4 text-center border-0 shadow-sm mb-4">
                                    <i className="bi bi-info-circle-fill display-6 text-primary mb-2 d-block"></i>
                                    <h6 className="fw-bold">Gestiune Panou</h6>
                                    <p className="mb-0 text-muted small">Nu poți plasa oferte la propriul tău produs.</p>
                                </div>
                            ) : (
                                <div className="p-4 bg-light rounded-5 border border-dashed border-primary border-2 mb-4">
                                    <label className="form-label fw-bolder text-dark mb-3">Crește miza acum</label>
                                    <form onSubmit={handleLicitare}>
                                        <div className="input-group input-group-lg mb-3 shadow-sm rounded-pill overflow-hidden border-0">
                                            <span className="input-group-text bg-white border-0 text-primary fw-bold">RON</span>
                                            <input type="number" className="form-control border-0 ps-0 fw-bold" value={sumaLicitata} onChange={(e) => setSumaLicitata(e.target.value)} placeholder={(parseFloat(licitatie.pretCurent) + 1).toString()} step="0.01" required />
                                        </div>
                                        <button type="submit" className="btn btn-primary btn-lg w-100 rounded-pill fw-bolder py-3 shadow-lg hover-up-btn">
                                            <i className="bi bi-hammer me-2"></i>Plasează Ofertă
                                        </button>
                                    </form>
                                </div>
                            )}

                            <div className="d-flex align-items-center p-3 rounded-4 bg-light">
                                <div className="avatar-md bg-white text-primary rounded-circle d-flex align-items-center justify-content-center me-3 shadow-sm" style={{ width: '50px', height: '50px' }}>
                                    <i className="bi bi-person-badge fs-4"></i>
                                </div>
                                <div>
                                    <small className="text-muted d-block">Vânzător autorizat</small>
                                    <span className="text-dark fw-bold">{sellerName}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}