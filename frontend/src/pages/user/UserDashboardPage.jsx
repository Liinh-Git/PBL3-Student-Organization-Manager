import { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyEvents, getMyOrganizations, getMyTasks } from '../../services/userService.js';
import LoadingSpinner from '../../components/shared/LoadingSpinner';
import './UserDashboardPage.css';
import {
  MONTHS_VI, WDAYS, daysInMonth, firstDay, toKey, fmtDate, fmtTime, TODAY_KEY,
  IcoL, IcoR, IcoCal, IcoTask, IcoX, IcoPin, IcoOrg, IcoFlag, IcoFolder, IcoClock, IcoChev,
  PriBadge, DueBadge
} from './_DashboardParts.jsx';

// ─── Popup ────────────────────────────────────────────────────────
function Popup({ info, onClose, navigate }) {
  const ref = useRef(null);
  useEffect(() => {
    const fn = e => { if (ref.current && !ref.current.contains(e.target)) onClose(); };
    document.addEventListener('mousedown', fn);
    return () => document.removeEventListener('mousedown', fn);
  }, [onClose]);
  if (!info) return null;
  const { item, top, left } = info;
  const isEvent = item.type === 'event';
  const bar = isEvent ? '#ff9b51' : item.isOverdue ? '#ef4444' : '#60a5fa';
  return (
    <div className="dash-popup-overlay">
      <div className="dash-popup" ref={ref} style={{ top, left }}>
        <div className="dash-popup-bar" style={{ background: bar }} />
        <div className="dash-popup-body">
          <div className="dash-popup-header">
            <h3 className="dash-popup-title">{isEvent ? item.name : item.taskName}</h3>
            <button className="dash-popup-close" onClick={onClose}><IcoX /></button>
          </div>
          <div className="dash-popup-meta">
            {isEvent ? (<>
              <div className="dash-popup-meta-row"><IcoCal s={13}/><span>{fmtDate(item.startDate)}{fmtTime(item.startDate) ? ` · ${fmtTime(item.startDate)}` : ''}</span></div>
              {item.location && <div className="dash-popup-meta-row"><IcoPin s={13}/><span>{item.location}</span></div>}
              <div className="dash-popup-meta-row"><IcoOrg s={13}/><span>{item.organizationName}</span></div>
            </>) : (<>
              <div className="dash-popup-meta-row"><IcoClock s={13}/><span>Deadline: {item.deadline ? fmtDate(item.deadline) : '—'}{item.isOverdue ? ' · Quá hạn' : ''}</span></div>
              <div className="dash-popup-meta-row"><PriBadge priority={item.priority}/><span style={{fontSize:'0.72rem',background:'#f1f5f9',color:'#475569',padding:'1px 6px',borderRadius:4,fontWeight:700}}>{item.status}</span></div>
            </>)}
          </div>
          <div className="dash-popup-context">
            <div className="dash-popup-context-row"><IcoOrg s={12}/><strong style={{fontSize:'0.74rem'}}>{item.organizationName}</strong></div>
            {!isEvent && item.taskSource !== 'Department' && (
              <div className="dash-popup-breadcrumb">
                <IcoCal s={10}/><span style={{fontWeight:600}}>{item.eventName}</span>
                <IcoChev/><IcoFlag s={10}/><span>{item.milestoneTitle}</span>
                <IcoChev/><IcoFolder s={10}/><span>{item.categoryName}</span>
              </div>
            )}
            {!isEvent && item.taskSource === 'Department' && (
              <div className="dash-popup-breadcrumb">
                <IcoFolder s={10}/><span style={{fontWeight:600}}>{item.departmentName ? `Phòng ban: ${item.departmentName}` : 'Task phòng ban'}</span>
              </div>
            )}
          </div>
          <div className="dash-popup-actions">
            {isEvent ? (<>
              <button className="dash-popup-btn" onClick={() => { navigate(`/events/${item.id}`); onClose(); }}>Xem chi tiết</button>
              {item.organizationId && <button className="dash-popup-btn dash-popup-btn--primary" onClick={() => { navigate(`/org/events/${item.id}?orgId=${item.organizationId}`); onClose(); }}>Không gian làm việc</button>}
            </>) : item.taskSource === 'Department' ? (
              <button className="dash-popup-btn dash-popup-btn--primary" onClick={() => { navigate(`/org/departments?orgId=${item.organizationId}`); onClose(); }}>Mở phòng ban</button>
            ) : (
              <button className="dash-popup-btn dash-popup-btn--primary" onClick={() => { navigate(`/org/events/${item.eventId}?orgId=${item.organizationId}`); onClose(); }}>Mở workspace</button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Mini Calendar ────────────────────────────────────────────────
function MiniCal({ year, month, onPrev, onNext, dots, onSelect, sel }) {
  const first = firstDay(year, month);
  const total = daysInMonth(year, month);
  const cells = Array(first).fill(null).concat(Array.from({ length: total }, (_, i) => i + 1));
  return (
    <div className="dash-mini-cal">
      <div className="dash-mini-cal-header">
        <span className="dash-mini-cal-title">{MONTHS_VI[month]}, {year}</span>
        <div className="dash-mini-cal-nav">
          <button onClick={onPrev}><IcoL /></button>
          <button onClick={onNext}><IcoR /></button>
        </div>
      </div>
      <div className="dash-mini-cal-grid">
        {WDAYS.map(d => <div key={d} className="dash-mini-cal-weekday">{d}</div>)}
        {cells.map((day, i) => {
          if (!day) return <div key={`e${i}`} />;
          const key = `${year}-${String(month+1).padStart(2,'0')}-${String(day).padStart(2,'0')}`;
          const d = dots[key] || {};
          const isToday = key === TODAY_KEY;
          return (
            <div key={key} className="dash-mini-cal-cell" onClick={() => onSelect(key)}>
              <div className={`dash-mini-cal-day${isToday?' dash-mini-cal-day--today':''}${sel===key&&!isToday?' dash-mini-cal-day--selected':''}`}>{day}</div>
              {(d.event||d.task) && <div className="dash-mini-cal-dots">{d.event&&<span className="dash-mini-cal-dot dash-mini-cal-dot--event"/>}{d.task&&<span className="dash-mini-cal-dot dash-mini-cal-dot--task"/>}</div>}
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ─── Calendar chip ────────────────────────────────────────────────
function Chip({ item, onClick }) {
  const isEvent = item.type === 'event';
  const isDone = !isEvent && item.status === 'Done';
  const cls = isEvent ? 'dash-chip--event' : item.isOverdue ? 'dash-chip--overdue' : isDone ? 'dash-chip--done' : 'dash-chip--task';
  return (
    <div className={`dash-chip ${cls}`} onClick={onClick} title={isEvent ? item.name : item.taskName}>
      <span className="dash-chip-icon">{isEvent ? <IcoCal /> : <IcoTask />}</span>
      <span className="dash-chip-text">{isEvent ? item.name : item.taskName}</span>
    </div>
  );
}

// ─── Month grid ───────────────────────────────────────────────────
function MonthGrid({ year, month, itemsByDate, onChipClick }) {
  const first = firstDay(year, month);
  const total = daysInMonth(year, month);
  const prevTotal = daysInMonth(year, month === 0 ? 11 : month - 1);
  const cells = [];
  for (let i = 0; i < first; i++) cells.push({ filler: true, day: prevTotal - first + i + 1 });
  for (let d = 1; d <= total; d++) cells.push({ filler: false, day: d });
  const rem = (7 - ((first + total) % 7)) % 7;
  for (let i = 1; i <= rem; i++) cells.push({ filler: true, day: i });

  return (
    <>
      <div className="dash-cal-weekdays">
        {['CN','T2','T3','T4','T5','T6','T7'].map(d => <div key={d} className="dash-cal-weekday-label">{d}</div>)}
      </div>
      <div className="dash-cal-month-grid">
        {cells.map((cell, i) => {
          const key = cell.filler ? null : `${year}-${String(month+1).padStart(2,'0')}-${String(cell.day).padStart(2,'0')}`;
          const items = key ? (itemsByDate[key] || []) : [];
          const isToday = key === TODAY_KEY;
          return (
            <div key={i} className={`dash-cal-day-cell${cell.filler?' dash-cal-day-cell--filler':''}${isToday?' dash-cal-day-cell--today':''}`}>
              <div className="dash-cal-day-num-wrap">
                <span className={`dash-cal-day-num${isToday?' dash-cal-day-num--today':''}${cell.filler?' dash-cal-day-num--filler':''}`}>{cell.day}</span>
              </div>
              {!cell.filler && (
                <div className="dash-chips-list">
                  {items.slice(0, 3).map((item, j) => (
                    <Chip key={j} item={item} onClick={e => { e.stopPropagation(); onChipClick(e, item); }} />
                  ))}
                  {items.length > 3 && <div className="dash-chip-overflow">+{items.length - 3} more</div>}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </>
  );
}

// ─── Agenda view ──────────────────────────────────────────────────
function AgendaView({ year, month, itemsByDate, onChipClick }) {
  const total = daysInMonth(year, month);
  const groups = [];
  for (let d = 1; d <= total; d++) {
    const key = `${year}-${String(month+1).padStart(2,'0')}-${String(d).padStart(2,'0')}`;
    const items = itemsByDate[key];
    if (items && items.length > 0) groups.push({ key, day: d, items });
  }
  if (groups.length === 0) return <div className="dash-right-empty" style={{padding:'2rem'}}>Không có sự kiện/task trong tháng này.</div>;
  const monShort = MONTHS_VI[month].replace('Tháng ','Th');
  return (
    <div className="dash-agenda-wrap">
      {groups.map(g => (
        <div key={g.key} className="dash-agenda-day-group">
          <div className="dash-agenda-day-header">
            <div className={`dash-agenda-day-badge${g.key===TODAY_KEY?' dash-agenda-day-badge--today':''}`}>
              <span className="dash-agenda-day-mon">{monShort}</span>
              <span className="dash-agenda-day-num">{g.day}</span>
            </div>
            <div className="dash-agenda-items">
              {g.items.map((item, j) => {
                const isEvent = item.type === 'event';
                const bar = isEvent ? '#ff9b51' : item.isOverdue ? '#ef4444' : '#60a5fa';
                return (
                  <div key={j} className="dash-agenda-item" onClick={e => onChipClick(e, item)}>
                    <div className="dash-agenda-item-bar" style={{ background: bar }} />
                    <div>
                      <div className="dash-agenda-item-name">{isEvent ? item.name : item.taskName}</div>
                      <div className="dash-agenda-item-meta">{isEvent ? (fmtTime(item.startDate)||'Cả ngày') : `Deadline: ${fmtDate(item.deadline)||'—'}`}</div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

// ─── Right panel ──────────────────────────────────────────────────
function RightPanel({ tasks, events, navigate, onChipClick }) {
  const now = new Date();
  const dueTasks = useMemo(() => tasks
    .filter(t => t.status !== 'Done' && t.status !== 'Cancelled')
    .sort((a, b) => {
      if (!a.deadline && !b.deadline) return 0;
      if (!a.deadline) return 1;
      if (!b.deadline) return -1;
      return new Date(a.deadline) - new Date(b.deadline);
    }).slice(0, 6), [tasks]);

  const upcomingEvents = useMemo(() => events
    .filter(e => new Date(e.startDate) >= now)
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
    .slice(0, 5), [events]);

  const monAbbr = d => MONTHS_VI[new Date(d).getMonth()].replace('Tháng ','Th');

  return (
    <aside className="dashboard-right">
      <div className="dash-right-header">
        <h2 className="dash-right-title">Tổng quan</h2>
        <p className="dash-right-sub">Ưu tiên sắp tới của bạn</p>
      </div>

      <div className="dash-right-section">
        <div className="dash-right-section-title">
          <span className="dash-right-section-icon"><IcoTask s={15}/></span>
          Task sắp đến hạn
        </div>
        {dueTasks.length === 0
          ? <p className="dash-right-empty">Không có task nào.</p>
          : dueTasks.map(t => (
            <div key={t.id} className="dash-task-card" onClick={e => onChipClick(e, { ...t, type: 'task' })}>
              <div className="dash-task-card-bar" style={{ background: t.isOverdue ? '#ef4444' : '#60a5fa' }} />
              <div className="dash-task-card-top">
                <p className="dash-task-card-name">{t.taskName}</p>
                <DueBadge deadline={t.deadline} isOverdue={t.isOverdue} />
              </div>
              <div className="dash-task-card-context">
                {t.taskSource === 'Department'
                  ? <>Phòng ban{t.departmentName ? `: ${t.departmentName}` : ''}</>
                  : <><IcoFolder s={10}/> {t.eventName} · {t.categoryName}</>}
              </div>
            </div>
          ))
        }
      </div>

      <div className="dash-right-divider" />

      <div className="dash-right-section">
        <div className="dash-right-section-title">
          <span className="dash-right-section-icon"><IcoCal s={15}/></span>
          Sự kiện sắp tới
        </div>
        {upcomingEvents.length === 0
          ? <p className="dash-right-empty">Không có sự kiện nào.</p>
          : upcomingEvents.map(ev => (
            <div key={ev.id} className="dash-event-row" onClick={() => navigate(`/events/${ev.id}`)}>
              <div className="dash-event-date-badge">
                <span className="dash-event-date-mon">{monAbbr(ev.startDate)}</span>
                <span className="dash-event-date-day">{new Date(ev.startDate).getDate()}</span>
              </div>
              <div className="dash-event-info">
                <p className="dash-event-info-name">{ev.name}</p>
                <div className="dash-event-info-meta"><IcoClock s={10}/>{fmtTime(ev.startDate)||'Cả ngày'}</div>
                <div className="dash-event-info-org"><IcoOrg s={10}/> {ev.organizationName}</div>
              </div>
            </div>
          ))
        }
      </div>
    </aside>
  );
}

// ─── Main page ────────────────────────────────────────────────────
function UserDashboardPage() {
  const navigate = useNavigate();
  const [viewDate, setViewDate] = useState(new Date());
  const [view, setView] = useState('month');
  const [selDay, setSelDay] = useState(TODAY_KEY);
  const [showEvents, setShowEvents] = useState(true);
  const [showTasks, setShowTasks] = useState(true);
  const [orgs, setOrgs] = useState([]);
  const [selOrgIds, setSelOrgIds] = useState([]);
  const [events, setEvents] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [popup, setPopup] = useState(null);

  const year = viewDate.getFullYear();
  const month = viewDate.getMonth();

  useEffect(() => {
    async function load() {
      setLoading(true); setError(null);
      try {
        const [orgData, evData, taskData] = await Promise.all([
          getMyOrganizations(),
          getMyEvents(),
          getMyTasks().catch(() => [])
        ]);
        const orgList = Array.isArray(orgData) ? orgData : [];
        setOrgs(orgList);
        setSelOrgIds(orgList.map(o => o.id));
        setEvents(Array.isArray(evData) ? evData : []);
        setTasks(Array.isArray(taskData) ? taskData : []);
      } catch (err) {
        setError(err.message || 'Không tải được dữ liệu');
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  const filteredEvents = useMemo(() => {
    if (!showEvents) return [];
    const selectedOrgSet = new Set((selOrgIds || []).map(id => String(id).toLowerCase()));
    return events.filter(e => selectedOrgSet.has(String(e.organizationId || e.orgId || '').toLowerCase()));
  }, [events, showEvents, selOrgIds]);

  const filteredTasks = useMemo(() => {
    if (!showTasks) return [];
    const selectedOrgSet = new Set((selOrgIds || []).map(id => String(id).toLowerCase()));
    return tasks.filter(t => selectedOrgSet.has(String(t.organizationId || t.orgId || '').toLowerCase()));
  }, [tasks, showTasks, selOrgIds]);

  const itemsByDate = useMemo(() => {
    const map = {};
    filteredEvents.forEach(e => {
      const key = toKey(e.startDate);
      if (!key) return;
      if (!map[key]) map[key] = [];
      map[key].push({ ...e, type: 'event' });
    });
    filteredTasks.forEach(t => {
      const key = toKey(t.deadline);
      if (!key) return;
      if (!map[key]) map[key] = [];
      map[key].push({ ...t, type: 'task' });
    });
    return map;
  }, [filteredEvents, filteredTasks]);

  const dotsByDate = useMemo(() => {
    const map = {};
    filteredEvents.forEach(e => { const k = toKey(e.startDate); if (k) { map[k] = map[k]||{}; map[k].event = true; } });
    filteredTasks.forEach(t => { const k = toKey(t.deadline); if (k) { map[k] = map[k]||{}; map[k].task = true; } });
    return map;
  }, [filteredEvents, filteredTasks]);

  function openPopup(e, item) {
    const rect = e.currentTarget.getBoundingClientRect();
    let top = rect.bottom + 6;
    let left = rect.left;
    if (left + 310 > window.innerWidth) left = window.innerWidth - 316;
    if (top + 260 > window.innerHeight) top = rect.top - 266;
    setPopup({ item, top, left });
  }

  function prevMonth() { setViewDate(d => new Date(d.getFullYear(), d.getMonth()-1, 1)); }
  function nextMonth() { setViewDate(d => new Date(d.getFullYear(), d.getMonth()+1, 1)); }

  if (loading) return <div className="app-page"><LoadingSpinner message="Đang tải dashboard..." /></div>;
  if (error) return <div className="app-page"><div className="app-error"><p>{error}</p></div></div>;

  return (
    <div className="app-page dashboard-page">
      <div className="dashboard-body">
        {/* LEFT */}
        <aside className="dashboard-left">
          <MiniCal year={year} month={month} onPrev={prevMonth} onNext={nextMonth} dots={dotsByDate} onSelect={setSelDay} sel={selDay} />
          <div className="dash-layers">
            <div className="dash-section-label">Hiển thị</div>
            <label className="dash-toggle-row">
              <input type="checkbox" checked={showEvents} onChange={e => setShowEvents(e.target.checked)} />
              <span className="dash-toggle-label">Sự kiện của tôi</span>
              <span className="dash-toggle-dot" style={{ background: '#ff9b51' }} />
            </label>
            <label className="dash-toggle-row">
              <input type="checkbox" checked={showTasks} onChange={e => setShowTasks(e.target.checked)} />
              <span className="dash-toggle-label">Task deadlines</span>
              <span className="dash-toggle-dot" style={{ background: '#60a5fa' }} />
            </label>
          </div>
          <div className="dash-org-filter">
            <div className="dash-org-filter-header">
              <div className="dash-section-label" style={{ marginBottom: 0 }}>Tổ chức</div>
              <button className="dash-select-all-btn" onClick={() => setSelOrgIds(selOrgIds.length === orgs.length ? [] : orgs.map(o => o.id))}>
                {selOrgIds.length === orgs.length ? 'Bỏ chọn tất cả' : 'Chọn tất cả'}
              </button>
            </div>
            <div style={{ marginTop: 10 }}>
              {orgs.map(org => (
                <label key={org.id} className="dash-org-row">
                  <input type="checkbox" checked={selOrgIds.includes(org.id)} onChange={e => setSelOrgIds(e.target.checked ? [...selOrgIds, org.id] : selOrgIds.filter(id => id !== org.id))} />
                  <span className="dash-org-name" title={org.name}>{org.name}</span>
                </label>
              ))}
              {orgs.length === 0 && <p style={{ fontSize: '0.74rem', color: '#64748b', margin: 0 }}>Chưa tham gia tổ chức nào</p>}
            </div>
          </div>
        </aside>

        {/* CENTER */}
        <main className="dashboard-center">
          <div className="dash-cal-toolbar">
            <div className="dash-cal-nav">
              <button className="dash-today-btn" onClick={() => { setViewDate(new Date()); setSelDay(TODAY_KEY); }}>Hôm nay</button>
              <div className="dash-month-nav">
                <button onClick={prevMonth}><IcoL /></button>
                <span className="dash-month-label">{MONTHS_VI[month]}, {year}</span>
                <button onClick={nextMonth}><IcoR /></button>
              </div>
            </div>
            <div className="dash-view-toggle">
              {['month','agenda'].map(v => (
                <button key={v} className={`dash-view-btn${view===v?' dash-view-btn--active':''}`} onClick={() => setView(v)}>
                  {v === 'month' ? 'Tháng' : 'Agenda'}
                </button>
              ))}
            </div>
          </div>
          <div className="dash-cal-grid-wrap">
            {view === 'month'
              ? <MonthGrid year={year} month={month} itemsByDate={itemsByDate} onChipClick={openPopup} />
              : <AgendaView year={year} month={month} itemsByDate={itemsByDate} onChipClick={openPopup} />
            }
          </div>
        </main>

        {/* RIGHT */}
        <RightPanel tasks={filteredTasks} events={filteredEvents} navigate={navigate} onChipClick={openPopup} />
      </div>

      {popup && <Popup info={popup} onClose={() => setPopup(null)} navigate={navigate} />}
    </div>
  );
}

export default UserDashboardPage;
