namespace Org.Backend.Domain.Enums;

// ── User ──────────────────────────────────────────────────
public enum UserStatus        { Active, Inactive, Banned }

// ── Organisation ──────────────────────────────────────────
public enum OrgStatus         { Active, Inactive }

// ── ActivityHistory ───────────────────────────────────────
public enum ActivityType      { EventCreated, MemberJoined, RoleChanged, RequestApproved, TaskCompleted, Other }

// ── Request ───────────────────────────────────────────────
public enum RequestType       { JoinClub, ApproveEvent, ResourceBorrow }
public enum RequestStatus     { Pending, Approved, Rejected }

// ── Resource ──────────────────────────────────────────────
public enum ResourceStatus    { Available, InUse, Unavailable }

// ── Attendee ──────────────────────────────────────────────
public enum AttendeeStatus    { Registered, Attended, Cancelled }

// ── DigitalAsset ──────────────────────────────────────────
public enum FileType          { Image, Document, Spreadsheet, Video }
