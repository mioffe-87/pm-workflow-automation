# Build Tasks: Sprint Bandwidth Dashboard

Generated from: .design/sprint-bandwidth-dashboard/DESIGN_BRIEF.md  
Date: Oct 1, 2026  
Philosophy: Minimal data dashboard — clinical precision, zero decoration (Swiss Grid / Linear aesthetic)

---

## Foundation

- [x] **Flip H1/subtitle hierarchy**: Sprint name is H1. "Sprint Bandwidth Dashboard" moved to small all-caps uppercase label above H1.
- [x] **Remove capacity slider**: Deleted. `CAPACITY = 10` hardcoded constant.
- [x] **Remove legend**: Deleted entirely.

---

## Core UI

- [x] **Hero stat — "X devs need work"**: Large 48px orange number in a tinted panel above the KPI strip. Shows count + sprint end date + days remaining.
- [x] **Collapse Action Required badges to "Needs work"**: All three badge variants unified to single `Needs work` orange pill.
- [x] **JIRA deep-link on every dev name**: `<a>` wrapper with `jiraLink(name)` JQL URL, `target="_blank"`, `aria-label`. `DevName` shared component used in all cards.
- [x] **Capacity bar in Full Team Load cards**: Replaced proportional bar with `CapacityBar` component — pts remaining / effective cap, capped at 100%. Fill color: green ≤60%, orange ≤100%, red overflow. Red outline when over.

- [x] **Sprint end footer**: Centered footer showing full sprint end date + calendar days remaining. Computed from `SPRINT_END` constant.

---

## Interactions & States

- [x] **Filter pill state improvement**: Active filter pill uses neutral `#6B7280` (not blue). Filter label updated: "Need Work" instead of "Under".
- [x] **Zero-state for Action Required**: If `actionCount === 0`, renders a `Callout` with success tone: "✅ All developers have queued work."
- [x] **Overloaded dev visual emphasis**: `borderLeft: '3px solid #EF4444'` on overloaded cards in Full Team Load.

---

## Polish

- [x] **Accessibility pass**: All JIRA links have `aria-label`. Status pills use both color and text label. `CapacityBar` uses color + outline for overflow state.
- [x] **Remove unused code**: Deleted `tableHeaders`, `tableRows`, `setCapacity`, capacity slider, legend, `H3`, `Table`. Cleaned all unused imports.

---

## Review

- [ ] **Design review**: Run `/design-review` against `.design/sprint-bandwidth-dashboard/DESIGN_BRIEF.md`.
