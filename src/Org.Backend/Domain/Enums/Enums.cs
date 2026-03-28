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

// ── Milestone & Task ──────────────────────────────────────
public enum MilestoneStatus   { NotStarted, InProgress, Completed }
public enum TaskStatus        { Todo, InProgress, Done }
public enum TaskPriority      { Low, Medium, High }

// ── Event ─────────────────────────────────────────────────
public enum EventStatus       { Draft, Planning, Ongoing, Completed }

// ── Attendee ──────────────────────────────────────────────
public enum AttendeeStatus    { Registered, Attended, Cancelled }

// ── DigitalAsset ──────────────────────────────────────────
public enum FileType          { Image, Document, Spreadsheet, Video }
