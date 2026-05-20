// Shared helpers & sub-components for UserDashboardPage
export const MONTHS_VI = ['Tháng 1','Tháng 2','Tháng 3','Tháng 4','Tháng 5','Tháng 6','Tháng 7','Tháng 8','Tháng 9','Tháng 10','Tháng 11','Tháng 12'];
export const WDAYS = ['CN','T2','T3','T4','T5','T6','T7'];

export function daysInMonth(y,m){return new Date(y,m+1,0).getDate();}
export function firstDay(y,m){return new Date(y,m,1).getDay();}
export function toKey(d){if(!d)return null;const dt=new Date(d);if(isNaN(dt))return null;return `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')}`;}
export function fmtDate(d){if(!d)return '';return new Date(d).toLocaleDateString('vi-VN',{day:'2-digit',month:'2-digit',year:'numeric'});}
export function fmtTime(d){if(!d)return '';return new Date(d).toLocaleTimeString('vi-VN',{hour:'2-digit',minute:'2-digit'});}
export const TODAY_KEY = toKey(new Date());

export const IcoL=()=><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><polyline points="15 18 9 12 15 6"/></svg>;
export const IcoR=()=><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><polyline points="9 18 15 12 9 6"/></svg>;
export const IcoCal=({s=12})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>;
export const IcoTask=({s=12})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="9 11 12 14 22 4"/><path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/></svg>;
export const IcoX=({s=14})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>;
export const IcoPin=({s=11})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>;
export const IcoOrg=({s=11})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 9h1"/><path d="M14 9h1"/></svg>;
export const IcoFlag=({s=11})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"/><line x1="4" y1="22" x2="4" y2="15"/></svg>;
export const IcoFolder=({s=11})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>;
export const IcoClock=({s=11})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>;
export const IcoChev=({s=10})=><svg width={s} height={s} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><polyline points="9 18 15 12 9 6"/></svg>;

export function PriBadge({priority}){
  return <span className={`dash-priority-badge dash-priority--${priority}`}>{priority}</span>;
}

export function DueBadge({deadline,isOverdue}){
  if(!deadline)return null;
  const key=toKey(deadline);
  if(isOverdue)return <span className="dash-task-due-badge dash-task-due-badge--overdue">Quá hạn</span>;
  if(key===TODAY_KEY)return <span className="dash-task-due-badge dash-task-due-badge--today">Hôm nay</span>;
  const diff=Math.ceil((new Date(deadline)-new Date())/86400000);
  if(diff<=3)return <span className="dash-task-due-badge dash-task-due-badge--soon">{diff}d</span>;
  return <span className="dash-task-due-badge dash-task-due-badge--soon">{fmtDate(deadline)}</span>;
}
