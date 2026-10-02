# Information Architecture: Sprint Bandwidth Dashboard

## Site Map

Single-page canvas. No routing or navigation. One view, one purpose.

```
Sprint Bandwidth Dashboard (canvas)
└── /sprint-bandwidth-dashboard.canvas.tsx
```

## Navigation Model

- **Primary navigation**: None. Single-page tool.
- **Secondary navigation**: Filter pills (All / Under / On Track / Overloaded) — scoped to the Full Team Load section only. Action Required is always visible regardless of filter.
- **Utility navigation**: None.
- **Mobile navigation**: Out of scope. Fixed-width canvas only.

## Content Hierarchy

### Sprint Bandwidth Dashboard (single view)

1. **Page identity** — Sprint name as H1, tool subtitle beneath, fetch timestamp + auto-refresh notice. Answers: "what sprint am I looking at, and how fresh is this?"
2. **Hero stat** — "X devs need work assigned" as a single prominent number. Answers the primary question in under 1 second.
3. **KPI strip** — 5 supporting stats: pts done, pts remaining, % complete, on-track count, overloaded count. Secondary context.
4. **Action Required section** — Uniform 3-column grid of under-loaded dev cards, each with badge, pts remaining, pts available, and JIRA link. The actionable surface.
5. **Filter controls** — One row: filter pills (All / Under / On Track / Overloaded). Scopes the section below.
6. **Full Team Load grid** — All 13 devs, sorted by pts remaining descending. Capacity bar per dev. Red/green/orange pill. Clicking name opens JIRA.
7. **Footer** — Sprint end date reminder only: "Sprint ends Oct 9, 2026".

## User Flows

### Primary flow: Morning assignment check (daily, < 2 min)
1. PM opens canvas
2. Sees hero stat: "7 devs need work"
3. Scans Action Required cards — sees names, pts available
4. Clicks a dev name → JIRA opens with that dev's filter pre-applied
5. Assigns a backlog ticket in JIRA
6. Returns to dashboard, scans remaining Action Required cards

### Secondary flow: Overload check
1. PM clicks "Overloaded (3)" filter pill
2. Full Team Load grid filters to show only red cards
3. PM sees Veerendra: 23 pts remaining — flags for team lead
4. Clicks Veerendra's name → JIRA opens

### Edge case: Sprint end approaching
1. Footer shows "Sprint ends Oct 9" — PM notices 2 days left
2. Reconsiders assigning large-pt tickets to under-loaded devs
3. Decision made in context, no additional screen needed

## Naming Conventions

| Concept | Label in UI | Notes |
|---|---|---|
| Developer load state | Under / OK / Over | Never "Under-loaded" or "Overloaded" in pills — too long |
| Points | pts | Abbreviated everywhere. "Story points" only in tooltips or explanatory text |
| Available capacity | "~X pts available" | Tilde signals approximation |
| Badge for Action Required | "Needs work" | Replaces "No tickets" / "Done — needs more work" / "Under-loaded" |
| Sprint identifier | Shown as H1 verbatim: `CSP-Core-FY27Q1S5-9/28-10/09` | Never abbreviated |
| Fetch time | "Fetched: Sep 30, 2026 · 9:35 PM" | Full date+time, always |

## Component Reuse Map

| Component | Used in | Notes |
|---|---|---|
| Dev card (name + badge + stats + bar) | Action Required, Full Team Load | Same card shape; Action Required subset shows "pts available" stat instead of "pts to do" |
| Capacity bar | Full Team Load cards | New: pts remaining / 10pt cap, capped at 100% |
| Status pill (Under/OK/Over) | All dev cards | Color-coded: orange / green / red |
| JIRA deep-link | Dev name in every card | Opens `project=CSP AND assignee=X AND sprint=current` |

## Content Growth Plan

- **Per-sprint**: Data is replaced entirely each sprint. No accumulation.
- **Team changes**: Adding/removing a dev = one-line data array change.
- **Capacity overrides**: `capacityPct` field on Dev interface handles part-time devs.
- **Multi-sprint history**: Out of scope for this brief.

## URL Strategy

Canvas has no URL routing. JIRA deep-links follow this pattern:
```
https://jira-dc.paloaltonetworks.com/issues/?jql=
  project%20%3D%20CSP%20AND%20assignee%20%3D%20%22{displayName}%22%20AND%20sprint%20%3D%20%22CSP-Core-FY27Q1S5%22
```
One link per dev, constructed from their `name` field. Opens in new tab.
