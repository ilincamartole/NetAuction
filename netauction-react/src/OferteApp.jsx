import { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';
import toast, { Toaster } from 'react-hot-toast';

export default function OferteApp() {
    const [bids, setBids] = useState([]);
    const [loading, setLoading] = useState(true);
    const [statusFilter, setStatusFilter] = useState('toate');

    useEffect(() => {
        fetch('/Licitatii/GetDashboardData')
            .then(res => res.json())
            .then(data => {
                setBids(data);
                setLoading(false);
                const areWins = data.some(b => b.licitatie.esteIncheiata && b.licitatie.castigatorId === b.userId);
                if (areWins) {
                    toast.success('Ai licitații câștigate recent! 🎉', { duration: 5000 });
                }
            })
            .catch(err => {
                console.error(err);
                setLoading(false);
                toast.error('Eroare la încărcarea istoricului.');
            });
    }, []);

    const totalBids = bids.length;
    let castigate = 0;
    let lider = 0;
    let depasite = 0;

    bids.forEach(bid => {
        if (bid.licitatie.esteIncheiata) {
            if (bid.licitatie.castigatorId === bid.userId) castigate++;
        } else {
            if (bid.suma === bid.licitatie.pretCurent) lider++;
            else depasite++;
        }
    });

    const filteredBids = bids.filter(bid => {
        if (statusFilter === 'toate') return true;
        if (statusFilter === 'castigate') return bid.licitatie.esteIncheiata && bid.licitatie.castigatorId === bid.userId;
        if (statusFilter === 'lider') return !bid.licitatie.esteIncheiata && bid.suma === bid.licitatie.pretCurent;
        if (statusFilter === 'depasite') return !bid.licitatie.esteIncheiata && bid.suma !== bid.licitatie.pretCurent;
        return true;
    });

    const chartData = [
        { name: 'Câștigate', value: castigate, filterKey: 'castigate' },
        { name: 'Lider de preț', value: lider, filterKey: 'lider' },
        { name: 'Mize depășite', value: depasite, filterKey: 'depasite' }
    ].filter(item => item.value > 0);

    const COLORS = ['#198754', '#0d6efd', '#dc3545'];

    const handleChartClick = (state) => {
        if (state && state.filterKey) {
            setStatusFilter(state.filterKey);
        }
    };

    if (loading) {
        return (
            <div className="text-center p-5 animate__animated animate__fadeIn">
                <div className="spinner-border text-primary" role="status" style={{ width: '3rem', height: '3rem' }}></div>
                <p className="mt-3 text-muted fw-bold">Se încarcă istoricul activităților...</p>
            </div>
        );
    }

    return (
        <div className="container-fluid py-5 px-lg-5" style={{ background: 'linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)', minHeight: '90vh' }}>
            <Toaster position="top-right" />

            <div className="row mb-5 g-4 animate__animated animate__fadeIn">
                <div className="col-12 col-xl-4">
                    <div className="p-4 rounded-5 shadow-sm border-0 bg-white h-100 d-flex flex-column justify-content-center">
                        <h6 className="text-uppercase text-muted fw-bold small mb-2 tracking-wider">Panou Control</h6>
                        <h2 className="fw-bolder mb-0 display-6">Activitatea Mea</h2>
                        <p className="text-muted small mb-0 mt-2">Centralizator în timp real al mizei și performanței.</p>
                    </div>
                </div>

                <div className="col-6 col-xl-4">
                    <div className="p-4 rounded-5 shadow-sm border-0 bg-white h-100 d-flex align-items-center" style={{ cursor: 'pointer' }} onClick={() => setStatusFilter('lider')}>
                        <div className="icon-shape bg-primary-subtle text-primary rounded-4 p-3 me-4 shadow-sm" style={{ width: '65px', height: '65px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <i className="bi bi-lightning-charge fs-2"></i>
                        </div>
                        <div>
                            <h6 className="text-muted small mb-0 fw-bold">Oferte Active Lider</h6>
                            <h3 className="fw-bolder mb-0 display-5 text-primary">{lider}</h3>
                        </div>
                    </div>
                </div>

                <div className="col-6 col-xl-4">
                    <div className="p-4 rounded-5 shadow-sm border-0 bg-white h-100 d-flex align-items-center" style={{ cursor: 'pointer' }} onClick={() => setStatusFilter('castigate')}>
                        <div className="icon-shape bg-success-subtle text-success rounded-4 p-3 me-4 shadow-sm" style={{ width: '65px', height: '65px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <i className="bi bi-trophy fs-2"></i>
                        </div>
                        <div>
                            <h6 className="text-muted small mb-0 fw-bold">Licitații Câștigate</h6>
                            <h3 className="fw-bolder mb-0 display-5 text-success">{castigate}</h3>
                        </div>
                    </div>
                </div>
            </div>

            {totalBids === 0 ? (
                <div className="card border-0 shadow-lg rounded-5 p-5 text-center bg-white">
                    <div className="card-body">
                        <i className="bi bi-search display-1 text-muted mb-4"></i>
                        <h3 className="fw-bolder text-dark">Nu ai nicio activitate încă</h3>
                        <p className="text-muted fs-5 mb-4">Începe să licitezi pe site pentru a debloca graficele de performanță.</p>
                    </div>
                </div>
            ) : (
                <div className="row g-4">
                    <div className="col-lg-8 animate__animated animate__fadeInLeft">
                        <div className="card border-0 shadow-lg rounded-5 overflow-hidden bg-white h-100">
                            <div className="card-header bg-white p-4 border-0 d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3">
                                <h4 className="fw-bold mb-0 text-dark">Istoric Tranzacții</h4>
                                <div className="d-flex flex-wrap gap-2">
                                    <button onClick={() => setStatusFilter('toate')} className={`btn btn-sm rounded-pill px-3 fw-bold transition-all ${statusFilter === 'toate' ? 'btn-dark' : 'btn-light text-secondary'}`}>Toate ({totalBids})</button>
                                    <button onClick={() => setStatusFilter('castigate')} className={`btn btn-sm rounded-pill px-3 fw-bold transition-all ${statusFilter === 'castigate' ? 'btn-success' : 'btn-light text-success'}`}>Câștigate ({castigate})</button>
                                    <button onClick={() => setStatusFilter('lider')} className={`btn btn-sm rounded-pill px-3 fw-bold transition-all ${statusFilter === 'lider' ? 'btn-primary' : 'btn-light text-primary'}`}>Active ({lider})</button>
                                    <button onClick={() => setStatusFilter('depasite')} className={`btn btn-sm rounded-pill px-3 fw-bold transition-all ${statusFilter === 'depasite' ? 'btn-danger' : 'btn-light text-danger'}`}>Depășite ({depasite})</button>
                                </div>
                            </div>

                            <div className="table-responsive">
                                <table className="table table-hover align-middle mb-0">
                                    <thead className="bg-light text-muted small text-uppercase fw-bold">
                                        <tr>
                                            <th className="ps-4 py-3">Produs</th>
                                            <th className="py-3">Miza Ta</th>
                                            <th className="py-3">Status Activitate</th>
                                            <th className="text-end pe-4 py-3">Acțiune</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {filteredBids.length > 0 ? (
                                            filteredBids.map(bid => (
                                                <tr key={bid.id} className="animate__animated animate__fadeIn" style={{ transition: 'all 0.2s ease-in-out' }} onMouseEnter={(e) => e.currentTarget.style.transform = 'scale(1.008)'} onMouseLeave={(e) => e.currentTarget.style.transform = 'none'}>
                                                    <td className="ps-4 py-4">
                                                        <div className="fw-bolder text-dark fs-6">{bid.licitatie.titlu}</div>
                                                        <small className="text-muted fw-medium">{bid.licitatie.categorie}</small>
                                                    </td>
                                                    <td>
                                                        <span className="fw-bold text-primary fs-5">{bid.suma} <small className="fs-6 text-muted">RON</small></span>
                                                    </td>
                                                    <td>
                                                        {bid.licitatie.esteIncheiata ? (
                                                            bid.licitatie.castigatorId === bid.userId ? (
                                                                <span className="badge bg-success rounded-pill px-3 py-2 shadow-sm animate__animated animate__pulse animate__infinite">Câștigător</span>
                                                            ) : (
                                                                <span className="badge bg-secondary text-white rounded-pill px-3 py-2">Finalizat</span>
                                                            )
                                                        ) : (
                                                            bid.suma === bid.licitatie.pretCurent ? (
                                                                <span className="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-3 py-2 fw-bold">Lider curent</span>
                                                            ) : (
                                                                <span className="badge bg-danger-subtle text-danger border border-danger-subtle rounded-pill px-3 py-2 fw-bold">Depășit</span>
                                                            )
                                                        )}
                                                    </td>
                                                    <td className="text-end pe-4">
                                                        <a href={`/Licitatii/Details/${bid.licitatieId}`} className="btn btn-sm btn-outline-primary rounded-pill px-4 fw-bold shadow-sm">Fișă produs <i className="bi bi-arrow-right ms-1"></i></a>
                                                    </td>
                                                </tr>
                                            ))
                                        ) : (
                                            <tr>
                                                <td colSpan="4" className="text-center p-5 text-muted fw-medium">
                                                    Nu există înregistrări pentru selecția curentă.
                                                </td>
                                            </tr>
                                        )}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>

                    <div className="col-lg-4 animate__animated animate__fadeInRight">
                        <div className="card border-0 shadow-lg rounded-5 bg-white p-4 h-100 text-center d-flex flex-column justify-content-between">
                            <div>
                                <h4 className="fw-bold text-dark mb-1">Distribuție Portofoliu</h4>
                                <p className="text-muted small">Interacțiune vizuală rapidă</p>
                            </div>

                            <div style={{ width: '100%', height: 260 }}>
                                {chartData.length > 0 ? (
                                    <ResponsiveContainer>
                                        <PieChart>
                                            <Pie
                                                data={chartData}
                                                cx="50%"
                                                cy="50%"
                                                innerRadius={65}
                                                outerRadius={85}
                                                paddingAngle={4}
                                                dataKey="value"
                                                onClick={handleChartClick}
                                                style={{ cursor: 'pointer' }}
                                            >
                                                {chartData.map((entry, index) => (
                                                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                                ))}
                                            </Pie>
                                            <Tooltip />
                                            <Legend verticalAlign="bottom" height={36} />
                                        </PieChart>
                                    </ResponsiveContainer>
                                ) : (
                                    <p className="text-muted pt-5 small">Date statistice insuficiente.</p>
                                )}
                            </div>

                            <div className="bg-light p-3 rounded-4 small text-muted text-start border">
                                <i className="bi bi-info-circle-fill text-primary me-2"></i>
                                Selectarea unei categorii din grafic izolează automat acele repere în panoul de tranzacții din stânga.
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}