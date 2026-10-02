# Design Brief: Sprint Bandwidth Dashboard

## Problem

A PM running a 13-person engineering sprint has no fast way to see who is about to go idle. The JIRA board shows ticket status but not developer bandwidth — to know that Caitlin has 3 pts left and nothing queued, you'd need to manually click through 13 profiles. By the time you've done that, the sprint is half over and under-loaded devs have lost days.

## Solution

A single-screen dashboard that reads story points from JIRA every morning and answers one question in under 3 seconds: **who needs work assigned today?** Under-loaded developers are surfaced prominently at the top. Every dev card links directly to their JIRA filter. The PM can make assignment decisions without leaving this view.

## Experience Principles

1. **Action over analysis** — The first thing you see must be actionable, not informational. KPI stats support the action; they do not lead it.
2. **Signal clarity over completeness** — Three states only: needs work (orange), on track (green), overloaded (red). No gradients, no maybes. A dev is in exactly one state.
3. **Glanceable at projection distance** — Dev names are bold and large enough to read across a standup room. Status colors carry meaning without requiring text labels.

## Aesthetic Direction

- **Philosophy**: Minimal data dashboard — clinical precision, zero decoration. Inspired by Linear's issue tracker and Vercel's deployment dashboard.
- **Tone**: Authoritative, calm, fast. This is a tool, not a product. It should feel like a well-formatted spreadsheet, not a SaaS marketing page.
- **Reference points**: Linear project views, GitHub Actions run summary, Vercel deployment table.
- **Anti-references**: Colorful BI dashboards (Tableau, PowerBI). No gradients, no chart animations, no rounded pill nav bars.

## Existing Patterns

This is a Cursor Canvas component using the `cursor/canvas` SDK. All layout and theming must use SDK primitives.

- **Typography**: `H1`, `H2`, `Text` from `cursor/canvas`. Size and weight controlled via `style` prop.
- **Colors**: `useHostTheme()` for backgrounds and neutral text. Hardcoded hex for status: `#F97316` (orange/under), `#EF4444` (red/over), `#22C55E` (green/ok).
- **Spacing**: `Stack gap={N}`, `Row gap={N}`, `Grid columns={N} gap={N}` — all SDK-native.
- **Components in use**: `Card`, `CardHeader`, `CardBody`, `Pill`, `Divider`, `Stat`, `Grid`, `Row`, `Stack`, `Text`, `H1`, `H2`.

## Component Inventory

| Component | Status | Notes |
|---|---|---|
| Hero stat ("X devs need work") | **Modify** | Currently a Pill next to H2. Promote to a large `Stat` above the fold. |
| Action Required section | **Modify** | Three badge variants → one "Needs work" badge. Uniform 3-col grid. |
| Dev name in cards | **Modify** | Bold 15px — already done. Add JIRA deep-link `<a>` wrapper. |
| KPI strip | **Modify** | Keep 6 stats but deprioritize visually — below the hero stat, above Action Required. |
| Capacity slider (6/8/10/13/16 pts) | **Remove** | Hardcoded at 10 pts. Control never used. |
| Full Team Load cards | **Modify** | Add capacity bar: pts remaining vs. 10pt cap, capped at 100%. Replaces current proportional bar. |
| Filter pills (all/under/ok/over) | **Keep** | Retain for filtering. |
| Legend | **Remove** | Status colors are self-evident. |
| JIRA deep-link | **New** | Each dev name becomes a link to `project=CSP AND assignee=X AND sprint=current`. |

## Key Interactions

- **Page load**: "Action Required" section is immediately visible. No scroll needed to see who needs work.
- **Dev name click**: Opens JIRA filtered view for that developer in a new tab.
- **Filter pill click**: Filters the "Full Team Load" grid to that state only. Action Required section always shows regardless of filter.
- **Capacity threshold**: Hardcoded 10 pts. Not interactive.
- **Data freshness**: Header shows "Fetched: [date/time] · Refreshes daily 7:00 AM PST". No manual refresh button.

## Responsive Behavior

Fixed layout only. `maxWidth: 1200px`, centered. Canvas renders at fixed width — no breakpoint handling needed or desired.

## Accessibility Requirements

- Status must never rely on color alone — each dev card shows both the colored pill label AND a text descriptor in the card body.
- Dev name links must have descriptive `aria-label` (e.g., "Open JIRA for Caitlin Lee").
- Contrast: all status colors (#F97316, #EF4444, #22C55E) on white text pass WCAG AA at their usage sizes.

## Out of Scope

- Editing sprint assignments from within the dashboard.
- Historical sprint comparison (velocity trends, burndown).
- Mobile / responsive layout.
- Per-developer notes or comments.
- Authentication / access control (PM-only by convention, not enforcement).
- Real-time WebSocket updates (daily cron refresh is sufficient).
