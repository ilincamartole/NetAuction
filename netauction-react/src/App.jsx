import { useState, useEffect } from 'react';
import toast, { Toaster } from 'react-hot-toast';

function TypewriterHtml({ html }) {
    const [displayedHtml, setDisplayedHtml] = useState("");
    useEffect(() => {
        let i = 0;
        setDisplayedHtml("");
        const interval = setInterval(() => {
            if (i < html.length) {
                setDisplayedHtml(html.substring(0, i + 6));
                i += 6;
            } else {
                clearInterval(interval);
            }
        }, 12);
        return () => clearInterval(interval);
    }, [html]);
    return <div className="animate__animated animate__fadeIn" dangerouslySetInnerHTML={{ __html: displayedHtml }} />;
}

export default function App() {
    const [categorie, setCategorie] = useState("0");
    const [buget, setBuget] = useState("");
    const [preferinte, setPreferinte] = useState("");
    const [loading, setLoading] = useState(false);
    const [raspuns, setRaspuns] = useState("");
    const [eroare, setEroare] = useState(false);

    const categorii = [
        { value: "0", text: "Electronice" },
        { value: "1", text: "Auto" },
        { value: "2", text: "Imobiliare" },
        { value: "3", text: "Moda" }
    ];

    const handleTrimite = async () => {
        if (!preferinte.trim()) {
            toast.error('Te rog să descrii ce anume te interesează!', { icon: '⚠️' });
            return;
        }

        setLoading(true);
        setEroare(false);
        setRaspuns("");
        const loadToast = toast.loading('Consultantul analizează ofertele din piață...');

        try {
            const response = await fetch('/api/ai/consultanta-cumparaturi', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Categorie: parseInt(categorie),
                    BugetMaxim: buget ? parseFloat(buget) : null,
                    PreferinteUser: preferinte
                })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message || "Eroare server.");

            setRaspuns(data.raspuns);
            toast.success('Ghidul de achiziție a fost finalizat!', { id: loadToast });
        } catch (error) {
            console.error(error);
            setEroare(true);
            toast.error('A eșuat conexiunea cu serverul.', { id: loadToast });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="row g-5 text-start animate__animated animate__fadeIn">
            <Toaster position="top-right" />

            <div className="col-lg-7">
                <div className="card border-0 shadow-lg rounded-5 p-4 bg-white">
                    <div className="d-flex align-items-center mb-4">
                        <div className="avatar-md text-primary rounded-circle d-flex align-items-center justify-content-center me-3" style={{ width: '55px', height: '55px', backgroundColor: '#e3f2fd' }}>
                            <i className="bi bi-robot fs-3 text-primary animate__animated animate__pulse animate__infinite"></i>
                        </div>
                        <div>
                            <h3 className="fw-bold text-dark mb-1">Ghid de Cumpărături Inteligent</h3>
                            <p className="text-muted small mb-0">Analiză personalizată pe baza algoritmilor predictivi</p>
                        </div>
                    </div>
                    <hr className="text-muted opacity-25 mb-4" />

                    <div className="mb-4">
                        <label className="form-label fw-bolder text-dark mb-2"><i className="bi bi-tags me-1 text-primary"></i> Categoria căutată</label>
                        <select className="form-select border-0 bg-light p-3 rounded-4 shadow-sm fw-medium" value={categorie} onChange={(e) => setCategorie(e.target.value)}>
                            {categorii.map(c => <option key={c.value} value={c.value}>{c.text}</option>)}
                        </select>
                    </div>

                    <div className="mb-4">
                        <label className="form-label fw-bolder text-dark mb-2"><i className="bi bi-wallet2 me-1 text-primary"></i> Buget alocat (RON)</label>
                        <input type="number" className="form-control border-0 bg-light p-3 rounded-4 fw-bold shadow-sm" placeholder="Lasă liber pentru buget nelimitat" value={buget} onChange={(e) => setBuget(e.target.value)} />
                    </div>

                    <div className="mb-4">
                        <label className="form-label fw-bolder text-dark mb-2"><i className="bi bi-chat-right-text me-1 text-primary"></i> Specificații dorite sau preferințe</label>
                        <textarea className="form-control border-0 bg-light p-3 rounded-4 text-secondary shadow-sm" rows="5" placeholder="Ex: Caut un telefon cu autonomie mare a bateriei..." value={preferinte} onChange={(e) => setPreferinte(e.target.value)}></textarea>
                    </div>

                    <button onClick={handleTrimite} className="btn btn-primary btn-lg w-100 rounded-pill fw-bolder py-3 shadow-lg hover-up-btn mt-2" disabled={loading} style={{ transition: 'all 0.3s ease' }}>
                        {loading ? "Se generează raportul..." : "Generează Strategie Recomandată"}
                    </button>
                </div>
            </div>

            <div className="col-lg-5">
                <div className="card border-0 shadow-lg rounded-5 overflow-hidden h-100 bg-white">
                    <div className="bg-primary p-4 text-white text-center">
                        <div className="fs-4 fw-bolder"><i className="bi bi-journal-check me-2"></i>Raportul Tău Personalizat</div>
                    </div>
                    <div className="card-body p-4 p-xl-5 d-flex align-items-center justify-content-center" style={{ minHeight: '420px' }}>
                        {loading && <p className="text-muted fw-bold animate__animated animate__pulse animate__infinite">Se parsează datele din piață...</p>}
                        {!loading && !raspuns && !eroare && (
                            <div className="text-center text-muted p-3">
                                <i className="bi bi-chat-left-dots text-primary opacity-50 display-3 mb-3 d-block"></i>
                                <h5 className="fw-bold text-dark">Sistem pregătit</h5>
                                <p className="small text-secondary px-3">Completează criteriile din panoul din stânga pentru a rula asistentul tactic.</p>
                            </div>
                        )}
                        {!loading && eroare && <p className="fw-bold text-danger">Conexiunea a eșuat. Reîncearcă.</p>}
                        {!loading && raspuns && <div className="leading-relaxed fs-6 w-100 text-dark"><TypewriterHtml html={raspuns} /></div>}
                    </div>
                </div>
            </div>
        </div>
    );
}