import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'

type Vehicle = { id: string; brand: string; model: string; year: number; mileage: number; dailyRate: number; extraKilometerRate: number; status: number }
type Rental = { id: string; vehicleId: string; vehicle?: Vehicle; startDate: string; expectedReturnDate: string; actualReturnDate?: string; startMileage: number; totalAmount: number; status: number }
type Profile = { id: string; fullName: string; email: string; phoneNumber?: string; nationalIdNumber?: string; driverLicenseExpiry?: string; role: string }
type Damage = { id: string; rentalId: string; description: string; amount: number; status: number; reportedAt: string; assessedAt?: string; brand: string; model: string; fullName?: string; email?: string }
type AdminRental = { id: string; expectedReturnDate: string; startMileage: number; totalAmount: number; customer: string; email: string; vehicle: string }
type Lang = 'tr' | 'en'

const words = {
  tr: { cars:'Araçlar', about:'Hakkımızda', contact:'Bize ulaşın', login:'Giriş yap', logout:'Çıkış yap', eyebrow:'Yolun hazır', hero:'İstediğin yere, kendi hızında.', intro:'Planına uyan aracı seç, tarihini belirle ve yolculuğuna hemen başla.', available:'Kiralamaya hazır araçlar', noCars:'Şu anda listelenecek araç yok.', loading:'Araçlar yükleniyor…', availableTag:'Müsait', day:'gün', rent:'Kirala', loginTitle:'Tekrar hoş geldin', register:'Hesap oluştur', fullName:'Ad soyad', email:'E-posta', password:'Şifre', cancel:'Vazgeç', account:'Hesabım', profile:'Profilim', reservations:'Rezervasyonlar', history:'Geçmiş kiralamalar', damages:'Hasar ve ödemeler', admin:'Admin paneli', settings:'Ayarlar', save:'Kaydet', phone:'Telefon', nationalId:'T.C. kimlik numarası', expiry:'Ehliyet geçerlilik tarihi', noRentals:'Henüz kiralamanız yok.', extend:'Süreyi uzat', return:'İade et', returnDate:'İade tarihi', mileage:'İade kilometresi', report:'Hasar bildir', description:'Hasar açıklaması', pay:'Simülasyon ödemesi', assess:'Tutarı belirle', message:'Mesaj', subject:'Konu', send:'Mesaj gönder', contactTitle:'Size nasıl yardımcı olabiliriz?', aboutTitle:'Yolculuğunuz, sizin kurallarınızla.', aboutText:'Şeffaf günlük ücretler, çevrimiçi rezervasyon ve kiralama takibi tek yerde.', signedIn:'Giriş başarılı.', saved:'Bilgiler kaydedildi.', error:'İstek tamamlanamadı.', language:'Dil', close:'Kapat', forgot:'Şifremi unuttum', newPassword:'Yeni şifre', confirmPassword:'Şifreyi tekrar gir', sendReset:'Sıfırlama bağlantısı gönder', adminRentals:'Aktif kiralamalar', noDamage:'Hasar kaydı yok.', pending:'Değerlendirme bekliyor', paid:'Ödendi', resetInfo:'Geliştirme ortamında sıfırlama bağlantısı API terminaline yazılır.' },
  en: { cars:'Cars', about:'About', contact:'Contact', login:'Sign in', logout:'Sign out', eyebrow:'Your journey starts here', hero:'Go where you want, at your own pace.', intro:'Choose a car that fits your plans, pick your dates, and get on the road.', available:'Cars ready to rent', noCars:'No cars are listed right now.', loading:'Loading cars…', availableTag:'Available', day:'day', rent:'Rent', loginTitle:'Welcome back', register:'Create account', fullName:'Full name', email:'Email', password:'Password', cancel:'Cancel', account:'My account', profile:'My profile', reservations:'Reservations', history:'Rental history', damages:'Damage & payments', admin:'Admin panel', settings:'Settings', save:'Save', phone:'Phone', nationalId:'Turkish national ID number', expiry:'Driving licence expiry', noRentals:'No rentals yet.', extend:'Extend', return:'Return', returnDate:'Return date', mileage:'Return mileage', report:'Report damage', description:'Damage description', pay:'Simulate payment', assess:'Set amount', message:'Message', subject:'Subject', send:'Send message', contactTitle:'How can we help?', aboutTitle:'Your journey, your way.', aboutText:'Transparent daily rates, online reservations, and rental tracking in one place.', signedIn:'Signed in successfully.', saved:'Changes saved.', error:'The request failed.', language:'Language', close:'Close', forgot:'Forgot password?', newPassword:'New password', confirmPassword:'Confirm password', sendReset:'Send reset link', adminRentals:'Active rentals', noDamage:'No damage reports.', pending:'Awaiting assessment', paid:'Paid', resetInfo:'In development, the reset link is written to the API terminal.' }
} satisfies Record<Lang, Record<string, string>>

const tokenKey = 'rentcar_access_token'
const langKey = 'rentcar_language'
const fmtDate = (value: string, lang: Lang) => new Date(value).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US')
const money = (amount: number, lang: Lang) => new Intl.NumberFormat(lang === 'tr' ? 'tr-TR' : 'en-US', { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 }).format(amount)

export default function App() {
  const [lang, setLang] = useState<Lang>(() => localStorage.getItem(langKey) === 'en' ? 'en' : 'tr')
  const t = useCallback((key: keyof typeof words.tr) => words[lang][key] ?? words.tr[key] ?? key, [lang])
  const [token, setToken] = useState(() => localStorage.getItem(tokenKey) ?? '')
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [rentals, setRentals] = useState<Rental[]>([])
  const [damages, setDamages] = useState<Damage[]>([])
  const [adminDamages, setAdminDamages] = useState<Damage[]>([])
  const [adminRentals, setAdminRentals] = useState<AdminRental[]>([])
  const [profile, setProfile] = useState<Profile | null>(null)
  const [authOpen, setAuthOpen] = useState(false)
  const [registering, setRegistering] = useState(false)
  const [forgot, setForgot] = useState(false)
  const [accountOpen, setAccountOpen] = useState(false)
  const [pane, setPane] = useState('profile')
  const [notice, setNotice] = useState('')
  const [error, setError] = useState(false)
  const [busy, setBusy] = useState(false)
  const [resetToken] = useState(() => new URLSearchParams(location.search).get('resetToken') ?? '')
  const auth = useCallback(async <T,>(path: string, options: RequestInit = {}): Promise<T> => {
    const response = await fetch(path, { ...options, headers: { ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...(token ? { Authorization: `Bearer ${token}` } : {}), ...options.headers } })
    const type = response.headers.get('content-type') ?? ''
    const data = type.includes('json') ? await response.json() : await response.text()
    if (!response.ok) throw new Error(typeof data === 'string' ? data : data.message ?? data.title ?? t('error'))
    return data as T
  }, [token, t])
  const notify = useCallback((message: string, isError = false) => { setNotice(message); setError(isError); window.setTimeout(() => setNotice(''), 5000) }, [])
  const loadCars = useCallback(async () => {
    try {
      const result = await auth<unknown>('/api/vehicles')
      if (!Array.isArray(result)) throw new Error(t('error'))
      setVehicles(result as Vehicle[])
    } catch (e) { setVehicles([]); notify(e instanceof Error ? e.message : t('error'), true) }
  }, [auth, notify, t])
  const loadAccount = useCallback(async () => {
    try {
      const [p, r, d] = await Promise.all([auth<Profile>('/api/profile'), auth<Rental[]>('/api/rentals'), auth<Damage[]>('/api/damages')])
      setProfile(p); setRentals(r); setDamages(d)
    } catch (e) { notify(e instanceof Error ? e.message : t('error'), true) }
  }, [auth, notify, t])
  useEffect(() => { void loadCars() }, [loadCars])
  useEffect(() => { if (token) void loadAccount() }, [token, loadAccount])
  useEffect(() => { if (token && pane === 'adminRentals') void auth<AdminRental[]>('/api/rentals/admin').then(setAdminRentals).catch(e => notify(e instanceof Error ? e.message : t('error'), true)) }, [token, pane, auth, notify, t])
  useEffect(() => { if (token && pane === 'admin') void auth<Damage[]>('/api/damages/admin').then(setAdminDamages).catch(e => notify(e instanceof Error ? e.message : t('error'), true)) }, [token, pane, auth, notify, t])
  useEffect(() => { document.documentElement.lang = lang; localStorage.setItem(langKey, lang) }, [lang])
  useEffect(() => { if (resetToken) { setAuthOpen(true); setForgot(false) } }, [resetToken])
  const cars = useMemo(() => vehicles.filter(v => v.status === 0), [vehicles])

  async function submitAuth(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true)
    const form = new FormData(event.currentTarget)
    try {
      if (resetToken) {
        if (form.get('newPassword') !== form.get('confirmPassword')) throw new Error(lang === 'tr' ? 'Şifreler eşleşmiyor.' : 'The passwords do not match.')
        await auth('/api/auth/reset-password', { method: 'POST', body: JSON.stringify({ token: resetToken, newPassword: form.get('newPassword') }) })
        history.replaceState({}, '', location.pathname); setAuthOpen(false); notify(t('saved')); return
      }
      if (forgot) {
        await auth('/api/auth/forgot-password', { method: 'POST', body: JSON.stringify({ email: form.get('email') }) })
        setForgot(false); notify(t('resetInfo')); return
      }
      const data = await auth<{ accessToken: string }>(`/api/auth/${registering ? 'register' : 'login'}`, { method: 'POST', body: JSON.stringify(registering ? { fullName: form.get('fullName'), email: form.get('email'), password: form.get('password') } : { email: form.get('email'), password: form.get('password') }) })
      localStorage.setItem(tokenKey, data.accessToken); setToken(data.accessToken); setAuthOpen(false); notify(t('signedIn'))
    } catch (e) { notify(e instanceof Error ? e.message : t('error'), true) } finally { setBusy(false) }
  }
  async function rent(vehicleId: string, date: string) {
    if (!token) { setAuthOpen(true); notify(t('loginTitle')); return }
    try { await auth('/api/rentals', { method: 'POST', body: JSON.stringify({ vehicleId, expectedReturnDate: new Date(`${date}T23:59:00`).toISOString() }) }); notify(t('saved')); await Promise.all([loadCars(), loadAccount()]) }
    catch (e) { notify(e instanceof Error ? e.message : t('error'), true) }
  }
  async function updateProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); const form = new FormData(event.currentTarget)
    try { await auth('/api/profile', { method: 'PUT', body: JSON.stringify({ fullName: form.get('fullName'), phoneNumber: form.get('phoneNumber') || null, nationalIdNumber: form.get('nationalIdNumber'), driverLicenseExpiry: form.get('driverLicenseExpiry') || null }) }); notify(t('saved')); await loadAccount() }
    catch (e) { notify(e instanceof Error ? e.message : t('error'), true) }
  }
  async function returnRental(id: string, startMileage: number, admin = false) {
    const raw = prompt(t('mileage'), String(startMileage)); if (raw === null) return
    try { await auth(admin ? `/api/rentals/admin/${id}/return` : `/api/rentals/${id}/return`, { method: 'POST', body: JSON.stringify({ endMileage: Number(raw) }) }); notify(t('saved')); await Promise.all([loadCars(), loadAccount()]) }
    catch (e) { notify(e instanceof Error ? e.message : t('error'), true) }
  }
  async function doRentalAction(id: string, action: 'extend' | 'return', startMileage = 0) {
    if (action === 'return') return returnRental(id, startMileage)
    const date = prompt(t('returnDate')); if (!date) return
    try { await auth(`/api/rentals/${id}/extend`, { method: 'PUT', body: JSON.stringify({ newExpectedReturnDate: new Date(`${date}T23:59:00`).toISOString() }) }); notify(t('saved')); await loadAccount() }
    catch (e) { notify(e instanceof Error ? e.message : t('error'), true) }
  }
  async function submitDamage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); const form = new FormData(event.currentTarget)
    try { await auth(`/api/damages/rentals/${form.get('rentalId')}`, { method: 'POST', body: JSON.stringify({ description: form.get('description') }) }); notify(t('saved')); await loadAccount() }
    catch (e) { notify(e instanceof Error ? e.message : t('error'), true) }
  }
  function logout() { localStorage.removeItem(tokenKey); setToken(''); setProfile(null); setRentals([]); setDamages([]); setAccountOpen(false) }

  return <>
    <header className="topbar shell"><a className="brand" href="#top"><span className="logo">↗</span> yol<span className="accent">.</span></a><nav className="nav"><a href="#cars">{t('cars')}</a><a href="#about">{t('about')}</a><a href="#contact">{t('contact')}</a></nav><div className="top-actions"><select className="lang" value={lang} onChange={e => setLang(e.target.value as Lang)} aria-label={t('language')}><option value="tr">TR</option><option value="en">EN</option></select>{token ? <button className="btn secondary" onClick={() => { setAccountOpen(true); setPane('profile') }}>◉ {profile?.fullName ?? t('account')}</button> : <button className="btn" onClick={() => { setAuthOpen(true); setRegistering(false) }}>{t('login')}</button>}</div></header>
    <main id="top" className="shell"><section className="hero"><div><div className="eyebrow">{t('eyebrow')}</div><h1>{t('hero')}</h1><p>{t('intro')}</p></div><div className="hero-mark" aria-hidden="true">🚘</div></section>
      {notice && <div className={`notice ${error ? 'error' : ''}`} role="status">{notice}</div>}
      <section id="cars"><div className="toolbar"><h2>{t('available')}</h2><span className="count">{cars.length}</span></div><div className="grid">{cars.length ? cars.map(car => <VehicleCard key={car.id} car={car} lang={lang} t={t} onRent={rent} />) : <div className="empty" style={{ gridColumn: '1/-1' }}>{vehicles.length ? t('noCars') : t('loading')}</div>}</div></section>
      <section id="about" className="section"><div className="section-heading"><h2>{t('aboutTitle')}</h2></div><p className="muted-copy">{t('aboutText')}</p></section>
      <section id="contact" className="section"><div className="section-heading"><h2>{t('contactTitle')}</h2></div><form className="form-grid" onSubmit={async e => { e.preventDefault(); const f = new FormData(e.currentTarget); try { await auth('/api/contact', { method: 'POST', body: JSON.stringify({ name: f.get('name'), email: f.get('email'), subject: f.get('subject'), message: f.get('message') }) }); e.currentTarget.reset(); notify(t('saved')) } catch (err) { notify(err instanceof Error ? err.message : t('error'), true) } }}><Field label={t('fullName')} name="name" required /><Field label={t('email')} name="email" type="email" required /><Field label={t('subject')} name="subject" required full /><label className="field full">{t('message')}<textarea name="message" required minLength={10} maxLength={3000} /></label><div className="full form-actions"><button className="btn">{t('send')}</button></div></form></section>
    </main><footer className="footer shell"><span>yol. · {t('available')}</span><a href="#contact">{t('contact')}</a></footer>
    {authOpen && <Modal onClose={() => setAuthOpen(false)} title={resetToken ? t('newPassword') : forgot ? t('forgot') : registering ? t('register') : t('loginTitle')}><form className="form-grid" onSubmit={submitAuth}>{resetToken ? <><Field label={t('newPassword')} name="newPassword" type="password" minLength={8} required /><Field label={t('confirmPassword')} name="confirmPassword" type="password" minLength={8} required /></> : <>{registering && <Field label={t('fullName')} name="fullName" required full />}<Field label={t('email')} name="email" type="email" required full />{!forgot && <Field label={t('password')} name="password" type="password" minLength={registering ? 8 : 1} required full />}</>}{forgot && <p className="hint full">{t('resetInfo')}</p>}<div className="form-actions full"><button className="btn" disabled={busy}>{busy ? '…' : forgot ? t('sendReset') : resetToken ? t('save') : registering ? t('register') : t('login')}</button></div></form>{!resetToken && <div className="auth-switch">{!registering && !forgot && <button className="text-button" onClick={() => setForgot(true)}>{t('forgot')}</button>}<button className="text-button" onClick={() => { setForgot(false); setRegistering(!registering) }}>{registering ? t('login') : t('register')}</button></div>}</Modal>}
    {accountOpen && <Modal onClose={() => setAccountOpen(false)} title={profile?.fullName ?? t('account')} wide><div className="account-layout"><nav className="account-nav">{[['profile',t('profile')],['reservations',t('reservations')],['history',t('history')],['damages',t('damages')],...(profile?.role === 'Admin' ? [['admin',t('admin')],['adminRentals',t('adminRentals')]] : []),['settings',t('settings')]].map(([id,label]) => <button key={id} className={pane===id?'active':''} onClick={() => setPane(id)}>{label}</button>)}<button onClick={logout}>{t('logout')}</button></nav><div className="account-content">
      {pane==='profile' && <><h3>{t('profile')}</h3><form className="form-grid" onSubmit={updateProfile}><Field label={t('fullName')} name="fullName" defaultValue={profile?.fullName} required /><Field label={t('email')} name="email" defaultValue={profile?.email} disabled /><Field label={t('phone')} name="phoneNumber" type="tel" defaultValue={profile?.phoneNumber} /><Field label={t('nationalId')} name="nationalIdNumber" defaultValue={profile?.nationalIdNumber} pattern="[1-9][0-9]{10}" maxLength={11} required /><Field label={t('expiry')} name="driverLicenseExpiry" type="date" defaultValue={profile?.driverLicenseExpiry?.slice(0,10)} full /><div className="form-actions full"><button className="btn">{t('save')}</button></div></form></>}
      {(pane==='reservations'||pane==='history') && <><h3>{pane==='history'?t('history'):t('reservations')}</h3>{rentals.filter(r => pane==='history' ? r.status!==0 : r.status===0).map(r => <article className="rental" key={r.id}><div><strong>{r.vehicle?.brand} {r.vehicle?.model}</strong><div className="rental-meta">{fmtDate(r.startDate,lang)} – {fmtDate(r.expectedReturnDate,lang)} · {money(r.totalAmount,lang)}</div></div><div className="rental-actions">{r.status===0 && <><button className="btn secondary" onClick={() => doRentalAction(r.id,'extend')}>{t('extend')}</button><button className="btn" onClick={() => doRentalAction(r.id,'return',r.startMileage)}>{t('return')}</button></>}</div></article>)}{!rentals.some(r => pane==='history' ? r.status!==0 : r.status===0) && <div className="empty">{t('noRentals')}</div>}</>}
      {pane==='adminRentals' && <><h3>{t('adminRentals')}</h3>{adminRentals.map(r=><article className="rental" key={r.id}><div><strong>{r.vehicle} · {r.customer}</strong><div className="rental-meta">{r.email} · {t('returnDate')}: {fmtDate(r.expectedReturnDate,lang)} · {money(r.totalAmount,lang)}</div></div><button className="btn" onClick={()=>void returnRental(r.id,r.startMileage,true)}>{t('return')}</button></article>)}{!adminRentals.length && <div className="empty">{t('noRentals')}</div>}</>}
      {pane==='damages' && <><h3>{t('damages')}</h3>{damages.map(d => <article className="rental" key={d.id}><div><strong>{d.brand} {d.model}</strong><div className="rental-meta">{d.description} · {d.assessedAt ? money(d.amount,lang) : t('pending')}</div></div>{d.assessedAt && d.status!==1 && <button className="btn" onClick={async () => { try { await auth(`/api/damages/${d.id}/pay`,{method:'POST'}); await loadAccount(); notify(t('saved')) } catch(e) { notify(e instanceof Error ? e.message : t('error'),true) } }}>{t('pay')}</button>}<span>{d.status===1?t('paid'):''}</span></article>)}{!damages.length && <div className="empty">{t('noDamage')}</div>}<form className="form-grid spaced" onSubmit={submitDamage}><label className="field full">{t('reservations')}<select name="rentalId" required>{rentals.map(r=><option key={r.id} value={r.id}>{r.vehicle?.brand} {r.vehicle?.model}</option>)}</select></label><label className="field full">{t('description')}<textarea name="description" minLength={5} required /></label><div className="form-actions full"><button className="btn secondary">{t('report')}</button></div></form></>}
      {pane==='admin' && <><h3>{t('admin')}</h3>{adminDamages.map(d=><DamageReview key={d.id} damage={d} lang={lang} onSave={async amount => { await auth(`/api/damages/${d.id}/assess`,{method:'PUT',body:JSON.stringify({amount})}); setAdminDamages(await auth<Damage[]>('/api/damages/admin')); notify(t('saved')) }} />)}{!adminDamages.length && <div className="empty">{t('noDamage')}</div>}</>}
      {pane==='settings' && <><h3>{t('settings')}</h3><label className="field">{t('language')}<select className="lang" value={lang} onChange={e=>setLang(e.target.value as Lang)}><option value="tr">Türkçe</option><option value="en">English</option></select></label></>}
    </div></div></Modal>}
  </>
}

function Field({ label, name, type='text', required=false, defaultValue, disabled=false, pattern, maxLength, minLength, full=false }: { label:string; name:string; type?:string; required?:boolean; defaultValue?:string; disabled?:boolean; pattern?:string; maxLength?:number; minLength?:number; full?:boolean }) { return <label className={`field ${full?'full':''}`}>{label}<input name={name} type={type} required={required} defaultValue={defaultValue??''} disabled={disabled} pattern={pattern} maxLength={maxLength} minLength={minLength} /></label> }
function VehicleCard({car,lang,t,onRent}:{car:Vehicle;lang:Lang;t:(key:keyof typeof words.tr)=>string;onRent:(id:string,date:string)=>void}) { const [date,setDate]=useState(()=>{const d=new Date();d.setDate(d.getDate()+1);return d.toISOString().slice(0,10)});return <article className="car"><div className="car-art"><CarDrawing/><span className="status">{t('availableTag')}</span></div><div className="car-body"><h3 className="car-name">{car.brand} {car.model}</h3><div className="car-year">{car.year}</div><div className="specs"><span>{car.mileage.toLocaleString(lang==='tr'?'tr-TR':'en-US')} km</span><span>+ {money(car.extraKilometerRate,lang)} / km</span></div><div className="price-row"><div className="price">{money(car.dailyRate,lang)} <small>/ {t('day')}</small></div></div><form className="rent-form" onSubmit={e=>{e.preventDefault();onRent(car.id,date)}}><input type="date" value={date} min={new Date(Date.now()+86400000).toISOString().slice(0,10)} onChange={e=>setDate(e.target.value)} required aria-label={t('returnDate')}/><button className="btn">{t('rent')}</button></form></div></article> }
function CarDrawing(){return <svg viewBox="0 0 360 150" fill="none" aria-hidden="true"><path d="M53 94l15-36c4-10 12-16 23-18l35-7c10-2 17-8 24-16l15-17c5-6 12-9 20-9h38c10 0 18 5 23 13l20 34 27 8c13 4 21 14 23 28l2 20c1 8-5 15-13 15h-13a29 29 0 0 0-58 0H129a29 29 0 0 0-58 0H56c-9 0-15-6-14-15l1-8c1-5 4-9 10-12z" fill="#C5F36B"/><path d="M113 43l30-6c8-2 13-6 20-13l10-11h40l18 31H113z" fill="#9BAF7B"/><circle cx="100" cy="109" r="19" fill="#0B1220" stroke="#75846A" strokeWidth="7"/><circle cx="271" cy="109" r="19" fill="#0B1220" stroke="#75846A" strokeWidth="7"/></svg>}
function Modal({children,title,onClose,wide=false}:{children:React.ReactNode;title:string;onClose:()=>void;wide?:boolean}){return <div className="modal open" onMouseDown={e=>{if(e.target===e.currentTarget)onClose()}}><section className={`dialog ${wide?'wide':''}`} role="dialog" aria-modal="true"><header className="dialog-head"><h2>{title}</h2><button className="close" aria-label="Close" onClick={onClose}>×</button></header>{children}</section></div>}
function DamageReview({damage,lang,onSave}:{damage:Damage;lang:Lang;onSave:(amount:number)=>Promise<void>}){const[amount,setAmount]=useState(damage.amount?String(damage.amount):'');return <article className="rental"><div><strong>{damage.fullName} · {damage.brand} {damage.model}</strong><div className="rental-meta">{damage.email} · {damage.description} · {fmtDate(damage.reportedAt,lang)}</div></div>{damage.status!==1&&<form className="rental-actions" onSubmit={e=>{e.preventDefault();void onSave(Number(amount))}}><input className="date-input" type="number" min="0.01" step="0.01" value={amount} onChange={e=>setAmount(e.target.value)} required/><button className="btn secondary">{lang==='tr'?'Tutarı kaydet':'Save amount'}</button></form>}</article>}
