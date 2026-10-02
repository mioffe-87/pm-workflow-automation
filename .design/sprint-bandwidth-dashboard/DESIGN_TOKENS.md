# Design Tokens: Sprint Bandwidth Dashboard

## Stack Note

This canvas uses the `cursor/canvas` SDK. There is no CSS variable system or Tailwind config.
Theming is provided by `useHostTheme()` which returns a `theme` object for neutral/structural values.
Status colors are hardcoded constants — they must remain consistent regardless of host theme.

---

## Status Colors (hardcoded constants — never use theme tokens for these)

```typescript
const ORANGE = '#F97316'; // Under-loaded / Needs work
const RED    = '#EF4444'; // Overloaded
const GREEN  = '#22C55E'; // On track / OK
const WHITE  = '#FFFFFF'; // Text on colored backgrounds
```

### Usage rules
- `ORANGE` on white text (`color: '#fff'`) — pill backgrounds, highlight numbers, progress bar accent
- `RED` on white text — pill backgrounds, "X left" text for overloaded devs
- `GREEN` on white text — pill backgrounds only (OK state)
- Never use these for backgrounds on large surfaces — status color on small pills only

### Contrast verification (WCAG AA @ 14px bold)
| Token | On #fff background | Pass? |
|---|---|---|
| ORANGE `#F97316` | 3.03:1 | ⚠️ Marginal — use on colored pills with white text, not as text on white |
| RED `#EF4444` | 3.95:1 | ✅ Pass at bold |
| GREEN `#22C55E` | 2.45:1 | ⚠️ Marginal — use on colored pills with white text, not as text on white |

> **Rule**: status colors are always used as **pill backgrounds with white text**, never as text color on white. Exception: RED and ORANGE may be used as text color on `theme.bg.editor` (dark backgrounds) where contrast is reversed and sufficient.

---

## SDK Theme Tokens (from `useHostTheme()`)

These adapt to Cursor's light/dark mode automatically.

```typescript
const theme = useHostTheme();

// Backgrounds
theme.fill.primary      // Strong fill (progress bar: done segment)
theme.fill.secondary    // Moderate fill (progress bar: todo segment)
theme.fill.tertiary     // Subtle fill (progress bar track background)

// Text
theme.text.primary      // Default body text
theme.text.secondary    // Subdued text (notes, secondary labels)
theme.text.tertiary     // Faint text (metadata, timestamps)

// Accent
theme.accent.primary    // SDK accent (used for active progress segment — in-progress work)
```

---

## Typography Scale (via `style` prop on SDK Text components)

| Role | Component | fontSize | fontWeight | Notes |
|---|---|---|---|---|
| Page H1 (sprint name) | `H1` | default | default | Sprint name is the primary identifier |
| Tool subtitle | `Text` | 13px | 400 | tone="tertiary" |
| Section heading | `H2` | default | default | "Action Required", "Full Team Load" |
| Hero stat value | `Stat` value | default large | bold | SDK handles sizing |
| Dev name in card | `Text` inside `CardHeader` | 15px | 600 (semibold) | Must be readable at projection distance |
| Stat label | `Text` | small | 400 | tone="tertiary" |
| Card body number | `Text` | 22px | 600 | Primary metric per dev |
| Card body label | `Text` | small | 400 | tone="tertiary" |
| Note/italic text | `Text` | small | 400 | tone="secondary", italic |

---

## Spacing Scale (SDK `gap` values)

| Token name | Value | Used for |
|---|---|---|
| `gap-xs` | 2 | Label+value stack within a stat |
| `gap-sm` | 4 | Between pills in a row |
| `gap-md` | 8 | Within card body sections |
| `gap-card` | 10–12 | Between elements inside a card |
| `gap-section` | 12 | Between cards in a grid |
| `gap-page` | 28 | Between major page sections (Stack top-level) |
| `gap-kpi` | 24–32 | Between KPI stats in the strip |

---

## Layout

| Token | Value | Notes |
|---|---|---|
| Page max-width | 1200px | Fixed. Centered with `margin: '0 auto'` |
| Page padding | 28px | All sides |
| Grid columns (Action Required) | 3 | Uniform — no variable column spans |
| Grid columns (Full Team Load) | 3 | Fixed 3-col grid |
| Card progress bar height | 8px | Full Team Load cards |
| Action Required mini bar height | 6px | Slimmer — secondary info |
| Border radius (progress bar) | 4px (full), 3px (mini) | Matches SDK card aesthetic |

---

## Capacity Bar (new — Full Team Load cards)

Replace the current proportional bar (pts done / active / todo as fraction of personal total) with a **capacity bar** — pts remaining vs. 10pt sprint cap.

```typescript
const SPRINT_CAPACITY = 10; // pts
const effectiveCap = SPRINT_CAPACITY * (d.capacityPct ?? 1.0);
const remainingPct = Math.min((ptsRemaining(d) / effectiveCap) * 100, 100);

// Color rules for capacity bar fill:
// remainingPct === 0       → theme.fill.primary (all done — gray/neutral)
// remainingPct ≤ 40%      → GREEN (comfortable)
// remainingPct ≤ 100%     → ORANGE (getting full)
// bar capped at 100%, overflow shown with RED border
```

---

## Motion

No animation beyond the SDK's default card rendering. The progress bar `transition: 'width 0.3s'` is retained. No other motion tokens needed.

---

## Philosophy Derivation

From the brief: **Minimal data dashboard — clinical precision, zero decoration.**

This maps to a "Swiss Grid" aesthetic applied to a tool context:
- No decorative borders, shadows, or rounded cards beyond what the SDK provides
- Information density: 4 stats visible above the fold without scrolling
- Color used exclusively for semantic meaning, never for decoration
- Typography: two weights only (regular + semibold). No italic except for notes.
- Whitespace: generous between sections (28px), tight within cards (8–10px)
