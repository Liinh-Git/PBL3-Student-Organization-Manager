namespace Org.Shared;

// ── Member ────────────────────────────────────────────────
public enum MemberRole
{
    Member = 0,
    Manager = 1,
    VicePresident = 2,
    President = 3
}

// ── Milestone & Task ──────────────────────────────────────
public enum MilestoneStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

// ── Event ─────────────────────────────────────────────────
public enum EventStatus
{
    Draft = 0,
    Planning = 1,
    Ongoing = 2,
    Completed = 3
}

