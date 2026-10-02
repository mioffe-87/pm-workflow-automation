# CSP AI Capabilities — Project Context

Stack: TanStack Start v1 (React 19, Vite 7), Tailwind v4 (src/styles.css), backend via Supabase (Lovable Cloud). Routes in src/routes (file-based). Main app lives under /synapse/* (branded 'CSP AI Capabilities').

## File tree
```
.env
.gitignore
.lovable/plan/npi-dashboard-2026-09-05.md
.lovable/plan/overall-capability-view-2026-09-18.md
.lovable/project.json
.prettierignore
.prettierrc
AGENTS.md
README.md
bunfig.toml
components.json
eslint.config.js
package.json
src/components/capability-classifier.tsx
src/components/cases-panel.tsx
src/components/metrics-panel.tsx
src/components/runbooks-panel.tsx
src/components/synapse-shell.tsx
src/components/time-ago.tsx
src/hooks/use-mobile.tsx
src/integrations/supabase/auth-attacher.ts
src/integrations/supabase/auth-middleware.ts
src/integrations/supabase/client.server.ts
src/integrations/supabase/client.ts
src/integrations/supabase/cron-auth.ts
src/integrations/supabase/previewAuthStorage.ts
src/integrations/supabase/types.ts
src/lib/error-capture.ts
src/lib/error-page.ts
src/lib/lovable-error-reporting.ts
src/lib/synapse.ts
src/lib/utils.ts
src/routeTree.gen.ts
src/router.tsx
src/routes/README.md
src/routes/__root.tsx
src/routes/api/public/pendo-alert.ts
src/routes/index.tsx
src/routes/synapse.about.tsx
src/routes/synapse.architecture.tsx
src/routes/synapse.capability-impact.tsx
src/routes/synapse.classifier.tsx
src/routes/synapse.context-graph.tsx
src/routes/synapse.csp-agentic-ai.tsx
src/routes/synapse.index.tsx
src/routes/synapse.roadmap.tsx
src/routes/synapse.signals.tsx
src/server.ts
src/start.ts
src/styles.css
supabase/config.toml
supabase/migrations/20260905151757_f427aecf-4352-464d-9da3-64e5fe18724f.sql
supabase/migrations/20260905152811_f275ca24-74c3-497d-ab92-d3aea9fa8c6d.sql
supabase/migrations/20260905153150_42447b9e-73db-4ba4-8ceb-c1e1c9307b5a.sql
supabase/migrations/20260905154732_e464da53-ba8b-41cb-9f1d-a5599a1494cf.sql
supabase/migrations/20260907010254_b292594e-9a2b-428f-abf5-a2d60985011b.sql
supabase/migrations/20260907064425_d2b57c8e-5c3c-4ced-bce9-801b094e2ac6.sql
supabase/migrations/20260910040429_f268bba5-0861-439c-826e-e5061c617e06.sql
tsconfig.json
vite.config.ts
```

## `package.json`

```json
{
  "name": "tanstack_start_ts",
  "private": true,
  "sideEffects": false,
  "type": "module",
  "scripts": {
    "dev": "vite dev",
    "build": "vite build",
    "build:dev": "vite build --mode development",
    "preview": "vite preview",
    "lint": "eslint .",
    "format": "prettier --write ."
  },
  "overrides": {
    "rolldown": "1.2.1"
  },
  "dependencies": {
    "@hookform/resolvers": "^5.2.2",
    "@radix-ui/react-accordion": "^1.2.12",
    "@radix-ui/react-alert-dialog": "^1.1.15",
    "@radix-ui/react-aspect-ratio": "^1.1.8",
    "@radix-ui/react-avatar": "^1.1.11",
    "@radix-ui/react-checkbox": "^1.3.3",
    "@radix-ui/react-collapsible": "^1.1.12",
    "@radix-ui/react-context-menu": "^2.2.16",
    "@radix-ui/react-dialog": "^1.1.15",
    "@radix-ui/react-dropdown-menu": "^2.1.16",
    "@radix-ui/react-hover-card": "^1.1.15",
    "@radix-ui/react-label": "^2.1.8",
    "@radix-ui/react-menubar": "^1.1.16",
    "@radix-ui/react-navigation-menu": "^1.2.14",
    "@radix-ui/react-popover": "^1.1.15",
    "@radix-ui/react-progress": "^1.1.8",
    "@radix-ui/react-radio-group": "^1.3.8",
    "@radix-ui/react-scroll-area": "^1.2.10",
    "@radix-ui/react-select": "^2.2.6",
    "@radix-ui/react-separator": "^1.1.8",
    "@radix-ui/react-slider": "^1.3.6",
    "@radix-ui/react-slot": "^1.2.4",
    "@radix-ui/react-switch": "^1.2.6",
    "@radix-ui/react-tabs": "^1.1.13",
    "@radix-ui/react-toggle": "^1.1.10",
    "@radix-ui/react-toggle-group": "^1.1.11",
    "@radix-ui/react-tooltip": "^1.2.8",
    "@supabase/supabase-js": "^2.115.0",
    "@tailwindcss/vite": "^4.2.1",
    "@tanstack/react-query": "^5.101.1",
    "@tanstack/react-router": "1.170.18",
    "@tanstack/react-start": "1.168.32",
    "@tanstack/router-plugin": "1.168.23",
    "class-variance-authority": "^0.7.1",
    "clsx": "^2.1.1",
    "cmdk": "^1.1.1",
    "date-fns": "^4.1.0",
    "embla-carousel-react": "^8.6.0",
    "input-otp": "^1.4.2",
    "lucide-react": "^0.575.0",
    "react": "^19.2.0",
    "react-day-picker": "^9.14.0",
    "react-dom": "^19.2.0",
    "react-hook-form": "^7.71.2",
    "react-resizable-panels": "^4.6.5",
    "recharts": "^2.15.4",
    "sonner": "^2.0.7",
    "tailwind-merge": "^3.5.0",
    "tailwindcss": "^4.2.1",
    "tw-animate-css": "^1.3.4",
    "vaul": "^1.1.2",
    "vite-tsconfig-paths": "^6.0.2",
    "zod": "^3.25.76"
  },
  "devDependencies": {
    "@eslint/js": "^9.32.0",
    "@lovable.dev/vite-tanstack-config": "2.23.1",
    "@types/node": "^22.16.5",
    "@types/react": "^19.2.0",
    "@types/react-dom": "^19.2.0",
    "@vitejs/plugin-react": "^5.2.0",
    "eslint": "^9.32.0",
    "eslint-config-prettier": "^10.1.1",
    "eslint-plugin-prettier": "^5.2.6",
    "eslint-plugin-react-hooks": "^5.2.0",
    "eslint-plugin-react-refresh": "^0.4.20",
    "globals": "^15.15.0",
    "nitro": "3.0.260603-beta",
    "prettier": "^3.7.3",
    "typescript": "^5.8.3",
    "typescript-eslint": "^8.56.1",
    "vite": "8.1.5"
  }
}

```

## `src/components/capability-classifier.tsx`

```tsx
import { Fragment, useMemo, useState } from "react";
import { Card, CardContent } from "@/components/ui/card";

export const PATTERNS = [
  "Autonomous Orchestration",
  "Predictive Optimization",
  "Closed-Loop Remediation",
  "Contextual Insights",
  "Adaptive Governance",
] as const;
export type Pattern = (typeof PATTERNS)[number];

export const LEVELS = ["Autonomous", "Assisted", "Human in the Loop", "Manual today"] as const;
export type Level = (typeof LEVELS)[number];

export type Capability = {
  name: string;
  area: string;
  sub?: string;
  pattern: Pattern;
  level: Level;
  note: string;
};

// Groups follow the Customer Support Portal capability map exactly.
export const CAPABILITIES: Capability[] = [
  // Account management
  { name: "Account management", area: "Account management", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Agent prepares the change; an admin confirms account-level edits." },
  { name: "Default role management", area: "Account management", pattern: "Autonomous Orchestration", level: "Assisted", note: "Roles proposed from the pattern of existing users in the account." },
  { name: "Account linking to enable sharing of assets", area: "Account management", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Affinity links carry entitlement risk — approval stays with a person." },
  { name: "Enable 3rd party IDP", area: "Account management", pattern: "Closed-Loop Remediation", level: "Assisted", note: "Misconfigured IdP is detected and the corrective step is drafted." },
  { name: "User management", area: "Account management", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Create, update and deactivate users from a verified request." },
  { name: "Enterprise RBAC for products", area: "Account management", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Access widening is always reviewed." },
  { name: "RBAC — assign roles to access specific products", area: "Account management", pattern: "Adaptive Governance", level: "Assisted", note: "Agent proposes the role set; the admin grants it." },
  { name: "Account registration link to add new users", area: "Account management", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Link issued and expired without a case." },
  { name: "Admin approval for new users", area: "Account management", pattern: "Adaptive Governance", level: "Assisted", note: "Agent ranks pending approvals by risk and account fit." },
  { name: "CPSP user access to LIVEcommunity training", area: "Account management", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Provisioning that used to be manual now runs end to end." },
  { name: "Download CSV of users in an account", area: "Account management", pattern: "Contextual Insights", level: "Autonomous", note: "Generated on request, scoped to the caller's account." },

  // Asset management
  { name: "Asset management", area: "Asset management", sub: "Visibility & insights", pattern: "Autonomous Orchestration", level: "Assisted", note: "Bulk actions staged by the agent, executed on confirmation." },
  { name: "Unified product view", area: "Asset management", sub: "Visibility & insights", pattern: "Contextual Insights", level: "Autonomous", note: "The account's state assembled in one place, continuously." },
  { name: "Manage tags", area: "Asset management", sub: "Visibility & insights", pattern: "Contextual Insights", level: "Assisted", note: "Tags suggested from asset grouping patterns." },
  { name: "Best practice assessments", area: "Asset management", sub: "Visibility & insights", pattern: "Predictive Optimization", level: "Assisted", note: "Findings prioritised by exposure, not listed flat." },
  { name: "Find assets with BPAs", area: "Asset management", sub: "Visibility & insights", pattern: "Contextual Insights", level: "Autonomous", note: "Assembled from assessment history continuously." },

  { name: "Activate / deactivate licenses", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Highest-volume self-service action; fully executable in-CSP." },
  { name: "Activate Cloud Delivered Security Services (CDSS)", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Closed-Loop Remediation", level: "Autonomous", note: "Failed activations retried and verified by re-running the action." },
  { name: "Manage add-on CDSS licenses", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Autonomous Orchestration", level: "Autonomous", note: "URL filtering, GlobalProtect, DNS Security and WildFire handled in-CSP." },
  { name: "Deactivate add-on license", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Reversible and low risk; no case needed." },
  { name: "Trial licenses", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Self-serve activation with no human step." },
  { name: "Upgrade PAN-OS license", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Predictive Optimization", level: "Assisted", note: "Recommended release chosen from installed base and advisories." },
  { name: "Transfer licenses between defective hardware", area: "Asset management", sub: "Licenses & subscriptions", pattern: "Closed-Loop Remediation", level: "Assisted", note: "Agent matches the spare and pre-fills the transfer." },

  { name: "Bootstrap file management", area: "Asset management", sub: "Onboarding & configuration", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Composed from the account's own configuration." },
  { name: "Download bootstrap file for VM Flex NGFWs", area: "Asset management", sub: "Onboarding & configuration", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Preconfigured and delivered on request." },
  { name: "Day 1 configuration", area: "Asset management", sub: "Onboarding & configuration", pattern: "Autonomous Orchestration", level: "Assisted", note: "Agent drafts the starting config; the admin reviews once." },

  { name: "Decommission PA-Series assets", area: "Asset management", sub: "Lifecycle, transfers & RMA", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Irreversible — confirmed by an admin." },
  { name: "Transfer assets between accounts", area: "Asset management", sub: "Lifecycle, transfers & RMA", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Ownership change requires an accountable approver." },
  { name: "Manage RMA shipments", area: "Asset management", sub: "Lifecycle, transfers & RMA", pattern: "Closed-Loop Remediation", level: "Assisted", note: "Status tracked and stalled shipments chased automatically." },
  { name: "Manage Prisma SD-WAN partial shipments", area: "Asset management", sub: "Lifecycle, transfers & RMA", pattern: "Closed-Loop Remediation", level: "Assisted", note: "Split shipments reconciled and gaps chased." },

  { name: "Asset management — call-to-action alerts", area: "Asset management", sub: "Renewals, expiry & alerts", pattern: "Predictive Optimization", level: "Autonomous", note: "Pushed when the agent detects the condition, not on a schedule." },
  { name: "Find assets with add-ons expiring in 7 days", area: "Asset management", sub: "Renewals, expiry & alerts", pattern: "Predictive Optimization", level: "Autonomous", note: "Detected and surfaced as an actionable alert, not a report." },
  { name: "Find assets with expired add-ons", area: "Asset management", sub: "Renewals, expiry & alerts", pattern: "Predictive Optimization", level: "Assisted", note: "Lapse explained with the renewal path attached." },
  { name: "Grace period expiration", area: "Asset management", sub: "Renewals, expiry & alerts", pattern: "Predictive Optimization", level: "Autonomous", note: "Applied before a lapse is visible to the customer." },

  { name: "FW Flex credit pool management", area: "Asset management", sub: "Credits & consumption", pattern: "Predictive Optimization", level: "Assisted", note: "Consumption forecast flags pools heading for exhaustion." },
  { name: "Training credit dashboard for customers", area: "Asset management", sub: "Credits & consumption", pattern: "Contextual Insights", level: "Autonomous", note: "Balance and expiry surfaced before credits are lost." },

  { name: "Download CSV files containing assets", area: "Asset management", sub: "Reports & exports", pattern: "Contextual Insights", level: "Autonomous", note: "Generated and delivered without a case." },
  { name: "Download CSV for ASC files (backline SKUs)", area: "Asset management", sub: "Reports & exports", pattern: "Contextual Insights", level: "Autonomous", note: "Scoped to the partner's ASC customer cohort." },


  // Case management
  { name: "Create, manage, close support cases", area: "Case management", pattern: "Autonomous Orchestration", level: "Assisted", note: "Case drafted from the signal; a person owns the outcome." },

  // Order management
  { name: "CSSP order management", area: "Order management", pattern: "Autonomous Orchestration", level: "Assisted", note: "Order exceptions flagged with the correcting action." },
  { name: "Prisma SD-WAN partial shipments", area: "Order management", pattern: "Closed-Loop Remediation", level: "Assisted", note: "Multi-location shipments reconciled automatically." },

  // ASC partner features
  { name: "Access controls for ASC partners", area: "ASC partner features", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Cross-organisation access stays under review." },
  
  { name: "ASC scorecard", area: "ASC partner features", pattern: "Contextual Insights", level: "Autonomous", note: "Computed continuously from support outcomes." },
  

  // Enterprise agreements
  { name: "Dashboard of site licenses, ELAs and ESAs", area: "Enterprise agreements", pattern: "Contextual Insights", level: "Autonomous", note: "Spans multiple CSP accounts in one view." },
  { name: "Activate assets against ELA and ESA", area: "Enterprise agreements", pattern: "Autonomous Orchestration", level: "Autonomous", note: "Entitlement matched and applied automatically." },
  { name: "ELA / ESA consumption dashboards", area: "Enterprise agreements", pattern: "Predictive Optimization", level: "Autonomous", note: "Burn-down projected against the agreement term." },
  { name: "Download CSV of assets associated with ELA / ESA", area: "Enterprise agreements", pattern: "Contextual Insights", level: "Autonomous", note: "Report delivered without a case." },

  // FedRAMP
  { name: "FedRAMP Moderate", area: "FedRAMP", pattern: "Adaptive Governance", level: "Manual today", note: "Handled outside CSP in a third-party case system." },
  { name: "FedRAMP IL4 High", area: "FedRAMP", pattern: "Adaptive Governance", level: "Manual today", note: "Separate instance and SSO — outside autonomous scope." },
  { name: "Commercial instance", area: "FedRAMP", pattern: "Contextual Insights", level: "Autonomous", note: "Default path for all non-FedRAMP users." },

  // Software upgrades
  { name: "Download software updates", area: "Software upgrades", pattern: "Predictive Optimization", level: "Assisted", note: "Recommended release surfaced against the installed base." },

  // Product specific features
  { name: "Display WildFire portals", area: "Product specific features", pattern: "Contextual Insights", level: "Autonomous", note: "Regional portal resolved for the user." },
  { name: "WildFire API usage dashboard", area: "Product specific features", pattern: "Contextual Insights", level: "Autonomous", note: "Usage anomalies called out, not just charted." },
  { name: "API key management", area: "Product specific features", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Key issuance and rotation stay approver-gated." },

  // Documentation
  { name: "Catalog of CSP Knowledge Base articles", area: "Documentation", pattern: "Contextual Insights", level: "Autonomous", note: "Retrieved and cited against the user's actual failure." },

  // Other features
  { name: "Customer impersonation", area: "Other features", pattern: "Adaptive Governance", level: "Human in the Loop", note: "Privacy-bound; every session reviewed and logged." },
  { name: "Professional Services engagement", area: "Other features", pattern: "Contextual Insights", level: "Assisted", note: "Agent frames the scope; the engagement is human-owned." },
  { name: "Groups", area: "Other features", pattern: "Contextual Insights", level: "Assisted", note: "Legacy grouping; membership suggested from asset patterns." },
  { name: "Resources and portal links", area: "Other features", pattern: "Contextual Insights", level: "Manual today", note: "Static navigation; candidate for contextual retrieval." },
];

export const DOMAINS = [
  "Account Management",
  "User Management",
  "Asset Management",
  "Case Management",
  "Others",
] as const;
export type Domain = (typeof DOMAINS)[number];

// Capabilities that sit under "Account management" today but are really user administration.
const USER_MANAGEMENT = new Set([
  "User management",
  "Default role management",
  "Account registration link to add new users",
  "Admin approval for new users",
  "CPSP user access to LIVEcommunity training",
  "Download CSV of users in an account",
]);

export function domainOf(c: Capability): Domain {
  if (USER_MANAGEMENT.has(c.name)) return "User Management";
  if (c.area === "Account management") return "Account Management";
  if (c.area === "Asset management") return "Asset Management";
  if (c.area === "Case management") return "Case Management";
  return "Others";
}

// Sub-group label inside a domain: the asset sub-group, or the original map group.
export function subOf(c: Capability): string | undefined {
  if (c.sub) return c.sub;
  const d = domainOf(c);
  if (d === "Others") return c.area;
  return undefined;
}

export const JOURNEY_STAGES = [
  "Onboard",
  "Provisioning",
  "Operate & monitor",
  "Renew & expand",
  "Support & resolve",
  "Govern & report",
] as const;
export type JourneyStage = (typeof JOURNEY_STAGES)[number];

const JOURNEY_OVERRIDE: Record<string, JourneyStage> = {
  "Account management": "Onboard",
  "Account linking to enable sharing of assets": "Onboard",
  "Enable 3rd party IDP": "Onboard",
  "User management": "Onboard",
  "Default role management": "Onboard",
  "Account registration link to add new users": "Onboard",
  "Admin approval for new users": "Onboard",
  "CPSP user access to LIVEcommunity training": "Onboard",
  "Enterprise RBAC for products": "Govern & report",
  "RBAC — assign roles to access specific products": "Govern & report",
  "Download CSV of users in an account": "Govern & report",
  "Asset management": "Operate & monitor",
  "Create, manage, close support cases": "Support & resolve",
  "CSSP order management": "Renew & expand",
  "Prisma SD-WAN partial shipments": "Provisioning",
  "Access controls for ASC partners": "Govern & report",
  "ASC scorecard": "Govern & report",
  "Dashboard of site licenses, ELAs and ESAs": "Govern & report",
  "Activate assets against ELA and ESA": "Provisioning",
  "ELA / ESA consumption dashboards": "Renew & expand",
  "Download CSV of assets associated with ELA / ESA": "Govern & report",
  "FedRAMP Moderate": "Support & resolve",
  "FedRAMP IL4 High": "Support & resolve",
  "Commercial instance": "Onboard",
  "Download software updates": "Operate & monitor",
  "Display WildFire portals": "Operate & monitor",
  "WildFire API usage dashboard": "Operate & monitor",
  "API key management": "Govern & report",
  "Catalog of CSP Knowledge Base articles": "Support & resolve",
  "Customer impersonation": "Support & resolve",
  "Professional Services engagement": "Provisioning",
  Groups: "Operate & monitor",
  "Resources and portal links": "Onboard",
};

const JOURNEY_BY_SUB: Record<string, JourneyStage> = {
  "Visibility & insights": "Operate & monitor",
  "Licenses & subscriptions": "Provisioning",
  "Onboarding & configuration": "Onboard",
  "Lifecycle, transfers & RMA": "Support & resolve",
  "Renewals, expiry & alerts": "Renew & expand",
  "Credits & consumption": "Renew & expand",
  "Reports & exports": "Govern & report",
};

export function journeyOf(c: Capability): JourneyStage {
  const byName = JOURNEY_OVERRIDE[c.name];
  if (byName) return byName;
  const bySub = c.sub ? JOURNEY_BY_SUB[c.sub] : undefined;
  if (bySub) return bySub;
  return "Operate & monitor";
}

export const JOURNEY_CLASS: Record<JourneyStage, string> = {
  Onboard: "bg-[hsl(200_75%_93%)] text-[hsl(205_70%_28%)]",
  "Provisioning": "bg-[hsl(165_55%_90%)] text-[hsl(168_60%_24%)]",
  "Operate & monitor": "bg-[hsl(45_90%_90%)] text-[hsl(35_75%_30%)]",
  "Renew & expand": "bg-[hsl(268_65%_94%)] text-[hsl(268_55%_38%)]",
  "Support & resolve": "bg-[hsl(12_80%_93%)] text-[hsl(12_65%_34%)]",
  "Govern & report": "bg-[hsl(220_20%_92%)] text-[hsl(220_20%_32%)]",
};

export const PATTERN_CLASS: Record<Pattern, string> = {
  "Autonomous Orchestration": "bg-[hsl(190_70%_92%)] text-[hsl(190_70%_25%)]",
  "Predictive Optimization": "bg-[hsl(255_70%_94%)] text-[hsl(255_55%_38%)]",
  "Closed-Loop Remediation": "bg-[hsl(150_55%_90%)] text-[hsl(150_60%_24%)]",
  "Contextual Insights": "bg-[hsl(38_90%_90%)] text-[hsl(30_75%_30%)]",
  "Adaptive Governance": "bg-[hsl(340_70%_94%)] text-[hsl(340_60%_36%)]",
};

export const LEVEL_CLASS: Record<Level, string> = {
  Autonomous: "bg-status-delivered text-status-delivered-foreground",
  Assisted: "bg-status-progress text-status-progress-foreground",
  "Human in the Loop": "bg-accent text-accent-foreground",
  "Manual today": "bg-muted text-muted-foreground",
};

export const LEVEL_BAR: Record<Level, string> = {
  Autonomous: "bg-status-delivered",
  Assisted: "bg-status-progress",
  "Human in the Loop": "bg-accent",
  "Manual today": "bg-muted-foreground/40",
};

const PILL = "inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold";

export function CapabilityClassifier() {
  const [pattern, setPattern] = useState<Pattern | null>(null);
  const [level, setLevel] = useState<Level | null>(null);
  const [domain, setDomain] = useState<Domain | null>(null);
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());

  const domainSummary = useMemo(
    () =>
      DOMAINS.map((d) => {
        const items = CAPABILITIES.filter((c) => domainOf(c) === d);
        const counts = LEVELS.map((l) => ({
          level: l,
          count: items.filter((c) => c.level === l).length,
        }));
        const patternCounts = PATTERNS.map((p) => ({
          pattern: p,
          count: items.filter((c) => c.pattern === p).length,
        })).sort((a, b) => b.count - a.count);
        const noHuman = items.filter((c) => c.level === "Autonomous").length;
        return {
          domain: d,
          total: items.length,
          counts,
          topPattern: patternCounts[0]?.pattern ?? "—",
          share: items.length ? Math.round((noHuman / items.length) * 100) : 0,
        };
      }),
    [],
  );

  const toggleGroup = (name: string) =>
    setCollapsed((prev) => {
      const next = new Set(prev);
      if (next.has(name)) next.delete(name);
      else next.add(name);
      return next;
    });

  const rows = CAPABILITIES.filter(
    (c) =>
      (!pattern || c.pattern === pattern) &&
      (!level || c.level === level) &&
      (!domain || domainOf(c) === domain),
  );

  const groups = DOMAINS.map((name) => {
    const items = rows.filter((c) => domainOf(c) === name);
    const subNames = Array.from(
      new Set(items.map((c) => subOf(c)).filter(Boolean) as string[]),
    );
    const subs = subNames.map((s) => ({
      name: s,
      key: `${name} / ${s}`,
      items: items.filter((c) => subOf(c) === s),
    }));
    return { name, items, subs, loose: items.filter((c) => !subOf(c)) };
  }).filter((g) => g.items.length > 0);

  const total = CAPABILITIES.length;
  const automatable = CAPABILITIES.filter((c) => c.level === "Autonomous").length;
  const assisted = CAPABILITIES.filter((c) => c.level === "Assisted").length;
  const gated = CAPABILITIES.filter((c) => c.level === "Human in the Loop").length;
  const manual = CAPABILITIES.filter((c) => c.level === "Manual today").length;

  return (
    <section className="space-y-6">
      <div className="border-b border-border pb-5">
        <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">Classification</p>
        <h2 className="mt-1 font-display text-3xl font-bold tracking-tight">
          Capability Automation Classifier
        </h2>
        <p className="mt-2 max-w-3xl text-sm text-muted-foreground">
          Every Customer Support Portal capability mapped to the agentic pattern that fits it and the
          level of autonomy it can safely run at — so the automation sequence is chosen on evidence,
          not appetite.
        </p>
      </div>

      <div className="space-y-3">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <p className="text-xs font-bold uppercase tracking-[0.16em] text-muted-foreground">
            Overall capability view
          </p>
          <p className="text-xs text-muted-foreground">
            {total} capabilities · {Math.round((automatable / total) * 100)}% can run without a human
          </p>
        </div>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-5">
          {domainSummary.map((d) => {
            const selected = domain === d.domain;
            return (
              <button
                key={d.domain}
                type="button"
                onClick={() => setDomain(selected ? null : d.domain)}
                aria-pressed={selected}
                className={`rounded-xl border bg-card p-4 text-left transition-all ${
                  selected ? "border-primary ring-2 ring-primary/30" : "border-border hover:border-primary/40"
                }`}
              >
                <p className="text-sm font-bold">{d.domain}</p>
                <p className="mt-2 font-display text-3xl font-bold tracking-tight">
                  {d.total}
                  <span className="ml-1 text-xs font-semibold text-muted-foreground">
                    capabilities
                  </span>
                </p>
                <div className="mt-3 flex h-2 overflow-hidden rounded-full bg-muted">
                  {d.counts.map((c) =>
                    c.count ? (
                      <span
                        key={c.level}
                        title={`${c.level}: ${c.count}`}
                        className={LEVEL_BAR[c.level]}
                        style={{ width: `${(c.count / d.total) * 100}%` }}
                      />
                    ) : null,
                  )}
                </div>
                <p className="mt-2 text-xs text-muted-foreground">
                  {d.counts.map((c) => `${c.count} ${c.level.toLowerCase()}`).join(" · ")}
                </p>
                <p className="mt-2 text-xs font-semibold text-foreground">
                  {d.share}% without a human
                </p>
                <p className="text-xs text-muted-foreground">Mostly {d.topPattern}</p>
              </button>
            );
          })}
        </div>
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {[
          { label: "Runs autonomously", value: automatable, note: "No human step required" },
          { label: "Agent assisted", value: assisted, note: "Agent prepares, person confirms" },
          { label: "Human in the loop", value: gated, note: "Approval is the control" },
          { label: "Manual today", value: manual, note: "Not yet in agentic scope" },
        ].map((s) => (
          <Card key={s.label}>
            <CardContent className="p-5">
              <p className="text-xs font-bold uppercase tracking-[0.16em] text-muted-foreground">
                {s.label}
              </p>
              <p className="mt-2 font-display text-3xl font-bold tracking-tight">
                {s.value}
                <span className="ml-1 text-base font-semibold text-muted-foreground">/ {total}</span>
              </p>
              <p className="mt-1 text-sm text-muted-foreground">{s.note}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardContent className="space-y-3 p-5">
          <p className="text-xs font-bold uppercase tracking-[0.16em] text-muted-foreground">
            Distribution by agentic pattern
          </p>
          <div className="space-y-2">
            {PATTERNS.map((p) => {
              const count = CAPABILITIES.filter((c) => c.pattern === p).length;
              return (
                <div key={p} className="flex items-center gap-3">
                  <span className="w-52 shrink-0 text-sm font-semibold">{p}</span>
                  <div className="h-2.5 flex-1 overflow-hidden rounded-full bg-muted">
                    <div
                      className="h-full rounded-full bg-primary"
                      style={{ width: `${(count / total) * 100}%` }}
                    />
                  </div>
                  <span className="w-8 text-right text-sm font-bold tabular-nums">{count}</span>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => {
            setPattern(null);
            setLevel(null);
            setDomain(null);
          }}
          className="rounded-full border border-border bg-card px-3 py-1 text-xs font-medium text-muted-foreground transition-colors hover:text-foreground"
        >
          All ({total})
        </button>
        {PATTERNS.map((p) => (
          <button
            key={p}
            type="button"
            onClick={() => setPattern(pattern === p ? null : p)}
            aria-pressed={pattern === p}
            className={`rounded-full px-3 py-1 text-xs font-semibold transition-all ${PATTERN_CLASS[p]} ${
              pattern === p ? "ring-2 ring-primary/40" : "opacity-80 hover:opacity-100"
            }`}
          >
            {p} ({CAPABILITIES.filter((c) => c.pattern === p).length})
          </button>
        ))}
        <span className="mx-1 h-5 w-px bg-border" />
        {LEVELS.map((l) => (
          <button
            key={l}
            type="button"
            onClick={() => setLevel(level === l ? null : l)}
            aria-pressed={level === l}
            className={`rounded-full px-3 py-1 text-xs font-semibold transition-all ${LEVEL_CLASS[l]} ${
              level === l ? "ring-2 ring-primary/40" : "opacity-80 hover:opacity-100"
            }`}
          >
            {l}
          </button>
        ))}
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <select
          value={domain ?? ""}
          onChange={(e) => setDomain((e.target.value || null) as Domain | null)}
          className="rounded-md border border-border bg-card px-2 py-1 text-xs font-semibold"
        >
          <option value="">All groups</option>
          {DOMAINS.map((a) => (
            <option key={a} value={a}>
              {a}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={() => setCollapsed(new Set())}
          className="rounded-full border border-border bg-card px-3 py-1 text-xs font-medium text-muted-foreground transition-colors hover:text-foreground"
        >
          Expand all
        </button>
        <button
          type="button"
          onClick={() =>
            setCollapsed(
              new Set([
                ...groups.map((g) => g.name),
                ...groups.flatMap((g) => g.subs.map((s) => s.key)),
              ]),
            )
          }
          className="rounded-full border border-border bg-card px-3 py-1 text-xs font-medium text-muted-foreground transition-colors hover:text-foreground"
        >
          Collapse all
        </button>
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[1080px] text-left text-sm">
              <thead className="border-b border-border bg-muted/40">
                <tr className="text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                  <th className="px-4 py-3">Capability</th>
                  <th className="px-4 py-3">Customer journey</th>
                  <th className="px-4 py-3">Agentic pattern</th>
                  <th className="px-4 py-3">Autonomy</th>
                  <th className="px-4 py-3">Why</th>
                </tr>
              </thead>
              <tbody>
                {groups.map((g) => {
                  const isOpen = !collapsed.has(g.name);
                  return (
                    <Fragment key={g.name}>
                      <tr className="border-b border-border bg-muted/30">
                        <td colSpan={5} className="px-2 py-0">
                          <button
                            type="button"
                            onClick={() => toggleGroup(g.name)}
                            aria-expanded={isOpen}
                            className="flex w-full items-center gap-2 px-2 py-2.5 text-left"
                          >
                            <span
                              className={`text-muted-foreground transition-transform ${isOpen ? "rotate-90" : ""}`}
                              aria-hidden
                            >
                              ▶
                            </span>
                            <span className="text-xs font-bold uppercase tracking-[0.14em]">
                              {g.name}
                            </span>
                            <span className="text-xs font-semibold text-muted-foreground">
                              ({g.items.length})
                            </span>
                          </button>
                        </td>
                      </tr>
                      {isOpen
                        ? [
                            ...g.loose.map((c) => (
                              <tr key={c.name} className="border-b border-border/60">
                                <td className="px-4 py-3 pl-9 font-semibold">{c.name}</td>
                                <td className="px-4 py-3">
                                  <span className={`${PILL} ${JOURNEY_CLASS[journeyOf(c)]}`}>
                                    {journeyOf(c)}
                                  </span>
                                </td>
                                <td className="px-4 py-3">
                                  <span className={`${PILL} ${PATTERN_CLASS[c.pattern]}`}>
                                    {c.pattern}
                                  </span>
                                </td>
                                <td className="px-4 py-3">
                                  <span className={`${PILL} ${LEVEL_CLASS[c.level]}`}>
                                    {c.level}
                                  </span>
                                </td>
                                <td className="px-4 py-3 text-muted-foreground">{c.note}</td>
                              </tr>
                            )),
                            ...g.subs.map((s) => {
                              const subOpen = !collapsed.has(s.key);
                              return (
                                <Fragment key={s.key}>
                                  <tr className="border-b border-border/60 bg-muted/10">
                                    <td colSpan={5} className="px-2 py-0">
                                      <button
                                        type="button"
                                        onClick={() => toggleGroup(s.key)}
                                        aria-expanded={subOpen}
                                        className="flex w-full items-center gap-2 py-2 pl-9 pr-2 text-left"
                                      >
                                        <span
                                          className={`text-muted-foreground transition-transform ${subOpen ? "rotate-90" : ""}`}
                                          aria-hidden
                                        >
                                          ▸
                                        </span>
                                        <span className="text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground">
                                          {s.name}
                                        </span>
                                        <span className="text-[11px] font-semibold text-muted-foreground">
                                          ({s.items.length})
                                        </span>
                                      </button>
                                    </td>
                                  </tr>
                                  {subOpen
                                    ? s.items.map((c) => (
                                        <tr key={c.name} className="border-b border-border/60">
                                          <td className="px-4 py-3 pl-16 font-semibold">{c.name}</td>
                                          <td className="px-4 py-3">
                                            <span className={`${PILL} ${JOURNEY_CLASS[journeyOf(c)]}`}>
                                              {journeyOf(c)}
                                            </span>
                                          </td>
                                          <td className="px-4 py-3">
                                            <span className={`${PILL} ${PATTERN_CLASS[c.pattern]}`}>
                                              {c.pattern}
                                            </span>
                                          </td>
                                          <td className="px-4 py-3">
                                            <span className={`${PILL} ${LEVEL_CLASS[c.level]}`}>
                                              {c.level}
                                            </span>
                                          </td>
                                          <td className="px-4 py-3 text-muted-foreground">
                                            {c.note}
                                          </td>
                                        </tr>
                                      ))
                                    : null}
                                </Fragment>
                              );
                            }),
                          ]
                        : null}
                    </Fragment>
                  );
                })}
                {groups.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">
                      No capabilities match these filters.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </section>
  );
}

```

## `src/components/cases-panel.tsx`

```tsx
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { TimeAgo } from "@/components/time-ago";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { supabase } from "@/integrations/supabase/client";
import {
  CASE_STATUSES,
  LABEL,
  PILL,
  SEVERITIES,
  type SynapseCase,
  casesQuery,
  severityClass,
  signalsQuery,
  stateClass,
} from "@/lib/synapse";

type Draft = {
  id?: string;
  title: string;
  severity: string;
  status: string;
  owner: string;
  customer: string;
  root_cause: string;
  next_step: string;
  resolution: string;
  signal_id: string | null;
};

const EMPTY: Draft = {
  title: "",
  severity: "medium",
  status: "triaged",
  owner: "",
  customer: "",
  root_cause: "",
  next_step: "",
  resolution: "",
  signal_id: null,
};

export function CasesPanel() {
  const { data: cases = [] } = useQuery(casesQuery());
  const { data: signals = [] } = useQuery(signalsQuery());
  const qc = useQueryClient();
  const [filter, setFilter] = useState("open");
  const [draft, setDraft] = useState<Draft | null>(null);

  const invalidate = () => qc.invalidateQueries({ queryKey: ["synapse"] });

  const save = useMutation({
    mutationFn: async (d: Draft) => {
      const payload = {
        title: d.title.trim(),
        severity: d.severity,
        status: d.status,
        owner: d.owner.trim() || "Unassigned",
        customer: d.customer.trim() || "Unknown",
        root_cause: d.root_cause,
        next_step: d.next_step,
        resolution: d.resolution,
        signal_id: d.signal_id,
      };
      const { error } = d.id
        ? await supabase.from("synapse_cases").update(payload).eq("id", d.id)
        : await supabase.from("synapse_cases").insert(payload);
      if (error) throw error;
    },
    onSuccess: () => {
      setDraft(null);
      invalidate();
      toast.success("Case saved");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const setStatus = useMutation({
    mutationFn: async ({ c, status }: { c: SynapseCase; status: string }) => {
      const { error } = await supabase
        .from("synapse_cases")
        .update({ status })
        .eq("id", c.id);
      if (error) throw error;
      if (status === "resolved" && c.signal_id) {
        await supabase.from("synapse_signals").update({ status: "resolved" }).eq("id", c.signal_id);
      }
    },
    onSuccess: (_d, v) => {
      invalidate();
      toast.success(v.status === "resolved" ? "Case marked resolved" : "Case reopened");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: async (id: string) => {
      const { error } = await supabase.from("synapse_cases").delete().eq("id", id);
      if (error) throw error;
    },
    onSuccess: () => {
      invalidate();
      toast.success("Case removed");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const rows = cases.filter((c) =>
    filter === "open" ? c.status !== "resolved" : filter === "all" ? true : c.status === filter,
  );

  const toDraft = (c: SynapseCase): Draft => ({
    id: c.id,
    title: c.title,
    severity: c.severity,
    status: c.status,
    owner: c.owner,
    customer: c.customer,
    root_cause: c.root_cause,
    next_step: c.next_step,
    resolution: c.resolution ?? "",
    signal_id: c.signal_id,
  });

  const resolvedCases = cases.filter((c) => c.status === "resolved");
  const closeRate = cases.length
    ? Math.round((resolvedCases.length / cases.length) * 100)
    : 0;

  return (
    <>
      <div className="mb-5 flex flex-wrap items-center justify-end gap-2">
        <div className="flex items-center gap-2">
          <Select value={filter} onValueChange={setFilter}>
            <SelectTrigger className="w-[170px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="open">Open only</SelectItem>
              <SelectItem value="all">All cases</SelectItem>
              {CASE_STATUSES.map((s) => (
                <SelectItem key={s} value={s}>
                  {LABEL[s]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button onClick={() => setDraft({ ...EMPTY })}>
            <Plus className="size-4" /> New case
          </Button>
        </div>
      </div>
      <div className="mb-5 grid gap-4 sm:grid-cols-3">
        {[
          { label: "Open", value: String(cases.length - resolvedCases.length), hint: "still need a human" },
          { label: "Resolved", value: String(resolvedCases.length), hint: `of ${cases.length} cases` },
          { label: "Close rate", value: `${closeRate}%`, hint: "cases marked resolved" },
        ].map((s) => (
          <Card key={s.label}>
            <CardContent className="p-5">
              <p className="text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                {s.label}
              </p>
              <p className="mt-2 font-display text-3xl font-bold tracking-tight">{s.value}</p>
              <p className="mt-1 text-xs text-muted-foreground">{s.hint}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        {rows.map((c) => {
          const signal = signals.find((s) => s.id === c.signal_id);
          return (
            <Card key={c.id}>
              <CardContent className="p-5">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <p className="font-display text-base font-bold leading-snug">{c.title}</p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {c.customer} · {c.owner} · updated <TimeAgo iso={c.updated_at ?? c.created_at} />
                    </p>
                  </div>
                  <div className="flex shrink-0 gap-1.5">
                    <span className={`${PILL} ${severityClass(c.severity)}`}>
                      {LABEL[c.severity] ?? c.severity}
                    </span>
                    <span className={`${PILL} ${stateClass(c.status)}`}>
                      {LABEL[c.status] ?? c.status}
                    </span>
                  </div>
                </div>

                <dl className="mt-4 space-y-3 text-sm">
                  <div>
                    <dt className="text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground">
                      Ranked root cause
                    </dt>
                    <dd className="mt-1 text-muted-foreground">{c.root_cause || "—"}</dd>
                  </div>
                  <div>
                    <dt className="text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground">
                      Recommended next step
                    </dt>
                    <dd className="mt-1 text-muted-foreground">{c.next_step || "—"}</dd>
                  </div>
                  {signal ? (
                    <div>
                      <dt className="text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground">
                        Raised by signal
                      </dt>
                      <dd className="mt-1 text-muted-foreground">{signal.summary}</dd>
                    </div>
                  ) : null}
                  {c.status === "resolved" ? (
                    <div>
                      <dt className="text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground">
                        Resolution
                      </dt>
                      <dd className="mt-1 text-muted-foreground">
                        {c.resolution || "Closed without notes"}
                        {c.resolved_at
                          ? ` · closed ${new Date(c.resolved_at).toLocaleDateString()}`
                          : ""}
                      </dd>
                    </div>
                  ) : null}
                </dl>

                <div className="mt-4 flex items-center gap-2">
                  <Button
                    size="sm"
                    variant={c.status === "resolved" ? "outline" : "default"}
                    onClick={() =>
                      setStatus.mutate({
                        c,
                        status: c.status === "resolved" ? "in_progress" : "resolved",
                      })
                    }
                    disabled={setStatus.isPending}
                  >
                    {c.status === "resolved" ? "Reopen" : "Mark resolved"}
                  </Button>
                  <Button variant="outline" size="sm" onClick={() => setDraft(toDraft(c))}>
                    Edit
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label="Delete case"
                    onClick={() => remove.mutate(c.id)}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          );
        })}
        {rows.length === 0 ? (
          <Card>
            <CardContent className="p-10 text-center text-sm text-muted-foreground">
              No cases in this view.
            </CardContent>
          </Card>
        ) : null}
      </div>

      <Dialog open={draft !== null} onOpenChange={(o) => (o ? null : setDraft(null))}>
        <DialogContent className="max-h-[85vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{draft?.id ? "Edit case" : "New case"}</DialogTitle>
          </DialogHeader>
          {draft ? (
            <div className="grid gap-4">
              <div className="grid gap-1.5">
                <Label>
                  Title <span className="text-destructive">*</span>
                </Label>
                <Input
                  value={draft.title}
                  onChange={(e) => setDraft({ ...draft, title: e.target.value })}
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="grid gap-1.5">
                  <Label>Owner</Label>
                  <Input
                    value={draft.owner}
                    onChange={(e) => setDraft({ ...draft, owner: e.target.value })}
                    placeholder="Unassigned"
                  />
                </div>
                <div className="grid gap-1.5">
                  <Label>Customer</Label>
                  <Input
                    value={draft.customer}
                    onChange={(e) => setDraft({ ...draft, customer: e.target.value })}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="grid gap-1.5">
                  <Label>Severity</Label>
                  <Select
                    value={draft.severity}
                    onValueChange={(v) => setDraft({ ...draft, severity: v })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {SEVERITIES.map((v) => (
                        <SelectItem key={v} value={v}>
                          {LABEL[v]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid gap-1.5">
                  <Label>Status</Label>
                  <Select
                    value={draft.status}
                    onValueChange={(v) => setDraft({ ...draft, status: v })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {CASE_STATUSES.map((v) => (
                        <SelectItem key={v} value={v}>
                          {LABEL[v]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="grid gap-1.5">
                <Label>Linked signal</Label>
                <Select
                  value={draft.signal_id ?? "none"}
                  onValueChange={(v) => setDraft({ ...draft, signal_id: v === "none" ? null : v })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">No linked signal</SelectItem>
                    {signals.map((s) => (
                      <SelectItem key={s.id} value={s.id}>
                        {s.summary}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid gap-1.5">
                <Label>Ranked root cause</Label>
                <Textarea
                  rows={3}
                  value={draft.root_cause}
                  onChange={(e) => setDraft({ ...draft, root_cause: e.target.value })}
                />
              </div>
              <div className="grid gap-1.5">
                <Label>Recommended next step</Label>
                <Textarea
                  rows={3}
                  value={draft.next_step}
                  onChange={(e) => setDraft({ ...draft, next_step: e.target.value })}
                />
              </div>
              <div className="grid gap-1.5">
                <Label>How it was resolved</Label>
                <Textarea
                  rows={3}
                  placeholder="What closed this case out"
                  value={draft.resolution}
                  onChange={(e) => setDraft({ ...draft, resolution: e.target.value })}
                />
              </div>
            </div>
          ) : null}
          <DialogFooter>
            <Button
              onClick={() => draft && save.mutate(draft)}
              disabled={!draft?.title.trim() || save.isPending}
            >
              Save case
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

```

## `src/components/metrics-panel.tsx`

```tsx
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { ShieldCheck, Sparkles, Users } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  LABEL,
  PILL,
  casesQuery,
  runbooksQuery,
  runsQuery,
  severityClass,
  signalsQuery,
  stateClass,
} from "@/lib/synapse";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

const SECTIONS = [
  { id: "performance", label: "Performance metrics" },
  { id: "forecast", label: "Impact forecast" },
] as const;

function countBy<T>(items: T[], key: (item: T) => string) {
  const map = new Map<string, number>();
  for (const item of items) {
    const k = key(item);
    map.set(k, (map.get(k) ?? 0) + 1);
  }
  return [...map.entries()].map(([name, value]) => ({ name, value }));
}

function Stat({ label, value, hint }: { label: string; value: string; hint: string }) {
  return (
    <Card>
      <CardContent className="p-5">
        <p className="text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
          {label}
        </p>
        <p className="mt-2 font-display text-3xl font-bold tracking-tight">{value}</p>
        <p className="mt-1 text-xs text-muted-foreground">{hint}</p>
      </CardContent>
    </Card>
  );
}

export function MetricsPanel() {
  const [section, setSection] = useState<(typeof SECTIONS)[number]["id"]>("performance");

  return (
    <>
      <div className="mb-6 flex flex-wrap gap-2 border-b border-border pb-3">
        {SECTIONS.map((s) => (
          <button
            key={s.id}
            type="button"
            onClick={() => setSection(s.id)}
            className={`rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
              section === s.id
                ? "bg-primary text-primary-foreground"
                : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
            }`}
          >
            {s.label}
          </button>
        ))}
      </div>

      {section === "performance" ? <PerformanceSection /> : <ForecastSection />}
    </>
  );
}

function PerformanceSection() {
  const { data: signals = [] } = useQuery(signalsQuery());
  const { data: cases = [] } = useQuery(casesQuery());
  const { data: runbooks = [] } = useQuery(runbooksQuery());
  const { data: runs = [] } = useQuery(runsQuery());

  const healed = signals.filter((s) => s.status === "resolved" || s.status === "healing").length;
  const selfHealRate = signals.length ? Math.round((healed / signals.length) * 100) : 0;
  const escalationRate = signals.length ? Math.round((cases.length / signals.length) * 100) : 0;
  const resolvedCases = cases.filter((c) => c.status === "resolved").length;
  const runSuccess = runs.length
    ? Math.round((runs.filter((r) => r.result === "succeeded").length / runs.length) * 100)
    : 0;

  const bySource = countBy(signals, (s) => s.source);
  const bySeverity = countBy(signals, (s) => LABEL[s.severity] ?? s.severity);
  const byReason = countBy(signals, (s) => s.reason_code);
  const byTier = countBy(runbooks, (r) => LABEL[r.tier] ?? r.tier);

  return (
    <>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Stat
          label="Handled by agents"
          value={`${selfHealRate}%`}
          hint="Signals resolved or self-healing"
        />
        <Stat label="Escalation rate" value={`${escalationRate}%`} hint="Signals that became cases" />
        <Stat
          label="Cases resolved"
          value={`${resolvedCases}/${cases.length}`}
          hint="Closed by an engineer"
        />
        <Stat label="Run success" value={`${runSuccess}%`} hint={`${runs.length} runbook runs`} />
      </div>

      <div className="mt-6 grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="font-display text-base">Signals by source</CardTitle>
          </CardHeader>
          <CardContent className="h-[300px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={bySource} layout="vertical" margin={{ left: 10, right: 20 }}>
                <CartesianGrid horizontal={false} stroke="var(--border)" />
                <XAxis type="number" allowDecimals={false} stroke="var(--muted-foreground)" fontSize={12} />
                <YAxis
                  type="category"
                  dataKey="name"
                  width={160}
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                />
                <Tooltip
                  contentStyle={{
                    background: "var(--popover)",
                    border: "1px solid var(--border)",
                    borderRadius: 12,
                    color: "var(--popover-foreground)",
                  }}
                />
                <Bar dataKey="value" fill="var(--chart-1)" radius={[0, 6, 6, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="font-display text-base">Severity mix</CardTitle>
          </CardHeader>
          <CardContent className="h-[300px]">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={bySeverity} dataKey="value" nameKey="name" innerRadius={60} outerRadius={95}>
                  {bySeverity.map((entry, i) => (
                    <Cell key={entry.name} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                  ))}
                </Pie>
                <Legend />
                <Tooltip
                  contentStyle={{
                    background: "var(--popover)",
                    border: "1px solid var(--border)",
                    borderRadius: 12,
                    color: "var(--popover-foreground)",
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="font-display text-base">Reason codes</CardTitle>
          </CardHeader>
          <CardContent className="h-[320px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={byReason} margin={{ left: 0, right: 10, bottom: 60 }}>
                <CartesianGrid vertical={false} stroke="var(--border)" />
                <XAxis
                  dataKey="name"
                  angle={-30}
                  textAnchor="end"
                  interval={0}
                  height={70}
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                />
                <YAxis allowDecimals={false} stroke="var(--muted-foreground)" fontSize={12} />
                <Tooltip
                  contentStyle={{
                    background: "var(--popover)",
                    border: "1px solid var(--border)",
                    borderRadius: 12,
                    color: "var(--popover-foreground)",
                  }}
                />
                <Bar dataKey="value" fill="var(--chart-2)" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="font-display text-base">Runbooks by autonomy tier</CardTitle>
          </CardHeader>
          <CardContent className="h-[320px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={byTier} margin={{ left: 0, right: 10, bottom: 40 }}>
                <CartesianGrid vertical={false} stroke="var(--border)" />
                <XAxis dataKey="name" stroke="var(--muted-foreground)" fontSize={11} />
                <YAxis allowDecimals={false} stroke="var(--muted-foreground)" fontSize={12} />
                <Tooltip
                  contentStyle={{
                    background: "var(--popover)",
                    border: "1px solid var(--border)",
                    borderRadius: 12,
                    color: "var(--popover-foreground)",
                  }}
                />
                <Bar dataKey="value" fill="var(--chart-3)" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>
    </>
  );
}

const SEVERITY_WEIGHT: Record<string, number> = {
  low: 1,
  medium: 2.5,
  high: 5,
  critical: 9,
};

function words(text: string) {
  return text
    .toLowerCase()
    .split(/[^a-z0-9]+/)
    .filter((w) => w.length > 3);
}

function ForecastSection() {
  const { data: signals = [] } = useQuery(signalsQuery());
  const { data: cases = [] } = useQuery(casesQuery());
  const { data: runbooks = [] } = useQuery(runbooksQuery());
  const { data: runs = [] } = useQuery(runsQuery());

  const [selectedId, setSelectedId] = useState(signals[0]?.id ?? "");
  const signal = signals.find((s) => s.id === selectedId) ?? signals[0];

  if (!signal) {
    return (
      <Card>
        <CardContent className="p-10 text-center text-sm text-muted-foreground">
          Add a signal in the inbox first — the forecast is built from live signal data.
        </CardContent>
      </Card>
    );
  }

  const cohort = signals.filter(
    (s) => s.reason_code === signal.reason_code || s.source === signal.source,
  );
  const cohortIds = new Set(cohort.map((s) => s.id));
  const linkedCases = cases.filter((c) => c.signal_id && cohortIds.has(c.signal_id));

  const keys = new Set([...words(signal.summary), ...words(signal.reason_code)]);
  const matching = runbooks.filter((r) => {
    const sig = words(`${r.trigger_signature} ${r.name} ${r.description}`);
    return sig.some((w) => keys.has(w));
  });
  const liveMatching = matching.filter((r) => r.enabled);
  const bestRate = liveMatching.length ? Math.max(...liveMatching.map((r) => r.success_rate)) : 0;

  const cohortRuns = runs.filter((r) => r.signal_id && cohortIds.has(r.signal_id));
  const succeeded = cohortRuns.filter((r) => r.result === "succeeded").length;
  const observedRate = cohortRuns.length ? Math.round((succeeded / cohortRuns.length) * 100) : 0;
  const automationRate = Math.max(bestRate, observedRate);

  const weight = SEVERITY_WEIGHT[signal.severity] ?? 2.5;
  const observedPerSignal = cohort.length ? linkedCases.length / cohort.length : 0;
  const casesPerSignal = Math.max(observedPerSignal, 1) * weight;
  const impacted = Math.round(cohort.length * casesPerSignal);
  const automated = Math.round((impacted * automationRate) / 100);
  const humanCases = impacted - automated;

  const preventionRate = liveMatching.length ? Math.min(95, automationRate + 10) : 0;
  const quarterly = Math.max(1, Math.round(impacted / 2));
  const forecast = ["Next quarter", "+2 quarters", "+3 quarters", "+4 quarters"].map(
    (label, i) => {
      const growth = 1 + i * 0.15;
      const base = Math.round(quarterly * growth);
      const prevented = Math.round((base * preventionRate) / 100);
      return { label, base, prevented, remaining: base - prevented };
    },
  );
  const preventedTotal = forecast.reduce((sum, f) => sum + f.prevented, 0);

  return (
    <>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-muted-foreground">
          Pick a signal and CSP AI Capabilities projects the case volume it drives, how much of that it can
          automate today, and how many future cases a live runbook prevents.
        </p>
        <Select value={signal.id} onValueChange={setSelectedId}>
          <SelectTrigger className="w-[360px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {signals.map((s) => (
              <SelectItem key={s.id} value={s.id}>
                {s.summary}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Card>
        <CardContent className="flex flex-wrap items-center gap-3 p-5">
          <span className={`${PILL} ${severityClass(signal.severity)}`}>
            {LABEL[signal.severity] ?? signal.severity}
          </span>
          <span className={`${PILL} ${stateClass(signal.status)}`}>
            {LABEL[signal.status] ?? signal.status}
          </span>
          <p className="text-sm font-semibold">{signal.summary}</p>
          <p className="text-sm text-muted-foreground">
            {signal.source} · {signal.customer} · {signal.reason_code}
          </p>
        </CardContent>
      </Card>

      <div className="mt-6 grid gap-4 md:grid-cols-3">
        <ImpactStat
          icon={<Users className="size-4" />}
          label="Cases impacted"
          value={impacted}
          note={`${cohort.length} similar signal(s), ${linkedCases.length} case(s) already linked`}
        />
        <ImpactStat
          icon={<Sparkles className="size-4" />}
          label="Cases automated"
          value={automated}
          note={`${automationRate}% handled by ${liveMatching.length} live runbook(s) — ${humanCases} still reach a human`}
        />
        <ImpactStat
          icon={<ShieldCheck className="size-4" />}
          label="Cases prevented (next 4 qtrs)"
          value={preventedTotal}
          note={
            preventionRate
              ? `${preventionRate}% of recurrence removed once the runbook stays live`
              : "No live runbook matches this signal yet"
          }
        />
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-[1.4fr_1fr]">
        <Card>
          <CardHeader>
            <CardTitle>Future case volume, with and without healing</CardTitle>
          </CardHeader>
          <CardContent className="h-[320px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={forecast}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                <XAxis dataKey="label" tick={{ fontSize: 12 }} stroke="var(--muted-foreground)" />
                <YAxis tick={{ fontSize: 12 }} stroke="var(--muted-foreground)" allowDecimals={false} />
                <Tooltip
                  contentStyle={{
                    background: "var(--card)",
                    border: "1px solid var(--border)",
                    borderRadius: 12,
                  }}
                />
                <Bar dataKey="prevented" name="Prevented" stackId="a" fill="var(--chart-1)" radius={[0, 0, 4, 4]} />
                <Bar dataKey="remaining" name="Still expected" stackId="a" fill="var(--chart-4)" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Runbooks that cover this signal</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {matching.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                No runbook matches this signal yet. Authoring one is what turns the impacted
                volume above into prevented volume.
              </p>
            ) : (
              matching.map((r) => (
                <div
                  key={r.id}
                  className="flex items-start justify-between gap-3 rounded-lg border border-border p-3"
                >
                  <div className="min-w-0">
                    <p className="text-sm font-semibold">{r.name}</p>
                    <p className="mt-0.5 text-xs text-muted-foreground">
                      {LABEL[r.tier] ?? r.tier} · {r.success_rate}% success
                    </p>
                  </div>
                  <span
                    className={`${PILL} ${
                      r.enabled
                        ? "bg-status-delivered text-status-delivered-foreground"
                        : "bg-status-planned text-status-planned-foreground"
                    }`}
                  >
                    {r.enabled ? "Live" : "Shadow"}
                  </span>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Cases already tied to this signal type</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          {linkedCases.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No cases recorded for this signal type yet — the forecast is based on severity and
              signal volume alone.
            </p>
          ) : (
            linkedCases.slice(0, 8).map((c) => (
              <div
                key={c.id}
                className="flex items-center justify-between gap-3 border-b border-border pb-2 last:border-0"
              >
                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold">{c.title}</p>
                  <p className="text-xs text-muted-foreground">
                    {c.customer} · {c.owner}
                  </p>
                </div>
                <span className={`${PILL} ${stateClass(c.status)}`}>
                  {LABEL[c.status] ?? c.status}
                </span>
              </div>
            ))
          )}
        </CardContent>
      </Card>
    </>
  );
}

function ImpactStat({
  icon,
  label,
  value,
  note,
}: {
  icon: React.ReactNode;
  label: string;
  value: number;
  note: string;
}) {
  return (
    <Card>
      <CardContent className="p-5">
        <div className="flex items-center gap-2 text-muted-foreground">
          {icon}
          <p className="text-[11px] font-bold uppercase tracking-[0.14em]">{label}</p>
        </div>
        <p className="mt-2 font-display text-4xl font-bold tracking-tight">{value}</p>
        <p className="mt-1 text-xs text-muted-foreground">{note}</p>
      </CardContent>
    </Card>
  );
}

```

## `src/components/runbooks-panel.tsx`

```tsx
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Play, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { TimeAgo } from "@/components/time-ago";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { supabase } from "@/integrations/supabase/client";
import { LABEL, PILL, TIERS, runbooksQuery, runsQuery, stateClass } from "@/lib/synapse";

type Draft = {
  id?: string;
  name: string;
  tier: string;
  trigger_signature: string;
  description: string;
  enabled: boolean;
  success_rate: number;
};

const EMPTY: Draft = {
  name: "",
  tier: "tier1",
  trigger_signature: "",
  description: "",
  enabled: false,
  success_rate: 0,
};

export function RunbooksPanel() {
  const { data: runbooks = [], isLoading } = useQuery(runbooksQuery());
  const { data: runs = [] } = useQuery(runsQuery());
  const qc = useQueryClient();
  const [draft, setDraft] = useState<Draft | null>(null);

  const invalidate = () => qc.invalidateQueries({ queryKey: ["synapse"] });

  const save = useMutation({
    mutationFn: async (d: Draft) => {
      const payload = {
        name: d.name.trim(),
        tier: d.tier,
        trigger_signature: d.trigger_signature,
        description: d.description,
        enabled: d.enabled,
        success_rate: Number(d.success_rate) || 0,
      };
      const { error } = d.id
        ? await supabase.from("synapse_runbooks").update(payload).eq("id", d.id)
        : await supabase.from("synapse_runbooks").insert(payload);
      if (error) throw error;
    },
    onSuccess: () => {
      setDraft(null);
      invalidate();
      toast.success("Runbook saved");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggle = useMutation({
    mutationFn: async ({ id, enabled }: { id: string; enabled: boolean }) => {
      const { error } = await supabase.from("synapse_runbooks").update({ enabled }).eq("id", id);
      if (error) throw error;
    },
    onSuccess: invalidate,
    onError: (e: Error) => toast.error(e.message),
  });

  const runNow = useMutation({
    mutationFn: async (runbookId: string) => {
      const { error } = await supabase.from("synapse_runs").insert({
        runbook_id: runbookId,
        result: "succeeded",
        notes: "Manual run recorded from the runbook library.",
      });
      if (error) throw error;
    },
    onSuccess: () => {
      invalidate();
      toast.success("Run recorded");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: async (id: string) => {
      const { error } = await supabase.from("synapse_runbooks").delete().eq("id", id);
      if (error) throw error;
    },
    onSuccess: () => {
      invalidate();
      toast.success("Runbook removed");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const nameOf = (id: string) => runbooks.find((r) => r.id === id)?.name ?? "Unknown runbook";

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <p className="max-w-3xl text-sm text-muted-foreground">
          Every healing action, its autonomy tier and whether it is allowed to run without a human.
        </p>
        <Button onClick={() => setDraft({ ...EMPTY })}>
          <Plus className="size-4" /> New runbook
        </Button>
      </div>

      <div className="grid gap-4 lg:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
        <div className="space-y-3">
          {isLoading ? (
            <p className="text-sm text-muted-foreground">Loading runbooks…</p>
          ) : runbooks.length === 0 ? (
            <p className="text-sm text-muted-foreground">No runbooks yet.</p>
          ) : (
            runbooks.map((r) => (
              <Card key={r.id}>
                <CardContent className="p-5">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="font-display text-base font-bold">{r.name}</p>
                        <span className={`${PILL} bg-accent text-accent-foreground`}>
                          {LABEL[r.tier] ?? r.tier}
                        </span>
                        {r.enabled ? (
                          <span className={`${PILL} ${stateClass("resolved")}`}>Live</span>
                        ) : (
                          <span className={`${PILL} ${stateClass("new")}`}>Shadow</span>
                        )}
                      </div>
                      <p className="mt-2 text-sm text-muted-foreground">{r.description}</p>
                      <p className="mt-2 font-mono text-xs text-muted-foreground">
                        trigger: {r.trigger_signature || "—"}
                      </p>
                    </div>
                    <div className="flex items-center gap-3">
                      <div className="text-right">
                        <p className="font-display text-xl font-bold">{r.success_rate}%</p>
                        <p className="text-[11px] uppercase tracking-[0.12em] text-muted-foreground">
                          Success
                        </p>
                      </div>
                      <Switch
                        checked={r.enabled}
                        aria-label="Toggle autonomous execution"
                        onCheckedChange={(v) => toggle.mutate({ id: r.id, enabled: v })}
                      />
                    </div>
                  </div>
                  <div className="mt-4 flex items-center gap-2">
                    <Button variant="outline" size="sm" onClick={() => runNow.mutate(r.id)}>
                      <Play className="size-3.5" /> Record run
                    </Button>
                    <Button variant="outline" size="sm" onClick={() => setDraft({ ...r })}>
                      Edit
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      aria-label="Delete runbook"
                      onClick={() => remove.mutate(r.id)}
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>

        <Card className="lg:sticky lg:top-6 lg:self-start">
          <CardHeader>
            <CardTitle className="font-display text-base">Run history</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {runs.length === 0 ? (
              <p className="text-sm text-muted-foreground">No runs recorded yet.</p>
            ) : (
              runs.slice(0, 15).map((run) => (
                <div key={run.id} className="border-b border-border pb-3 last:border-0 last:pb-0">
                  <div className="flex items-center justify-between gap-2">
                    <p className="truncate text-sm font-semibold">{nameOf(run.runbook_id)}</p>
                    <span className={`${PILL} ${stateClass(run.result)}`}>
                      {LABEL[run.result] ?? run.result}
                    </span>
                  </div>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {run.notes} · <TimeAgo iso={run.ran_at} />
                  </p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      </div>

      <Dialog open={draft !== null} onOpenChange={(o) => (o ? null : setDraft(null))}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{draft?.id ? "Edit runbook" : "New runbook"}</DialogTitle>
          </DialogHeader>
          {draft ? (
            <div className="grid gap-4">
              <div className="grid gap-1.5">
                <Label>
                  Name <span className="text-destructive">*</span>
                </Label>
                <Input
                  value={draft.name}
                  onChange={(e) => setDraft({ ...draft, name: e.target.value })}
                />
              </div>
              <div className="grid gap-1.5">
                <Label>Autonomy tier</Label>
                <Select value={draft.tier} onValueChange={(v) => setDraft({ ...draft, tier: v })}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {TIERS.map((t) => (
                      <SelectItem key={t} value={t}>
                        {LABEL[t]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid gap-1.5">
                <Label>Trigger signature</Label>
                <Input
                  value={draft.trigger_signature}
                  onChange={(e) => setDraft({ ...draft, trigger_signature: e.target.value })}
                  placeholder="queue.depth > threshold for 15m"
                />
              </div>
              <div className="grid gap-1.5">
                <Label>What it does</Label>
                <Textarea
                  rows={3}
                  value={draft.description}
                  onChange={(e) => setDraft({ ...draft, description: e.target.value })}
                />
              </div>
              <div className="grid grid-cols-2 items-end gap-3">
                <div className="grid gap-1.5">
                  <Label>Success rate (%)</Label>
                  <Input
                    type="number"
                    min={0}
                    max={100}
                    value={draft.success_rate}
                    onChange={(e) =>
                      setDraft({ ...draft, success_rate: Number(e.target.value) || 0 })
                    }
                  />
                </div>
                <div className="flex items-center gap-3 pb-2">
                  <Switch
                    checked={draft.enabled}
                    onCheckedChange={(v) => setDraft({ ...draft, enabled: v })}
                  />
                  <span className="text-sm font-semibold">Allowed to run</span>
                </div>
              </div>
            </div>
          ) : null}
          <DialogFooter>
            <Button
              onClick={() => draft && save.mutate(draft)}
              disabled={!draft?.name.trim() || save.isPending}
            >
              Save runbook
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

```

## `src/components/synapse-shell.tsx`

```tsx
import { Link } from "@tanstack/react-router";
import {
  Activity,
  FileText,
  Inbox,
  Sparkles,
  LayoutDashboard,
  Network,
  Share2,
  Star,
  TrendingUp,
} from "lucide-react";
import type { ReactNode } from "react";

const NAV = [
  { to: "/synapse", label: "Overview", icon: LayoutDashboard, exact: true },
  {
    to: "/synapse/roadmap",
    label: "AI Capabilities",
    icon: Star,
    exact: false,
    iconClass: "text-destructive fill-destructive",
  },
  { to: "/synapse/csp-agentic-ai", label: "CSP Agentic AI", icon: Sparkles, exact: false },
  { to: "/synapse/capability-impact", label: "Capability impact", icon: TrendingUp, exact: false },
  { to: "/synapse/architecture", label: "Architecture", icon: Network, exact: false },
  { to: "/synapse/context-graph", label: "Capabilities map", icon: Share2, exact: false },
  { to: "/synapse/signals", label: "Signal inbox", icon: Inbox, exact: false },
  {
    to: "/synapse/classifier",
    label: "Capability Automation Classifier",
    icon: LayoutDashboard,
    exact: false,
  },
  { to: "/synapse/about", label: "About CSP AI Capabilities", icon: Activity, exact: false },
] as const;


export function SynapseShell({
  title,
  subtitle,
  actions,
  children,
}: {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className="min-h-screen bg-background">
      <header className="border-b border-border bg-card/70 backdrop-blur">
        <div className="mx-auto flex max-w-[1500px] flex-col gap-4 px-6 py-5 sm:px-10 lg:flex-row lg:items-center lg:justify-between">
          <div className="flex items-center gap-3">
            <span className="grid size-10 place-items-center rounded-xl bg-primary text-primary-foreground shadow-soft">
              <Activity className="size-5" />
            </span>
            <div>
              <p className="font-display text-lg font-bold leading-none tracking-tight">CSP AI Capabilities</p>
              <p className="mt-1 text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">
                Watch · Diagnose · Heal
              </p>
            </div>
          </div>
          <nav className="flex flex-wrap items-center gap-1">
            {NAV.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                activeOptions={{ exact: item.exact }}
                className="inline-flex items-center gap-2 rounded-full px-3.5 py-2 text-sm font-semibold text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
                activeProps={{ className: "bg-accent text-accent-foreground" }}
              >
                <item.icon className={`size-4 ${"iconClass" in item ? item.iconClass : ""}`} />
                {item.label}
              </Link>
            ))}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-[1500px] px-6 py-8 sm:px-10">
        <div className="mb-6 flex flex-wrap items-end justify-between gap-4 border-b border-border pb-5">
          <div>
            <h1 className="font-display text-3xl font-bold tracking-tight sm:text-4xl">{title}</h1>
            {subtitle ? (
              <p className="mt-2 max-w-3xl text-sm text-muted-foreground">{subtitle}</p>
            ) : null}
          </div>
          {actions}
        </div>
        {children}
      </main>
    </div>
  );
}

```

## `src/components/time-ago.tsx`

```tsx
import { useEffect, useState } from "react";
import { timeAgo } from "@/lib/synapse";

/**
 * Renders a relative timestamp. Server render and first client render emit the
 * same placeholder so hydration never mismatches; the real value appears after mount.
 */
export function TimeAgo({ iso }: { iso?: string | null }) {
  const [label, setLabel] = useState("—");

  useEffect(() => {
    setLabel(timeAgo(iso));
    const t = setInterval(() => setLabel(timeAgo(iso)), 60_000);
    return () => clearInterval(t);
  }, [iso]);

  return <>{label}</>;
}

```

## `src/hooks/use-mobile.tsx`

```tsx
import * as React from "react";

const MOBILE_BREAKPOINT = 768;

export function useIsMobile() {
  const [isMobile, setIsMobile] = React.useState<boolean | undefined>(undefined);

  React.useEffect(() => {
    const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`);
    const onChange = () => {
      setIsMobile(window.innerWidth < MOBILE_BREAKPOINT);
    };
    mql.addEventListener("change", onChange);
    setIsMobile(window.innerWidth < MOBILE_BREAKPOINT);
    return () => mql.removeEventListener("change", onChange);
  }, []);

  return !!isMobile;
}

```

## `src/integrations/supabase/auth-attacher.ts`

```ts
// This file is automatically generated. Do not edit it directly.
import { createMiddleware } from '@tanstack/react-start'
import { supabase } from './client'

// Must be registered as a global `functionMiddleware` in `src/start.ts`; otherwise
// the browser never attaches the bearer token to serverFn RPCs.
export const attachSupabaseAuth = createMiddleware({ type: 'function' }).client(
  async ({ next }) => {
    const { data } = await supabase.auth.getSession()
    const token = data.session?.access_token
    return next({
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    })
  },
)

```

## `src/integrations/supabase/auth-middleware.ts`

```ts
// This file is automatically generated. Do not edit it directly.
import { createMiddleware } from '@tanstack/react-start'
import { getRequest } from '@tanstack/react-start/server'
import { createClient } from '@supabase/supabase-js'
import type { Database } from './types'




function isNewSupabaseApiKey(value: string): boolean {
  return value.startsWith('sb_publishable_') || value.startsWith('sb_secret_');
}

function createSupabaseFetch(supabaseKey: string): typeof fetch {
  return (input, init) => {
    const headers = new Headers(
      typeof Request !== 'undefined' && input instanceof Request ? input.headers : undefined,
    );

    if (init?.headers) {
      new Headers(init.headers).forEach((value, key) => headers.set(key, value));
    }

    // New Supabase API keys are opaque strings, not bearer JWTs.
    if (isNewSupabaseApiKey(supabaseKey) && headers.get('Authorization') === `Bearer ${supabaseKey}`) {
      headers.delete('Authorization');
    }

    headers.set('apikey', supabaseKey);
    return fetch(input, { ...init, headers });
  };
}

export const requireSupabaseAuth = createMiddleware({ type: 'function' }).server(
  async ({ next }) => {
    
    const SUPABASE_URL = process.env['SUPABASE_URL'];
    const SUPABASE_PUBLISHABLE_KEY = process.env['SUPABASE_PUBLISHABLE_KEY'];

    if (!SUPABASE_URL || !SUPABASE_PUBLISHABLE_KEY) {
      const missing = [
        ...(!SUPABASE_URL ? ['SUPABASE_URL'] : []),
        ...(!SUPABASE_PUBLISHABLE_KEY ? ['SUPABASE_PUBLISHABLE_KEY'] : []),
      ];
      const message = `Missing Supabase environment variable(s): ${missing.join(', ')}. Connect Supabase in Lovable Cloud.`;
      console.error(`[Supabase] ${message}`);
      throw new Error(message);
    }
    
    const request = getRequest();

    if (!request?.headers) {
      throw new Error('Unauthorized: No request headers available');
    }


    const authHeader = request.headers.get('authorization');

    if (!authHeader) {
      throw new Error('Unauthorized: No authorization header provided');
    }

    if (!authHeader.startsWith('Bearer ')) {
      throw new Error('Unauthorized: Only Bearer tokens are supported');
    }

    const token = authHeader.replace('Bearer ', '');
    if (!token) {
      throw new Error('Unauthorized: No token provided');
    }

    if (token.split('.').length !== 3) {
      throw new Error('Unauthorized: Invalid token');
    }

    const supabase = createClient<Database>(
      SUPABASE_URL!,
      SUPABASE_PUBLISHABLE_KEY!,
      {
        global: {
          fetch: createSupabaseFetch(SUPABASE_PUBLISHABLE_KEY!),
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
        auth: {
          storage: undefined,
          persistSession: false,
          autoRefreshToken: false,
        },
      }
    );

    const { data, error } = await supabase.auth.getClaims(token);
    if (error || !data?.claims) {
      throw new Error('Unauthorized: Invalid token');
    }

    if (!data.claims.sub) {
      throw new Error('Unauthorized: No user ID found in token');
    }

    return next({
      context: {
        supabase,
        userId: data.claims.sub,
        claims: data.claims,
      },
    });
  },
);

```

## `src/integrations/supabase/client.server.ts`

```ts
// This file is automatically generated. Do not edit it directly.
// Server-side Supabase client with service role key - bypasses RLS.
// Use this for admin operations in server functions and server routes only.
// For user-authenticated queries (with RLS), use the auth middleware instead.
import { createClient } from '@supabase/supabase-js';
import type { Database } from './types';

function isNewSupabaseApiKey(value: string): boolean {
  return value.startsWith('sb_publishable_') || value.startsWith('sb_secret_');
}

function createSupabaseFetch(supabaseKey: string): typeof fetch {
  return (input, init) => {
    const headers = new Headers(
      typeof Request !== 'undefined' && input instanceof Request ? input.headers : undefined,
    );

    if (init?.headers) {
      new Headers(init.headers).forEach((value, key) => headers.set(key, value));
    }

    // New Supabase API keys are opaque strings, not bearer JWTs.
    if (isNewSupabaseApiKey(supabaseKey) && headers.get('Authorization') === `Bearer ${supabaseKey}`) {
      headers.delete('Authorization');
    }

    headers.set('apikey', supabaseKey);
    return fetch(input, { ...init, headers });
  };
}

function createSupabaseAdminClient() {
  const SUPABASE_URL = process.env['SUPABASE_URL'];
  const SUPABASE_SERVICE_ROLE_KEY = process.env['SUPABASE_SERVICE_ROLE_KEY'];

  if (!SUPABASE_URL || !SUPABASE_SERVICE_ROLE_KEY) {
    const missing = [
      ...(!SUPABASE_URL ? ['SUPABASE_URL'] : []),
      ...(!SUPABASE_SERVICE_ROLE_KEY ? ['SUPABASE_SERVICE_ROLE_KEY'] : []),
    ];
    const message = `Missing Supabase environment variable(s): ${missing.join(', ')}. Connect Supabase in Lovable Cloud.`;
    console.error(`[Supabase] ${message}`);
    throw new Error(message);
  }

  return createClient<Database>(SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY, {
    global: {
      fetch: createSupabaseFetch(SUPABASE_SERVICE_ROLE_KEY),
    },
    auth: {
      storage: undefined,
      persistSession: false,
      autoRefreshToken: false,
    }
  });
}

let _supabaseAdmin: ReturnType<typeof createSupabaseAdminClient> | undefined;

// Server-side Supabase client with service role - bypasses RLS
// SECURITY: Only use this for trusted server-side operations, never expose to client code
// Load inside server handlers: const { supabaseAdmin } = await import("@/integrations/supabase/client.server");
// Top-level import is safe only in other .server.ts modules - route files and *.functions.ts ship to the client bundle.
export const supabaseAdmin = new Proxy({} as ReturnType<typeof createSupabaseAdminClient>, {
  get(_, prop, receiver) {
    if (!_supabaseAdmin) _supabaseAdmin = createSupabaseAdminClient();
    return Reflect.get(_supabaseAdmin, prop, receiver);
  },
});

```

## `src/integrations/supabase/client.ts`

```ts
// This file is automatically generated. Do not edit it directly.
import { createClient } from '@supabase/supabase-js';
import type { Database } from './types';
import { brokeredPreviewStorage } from './previewAuthStorage';

function isNewSupabaseApiKey(value: string): boolean {
  return value.startsWith('sb_publishable_') || value.startsWith('sb_secret_');
}

function createSupabaseFetch(supabaseKey: string): typeof fetch {
  return (input, init) => {
    const headers = new Headers(
      typeof Request !== 'undefined' && input instanceof Request ? input.headers : undefined,
    );

    if (init?.headers) {
      new Headers(init.headers).forEach((value, key) => headers.set(key, value));
    }

    // New Supabase API keys are opaque strings, not bearer JWTs.
    if (isNewSupabaseApiKey(supabaseKey) && headers.get('Authorization') === `Bearer ${supabaseKey}`) {
      headers.delete('Authorization');
    }

    headers.set('apikey', supabaseKey);
    return fetch(input, { ...init, headers });
  };
}


function createSupabaseClient() {
  // Use import.meta.env for client-side (Vite build-time replacement)
  // Fall back to process.env for SSR (server-side rendering)
  const SUPABASE_URL = import.meta.env['VITE_SUPABASE_URL'] || process.env['SUPABASE_URL'];
  const SUPABASE_PUBLISHABLE_KEY = import.meta.env['VITE_SUPABASE_PUBLISHABLE_KEY'] || process.env['SUPABASE_PUBLISHABLE_KEY'];

  if (!SUPABASE_URL || !SUPABASE_PUBLISHABLE_KEY) {
    const missing = [
      ...(!SUPABASE_URL ? ['SUPABASE_URL'] : []),
      ...(!SUPABASE_PUBLISHABLE_KEY ? ['SUPABASE_PUBLISHABLE_KEY'] : []),
    ];
    const message = `Missing Supabase environment variable(s): ${missing.join(', ')}. Connect Supabase in Lovable Cloud.`;
    console.error(`[Supabase] ${message}`);
    throw new Error(message);
  }

  return createClient<Database>(SUPABASE_URL, SUPABASE_PUBLISHABLE_KEY, {
    global: {
      fetch: createSupabaseFetch(SUPABASE_PUBLISHABLE_KEY),
    },
    auth: {
      storage: brokeredPreviewStorage(),
      persistSession: true,
      autoRefreshToken: true,
    },
  });
}

let _supabase: ReturnType<typeof createSupabaseClient> | undefined;

// Import the supabase client like this:
// import { supabase } from "@/integrations/supabase/client";
export const supabase = new Proxy({} as ReturnType<typeof createSupabaseClient>, {
  get(_, prop, receiver) {
    if (!_supabase) _supabase = createSupabaseClient();
    return Reflect.get(_supabase, prop, receiver);
  },
});


```

## `src/integrations/supabase/cron-auth.ts`

```ts
// This file is automatically generated. Do not edit it directly.
export async function authenticateCronRequest(
  request: Request,
): Promise<Response | null> {
  const currentSecret = process.env['LOVABLE_CRON_SECRET']
  const previousSecret = process.env['LOVABLE_CRON_SECRET_PREVIOUS']

  if (!currentSecret) {
    return new Response('Server configuration error', { status: 500 })
  }

  const match = /^Bearer ([^\s,]+)$/.exec(
    request.headers.get('authorization') ?? '',
  )
  const token = match?.[1]
  if (!token) {
    return new Response('Unauthorized', { status: 401 })
  }

  const { createHash, timingSafeEqual } = await import('node:crypto')
  const digest = (value: string) =>
    createHash('sha256').update(value, 'utf8').digest()
  const providedDigest = digest(token)
  const currentMatches = timingSafeEqual(providedDigest, digest(currentSecret))
  const previousMatches = timingSafeEqual(
    providedDigest,
    digest(previousSecret ?? currentSecret),
  )

  if (!currentMatches && !previousMatches) {
    return new Response('Unauthorized', { status: 401 })
  }

  return null
}

```

## `src/integrations/supabase/previewAuthStorage.ts`

```ts
// This file is automatically generated. Do not edit it directly.

// On a Lovable preview surface, broker the auth session to the editor over
// postMessage so the project's preview surfaces share one login; else localStorage.
export function brokeredPreviewStorage() {
  if (typeof window === 'undefined') return undefined;
  const host = location.hostname;
  const PREVIEW_ZONES = ['lovableproject.com', 'lovableproject-dev.com', 'lovable.app', 'gpt-eng.com', 'gptengineer.run'];
  const onPreviewZone = PREVIEW_ZONES.some((z) => host === z || host.endsWith('.' + z));
  // Read the id only from non-user-controlled host positions, so a user-named
  // preview--<name> host can't smuggle another project's id.
  const UUID = '[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}';
  const projectId = onPreviewZone
    ? (host.match(new RegExp('^(?:id-preview(?:-[a-z0-9]+)?|project)--(' + UUID + ')(?:-dev)?(?=\\.|$)', 'i'))?.[1]
        ?? host.match(new RegExp('^(' + UUID + ')(?=[.-])', 'i'))?.[1])
    : undefined;
  const framed = window.parent && window.parent !== window;
  if (!projectId || !framed) return localStorage;

  // Post only to the real editor ancestor, validated as a Lovable origin, so the
  // session token can never reach an untrusted embedder.
  const dev = host.endsWith('.lovableproject-dev.com') || host.endsWith('.gpt-eng.com');
  const EDITOR = dev
    ? /^https:\/\/([a-z0-9-]+\.)*(lovable\.dev|gptengineer\.app)$|^http:\/\/localhost:3000$/
    : /^https:\/\/([a-z0-9-]+\.)*(lovable\.dev|gptengineer\.app)$/;
  const ancestor = (location.ancestorOrigins && location.ancestorOrigins[0]) || (document.referrer ? new URL(document.referrer).origin : '');
  const editorOrigins = ancestor && EDITOR.test(ancestor)
    ? [ancestor]
    : (dev ? ['https://lovable.dev', 'http://localhost:3000'] : ['https://lovable.dev']);
  const RESULT = 'lovable-preview-auth:result';
  const TIMEOUT = 2000;
  const newId = () => Math.random().toString(36).slice(2) + Date.now().toString(36);

  const request = (type: string, key: string, value?: string): Promise<{ ok: boolean; value?: string | null } | null> =>
    new Promise((resolve) => {
      const requestId = newId();
      let done = false;
      let timer: ReturnType<typeof setTimeout>;
      const finish = (r: { ok: boolean; value?: string | null } | null) => {
        if (done) return;
        done = true;
        clearTimeout(timer);
        window.removeEventListener('message', onMessage);
        resolve(r);
      };
      const onMessage = (e: MessageEvent) => {
        if (editorOrigins.indexOf(e.origin) < 0) return;
        const d = e.data;
        if (d && d.type === RESULT && d.requestId === requestId) finish(d);
      };
      window.addEventListener('message', onMessage);
      const msg: Record<string, unknown> = { type, requestId, projectId, key };
      if (value !== undefined) msg['value'] = value;
      // targetOrigin per trusted editor origin, so a session token never reaches an arbitrary embedder.
      for (const origin of editorOrigins) window.parent.postMessage(msg, origin);
      timer = setTimeout(() => finish(null), TIMEOUT);
    });

  // The editor may not be listening yet at the first getItem, so retry once.
  let firstGet = true;
  const RETRY_DELAY = 250;

  return {
    getItem: async (key: string) => {
      let res = await request('lovable-preview-auth:get', key);
      if (!res && firstGet) {
        await new Promise((r) => setTimeout(r, RETRY_DELAY));
        res = await request('lovable-preview-auth:get', key);
      }
      firstGet = false;
      // '' is the logout tombstone: clear the local copy too so it can't resurrect if
      // the broker later goes silent. A null reply means never-synced -> keep local.
      if (res && res.ok && typeof res.value === 'string') {
        if (res.value === '') { localStorage.removeItem(key); return null; }
        return res.value;
      }
      return localStorage.getItem(key);
    },
    setItem: (key: string, value: string) => {
      localStorage.setItem(key, value);
      return request('lovable-preview-auth:set', key, value).then(() => undefined);
    },
    removeItem: (key: string) => {
      localStorage.removeItem(key);
      return request('lovable-preview-auth:remove', key).then(() => undefined);
    },
  };
}

```

## `src/integrations/supabase/types.ts`

```ts
export type Json =
  | string
  | number
  | boolean
  | null
  | { [key: string]: Json | undefined }
  | Json[]

export type Database = {
  // Allows to automatically instantiate createClient with right options
  // instead of createClient<Database, { PostgrestVersion: 'XX' }>(URL, KEY)
  __InternalSupabase: {
    PostgrestVersion: "14.5"
  }
  public: {
    Tables: {
      npi_projects: {
        Row: {
          created_at: string
          ga_quarter: string
          id: string
          impactful: boolean
          is_standard: boolean
          name: string
          notes: string | null
          owner: string
          product_line_id: string
          resources: number
          start_quarter: string
          status: string
          updated_at: string
        }
        Insert: {
          created_at?: string
          ga_quarter?: string
          id?: string
          impactful?: boolean
          is_standard?: boolean
          name: string
          notes?: string | null
          owner: string
          product_line_id: string
          resources?: number
          start_quarter?: string
          status?: string
          updated_at?: string
        }
        Update: {
          created_at?: string
          ga_quarter?: string
          id?: string
          impactful?: boolean
          is_standard?: boolean
          name?: string
          notes?: string | null
          owner?: string
          product_line_id?: string
          resources?: number
          start_quarter?: string
          status?: string
          updated_at?: string
        }
        Relationships: [
          {
            foreignKeyName: "npi_projects_product_line_id_fkey"
            columns: ["product_line_id"]
            isOneToOne: false
            referencedRelation: "product_lines"
            referencedColumns: ["id"]
          },
        ]
      }
      pendo_settings: {
        Row: {
          api_key: string
          auto_create: boolean
          created_at: string
          default_severity: string
          dropoff_pct_threshold: number
          error_rate_threshold: number
          id: string
          last_synced_at: string | null
          quiet_window_minutes: number
          rage_click_threshold: number
          region: string
          time_on_task_minutes: number
          updated_at: string
          webhook_secret: string
          zero_result_threshold: number
        }
        Insert: {
          api_key?: string
          auto_create?: boolean
          created_at?: string
          default_severity?: string
          dropoff_pct_threshold?: number
          error_rate_threshold?: number
          id?: string
          last_synced_at?: string | null
          quiet_window_minutes?: number
          rage_click_threshold?: number
          region?: string
          time_on_task_minutes?: number
          updated_at?: string
          webhook_secret?: string
          zero_result_threshold?: number
        }
        Update: {
          api_key?: string
          auto_create?: boolean
          created_at?: string
          default_severity?: string
          dropoff_pct_threshold?: number
          error_rate_threshold?: number
          id?: string
          last_synced_at?: string | null
          quiet_window_minutes?: number
          rage_click_threshold?: number
          region?: string
          time_on_task_minutes?: number
          updated_at?: string
          webhook_secret?: string
          zero_result_threshold?: number
        }
        Relationships: []
      }
      product_lines: {
        Row: {
          created_at: string
          id: string
          name: string
          sort_order: number
          speedboat_id: string
        }
        Insert: {
          created_at?: string
          id?: string
          name: string
          sort_order?: number
          speedboat_id: string
        }
        Update: {
          created_at?: string
          id?: string
          name?: string
          sort_order?: number
          speedboat_id?: string
        }
        Relationships: [
          {
            foreignKeyName: "product_lines_speedboat_id_fkey"
            columns: ["speedboat_id"]
            isOneToOne: false
            referencedRelation: "speedboats"
            referencedColumns: ["id"]
          },
        ]
      }
      quarter_product_line_counts: {
        Row: {
          count: number
          id: string
          product_line_id: string
          quarter: string
        }
        Insert: {
          count?: number
          id?: string
          product_line_id: string
          quarter: string
        }
        Update: {
          count?: number
          id?: string
          product_line_id?: string
          quarter?: string
        }
        Relationships: [
          {
            foreignKeyName: "quarter_product_line_counts_product_line_id_fkey"
            columns: ["product_line_id"]
            isOneToOne: false
            referencedRelation: "product_lines"
            referencedColumns: ["id"]
          },
        ]
      }
      quarter_resources: {
        Row: {
          id: string
          quarter: string
          resources: number
          sort_order: number
        }
        Insert: {
          id?: string
          quarter: string
          resources?: number
          sort_order?: number
        }
        Update: {
          id?: string
          quarter?: string
          resources?: number
          sort_order?: number
        }
        Relationships: []
      }
      quarter_stats: {
        Row: {
          id: string
          nonstandard_count: number
          quarter: string
          sort_order: number
          standard_count: number
        }
        Insert: {
          id?: string
          nonstandard_count?: number
          quarter: string
          sort_order?: number
          standard_count?: number
        }
        Update: {
          id?: string
          nonstandard_count?: number
          quarter?: string
          sort_order?: number
          standard_count?: number
        }
        Relationships: []
      }
      speedboats: {
        Row: {
          created_at: string
          id: string
          name: string
          sort_order: number
        }
        Insert: {
          created_at?: string
          id?: string
          name: string
          sort_order?: number
        }
        Update: {
          created_at?: string
          id?: string
          name?: string
          sort_order?: number
        }
        Relationships: []
      }
      synapse_cases: {
        Row: {
          created_at: string
          customer: string
          id: string
          next_step: string
          owner: string
          resolution: string
          resolved_at: string | null
          root_cause: string
          severity: string
          signal_id: string | null
          status: string
          title: string
          updated_at: string
        }
        Insert: {
          created_at?: string
          customer?: string
          id?: string
          next_step?: string
          owner?: string
          resolution?: string
          resolved_at?: string | null
          root_cause?: string
          severity?: string
          signal_id?: string | null
          status?: string
          title: string
          updated_at?: string
        }
        Update: {
          created_at?: string
          customer?: string
          id?: string
          next_step?: string
          owner?: string
          resolution?: string
          resolved_at?: string | null
          root_cause?: string
          severity?: string
          signal_id?: string | null
          status?: string
          title?: string
          updated_at?: string
        }
        Relationships: [
          {
            foreignKeyName: "synapse_cases_signal_id_fkey"
            columns: ["signal_id"]
            isOneToOne: false
            referencedRelation: "synapse_signals"
            referencedColumns: ["id"]
          },
        ]
      }
      synapse_runbooks: {
        Row: {
          created_at: string
          description: string
          enabled: boolean
          id: string
          name: string
          success_rate: number
          tier: string
          trigger_signature: string
          updated_at: string
        }
        Insert: {
          created_at?: string
          description?: string
          enabled?: boolean
          id?: string
          name: string
          success_rate?: number
          tier?: string
          trigger_signature?: string
          updated_at?: string
        }
        Update: {
          created_at?: string
          description?: string
          enabled?: boolean
          id?: string
          name?: string
          success_rate?: number
          tier?: string
          trigger_signature?: string
          updated_at?: string
        }
        Relationships: []
      }
      synapse_runs: {
        Row: {
          id: string
          notes: string
          ran_at: string
          result: string
          runbook_id: string
          signal_id: string | null
        }
        Insert: {
          id?: string
          notes?: string
          ran_at?: string
          result?: string
          runbook_id: string
          signal_id?: string | null
        }
        Update: {
          id?: string
          notes?: string
          ran_at?: string
          result?: string
          runbook_id?: string
          signal_id?: string | null
        }
        Relationships: [
          {
            foreignKeyName: "synapse_runs_runbook_id_fkey"
            columns: ["runbook_id"]
            isOneToOne: false
            referencedRelation: "synapse_runbooks"
            referencedColumns: ["id"]
          },
          {
            foreignKeyName: "synapse_runs_signal_id_fkey"
            columns: ["signal_id"]
            isOneToOne: false
            referencedRelation: "synapse_signals"
            referencedColumns: ["id"]
          },
        ]
      }
      synapse_signals: {
        Row: {
          created_at: string
          customer: string
          detected_at: string
          id: string
          reason_code: string
          severity: string
          source: string
          status: string
          summary: string
          updated_at: string
        }
        Insert: {
          created_at?: string
          customer?: string
          detected_at?: string
          id?: string
          reason_code?: string
          severity?: string
          source: string
          status?: string
          summary: string
          updated_at?: string
        }
        Update: {
          created_at?: string
          customer?: string
          detected_at?: string
          id?: string
          reason_code?: string
          severity?: string
          source?: string
          status?: string
          summary?: string
          updated_at?: string
        }
        Relationships: []
      }
    }
    Views: {
      [_ in never]: never
    }
    Functions: {
      [_ in never]: never
    }
    Enums: {
      [_ in never]: never
    }
    CompositeTypes: {
      [_ in never]: never
    }
  }
}

type DatabaseWithoutInternals = Omit<Database, "__InternalSupabase">

type DefaultSchema = DatabaseWithoutInternals[Extract<keyof Database, "public">]

export type Tables<
  DefaultSchemaTableNameOrOptions extends
    | keyof (DefaultSchema["Tables"] & DefaultSchema["Views"])
    | { schema: keyof DatabaseWithoutInternals },
  TableName extends (DefaultSchemaTableNameOrOptions extends {
    schema: keyof DatabaseWithoutInternals
  }
    ? keyof (DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Tables"] &
        DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Views"])
    : never) = never,
> = DefaultSchemaTableNameOrOptions extends {
  schema: keyof DatabaseWithoutInternals
}
  ? (DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Tables"] &
      DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Views"])[TableName] extends {
      Row: infer R
    }
    ? R
    : never
  : DefaultSchemaTableNameOrOptions extends keyof (DefaultSchema["Tables"] &
        DefaultSchema["Views"])
    ? (DefaultSchema["Tables"] &
        DefaultSchema["Views"])[DefaultSchemaTableNameOrOptions] extends {
        Row: infer R
      }
      ? R
      : never
    : never

export type TablesInsert<
  DefaultSchemaTableNameOrOptions extends
    | keyof DefaultSchema["Tables"]
    | { schema: keyof DatabaseWithoutInternals },
  TableName extends (DefaultSchemaTableNameOrOptions extends {
    schema: keyof DatabaseWithoutInternals
  }
    ? keyof DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Tables"]
    : never) = never,
> = DefaultSchemaTableNameOrOptions extends {
  schema: keyof DatabaseWithoutInternals
}
  ? DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Tables"][TableName] extends {
      Insert: infer I
    }
    ? I
    : never
  : DefaultSchemaTableNameOrOptions extends keyof DefaultSchema["Tables"]
    ? DefaultSchema["Tables"][DefaultSchemaTableNameOrOptions] extends {
        Insert: infer I
      }
      ? I
      : never
    : never

export type TablesUpdate<
  DefaultSchemaTableNameOrOptions extends
    | keyof DefaultSchema["Tables"]
    | { schema: keyof DatabaseWithoutInternals },
  TableName extends (DefaultSchemaTableNameOrOptions extends {
    schema: keyof DatabaseWithoutInternals
  }
    ? keyof DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Tables"]
    : never) = never,
> = DefaultSchemaTableNameOrOptions extends {
  schema: keyof DatabaseWithoutInternals
}
  ? DatabaseWithoutInternals[DefaultSchemaTableNameOrOptions["schema"]]["Tables"][TableName] extends {
      Update: infer U
    }
    ? U
    : never
  : DefaultSchemaTableNameOrOptions extends keyof DefaultSchema["Tables"]
    ? DefaultSchema["Tables"][DefaultSchemaTableNameOrOptions] extends {
        Update: infer U
      }
      ? U
      : never
    : never

export type Enums<
  DefaultSchemaEnumNameOrOptions extends
    | keyof DefaultSchema["Enums"]
    | { schema: keyof DatabaseWithoutInternals },
  EnumName extends (DefaultSchemaEnumNameOrOptions extends {
    schema: keyof DatabaseWithoutInternals
  }
    ? keyof DatabaseWithoutInternals[DefaultSchemaEnumNameOrOptions["schema"]]["Enums"]
    : never) = never,
> = DefaultSchemaEnumNameOrOptions extends {
  schema: keyof DatabaseWithoutInternals
}
  ? DatabaseWithoutInternals[DefaultSchemaEnumNameOrOptions["schema"]]["Enums"][EnumName]
  : DefaultSchemaEnumNameOrOptions extends keyof DefaultSchema["Enums"]
    ? DefaultSchema["Enums"][DefaultSchemaEnumNameOrOptions]
    : never

export type CompositeTypes<
  PublicCompositeTypeNameOrOptions extends
    | keyof DefaultSchema["CompositeTypes"]
    | { schema: keyof DatabaseWithoutInternals },
  CompositeTypeName extends (PublicCompositeTypeNameOrOptions extends {
    schema: keyof DatabaseWithoutInternals
  }
    ? keyof DatabaseWithoutInternals[PublicCompositeTypeNameOrOptions["schema"]]["CompositeTypes"]
    : never) = never,
> = PublicCompositeTypeNameOrOptions extends {
  schema: keyof DatabaseWithoutInternals
}
  ? DatabaseWithoutInternals[PublicCompositeTypeNameOrOptions["schema"]]["CompositeTypes"][CompositeTypeName]
  : PublicCompositeTypeNameOrOptions extends keyof DefaultSchema["CompositeTypes"]
    ? DefaultSchema["CompositeTypes"][PublicCompositeTypeNameOrOptions]
    : never

export const Constants = {
  public: {
    Enums: {},
  },
} as const

```

## `src/lib/error-capture.ts`

```ts
// Captures the original Error out-of-band so server.ts can recover the stack
// when h3 has already swallowed the throw into a generic 500 Response.

let lastCapturedError: { error: unknown; at: number } | undefined;
const TTL_MS = 5_000;

function record(error: unknown) {
  lastCapturedError = { error, at: Date.now() };
}

// h3's HTTPError serializes to {"status":500,"unhandled":true,"message":"HTTPError"} —
// no stack, no cause — so a plain console.error(error) reaches the log pipeline with
// the failure detail stripped. Expand Error-like args into a string that keeps the
// message, stack, and the full cause chain.
const CAUSE_DEPTH_LIMIT = 5;
const DESCRIPTION_LENGTH_LIMIT = 8_000;

export function describeError(error: unknown): string {
  const parts: string[] = [];
  let current: unknown = error;
  for (let depth = 0; depth < CAUSE_DEPTH_LIMIT && current != null; depth++) {
    if (!(current instanceof Error)) {
      parts.push(typeof current === "string" ? current : safeStringify(current));
      break;
    }
    const label = depth === 0 ? "" : "caused by: ";
    const status = describeStatus(current);
    parts.push(`${label}${current.stack ?? `${current.name}: ${current.message}`}${status}`);
    current = current.cause;
  }
  return parts.join("\n").slice(0, DESCRIPTION_LENGTH_LIMIT);
}

function describeStatus(error: Error): string {
  const { status, statusCode } = error as { status?: unknown; statusCode?: unknown };
  const value = status ?? statusCode;
  return typeof value === "number" ? ` (status ${value})` : "";
}

function safeStringify(value: unknown): string {
  try {
    return JSON.stringify(value) ?? String(value);
  } catch {
    return String(value);
  }
}

function isErrorLike(value: unknown): value is Error {
  return value instanceof Error;
}

// Wrap console.error so errors logged by any layer — including h3's internal
// unhandled-error logging, which this file cannot hook directly — are both
// recorded for consumeLastCapturedError and expanded before serialization.
const originalConsoleError = console.error.bind(console);
console.error = (...args: unknown[]) => {
  const expanded = args.map((arg) => {
    if (!isErrorLike(arg)) return arg;
    record(arg);
    return describeError(arg);
  });
  originalConsoleError(...expanded);
};

if (typeof globalThis.addEventListener === "function") {
  globalThis.addEventListener("error", (event) => record((event as ErrorEvent).error ?? event));
  globalThis.addEventListener("unhandledrejection", (event) =>
    record((event as PromiseRejectionEvent).reason),
  );
}

export function consumeLastCapturedError(): unknown {
  if (!lastCapturedError) return undefined;
  if (Date.now() - lastCapturedError.at > TTL_MS) {
    lastCapturedError = undefined;
    return undefined;
  }
  const { error } = lastCapturedError;
  lastCapturedError = undefined;
  return error;
}

```

## `src/lib/error-page.ts`

```ts
export function renderErrorPage(): string {
  return `<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>This page didn't load</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
      body { font: 15px/1.5 system-ui, -apple-system, sans-serif; background: #fafafa; color: #111; display: grid; place-items: center; min-height: 100vh; margin: 0; padding: 1.5rem; }
      .card { max-width: 28rem; width: 100%; text-align: center; padding: 2rem; }
      h1 { font-size: 1.25rem; margin: 0 0 0.5rem; }
      p { color: #4b5563; margin: 0 0 1.5rem; }
      .actions { display: flex; gap: 0.5rem; justify-content: center; flex-wrap: wrap; }
      a, button { padding: 0.5rem 1rem; border-radius: 0.375rem; font: inherit; cursor: pointer; text-decoration: none; border: 1px solid transparent; }
      .primary { background: #111; color: #fff; }
      .secondary { background: #fff; color: #111; border-color: #d1d5db; }
    </style>
  </head>
  <body>
    <div class="card">
      <h1>This page didn't load</h1>
      <p>Something went wrong on our end. You can try refreshing or head back home.</p>
      <div class="actions">
        <button class="primary" onclick="location.reload()">Try again</button>
        <a class="secondary" href="/">Go home</a>
      </div>
    </div>
  </body>
</html>`;
}

```

## `src/lib/lovable-error-reporting.ts`

```ts
type LovableErrorOptions = {
  mechanism?: "manual" | "onerror" | "unhandledrejection" | "react_error_boundary";
  handled?: boolean;
  severity?: "error" | "warning" | "info";
};

type LovableEvents = {
  captureException?: (
    error: unknown,
    context?: Record<string, unknown>,
    options?: LovableErrorOptions,
  ) => void;
};

declare global {
  interface Window {
    __lovableEvents?: LovableEvents;
    __lovableReportRuntimeError?: (payload: {
      message: string;
      stack?: string;
      filename?: string;
    }) => void;
  }
}

export function reportLovableError(error: unknown, context: Record<string, unknown> = {}) {
  if (typeof window === "undefined") return;
  window.__lovableEvents?.captureException?.(
    error,
    {
      source: "react_error_boundary",
      route: window.location.pathname,
      ...context,
    },
    {
      mechanism: "react_error_boundary",
      handled: false,
      severity: "error",
    },
  );
  // Prod React does not rethrow boundary-caught errors to window.onerror, so the
  // editor's telemetry never sees them. Forward to lovable.js's reporting hook,
  // which is present only inside the editor preview.
  // Loaders and server fns commonly throw a raw Response; String(it) is the
  // opaque "[object Response]", so pull out the status and URL instead.
  const message =
    error instanceof Response
      ? `Response ${error.status}${error.url ? ` at ${error.url}` : ""}`
      : error instanceof Error
        ? error.message
        : String(error);
  const stack = error instanceof Error ? error.stack : undefined;
  window.__lovableReportRuntimeError?.({
    message,
    ...(stack !== undefined && { stack }),
    filename: window.location.pathname,
  });
}

```

## `src/lib/synapse.ts`

```ts
import { queryOptions } from "@tanstack/react-query";
import { supabase } from "@/integrations/supabase/client";

export const SEVERITIES = ["low", "medium", "high", "critical"] as const;
export type Severity = (typeof SEVERITIES)[number];

export const SIGNAL_STATUSES = ["new", "triaged", "healing", "resolved"] as const;
export const CASE_STATUSES = ["triaged", "in_progress", "blocked", "resolved"] as const;
export const TIERS = ["tier1", "tier2", "tier3"] as const;
export const RUN_RESULTS = ["succeeded", "failed", "rolled_back"] as const;

export const LABEL: Record<string, string> = {
  new: "New",
  triaged: "Triaged",
  healing: "Self-healing",
  resolved: "Resolved",
  in_progress: "In progress",
  blocked: "Blocked",
  tier1: "Tier 1 · Autonomous",
  tier2: "Tier 2 · Approved",
  tier3: "Tier 3 · Advisory",
  succeeded: "Succeeded",
  failed: "Failed",
  rolled_back: "Rolled back",
  low: "Low",
  medium: "Medium",
  high: "High",
  critical: "Critical",
};

export const PILL =
  "inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-semibold";

export function severityClass(severity: string) {
  switch (severity) {
    case "critical":
      return "bg-destructive/15 text-destructive";
    case "high":
      return "bg-status-progress text-status-progress-foreground";
    case "medium":
      return "bg-accent text-accent-foreground";
    default:
      return "bg-status-planned text-status-planned-foreground";
  }
}

export function stateClass(state: string) {
  switch (state) {
    case "resolved":
    case "succeeded":
      return "bg-status-delivered text-status-delivered-foreground";
    case "healing":
    case "in_progress":
      return "bg-status-progress text-status-progress-foreground";
    case "failed":
    case "rolled_back":
    case "blocked":
      return "bg-destructive/15 text-destructive";
    default:
      return "bg-status-planned text-status-planned-foreground";
  }
}

export type Signal = {
  id: string;
  source: string;
  summary: string;
  severity: string;
  status: string;
  customer: string;
  reason_code: string;
  detected_at: string;
  updated_at?: string;
};

export type SynapseCase = {
  id: string;
  title: string;
  signal_id: string | null;
  severity: string;
  status: string;
  owner: string;
  root_cause: string;
  next_step: string;
  customer: string;
  resolution: string;
  resolved_at: string | null;
  created_at: string;
  updated_at?: string;
};

export type Runbook = {
  id: string;
  name: string;
  tier: string;
  trigger_signature: string;
  description: string;
  enabled: boolean;
  success_rate: number;
};

export type RunbookRun = {
  id: string;
  runbook_id: string;
  signal_id: string | null;
  result: string;
  notes: string;
  ran_at: string;
};

function unwrap<T>({ data, error }: { data: T | null; error: unknown }): T {
  if (error) throw error;
  return (data ?? []) as T;
}

export const signalsQuery = () =>
  queryOptions({
    queryKey: ["synapse", "signals"],
    queryFn: async () =>
      unwrap<Signal[]>(
        await supabase
          .from("synapse_signals")
          .select("*")
          .order("detected_at", { ascending: false }),
      ),
  });

export const casesQuery = () =>
  queryOptions({
    queryKey: ["synapse", "cases"],
    queryFn: async () =>
      unwrap<SynapseCase[]>(
        await supabase
          .from("synapse_cases")
          .select("*")
          .order("created_at", { ascending: false }),
      ),
  });

export const runbooksQuery = () =>
  queryOptions({
    queryKey: ["synapse", "runbooks"],
    queryFn: async () =>
      unwrap<Runbook[]>(
        await supabase.from("synapse_runbooks").select("*").order("tier").order("name"),
      ),
  });

export const runsQuery = () =>
  queryOptions({
    queryKey: ["synapse", "runs"],
    queryFn: async () =>
      unwrap<RunbookRun[]>(
        await supabase.from("synapse_runs").select("*").order("ran_at", { ascending: false }),
      ),
  });

export function timeAgo(iso?: string | null) {
  if (!iso) return "—";
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.round(diff / 60000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.round(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.round(hours / 24)}d ago`;
}

export const THEMES = [
  "Case creation",
  "Account & access",
  "Licensing & entitlements",
  "Onboarding & setup",
  "Feature adoption",
  "Search & guidance",
  "Platform reliability",
  "Other",
] as const;
export type Theme = (typeof THEMES)[number];

export function signalTheme(s: {
  summary: string;
  source?: string;
  reason_code?: string;
}): Theme {
  const t = `${s.summary} ${s.reason_code ?? ""}`.toLowerCase();
  if (/case form|case creation|submit case|attachment|severity guidance/.test(t))
    return "Case creation";
  if (/auth|login|sign[- ]in|token|saml|api key|password|access & authentication/.test(t))
    return "Account & access";
  if (/licen|entitlement|seat|trial|plan|billing/.test(t)) return "Licensing & entitlements";
  if (/onboard|setup|walkthrough|identity provider|config|upgrade|rollback|install/.test(t))
    return "Onboarding & setup";
  if (/adoption|untouched|usage fell|never used|dismiss|guide/.test(t)) return "Feature adoption";
  if (/search|zero-result|documentation|how-to|nps|feedback/.test(t)) return "Search & guidance";
  if (/rate-limit|429|error|failure|failing|rejected|retry|retries|latency|outage|rage click/.test(t))
    return "Platform reliability";
  return "Other";
}

```

## `src/lib/utils.ts`

```ts
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

```

## `src/router.tsx`

```tsx
import { QueryClient } from "@tanstack/react-query";
import { createRouter } from "@tanstack/react-router";
import { routeTree } from "./routeTree.gen";

export const getRouter = () => {
  const queryClient = new QueryClient();

  const router = createRouter({
    routeTree,
    context: { queryClient },
    scrollRestoration: true,
    defaultPreloadStaleTime: 0,
  });

  return router;
};

```

## `src/routes/README.md`

```md
# Routes

TanStack Start uses **file-based routing**. Every `.tsx` file in this directory
defines a route. Do **not** create `src/pages/`, `src/routes/_app/index.tsx`, or
`app/layout.tsx` — those are Next.js / Remix conventions. The only root layout
is `src/routes/__root.tsx`.

## Conventions

| File | URL |
| --- | --- |
| `index.tsx` | `/` |
| `about.tsx` | `/about` |
| `users/index.tsx` | `/users` |
| `users/$id.tsx` | `/users/:id` (dynamic — bare `$`, no curly braces) |
| `posts/{-$category}.tsx` | `/posts/:category?` (optional segment) |
| `files/$.tsx` | `/files/*` (splat — read via `_splat` param, never `*`) |
| `_layout.tsx` | layout route (renders children via `<Outlet />`) |
| `__root.tsx` | app shell — wraps every page; preserve `<Outlet />` |

`routeTree.gen.ts` is auto-generated. Don't edit it by hand.

```

## `src/routes/__root.tsx`

```tsx
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  Outlet,
  Link,
  createRootRouteWithContext,
  useRouter,
  HeadContent,
  Scripts,
} from "@tanstack/react-router";
import { useEffect, type ReactNode } from "react";

import appCss from "../styles.css?url";
import { Toaster } from "../components/ui/sonner";
import { reportLovableError } from "../lib/lovable-error-reporting";

function NotFoundComponent() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="max-w-md text-center">
        <h1 className="text-7xl font-bold text-foreground">404</h1>
        <h2 className="mt-4 text-xl font-semibold text-foreground">Page not found</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          The page you're looking for doesn't exist or has been moved.
        </p>
        <div className="mt-6">
          <Link
            to="/"
            className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
          >
            Go home
          </Link>
        </div>
      </div>
    </div>
  );
}

function ErrorComponent({ error, reset }: { error: Error; reset: () => void }) {
  console.error(error);
  const router = useRouter();
  useEffect(() => {
    reportLovableError(error, { boundary: "tanstack_root_error_component" });
  }, [error]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="max-w-md text-center">
        <h1 className="text-xl font-semibold tracking-tight text-foreground">
          This page didn't load
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Something went wrong on our end. You can try refreshing or head back home.
        </p>
        <div className="mt-6 flex flex-wrap justify-center gap-2">
          <button
            onClick={() => {
              router.invalidate();
              reset();
            }}
            className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
          >
            Try again
          </button>
          <a
            href="/"
            className="inline-flex items-center justify-center rounded-md border border-input bg-background px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-accent"
          >
            Go home
          </a>
        </div>
      </div>
    </div>
  );
}

export const Route = createRootRouteWithContext<{ queryClient: QueryClient }>()({
  head: () => ({
    meta: [
      { charSet: "utf-8" },
      { name: "viewport", content: "width=device-width, initial-scale=1" },
      { title: "CSP AI Capabilities" },
      { name: "description", content: "Watch, diagnose and heal product signals" },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
    links: [
      {
        rel: "stylesheet",
        href: appCss,
      },
      { rel: "preconnect", href: "https://fonts.googleapis.com" },
      { rel: "preconnect", href: "https://fonts.gstatic.com", crossOrigin: "anonymous" },
      {
        rel: "stylesheet",
        href: "https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600&family=Space+Grotesk:wght@600;700&display=swap",
      },
      { rel: "icon", href: "/favicon.ico", type: "image/x-icon" },
    ],
  }),

  shellComponent: RootShell,
  component: RootComponent,
  notFoundComponent: NotFoundComponent,
  errorComponent: ErrorComponent,
});

function RootShell({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <head>
        <HeadContent />
      </head>
      <body>
        {children}
        <Scripts />
      </body>
    </html>
  );
}

function RootComponent() {
  const { queryClient } = Route.useRouteContext();

  return (
    <QueryClientProvider client={queryClient}>
      {/* Required: nested routes render here. Removing <Outlet /> breaks all child routes. */}
      <Outlet />
      <Toaster richColors position="top-right" />
    </QueryClientProvider>
  );
}


```

## `src/routes/api/public/pendo-alert.ts`

```ts
import { createFileRoute } from "@tanstack/react-router";
import { createClient } from "@supabase/supabase-js";
import { z } from "zod";

const bodySchema = z.object({
  source: z.string().trim().max(120).optional(),
  summary: z.string().trim().min(1).max(500),
  customer: z.string().trim().max(160).optional(),
  severity: z.enum(["low", "medium", "high", "critical"]).optional(),
  reason_code: z.string().trim().max(160).optional(),
  metric: z
    .enum([
      "rage_clicks",
      "dropoff_pct",
      "zero_results",
      "error_rate",
      "time_on_task_minutes",
    ])
    .optional(),
  value: z.number().finite().optional(),
});

const THRESHOLD_COLUMN: Record<string, string> = {
  rage_clicks: "rage_click_threshold",
  dropoff_pct: "dropoff_pct_threshold",
  zero_results: "zero_result_threshold",
  error_rate: "error_rate_threshold",
  time_on_task_minutes: "time_on_task_minutes",
};

export const Route = createFileRoute("/api/public/pendo-alert")({
  server: {
    handlers: {
      POST: async ({ request }) => {
        const url = process.env["SUPABASE_URL"];
        const key = process.env["SUPABASE_SERVICE_ROLE_KEY"];
        if (!url || !key) return new Response("Not configured", { status: 500 });

        let payload: unknown;
        try {
          payload = await request.json();
        } catch {
          return new Response("Invalid JSON", { status: 400 });
        }
        const parsed = bodySchema.safeParse(payload);
        if (!parsed.success) {
          return Response.json({ error: "Invalid payload" }, { status: 400 });
        }
        const body = parsed.data;

        const supabase = createClient(url, key, { auth: { persistSession: false } });
        const { data: settings } = await supabase
          .from("pendo_settings")
          .select("*")
          .order("created_at", { ascending: true })
          .limit(1)
          .maybeSingle();

        if (!settings) return new Response("Not configured", { status: 500 });

        const secret = request.headers.get("x-pendo-secret") ?? "";
        if (!settings.webhook_secret || secret !== settings.webhook_secret) {
          return new Response("Invalid secret", { status: 401 });
        }
        if (!settings.auto_create) {
          return Response.json({ created: false, reason: "auto-create is off" });
        }

        if (body.metric && typeof body.value === "number") {
          const column = THRESHOLD_COLUMN[body.metric] ?? "";
          const limit = Number((settings as Record<string, unknown>)[column] ?? 0);
          if (body.value < limit) {
            return Response.json({ created: false, reason: "below threshold", limit });
          }
        }

        const since = new Date(
          Date.now() - Number(settings.quiet_window_minutes ?? 30) * 60_000,
        ).toISOString();
        const { data: recent } = await supabase
          .from("synapse_signals")
          .select("id")
          .eq("summary", body.summary)
          .gte("detected_at", since)
          .limit(1);
        if (recent && recent.length > 0) {
          return Response.json({ created: false, reason: "duplicate in quiet window" });
        }

        const { data: created, error } = await supabase
          .from("synapse_signals")
          .insert({
            source: body.source ?? "Click & page events",
            summary: body.summary,
            severity: body.severity ?? settings.default_severity ?? "medium",
            status: "new",
            customer: body.customer ?? "Unknown",
            reason_code: body.reason_code ?? "Unclassified",
          })
          .select("id")
          .single();

        if (error) return Response.json({ error: error.message }, { status: 500 });

        await supabase
          .from("pendo_settings")
          .update({ last_synced_at: new Date().toISOString() })
          .eq("id", settings.id);

        return Response.json({ created: true, id: created.id });
      },
    },
  },
});

```

## `src/routes/index.tsx`

```tsx
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "CSP AI Capabilities — Watch, Diagnose, Heal" },
      {
        name: "description",
        content:
          "CSP AI Capabilities turns product and support signals into diagnosed cases and self-healing runbooks across your platform.",
      },
      { property: "og:title", content: "CSP AI Capabilities — Watch, Diagnose, Heal" },
      {
        property: "og:description",
        content: "Signals, cases, runbooks and healing performance in one operating view.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  beforeLoad: () => {
    throw redirect({ to: "/synapse" });
  },
});

```

## `src/routes/synapse.about.tsx`

```tsx
import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { SynapseShell } from "@/components/synapse-shell";
import { RunbooksPanel } from "@/components/runbooks-panel";
import { MetricsPanel } from "@/components/metrics-panel";
import { CasesPanel } from "@/components/cases-panel";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Activity,
  BrainCircuit,
  ClipboardList,
  Eye,
  RefreshCcw,
  ShieldCheck,
  Wrench,
} from "lucide-react";

export const Route = createFileRoute("/synapse/about")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "CSP AI Capabilities — Watch, Diagnose, Heal" },
      {
        name: "description",
        content:
          "Agentic support agents that watch telemetry, diagnose root cause, run self-healing runbooks and auto-triage escalations before an engineer is engaged.",
      },
      { property: "og:title", content: "CSP AI Capabilities — Watch, Diagnose, Heal" },
      {
        property: "og:description",
        content:
          "The cognitive nerve center: continuous monitoring, evidence-based diagnosis, reversible self-healing and fully triaged escalations.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: SynapsePage,
});

const LOOP = [
  {
    icon: Eye,
    step: "01",
    title: "Observe",
    body: "Telemetry, logs, traces and health checks streamed and correlated per customer and asset.",
  },
  {
    icon: BrainCircuit,
    step: "02",
    title: "Diagnose",
    body: "Hypotheses ranked against known failure signatures, recent change events and similar past cases.",
  },
  {
    icon: Wrench,
    step: "03",
    title: "Act",
    body: "Approved self-healing runbooks executed inside policy, with verification and rollback.",
  },
  {
    icon: ClipboardList,
    step: "04",
    title: "Triage",
    body: "Anything unresolved packaged as a case with evidence, severity and a named next owner.",
  },
];

const CURRENT_STATE = [
  "Telemetry, logs and alerts live in separate tools, read only after a ticket lands",
  "Root-cause analysis restarts from zero on every case, engineer by engineer",
  "Known, safely reversible fixes still wait in a human queue",
  "Complex cases escalate under-triaged — missing logs, versions and reproduction steps",
];

const TRIGGERS = [
  {
    feed: "Application & service logs",
    looks: "Error bursts, repeated stack signatures, failed job retries",
    trigger: "Candidate incident with matched failure signature",
  },
  {
    feed: "Infrastructure telemetry",
    looks: "Saturation, latency drift, capacity thresholds, node health",
    trigger: "Degradation watch before customer impact",
  },
  {
    feed: "Configuration & change events",
    looks: "Recent deploys, policy edits, version and certificate changes",
    trigger: "Change-correlated root-cause hypothesis",
  },
  {
    feed: "Integration & API health",
    looks: "Auth failures, timeouts, connector error rates",
    trigger: "Connector-scoped self-healing runbook",
  },
  {
    feed: "Customer & case context",
    looks: "Entitlements, environment, open cases, prior incidents",
    trigger: "Severity and blast-radius scoring",
  },
];

const CSP_DATA = [
  {
    data: "Click & page events",
    contains: "Page views, clicks, element paths, session sequence",
    freshness: "Near real time",
    use: "Detects struggle patterns and repeat attempts",
  },
  {
    data: "Feature usage",
    contains: "Tagged feature adoption, frequency, last used",
    freshness: "Hourly",
    use: "Separates a defect from simple non-adoption",
  },
  {
    data: "Guides & in-app messaging",
    contains: "Guide views, dismissals, step completion",
    freshness: "Near real time",
    use: "Shows whether self-serve help actually landed",
  },
  {
    data: "Search & feedback",
    contains: "In-app search terms, zero-result searches, NPS and poll responses",
    freshness: "Daily",
    use: "Surfaces knowledge gaps and sentiment context",
  },
  {
    data: "Visitor & account metadata",
    contains: "Role, region, plan, account ID, cohort",
    freshness: "On sync",
    use: "Joins CSP to the graph and scopes any outreach",
  },
];

const PENDO_PAIN = [
  {
    signal: "High drop-off",
    tells: "Where users abandon a workflow",
    pain: "Users start case creation but don't complete it",
  },
  {
    signal: "Repeated clicks",
    tells: "UI confusion or inability to find something",
    pain: 'Users repeatedly click around looking for "Create Case"',
  },
  {
    signal: "Feature adoption",
    tells: "Whether customers discover/use a capability",
    pain: "New AI feature has very low adoption despite high eligibility",
  },
  {
    signal: "Time on task",
    tells: "Workflow friction/complexity",
    pain: "Customers spend 5+ minutes completing a simple admin request",
  },
  {
    signal: "Page/path analysis",
    tells: "Where users go before/after an action",
    pain: "Users visit multiple pages before finding entitlement information",
  },
  {
    signal: "Error/failed interactions",
    tells: "Potential usability or technical issues",
    pain: "Users repeatedly encounter validation errors during case creation",
  },
  {
    signal: "Search behavior",
    tells: "What customers are trying to find",
    pain: 'Frequent searches for "MFA reset" indicate an unresolved self-service need',
  },
  {
    signal: "Segmentation",
    tells: "Which customers experience the problem",
    pain: "Pain point concentrated among new customers or specific products",
  },
  {
    signal: "Feature comparison",
    tells: "Differences between successful and struggling users",
    pain: "Successful users use AI guidance; others abandon without it",
  },
  {
    signal: "Session/replay data",
    tells: "Qualitative evidence of frustration",
    pain: "Users repeatedly navigate backward/forward or try unexpected controls",
  },
];



const REASON_CODES = [
  {
    signal: "Rage / repeat clicks",
    means: "Control not responding or not understood",
    code: "Product not working as expected",
    action: "In-app guide, then prefilled triaged case",
  },
  {
    signal: "Onboarding funnel drop-off",
    means: "Setup blocked mid-flow",
    code: "Configuration / setup assistance",
    action: "Targeted walkthrough plus CSM alert",
  },
  {
    signal: "Repeated failed sign-in path",
    means: "Access or account state problem",
    code: "Access & authentication",
    action: "Self-service unlock offered proactively",
  },
  {
    signal: "Entitlement or licence page loops",
    means: "Licence visibility or allocation confusion",
    code: "Licensing & entitlements",
    action: "Entitlement summary surfaced in product",
  },
  {
    signal: "Failed in-app search, guide dismissed",
    means: "Knowledge gap, not a defect",
    code: "How-to / documentation",
    action: "Content fix and search-term routing",
  },
  {
    signal: "Feature non-adoption after purchase",
    means: "Value at risk, likely enablement request",
    code: "Enablement / adoption",
    action: "Guided activation campaign",
  },
];

const TIERS = [
  {
    tier: "Tier 1 · Autonomous",
    examples:
      "Clear stuck job queues, refresh expiring tokens, restart a failed worker, re-run a failed sync",
    control: "Pre-approved runbook, automatic verification, automatic rollback",
  },
  {
    tier: "Tier 2 · Approved",
    examples: "Configuration correction, entitlement repair, targeted cache or index rebuild",
    control: "Human approves the proposed diff, agent executes and verifies",
  },
  {
    tier: "Tier 3 · Advisory only",
    examples: "Data changes, security posture edits, customer-impacting restarts",
    control: "Agent never executes — it prepares the plan and evidence",
  },
];

const ESCALATION = [
  {
    title: "Severity & blast radius",
    body: "Scored from affected customers, entitlements and environment criticality — not from who shouted loudest.",
  },
  {
    title: "Ranked root cause",
    body: "Top hypotheses with supporting evidence and the checks already ruled out.",
  },
  {
    title: "Actions already taken",
    body: "Tier 1 remediation attempts, their verification results and any rollback.",
  },
  {
    title: "Routing",
    body: "Named team or specialist based on component ownership and prior resolution history.",
  },
  {
    title: "Reproduction package",
    body: "Log excerpts, config diffs, versions, timeline and correlated change events attached.",
  },
  {
    title: "Recommended next step",
    body: "A concrete proposed action the engineer can approve, adjust or reject.",
  },
];

const GUARDRAILS = [
  { title: "Shadow mode first", body: "Every new signature and runbook runs observe-only until precision clears its threshold." },
  { title: "Blast-radius limits", body: "Rate caps per action, per customer and per window. Breaching a cap pauses the agent." },
  { title: "Kill switch", body: "One control disables autonomous execution globally while monitoring and triage continue." },
  { title: "Least privilege", body: "Scoped credentials per runbook. No standing administrative access." },
  { title: "Data handling", body: "Log redaction before model calls; customer data stays inside approved boundaries." },
  { title: "Human authority", body: "Engineers can reverse any agent action, and their corrections are training signal." },
];

const METRICS = [
  { metric: "Mean time to detect", baseline: "[MTTD]", target: "[TARGET]" },
  { metric: "Root-cause accuracy (engineer-confirmed)", baseline: "[%]", target: "[TARGET]" },
  { metric: "Incidents self-healed without human touch", baseline: "[%]", target: "[TARGET]" },
  { metric: "Escalations arriving fully triaged", baseline: "[%]", target: "[TARGET]" },
  { metric: "Failed or rolled-back agent actions", baseline: "[COUNT]", target: "[CEILING]" },
  { metric: "Engineer hours returned per month", baseline: "[HOURS]", target: "[TARGET]" },
];

const PHASES = [
  {
    phase: "Phase 1 · Watch",
    body: "Ingest telemetry and log feeds for [SCOPE]. Correlate incidents, run shadow-mode diagnosis, publish precision weekly.",
  },
  {
    phase: "Phase 2 · Diagnose",
    body: "Auto-triage on live escalations: severity, routing, evidence package, recommended next step. Engineers grade every diagnosis.",
  },
  {
    phase: "Phase 3 · Heal",
    body: "Tier 1 self-healing enabled on the highest-precision signatures, then Tier 2 with approval — expanded runbook by runbook.",
  },
];

const ASKS = [
  {
    n: "1",
    title: "Grant feed access",
    body: "Read access and retention for the telemetry, log and change feeds in scope.",
  },
  {
    n: "2",
    title: "Fund Phase 1 & 2",
    body: "Monitoring, correlation and auto-triage in shadow mode: [BUDGET].",
  },
  {
    n: "3",
    title: "Approve the autonomy policy",
    body: "Tier definitions, precision thresholds and the kill-switch owner.",
  },
];

function SectionTitle({ eyebrow, title }: { eyebrow: string; title: string }) {
  return (
    <div className="mb-4">
      <p className="text-[11px] font-bold uppercase tracking-[0.16em] text-muted-foreground">
        {eyebrow}
      </p>
      <h3 className="mt-1 font-display text-xl font-bold tracking-tight sm:text-2xl">{title}</h3>
    </div>
  );
}

function Th({ children }: { children: React.ReactNode }) {
  return (
    <th className="px-4 py-3 text-left text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground">
      {children}
    </th>
  );
}

function Td({ children }: { children: React.ReactNode }) {
  return <td className="px-4 py-3 align-top text-sm">{children}</td>;
}

const TABS = [
  { id: "about", label: "About" },
  { id: "runbooks", label: "Runbooks" },
  { id: "metrics", label: "Metrics & impact" },
  { id: "cases", label: "Case queue" },
] as const;

function SynapsePage() {
  const [tab, setTab] = useState<(typeof TABS)[number]["id"]>("about");
  return (
    <SynapseShell
      title="CSP AI Capabilities — The Cognitive Nerve Center"
      subtitle="Agentic support agents that watch, diagnose and heal. Watch. Diagnose. Heal."
    >
      <div className="mb-6 flex flex-wrap gap-1 rounded-full border border-border bg-muted/40 p-1">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`rounded-full px-4 py-2 text-sm font-semibold transition-colors ${
              tab === t.id
                ? "bg-primary text-primary-foreground"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === "cases" ? <CasesPanel /> : tab === "metrics" ? <MetricsPanel /> : tab === "runbooks" ? <RunbooksPanel /> : (
      <div className="space-y-10">
        <Card className="overflow-hidden">
          <CardContent className="p-6 sm:p-8">
            <p className="text-[11px] font-bold uppercase tracking-[0.18em] text-primary">
              Mission statement
            </p>
            <p className="mt-3 max-w-4xl font-display text-xl font-semibold leading-snug sm:text-2xl">
              To unify fragmented telemetry into real-time cognitive action.
            </p>
            <p className="mt-3 max-w-4xl text-sm text-muted-foreground">
              We build the self-healing infrastructure of tomorrow by routing, analyzing and
              resolving critical system signals autonomously before they impact the user
              experience — bridging raw machine complexity and instant, automated remediation
              through deep contextual intelligence.
            </p>
            <div className="mt-5 flex flex-wrap gap-2">
              {["Telemetry monitoring", "Root-cause analysis", "Self-healing", "Auto-triage"].map(
                (t) => (
                  <span
                    key={t}
                    className="rounded-full border border-border bg-muted/50 px-3 py-1 text-[11px] font-semibold uppercase tracking-[0.1em] text-muted-foreground"
                  >
                    {t}
                  </span>
                ),
              )}
            </div>
          </CardContent>
        </Card>

        <section>
          <SectionTitle
            eyebrow="01 · Current state"
            title="Signals exist. Nobody is watching them until a customer complains."
          />
          <div className="grid gap-4 lg:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
            <Card>
              <CardContent className="p-6">
                <ul className="space-y-3">
                  {CURRENT_STATE.map((item) => (
                    <li key={item} className="flex gap-3 text-sm">
                      <span className="mt-2 size-1.5 shrink-0 rounded-full bg-primary" />
                      <span className="text-muted-foreground">{item}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>
            <div className="grid gap-4">
              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="font-display text-base">Baseline to fill</CardTitle>
                </CardHeader>
                <CardContent className="text-sm text-muted-foreground">
                  Mean time to detect [MTTD] · mean time to resolve [MTTR] · escalation rate [%]
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="font-display text-base">Cost of the delay</CardTitle>
                </CardHeader>
                <CardContent className="text-sm text-muted-foreground">
                  Engineer hours spent re-deriving known diagnoses: [HOURS / MONTH]
                </CardContent>
              </Card>
            </div>
          </div>
        </section>

        <section>
          <SectionTitle
            eyebrow="02 · The agentic loop"
            title="Observe → diagnose → act → triage → learn"
          />
          <p className="mb-4 max-w-3xl text-sm text-muted-foreground">
            The agent runs the loop continuously. Humans enter at the escalation boundary, not at
            the start.
          </p>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            {LOOP.map((s) => (
              <Card key={s.title}>
                <CardContent className="p-5">
                  <div className="flex items-center gap-3">
                    <span className="inline-flex size-9 items-center justify-center rounded-xl bg-primary/10 text-primary">
                      <s.icon className="size-4" />
                    </span>
                    <span className="font-display text-xs font-bold tracking-[0.14em] text-muted-foreground">
                      {s.step}
                    </span>
                  </div>
                  <p className="mt-3 font-display text-lg font-bold">{s.title}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{s.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
          <div className="mt-4 flex items-start gap-3 rounded-2xl border border-border bg-muted/40 p-4 text-sm text-muted-foreground">
            <RefreshCcw className="mt-0.5 size-4 shrink-0 text-primary" />
            <p>
              Every cycle writes back: confirmed signatures, failed hypotheses and remediation
              outcomes become the next cycle's evidence.
            </p>
          </div>
        </section>

        <section>
          <SectionTitle
            eyebrow="03 · Trigger layer"
            title="What the agent watches, and what it ignores"
          />
          <Card className="overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[720px] border-collapse">
                <thead className="border-b border-border bg-muted/40">
                  <tr>
                    <Th>Feed</Th>
                    <Th>What the agent looks for</Th>
                    <Th>Trigger it raises</Th>
                  </tr>
                </thead>
                <tbody>
                  {TRIGGERS.map((r) => (
                    <tr key={r.feed} className="border-b border-border/70 last:border-0">
                      <Td>
                        <span className="font-semibold">{r.feed}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.looks}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.trigger}</span>
                      </Td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
          <p className="mt-3 text-sm text-muted-foreground">
            Noise control is a first-class requirement: correlated events are collapsed into one
            incident, and suppression windows prevent alert storms from becoming action storms.
          </p>
        </section>

        <section>
          <SectionTitle
            eyebrow="04 · Behavioural signals"
            title="CSP signals feed the Context Graph"
          />
          <p className="mb-4 max-w-3xl text-sm text-muted-foreground">
            Telemetry shows what the platform did. CSP signals show what the customer experienced —
            joined on account and user, both land in the same graph the agent reasons over.
          </p>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            {[
              {
                n: "1 · Capture",
                body: "CSP events, guide interactions, search queries and page paths, tagged per feature and per account.",
              },
              {
                n: "2 · Resolve identity",
                body: "CSP visitor and account IDs are mapped to the customer, account and entitlement nodes already in the graph.",
              },
              {
                n: "3 · Derive signals",
                body: "Raw clickstream becomes named signals: rage clicks, repeat attempts, funnel drop-off, failed search, guide dismissal.",
              },
              {
                n: "4 · Serve to agents",
                body: "Signals sit beside telemetry, config changes and case history, so one query returns the full customer context.",
              },
            ].map((s) => (
              <Card key={s.n}>
                <CardContent className="p-5">
                  <p className="font-display text-sm font-bold text-primary">{s.n}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{s.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
          <Card className="mt-4">
            <CardContent className="p-5">
              <p className="text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                Where it lands in the graph
              </p>
              <p className="mt-2 font-display text-sm font-semibold">
                Customer → Account → User → Product → Feature → Behavioural signal → Case →
                Resolution
              </p>
              <p className="mt-2 text-sm text-muted-foreground">
                Correlating signals before each case creates the training labels for prediction,
                with consent and suppression rules enforced at the graph, not per use case.
              </p>
            </CardContent>
          </Card>

          <div className="mt-6">
            <Card className="overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[820px] border-collapse">
                  <thead className="border-b border-border bg-muted/40">
                    <tr>
                      <Th>CSP data</Th>
                      <Th>What it contains</Th>
                      <Th>Freshness</Th>
                      <Th>How the agent uses it</Th>
                    </tr>
                  </thead>
                  <tbody>
                    {CSP_DATA.map((r) => (
                      <tr key={r.data} className="border-b border-border/70 last:border-0">
                        <Td>
                          <span className="font-semibold">{r.data}</span>
                        </Td>
                        <Td>
                          <span className="text-muted-foreground">{r.contains}</span>
                        </Td>
                        <Td>
                          <span className="text-muted-foreground">{r.freshness}</span>
                        </Td>
                        <Td>
                          <span className="text-muted-foreground">{r.use}</span>
                        </Td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
            <div className="mt-4 grid gap-4 sm:grid-cols-3">
              {[
                {
                  t: "Join key",
                  b: "CSP account ID to CRM account ID, visitor ID to user record. Coverage today: [% MAPPED].",
                },
                {
                  t: "Prerequisite",
                  b: "Feature tagging on the journeys we want to predict — untagged screens are blind spots, owner [TAGGING OWNER].",
                },
                {
                  t: "Privacy",
                  b: "No payload or credential capture; consent and communication-suppression rules applied before any proactive outreach.",
                },
              ].map((c) => (
                <Card key={c.t}>
                  <CardContent className="p-5">
                    <p className="font-display text-sm font-bold">{c.t}</p>
                    <p className="mt-2 text-sm text-muted-foreground">{c.b}</p>
                  </CardContent>
                </Card>
              ))}
            </div>

            <div className="mt-6">
              <p className="mb-3 font-display text-base font-semibold">
                How Pendo can expose customer pain points
              </p>
              <Card className="overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[820px] border-collapse">
                    <thead className="border-b border-border bg-muted/40">
                      <tr>
                        <Th>Pendo signal</Th>
                        <Th>What it can tell you</Th>
                        <Th>Example customer pain point</Th>
                      </tr>
                    </thead>
                    <tbody>
                      {PENDO_PAIN.map((r) => (
                        <tr key={r.signal} className="border-b border-border/70 last:border-0">
                          <Td>
                            <span className="font-semibold">{r.signal}</span>
                          </Td>
                          <Td>
                            <span className="text-muted-foreground">{r.tells}</span>
                          </Td>
                          <Td>
                            <span className="text-muted-foreground">{r.pain}</span>
                          </Td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </Card>
            </div>
          </div>
        </section>

        <section>
          <SectionTitle eyebrow="05 · Flow" title="The flow, end to end" />
          <p className="mb-4 max-w-3xl text-sm text-muted-foreground">
            Two inputs, one scoring layer, three outcomes, one set of business results — closed by a
            learning loop.
          </p>
          <div className="space-y-3">
            <div className="grid gap-3 sm:grid-cols-2">
              {[
                { t: "CSP behavioural data", b: "Clicks, funnels, guides, search" },
                { t: "Platform telemetry & cases", b: "Logs, config changes, case history" },
              ].map((c) => (
                <Card key={c.t}>
                  <CardContent className="p-5">
                    <p className="font-display text-sm font-bold">{c.t}</p>
                    <p className="mt-1 text-sm text-muted-foreground">{c.b}</p>
                  </CardContent>
                </Card>
              ))}
            </div>
            <Card className="border-primary/40">
              <CardContent className="p-5">
                <p className="font-display text-sm font-bold text-primary">Context Graph</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Customer → Account → User → Product → Feature → Signal → Case
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-5">
                <p className="font-display text-sm font-bold">Prediction & agent decisioning</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Signal correlation, case-likelihood score, reason-code mapping
                </p>
              </CardContent>
            </Card>
            <div className="grid gap-3 sm:grid-cols-3">
              {[
                { t: "In-app guidance", b: "Fix it before a case opens" },
                { t: "Proactive outreach", b: "Targeted, suppression-aware" },
                { t: "Prefilled triaged case", b: "Context attached for the engineer" },
              ].map((c) => (
                <Card key={c.t}>
                  <CardContent className="p-5">
                    <p className="font-display text-sm font-bold">{c.t}</p>
                    <p className="mt-1 text-sm text-muted-foreground">{c.b}</p>
                  </CardContent>
                </Card>
              ))}
            </div>
            <div className="flex items-start gap-3 rounded-2xl border border-border bg-muted/40 p-4 text-sm text-muted-foreground">
              <Activity className="mt-0.5 size-4 shrink-0 text-primary" />
              <p>
                Business outcomes: cases avoided · lower handle time · faster time to resolution ·
                higher adoption & CSAT · support cost per account down.
              </p>
            </div>
          </div>
        </section>

        <section>
          <SectionTitle
            eyebrow="06 · Signal → reason code"
            title="Each signal maps to the case it predicts"
          />
          <Card className="overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[860px] border-collapse">
                <thead className="border-b border-border bg-muted/40">
                  <tr>
                    <Th>CSP signal</Th>
                    <Th>What it indicates</Th>
                    <Th>Case reason code</Th>
                    <Th>Preventive action</Th>
                  </tr>
                </thead>
                <tbody>
                  {REASON_CODES.map((r) => (
                    <tr key={r.signal} className="border-b border-border/70 last:border-0">
                      <Td>
                        <span className="font-semibold">{r.signal}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.means}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.code}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.action}</span>
                      </Td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
          <p className="mt-3 text-sm text-muted-foreground">
            Reason codes must be confirmed against the live case taxonomy — [TAXONOMY OWNER] — and
            each mapping runs in shadow mode until its precision clears [THRESHOLD].
          </p>
        </section>

        <section>
          <SectionTitle
            eyebrow="07 · Diagnosis"
            title="Diagnosis with evidence, not a guess with confidence"
          />
          <div className="grid gap-4 sm:grid-cols-3">
            {[
              {
                t: "Hypothesis generation",
                b: "Failure signatures, change timeline and similar resolved cases produce a ranked short list — not a single answer.",
              },
              {
                t: "Evidence assembly",
                b: "Each hypothesis carries the log lines, metrics windows and config diffs that support or contradict it.",
              },
              {
                t: "Confidence gating",
                b: "Above threshold the agent acts. Below it, the agent triages and escalates with its reasoning attached.",
              },
            ].map((c) => (
              <Card key={c.t}>
                <CardContent className="p-5">
                  <p className="font-display text-sm font-bold">{c.t}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{c.b}</p>
                </CardContent>
              </Card>
            ))}
          </div>
          <p className="mt-3 text-sm text-muted-foreground">
            An engineer can accept, correct or reject the diagnosis in seconds because the evidence
            is already in front of them. Corrections retrain the signature library.
          </p>
        </section>

        <section>
          <SectionTitle
            eyebrow="08 · Action tiers"
            title="Three tiers of action, one rule: reversible and verified"
          />
          <Card className="overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[760px] border-collapse">
                <thead className="border-b border-border bg-muted/40">
                  <tr>
                    <Th>Tier</Th>
                    <Th>Example actions</Th>
                    <Th>Control</Th>
                  </tr>
                </thead>
                <tbody>
                  {TIERS.map((r) => (
                    <tr key={r.tier} className="border-b border-border/70 last:border-0">
                      <Td>
                        <span className="font-semibold">{r.tier}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.examples}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.control}</span>
                      </Td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            <Card>
              <CardContent className="p-5">
                <p className="font-display text-sm font-bold">Verification is part of the fix</p>
                <p className="mt-2 text-sm text-muted-foreground">
                  An action is only complete when the original signal returns to normal. If it does
                  not, the agent rolls back and escalates.
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-5">
                <p className="font-display text-sm font-bold">Full audit trail</p>
                <p className="mt-2 text-sm text-muted-foreground">
                  Every action logged with trigger, hypothesis, evidence, actor, result and rollback
                  state.
                </p>
              </CardContent>
            </Card>
          </div>
        </section>

        <section>
          <SectionTitle
            eyebrow="09 · Escalation packet"
            title="What a human engineer receives instead of a raw ticket"
          />
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {ESCALATION.map((c) => (
              <Card key={c.title}>
                <CardContent className="p-5">
                  <p className="font-display text-sm font-bold">{c.title}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{c.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        <section>
          <SectionTitle
            eyebrow="10 · Guardrails"
            title="Autonomy earned tier by tier, never granted upfront"
          />
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {GUARDRAILS.map((c) => (
              <Card key={c.title}>
                <CardContent className="p-5">
                  <div className="flex items-center gap-2">
                    <ShieldCheck className="size-4 text-primary" />
                    <p className="font-display text-sm font-bold">{c.title}</p>
                  </div>
                  <p className="mt-2 text-sm text-muted-foreground">{c.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        <section>
          <SectionTitle
            eyebrow="11 · Measurement"
            title="Detection, diagnosis, action and safety — measured separately"
          />
          <Card className="overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[620px] border-collapse">
                <thead className="border-b border-border bg-muted/40">
                  <tr>
                    <Th>Metric</Th>
                    <Th>Baseline</Th>
                    <Th>Phase 1 target</Th>
                  </tr>
                </thead>
                <tbody>
                  {METRICS.map((r) => (
                    <tr key={r.metric} className="border-b border-border/70 last:border-0">
                      <Td>
                        <span className="font-semibold">{r.metric}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.baseline}</span>
                      </Td>
                      <Td>
                        <span className="text-muted-foreground">{r.target}</span>
                      </Td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
          <p className="mt-3 text-sm text-muted-foreground">
            Safety metrics are reported alongside efficiency metrics in the same review — a coverage
            gain bought with rollbacks is not a gain.
          </p>
        </section>

        <section>
          <SectionTitle eyebrow="12 · Roadmap" title="Watch → Diagnose → Heal" />
          <p className="mb-4 max-w-3xl text-sm text-muted-foreground">
            Each phase is gated on the previous phase's measured precision, not on a calendar date.
          </p>
          <div className="grid gap-4 sm:grid-cols-3">
            {PHASES.map((p) => (
              <Card key={p.phase}>
                <CardContent className="p-5">
                  <p className="font-display text-sm font-bold text-primary">{p.phase}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{p.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
          <p className="mt-3 text-sm text-muted-foreground">
            Prerequisites: feed access and retention, a runbook owner per component, and baselines
            captured before Phase 1 ships — [OWNERS] · [DATE].
          </p>
        </section>

        <section>
          <SectionTitle eyebrow="13 · The ask" title="Three decisions we need today" />
          <div className="grid gap-4 sm:grid-cols-3">
            {ASKS.map((a) => (
              <Card key={a.n}>
                <CardContent className="p-5">
                  <span className="inline-flex size-8 items-center justify-center rounded-full bg-primary/10 font-display text-sm font-bold text-primary">
                    {a.n}
                  </span>
                  <p className="mt-3 font-display text-base font-bold">{a.title}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{a.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
          <Card className="mt-4 border-primary/40">
            <CardContent className="p-6">
              <p className="font-display text-base font-semibold sm:text-lg">
                The goal is not fewer engineers. It is engineers who start at the diagnosis instead
                of the ticket — and issues resolved before the customer notices them.
              </p>
            </CardContent>
          </Card>
        </section>
      </div>
      )}
    </SynapseShell>
  );
}

```

## `src/routes/synapse.architecture.tsx`

```tsx
import { createFileRoute } from "@tanstack/react-router";
import { SynapseShell } from "@/components/synapse-shell";
import { Card, CardContent } from "@/components/ui/card";
import { CAPABILITIES, type Pattern } from "@/components/capability-classifier";
import { Badge } from "@/components/ui/badge";
import {
  ArrowDown,
  Boxes,
  Brain,
  Database,
  Eye,
  GraduationCap,
  Inbox,
  ListChecks,
  Radio,
  ShieldCheck,
  Wrench,
} from "lucide-react";

export const Route = createFileRoute("/synapse/architecture")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "Architecture Diagram — CSP AI Capabilities Platform" },
      {
        name: "description",
        content:
          "End-to-end CSP AI Capabilities architecture: behavioral and operational sources, ingestion, signal store, reasoning agents, runbook execution and feedback loops.",
      },
      { property: "og:title", content: "CSP AI Capabilities Architecture Diagram" },
      {
        property: "og:description",
        content:
          "How signals flow from Pendo and platform telemetry through ingestion, reasoning and automated mitigation in CSP AI Capabilities.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: ArchitecturePage,
});

const LAYERS = [
  {
    icon: Radio,
    name: "Sources",
    tag: "Layer 1",
    blurb: "Behavioral and operational telemetry captured where the customer works.",
    nodes: [
      "Pendo click & page events",
      "Feature usage / adoption",
      "Guides & in-app messaging",
      "Search & feedback (NPS)",
      "Visitor & account metadata",
      "Support case system",
      "API & platform logs",
    ],
  },
  {
    icon: Inbox,
    name: "Ingestion",
    tag: "Layer 2",
    blurb:
      "Webhook alerts and scheduled pulls, normalised, thresholded and de-duplicated before anything is created.",
    nodes: [
      "Pendo alert webhook (/api/public/pendo-alert)",
      "Shared-secret verification",
      "Scheduled Pendo API pull",
      "Threshold + quiet-window filter",
      "Duplicate suppression",
    ],
  },
  {
    icon: Database,
    name: "Signal store",
    tag: "Layer 3",
    blurb: "Every signal is persisted with source, customer, severity, reason code and theme.",
    nodes: [
      "synapse_signals",
      "synapse_cases",
      "synapse_runbooks",
      "synapse_runs",
      "pendo_settings",
    ],
  },
  {
    icon: Brain,
    name: "Reasoning",
    tag: "Layer 4",
    blurb:
      "Correlation across sessions, accounts and systems produces a diagnosed cause and a recommended action tier.",
    nodes: [
      "Theme classification",
      "Cross-system correlation",
      "Severity & blast radius",
      "Root-cause hypothesis",
      "Action tier selection",
    ],
  },
  {
    icon: Wrench,
    name: "Action",
    tag: "Layer 5",
    blurb: "Runbooks execute at the allowed autonomy tier — observe, suggest, or auto-resolve.",
    nodes: [
      "Runbook execution",
      "Case creation / enrichment",
      "In-app guidance",
      "Escalation packet",
      "Rollback & audit log",
    ],
  },
  {
    icon: ShieldCheck,
    name: "Feedback & governance",
    tag: "Layer 6",
    blurb: "Outcomes feed the metrics dashboard and tune thresholds, tiers and guardrails.",
    nodes: [
      "Resolution tracking",
      "Close-rate & MTTR metrics",
      "Impact forecast",
      "Guardrails & approvals",
      "Threshold tuning",
    ],
  },
];

const PILLARS = [
  {
    icon: Eye,
    name: "Perception & Understanding",
    role: "The context layer",
    step: "Intent & entity recognition",
    memory: "Contextual memory",
    detail:
      "Reads Pendo behaviour, CSP page and API events, case history, entitlements and account state to work out what the admin was trying to do.",
  },
  {
    icon: ListChecks,
    name: "Reasoning & Planning",
    role: "Core intelligence",
    step: "Goal decomposition",
    memory: "Dynamic pathfinding",
    detail:
      "Breaks 'renew this licence' or 'fix this failed activation' into checks, tests competing causes, and re-plans when a CSP system answers differently than expected.",
  },
  {
    icon: Wrench,
    name: "Execution & Action",
    role: "The tool-use engine",
    step: "CSP API toolkit",
    memory: "The action",
    detail:
      "Calls scoped CSP admin, licensing, asset and case APIs to run the fix, then re-runs the admin's original action to prove it worked.",
  },
  {
    icon: GraduationCap,
    name: "Continuous Learning & Governance",
    role: "Self-improving and safeguarded",
    step: "Built-in guardrails",
    memory: "Feedback loops",
    detail:
      "Every run is logged, reversible and scored. Autonomy is earned per runbook; irreversible actions (decommission, FedRAMP, impersonation) stay human-approved.",
  },
];

const PILLAR_OF_PATTERN: Record<Pattern, string> = {
  "Contextual Insights": "Perception & Understanding",
  "Predictive Optimization": "Reasoning & Planning",
  "Autonomous Orchestration": "Execution & Action",
  "Closed-Loop Remediation": "Execution & Action",
  "Adaptive Governance": "Continuous Learning & Governance",
};

const PILLAR_META = [
  {
    icon: Eye,
    pillar: "Perception & Understanding",
    summary:
      "CSP capabilities that assemble account, asset and usage context before anything is decided.",
  },
  {
    icon: ListChecks,
    pillar: "Reasoning & Planning",
    summary: "Capabilities that forecast, prioritise and pick the route before anything executes.",
  },
  {
    icon: Wrench,
    pillar: "Execution & Action",
    summary: "Capabilities the agent runs through scoped CSP APIs, including self-healing retries.",
  },
  {
    icon: GraduationCap,
    pillar: "Continuous Learning & Governance",
    summary: "Capabilities where access, ownership or risk keeps a human accountable.",
  },
];

const PILLAR_CAPABILITIES = PILLAR_META.map((meta) => ({
  ...meta,
  items: CAPABILITIES.filter((c) => PILLAR_OF_PATTERN[c.pattern] === meta.pillar).map(
    (c) => c.name,
  ),
}));

const INTERFACES = [
  {
    name: "Pendo → CSP AI Capabilities",
    detail:
      "Pendo alerts POST to the public endpoint with a shared secret; metric thresholds decide whether a signal is created.",
  },
  {
    name: "CSP AI Capabilities → Case system",
    detail:
      "Diagnosed signals become cases with evidence, customer, severity and reason code already attached.",
  },
  {
    name: "CSP AI Capabilities → Admin APIs",
    detail:
      "Tier-3 runbooks call scoped admin APIs for reversible fixes; every action is verified and logged.",
  },
  {
    name: "CSP AI Capabilities → Product & IT",
    detail:
      "Silent-friction and adoption trends roll up to product owners and IT operations for prioritisation.",
  },
];

function ArchitecturePage() {
  return (
    <SynapseShell
      title="Architecture diagram"
      subtitle="How a customer behaviour event travels from capture through reasoning to an automated or human resolution."
    >
      <div className="space-y-8">
        <Card>
          <CardContent className="p-5">
            <div className="mb-4">
              <p className="font-display text-lg font-bold">
                The four core pillars driving agentic AI in CSP
              </p>
              <p className="text-xs text-muted-foreground">
                What every CSP agent needs before it can act on an admin's behalf.
              </p>
            </div>
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              {PILLARS.map((p) => (
                <div
                  key={p.name}
                  className="flex flex-col gap-3 rounded-xl border border-border bg-muted/30 p-4"
                >
                  <span className="grid size-10 place-items-center rounded-xl bg-primary/10 text-primary">
                    <p.icon className="size-5" />
                  </span>
                  <div>
                    <p className="font-display text-base font-bold leading-tight">{p.name}</p>
                    <p className="text-xs text-muted-foreground">{p.role}</p>
                  </div>
                  <div className="flex flex-wrap gap-1.5">
                    <Badge variant="secondary">{p.step}</Badge>
                    <Badge variant="outline">{p.memory}</Badge>
                  </div>
                  <p className="text-xs text-muted-foreground">{p.detail}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <div className="space-y-4">
          {LAYERS.map((layer, index) => (
            <div key={layer.name} className="space-y-4">
              <Card className="overflow-hidden">
                <CardContent className="flex flex-col gap-4 p-5 lg:flex-row lg:items-start">
                  <div className="flex min-w-[240px] items-start gap-3">
                    <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                      <layer.icon className="size-5" />
                    </span>
                    <div>
                      <Badge variant="secondary" className="mb-1">
                        {layer.tag}
                      </Badge>
                      <p className="font-display text-lg font-bold leading-tight">{layer.name}</p>
                      <p className="mt-1 text-xs text-muted-foreground">{layer.blurb}</p>
                    </div>
                  </div>
                  <div className="flex flex-1 flex-wrap gap-2">
                    {layer.nodes.map((node) => (
                      <span
                        key={node}
                        className="rounded-lg border border-border bg-muted/40 px-3 py-2 text-xs font-semibold text-foreground"
                      >
                        {node}
                      </span>
                    ))}
                  </div>
                </CardContent>
              </Card>
              {index < LAYERS.length - 1 ? (
                <div className="flex justify-center text-muted-foreground">
                  <ArrowDown className="size-5" />
                </div>
              ) : null}
            </div>
          ))}
        </div>

        <Card>
          <CardContent className="p-5">
            <div className="mb-4">
              <p className="font-display text-lg font-bold">
                CSP agentic AI capabilities mapped to the four pillars
              </p>
              <p className="text-xs text-muted-foreground">
                Where every capability on the platform sits in the pillar model.
              </p>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              {PILLAR_CAPABILITIES.map((group) => (
                <div key={group.pillar} className="rounded-xl border border-border p-4">
                  <div className="flex items-start gap-3">
                    <span className="grid size-9 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                      <group.icon className="size-4" />
                    </span>
                    <div>
                      <p className="font-display text-base font-bold leading-tight">
                        {group.pillar}
                      </p>
                      <p className="text-xs text-muted-foreground">{group.summary}</p>
                    </div>
                    <Badge variant="secondary" className="ml-auto">
                      {group.items.length}
                    </Badge>
                  </div>
                  <div className="mt-3 flex flex-wrap gap-1.5">
                    {group.items.map((item) => (
                      <span
                        key={item}
                        className="rounded-lg border border-border bg-muted/40 px-2.5 py-1 text-xs font-semibold"
                      >
                        {item}
                      </span>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-5">
            <div className="mb-4 flex items-center gap-2">
              <Boxes className="size-4 text-primary" />
              <p className="font-display text-lg font-bold">Key interfaces</p>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              {INTERFACES.map((item) => (
                <div key={item.name} className="rounded-xl border border-border p-4">
                  <p className="text-sm font-bold">{item.name}</p>
                  <p className="mt-1 text-xs text-muted-foreground">{item.detail}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </SynapseShell>
  );
}

```

## `src/routes/synapse.capability-impact.tsx`

```tsx
import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { SynapseShell } from "@/components/synapse-shell";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { CheckCircle2, Radar, Search, ShieldCheck, Wrench } from "lucide-react";

export const Route = createFileRoute("/synapse/capability-impact")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "Business Impact by Agentic Capability — CSP Agentic AI" },
      {
        name: "description",
        content:
          "What each CSP Agentic AI capability delivers: deflection, MTTR reduction, autonomous resolution and product friction visibility, with owners and proof points.",
      },
      { property: "og:title", content: "Business Impact by Agentic Capability" },
      {
        property: "og:description",
        content:
          "Capability-by-capability view of business value, measured outcomes, who benefits and how each result is proven.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: CapabilityImpactPage,
});

const CAPABILITIES = [
  {
    icon: Radar,
    name: "Proactive Interception",
    phase: "Phase 1",
    summary:
      "Catches dead clicks, API drops and database desyncs the moment they happen, before the customer opens a case.",
    metrics: [
      { value: "25%", label: "Escalation deflection", tone: "positive" },
      { value: "6.62%", label: "Current friction rate addressed", tone: "plain" },
      { value: "0", label: "Customer reports required", tone: "plain" },
    ],
    beneficiaries: ["Customer", "Support"],
    outcomes: [
      "Users see an inline explanation instead of a dead end at the error.",
      "Administrative case volume drops as friction is resolved pre-case.",
      "High-friction pages get a measured before/after abandonment rate.",
    ],
    proof: "Compare case creation rate on intercepted journeys against the pre-launch baseline.",
  },
  {
    icon: Search,
    name: "Cross-System Root Cause",
    phase: "Phase 1",
    summary:
      "Correlates frontend logs, API errors and identity/role mismatches into one diagnosed incident per account and session.",
    metrics: [
      { value: "40%", label: "MTTR reduction", tone: "positive" },
      { value: ">95%", label: "RCA accuracy gate", tone: "plain" },
      { value: "1", label: "Incident per correlated failure", tone: "plain" },
    ],
    beneficiaries: ["CSR / TAC", "Support leadership"],
    outcomes: [
      "CSRs stop re-diagnosing the same role and sync mapping issues by hand.",
      "Tickets arrive with evidence, severity and blast radius already attached.",
      "Duplicate cases for one underlying failure collapse into a single incident.",
    ],
    proof: "Audit agent diagnoses against CSR-written root causes on the same tickets.",
  },
  {
    icon: Wrench,
    name: "Background Self-Resolution",
    phase: "Phase 2",
    summary:
      "Executes scoped, reversible fixes against admin APIs for the top system sync failures — verified, logged and rollback-ready.",
    metrics: [
      { value: "<5s", label: "Time to resolve", tone: "positive" },
      { value: "Top 3", label: "Sync failures self-healed", tone: "plain" },
      { value: "100%", label: "Actions reversible & logged", tone: "plain" },
    ],
    beneficiaries: ["Customer", "Support", "IT operations"],
    outcomes: [
      "Zero-ticket workflows: the user is unblocked before they can raise a case.",
      "Support queue no longer absorbs known, mechanical fixes.",
      "Every repair carries trigger, hypothesis, evidence, actor, result and rollback state.",
    ],
    proof: "Track auto-resolved volume, verification pass rate and rollback frequency per runbook.",
  },
  {
    icon: ShieldCheck,
    name: "Silent Friction Reporting",
    phase: "Phase 1",
    summary:
      "Surfaces unseen bugs and drop-offs to Product and R&D in daily and weekly automated friction reports.",
    metrics: [
      { value: "100%", label: "Friction visibility", tone: "positive" },
      { value: "Weekly", label: "Report cadence to R&D", tone: "plain" },
      { value: "0", label: "Manual reporting effort", tone: "plain" },
    ],
    beneficiaries: ["Product", "R&D", "Engineering leadership"],
    outcomes: [
      "Defects that never became cases finally reach a backlog with evidence.",
      "Roadmap prioritisation grounded in observed friction, not anecdote.",
      "Fix effectiveness measurable as friction rates fall report over report.",
    ],
    proof: "Count previously unreported defects opened from friction reports and their fix rate.",
  },
] as const;

const ROLLUP = [
  { value: "40%", label: "MTTR reduction" },
  { value: "<5s", label: "Autonomous resolution" },
  { value: "25%", label: "Escalation deflection" },
  { value: "100%", label: "Friction visibility" },
] as const;

const AI_IDEAS_ASSESSMENT: {
  idea: string;
  problem: string;
  capability: string;
  value: string;
  suitability: string;
  complexity: string;
  ttv: string;
  priority: string;
}[] = [
  {
    idea: "AI Search + Welcome Center",
    problem: "First-login confusion, activation issues, KB navigation failures",
    capability: "RAG, semantic search, conversational AI, workflow assistance",
    value: "High — 15–25% structural deflection target",
    suitability: "High",
    complexity: "Medium",
    ttv: "Near-term",
    priority: "P0",
  },
  {
    idea: "Ask CSP",
    problem: "Customers cannot find account, asset, entitlement, or case information",
    capability: "Account-aware conversational AI, RAG, confidence gating",
    value: "High — 15–25% target deflection",
    suitability: "High",
    complexity: "High",
    ttv: "Mid-term",
    priority: "P0",
  },
  {
    idea: "Contextual In-App Guidance",
    problem: "Users encounter errors without knowing what happened or what to do next",
    capability: "Context-aware guidance, event-triggered assistance",
    value: "High — reduces abandonment and escalations",
    suitability: "High",
    complexity: "Low–Medium",
    ttv: "Near-term",
    priority: "P0",
  },
  {
    idea: "Next-Best Article",
    problem: "Customers must search for relevant documentation",
    capability: "Semantic retrieval, contextual recommendation",
    value: "High — improves self-service",
    suitability: "High",
    complexity: "Low–Medium",
    ttv: "Near-term",
    priority: "P0",
  },
  {
    idea: "Dynamic Article Summaries",
    problem: "Users must read multiple documents to find an answer",
    capability: "Grounded summarization",
    value: "Medium–High",
    suitability: "High",
    complexity: "Low–Medium",
    ttv: "Near-term",
    priority: "P0",
  },
  {
    idea: "Direct KB Deep Links",
    problem: "Users land at the top of long articles and abandon the journey",
    capability: "Semantic retrieval and passage-level linking",
    value: "Medium",
    suitability: "Medium",
    complexity: "Low",
    ttv: "Near-term",
    priority: "P1",
  },
  {
    idea: "Visual Error Diagnosis",
    problem: "Customers struggle to describe screenshots, error banners, or failed screens",
    capability: "Multimodal AI, telemetry correlation, root-cause explanation",
    value: "High",
    suitability: "High",
    complexity: "High",
    ttv: "Mid-term",
    priority: "P1",
  },
  {
    idea: "Zero-Ticket Self-Resolution",
    problem: "Known issues require unnecessary case creation",
    capability: "Agentic workflows, runbook execution, reversible automation",
    value: "Very High",
    suitability: "High",
    complexity: "High",
    ttv: "Long-term",
    priority: "P1",
  },
  {
    idea: "Live API Lookups",
    problem: "Generic documentation does not answer account-specific questions",
    capability: "Tool-using AI, live API retrieval, account grounding",
    value: "High",
    suitability: "High",
    complexity: "High",
    ttv: "Mid-term",
    priority: "P0",
  },
  {
    idea: "Proactive Diagnostics",
    problem: "Customers discover misconfigurations, entitlement gaps, or sync failures too late",
    capability: "Predictive diagnostics, health checks, remediation recommendations",
    value: "Very High",
    suitability: "High",
    complexity: "High",
    ttv: "Long-term",
    priority: "P1",
  },
  {
    idea: "1-Click Execution Triggers",
    problem: "Approved actions still require manual configuration and execution",
    capability: "Human-in-the-loop agentic automation",
    value: "High",
    suitability: "High",
    complexity: "Medium–High",
    ttv: "Mid-term",
    priority: "P1",
  },
];

type IdeaRow = (typeof AI_IDEAS_ASSESSMENT)[number];
type FilterColumn = "value" | "suitability" | "complexity" | "ttv" | "priority";
type Filters = Record<FilterColumn, string>;

const EMPTY_FILTERS: Filters = {
  value: "",
  suitability: "",
  complexity: "",
  ttv: "",
  priority: "",
};

function FilterSelect({
  column,
  filters,
  setFilters,
}: {
  column: FilterColumn;
  filters: Filters;
  setFilters: React.Dispatch<React.SetStateAction<Filters>>;
}) {
  const options = Array.from(new Set(AI_IDEAS_ASSESSMENT.map((r) => r[column]))).sort();
  return (
    <select
      aria-label={`Filter by ${column}`}
      value={filters[column]}
      onChange={(e) => setFilters((f) => ({ ...f, [column]: e.target.value }))}
      className="mt-1 block w-full rounded-md border border-border bg-background px-2 py-1 text-xs font-normal text-foreground"
    >
      <option value="">All</option>
      {options.map((o) => (
        <option key={o} value={o}>
          {o}
        </option>
      ))}
    </select>
  );
}

function CapabilityImpactPage() {
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS);
  const visibleIdeas: IdeaRow[] = AI_IDEAS_ASSESSMENT.filter((row) =>
    (Object.keys(EMPTY_FILTERS) as FilterColumn[]).every(
      (key) => !filters[key] || row[key] === filters[key],
    ),
  );

  return (
    <SynapseShell
      title="Business Impact by Capability"
      subtitle="What each agentic capability is expected to deliver, who benefits, and how the result gets proven."
      actions={
        <Badge variant="secondary" className="uppercase tracking-[0.14em]">
          CSP Agentic AI
        </Badge>
      }
    >
      <div className="space-y-6">
        <Card>
          <CardContent className="p-0">
            <div className="border-b border-border px-6 py-4">
              <h2 className="font-display text-lg font-bold">AI Ideas Assessment</h2>
              <p className="mt-1 text-sm text-muted-foreground">
                A. Customer Self-Service &amp; Guidance
              </p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="bg-muted/50 text-left">
                    <th className="px-3 py-3 font-semibold">#</th>
                    <th className="px-3 py-3 font-semibold">AI idea</th>
                    <th className="px-3 py-3 font-semibold">Problem addressed</th>
                    <th className="px-3 py-3 font-semibold">AI capability</th>
                    <th className="px-3 py-3 font-semibold">
                      Business value
                      <FilterSelect column="value" filters={filters} setFilters={setFilters} />
                    </th>
                    <th className="px-3 py-3 font-semibold">
                      AI suitability
                      <FilterSelect column="suitability" filters={filters} setFilters={setFilters} />
                    </th>
                    <th className="px-3 py-3 font-semibold">
                      Complexity
                      <FilterSelect column="complexity" filters={filters} setFilters={setFilters} />
                    </th>
                    <th className="px-3 py-3 font-semibold">
                      Time to value
                      <FilterSelect column="ttv" filters={filters} setFilters={setFilters} />
                    </th>
                    <th className="px-3 py-3 font-semibold">
                      Priority
                      <FilterSelect column="priority" filters={filters} setFilters={setFilters} />
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {visibleIdeas.length === 0 ? (
                    <tr className="border-t border-border">
                      <td colSpan={9} className="px-3 py-6 text-center text-muted-foreground">
                        No ideas match the selected filters.
                      </td>
                    </tr>
                  ) : null}
                  {visibleIdeas.map((row, i) => (
                    <tr key={row.idea} className="border-t border-border align-top">
                      <td className="px-3 py-3 text-muted-foreground">{i + 1}</td>
                      <td className="px-3 py-3 font-semibold">{row.idea}</td>
                      <td className="px-3 py-3 text-muted-foreground">{row.problem}</td>
                      <td className="px-3 py-3 text-muted-foreground">{row.capability}</td>
                      <td className="px-3 py-3 font-semibold">{row.value}</td>
                      <td className="px-3 py-3 text-muted-foreground">{row.suitability}</td>
                      <td className="px-3 py-3 text-muted-foreground">{row.complexity}</td>
                      <td className="px-3 py-3 text-muted-foreground">{row.ttv}</td>
                      <td className="px-3 py-3">
                        <Badge variant={row.priority === "P0" ? "default" : "secondary"}>
                          {row.priority}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {ROLLUP.map((r) => (
            <Card key={r.label}>
              <CardContent className="pt-6">
                <p className="font-display text-4xl font-bold text-primary">{r.value}</p>
                <p className="mt-1 text-sm font-semibold">{r.label}</p>
              </CardContent>
            </Card>
          ))}
        </div>

        <div className="space-y-5">
          {CAPABILITIES.map((c) => (
            <Card key={c.name}>
              <CardContent className="pt-6">
                <div className="flex flex-wrap items-start justify-between gap-3 border-b border-border pb-4">
                  <div className="flex items-start gap-3">
                    <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                      <c.icon className="size-5" />
                    </span>
                    <div>
                      <h2 className="font-display text-lg font-bold">{c.name}</h2>
                      <p className="mt-1 max-w-2xl text-sm text-muted-foreground">{c.summary}</p>
                    </div>
                  </div>
                  <Badge variant="secondary">{c.phase}</Badge>
                </div>

                <div className="mt-4 grid gap-5 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)]">
                  <div className="space-y-3">
                    {c.metrics.map((m) => (
                      <div
                        key={m.label}
                        className="flex items-baseline gap-3 rounded-lg border border-border bg-muted/30 p-3"
                      >
                        <span
                          className={`font-display text-xl font-bold ${
                            m.tone === "positive" ? "text-primary" : ""
                          }`}
                        >
                          {m.value}
                        </span>
                        <span className="text-sm text-muted-foreground">{m.label}</span>
                      </div>
                    ))}
                    <div className="flex flex-wrap items-center gap-2 pt-1">
                      <span className="text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                        Benefits
                      </span>
                      {c.beneficiaries.map((b) => (
                        <Badge key={b} variant="outline">
                          {b}
                        </Badge>
                      ))}
                    </div>
                  </div>

                  <div>
                    <p className="text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                      Business outcomes
                    </p>
                    <ul className="mt-2 space-y-2 text-sm">
                      {c.outcomes.map((o) => (
                        <li key={o} className="flex gap-2 text-muted-foreground">
                          <CheckCircle2 className="mt-0.5 size-4 shrink-0 text-primary" />
                          <span>{o}</span>
                        </li>
                      ))}
                    </ul>
                    <div className="mt-3 rounded-lg bg-accent/40 p-3 text-sm">
                      <span className="font-semibold">How we prove it: </span>
                      <span className="text-muted-foreground">{c.proof}</span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </SynapseShell>
  );
}

```

## `src/routes/synapse.classifier.tsx`

```tsx
import { createFileRoute } from "@tanstack/react-router";
import { SynapseShell } from "@/components/synapse-shell";
import { CapabilityClassifier } from "@/components/capability-classifier";

export const Route = createFileRoute("/synapse/classifier")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "Capability Automation Classifier — CSP AI Capabilities" },
      {
        name: "description",
        content:
          "Every CSP portal capability mapped to its agentic pattern and autonomy level, with counts, distribution and filters.",
      },
      { property: "og:title", content: "Capability Automation Classifier — CSP AI Capabilities" },
      {
        property: "og:description",
        content: "Agentic pattern and autonomy level for every CSP portal capability.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: ClassifierPage,
});

function ClassifierPage() {
  return (
    <SynapseShell
      title="Capability Automation Classifier"
      subtitle="Every portal capability mapped to its agentic pattern and autonomy level — what runs autonomously today, what is agent assisted, what still needs a human, and what remains manual."
    >
      <CapabilityClassifier />
    </SynapseShell>
  );
}

```

## `src/routes/synapse.context-graph.tsx`

```tsx
import { useMemo, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { SynapseShell } from "@/components/synapse-shell";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { RotateCcw, ArrowRight, Building2, Users, Boxes, LifeBuoy, Layers } from "lucide-react";
import {
  CAPABILITIES,
  DOMAINS,
  LEVELS,
  PATTERNS,
  PATTERN_CLASS,
  LEVEL_CLASS,
  LEVEL_BAR,
  domainOf,
  subOf,
  type Capability,
  type Domain,
  type Level,
  type Pattern,
} from "@/components/capability-classifier";

export const Route = createFileRoute("/synapse/context-graph")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "Capabilities Map — CSP AI Capabilities" },
      {
        name: "description",
        content:
          "Every CSP capability laid out by domain, agentic pattern and how much of it runs without a human today.",
      },
      { property: "og:title", content: "Capabilities Map — CSP AI Capabilities" },
      {
        property: "og:description",
        content: "Domains, sub-groups, agentic patterns and autonomy across the full CSP capability map.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: ContextGraphPage,
});

const DOMAIN_ICON: Record<Domain, typeof Users> = {
  "Account Management": Building2,
  "User Management": Users,
  "Asset Management": Boxes,
  "Case Management": LifeBuoy,
  Others: Layers,
};

const DOMAIN_BLURB: Record<Domain, string> = {
  "Account Management": "Account structure, access model and identity configuration",
  "User Management": "User administration, roles, registration and approvals",
  "Asset Management": "Licences, onboarding, lifecycle, renewals and consumption",
  "Case Management": "Raising, working and closing support cases",
  Others: "Orders, partners, agreements, FedRAMP, docs and product features",
};

type View = "domain" | "pattern";

function ContextGraphPage() {
  const [view, setView] = useState<View>("domain");
  const [levels, setLevels] = useState<Level[]>([]);
  const [patterns, setPatterns] = useState<Pattern[]>([]);
  const [selected, setSelected] = useState<string | null>(null);

  const visible = useMemo(
    () =>
      CAPABILITIES.filter(
        (c) =>
          (levels.length === 0 || levels.includes(c.level)) &&
          (patterns.length === 0 || patterns.includes(c.pattern)),
      ),
    [levels, patterns],
  );

  const columns = useMemo(() => {
    if (view === "pattern") {
      return PATTERNS.map((p) => ({
        key: p as string,
        name: p as string,
        blurb: `${CAPABILITIES.filter((c) => c.pattern === p).length} capabilities across all domains`,
        icon: Layers,
        items: visible.filter((c) => c.pattern === p),
        groupBy: (c: Capability) => domainOf(c) as string,
      }));
    }
    return DOMAINS.map((d) => ({
      key: d as string,
      name: d as string,
      blurb: DOMAIN_BLURB[d],
      icon: DOMAIN_ICON[d],
      items: visible.filter((c) => domainOf(c) === d),
      groupBy: (c: Capability) => subOf(c) ?? "",
    }));
  }, [view, visible]);

  const active = selected ? CAPABILITIES.find((c) => c.name === selected) ?? null : null;
  const siblings = active
    ? CAPABILITIES.filter(
        (c) => c.name !== active.name && c.pattern === active.pattern && domainOf(c) === domainOf(active),
      )
    : [];

  const toggle = <T,>(list: T[], set: (v: T[]) => void, value: T) =>
    set(list.includes(value) ? list.filter((v) => v !== value) : [...list, value]);

  const totals = LEVELS.map((l) => ({
    level: l,
    count: visible.filter((c) => c.level === l).length,
  }));
  const autonomousShare = visible.length
    ? Math.round((visible.filter((c) => c.level === "Autonomous").length / visible.length) * 100)
    : 0;

  return (
    <SynapseShell
      title="Capabilities map"
      subtitle="Every capability in the CSP map, grouped by domain or by agentic pattern, coloured by how much of it runs without a human today."
    >
      <div className="space-y-6">
        <Card>
          <CardContent className="flex flex-col gap-4 p-5">
            <div className="flex flex-wrap items-center gap-2">
              <Button
                size="sm"
                variant={view === "domain" ? "default" : "outline"}
                onClick={() => setView("domain")}
              >
                By domain
              </Button>
              <Button
                size="sm"
                variant={view === "pattern" ? "default" : "outline"}
                onClick={() => setView("pattern")}
              >
                By agentic pattern
              </Button>
              <span className="ml-1 text-xs text-muted-foreground">
                {visible.length} of {CAPABILITIES.length} capabilities · {autonomousShare}% run without a
                human
              </span>
            </div>

            <div className="flex flex-wrap items-center gap-2 border-t border-border pt-4">
              {LEVELS.map((l) => (
                <button
                  key={l}
                  type="button"
                  onClick={() => toggle(levels, setLevels, l)}
                  className={`rounded-full px-3 py-1 text-xs font-semibold transition-colors ${
                    levels.includes(l)
                      ? LEVEL_CLASS[l]
                      : "bg-muted text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                  }`}
                >
                  {l}
                  <span className="ml-1.5 opacity-70">
                    {CAPABILITIES.filter((c) => c.level === l).length}
                  </span>
                </button>
              ))}
              <span className="mx-1 h-4 w-px bg-border" />
              {PATTERNS.map((p) => (
                <button
                  key={p}
                  type="button"
                  onClick={() => toggle(patterns, setPatterns, p)}
                  className={`rounded-full px-3 py-1 text-xs font-semibold transition-colors ${
                    patterns.includes(p)
                      ? PATTERN_CLASS[p]
                      : "bg-muted text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                  }`}
                >
                  {p}
                </button>
              ))}
              {(levels.length > 0 || patterns.length > 0 || selected) && (
                <Button
                  size="sm"
                  variant="ghost"
                  onClick={() => {
                    setLevels([]);
                    setPatterns([]);
                    setSelected(null);
                  }}
                  className="gap-1.5"
                >
                  <RotateCcw className="size-3.5" /> Reset
                </Button>
              )}
            </div>

            <div className="flex h-2 overflow-hidden rounded-full bg-muted">
              {totals.map((t) =>
                t.count ? (
                  <div
                    key={t.level}
                    className={LEVEL_BAR[t.level]}
                    style={{ width: `${(t.count / (visible.length || 1)) * 100}%` }}
                    title={`${t.level}: ${t.count}`}
                  />
                ) : null,
              )}
            </div>
          </CardContent>
        </Card>

        <div className="grid gap-4 lg:grid-cols-5">
          {columns.map((col) => {
            const groups = new Map<string, Capability[]>();
            for (const c of col.items) {
              const k = col.groupBy(c);
              groups.set(k, [...(groups.get(k) ?? []), c]);
            }
            return (
              <Card key={col.key} className="flex flex-col">
                <CardContent className="flex flex-1 flex-col gap-3 p-4">
                  <div className="flex items-start gap-2 border-b border-border pb-3">
                    <span className="grid size-8 shrink-0 place-items-center rounded-lg bg-primary/10 text-primary">
                      <col.icon className="size-4" />
                    </span>
                    <div>
                      <p className="font-display text-sm font-bold leading-tight">
                        {col.name}
                        <span className="ml-1.5 text-xs font-semibold text-muted-foreground">
                          {col.items.length}
                        </span>
                      </p>
                      <p className="text-[11px] text-muted-foreground">{col.blurb}</p>
                    </div>
                  </div>

                  {col.items.length === 0 ? (
                    <p className="py-6 text-center text-xs text-muted-foreground">
                      Nothing here with the current filters.
                    </p>
                  ) : (
                    [...groups.entries()].map(([groupName, items]) => (
                      <div key={groupName || "_"} className="space-y-2">
                        {groupName ? (
                          <p className="text-[10px] font-bold uppercase tracking-wide text-muted-foreground">
                            {groupName}
                          </p>
                        ) : null}
                        {items.map((c) => {
                          const isActive = selected === c.name;
                          return (
                            <button
                              key={c.name}
                              type="button"
                              onClick={() => setSelected(isActive ? null : c.name)}
                              className={`w-full rounded-xl border p-2.5 text-left transition-colors ${
                                isActive
                                  ? "border-primary bg-primary/5"
                                  : "border-border hover:bg-muted/50"
                              }`}
                            >
                              <p className="text-xs font-bold leading-snug">{c.name}</p>
                              <div className="mt-1.5 flex flex-wrap gap-1">
                                <span
                                  className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${LEVEL_CLASS[c.level]}`}
                                >
                                  {c.level}
                                </span>
                                {view === "domain" ? (
                                  <span
                                    className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${PATTERN_CLASS[c.pattern]}`}
                                  >
                                    {c.pattern}
                                  </span>
                                ) : null}
                              </div>
                            </button>
                          );
                        })}
                      </div>
                    ))
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>

        {active ? (
          <Card>
            <CardContent className="p-5">
              <div className="mb-3 flex flex-wrap items-center gap-2">
                <p className="font-display text-lg font-bold">{active.name}</p>
                <Badge variant="outline">{domainOf(active)}</Badge>
                {subOf(active) ? <Badge variant="outline">{subOf(active)}</Badge> : null}
                <span
                  className={`rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${LEVEL_CLASS[active.level]}`}
                >
                  {active.level}
                </span>
                <span
                  className={`rounded-full px-2.5 py-0.5 text-[11px] font-semibold ${PATTERN_CLASS[active.pattern]}`}
                >
                  {active.pattern}
                </span>
              </div>
              <p className="text-sm text-muted-foreground">{active.note}</p>
              <div className="mt-4 rounded-xl border border-border p-4">
                <p className="mb-2 text-xs font-bold uppercase tracking-wide text-muted-foreground">
                  Shares the same pattern in this domain
                </p>
                {siblings.length ? (
                  <ul className="space-y-1.5">
                    {siblings.map((s) => (
                      <li key={s.name} className="flex items-center gap-2 text-sm">
                        <ArrowRight className="size-3.5 text-primary" />
                        {s.name}
                        <span className="text-xs text-muted-foreground">· {s.level}</span>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-sm text-muted-foreground">
                    Stands alone — no other capability in this domain uses this pattern.
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        ) : (
          <p className="text-center text-xs text-muted-foreground">
            Click any capability to see why it sits at that autonomy level and what it groups with.
          </p>
        )}
      </div>
    </SynapseShell>
  );
}

```

## `src/routes/synapse.csp-agentic-ai.tsx`

```tsx
import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQueryClient, useSuspenseQuery, queryOptions } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { SynapseShell } from "@/components/synapse-shell";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { supabase } from "@/integrations/supabase/client";
import { LABEL, SEVERITIES } from "@/lib/synapse";
import { toast } from "sonner";
import {
  ArrowDown,
  BarChart3,
  CalendarCheck,
  CheckCircle2,
  Copy,
  Cpu,
  LineChart,
  Network,
  Save,
} from "lucide-react";

type Settings = {
  id: string;
  api_key: string;
  region: string;
  auto_create: boolean;
  webhook_secret: string;
  rage_click_threshold: number;
  dropoff_pct_threshold: number;
  zero_result_threshold: number;
  error_rate_threshold: number;
  time_on_task_minutes: number;
  quiet_window_minutes: number;
  default_severity: string;
  last_synced_at: string | null;
};

const settingsQuery = () =>
  queryOptions({
    queryKey: ["pendo-settings"],
    queryFn: async (): Promise<Settings> => {
      const { data, error } = await supabase
        .from("pendo_settings")
        .select("*")
        .order("created_at", { ascending: true })
        .limit(1)
        .maybeSingle();
      if (error) throw error;
      if (data) return data as Settings;
      const created = await supabase.from("pendo_settings").insert({}).select("*").single();
      if (created.error) throw created.error;
      return created.data as Settings;
    },
  });

export const Route = createFileRoute("/synapse/csp-agentic-ai")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "CSP Agentic AI — Proactive Support Intelligence" },
      {
        name: "description",
        content:
          "Executive briefing on CSP Agentic AI: proactive interception of platform errors, autonomous self-resolution in under five seconds and auto-triaged support tickets.",
      },
      { property: "og:title", content: "CSP Agentic AI — Proactive Support Intelligence" },
      {
        property: "og:description",
        content:
          "Capabilities, end-to-end decision flow, business impact and the Pendo connection that feeds signals into the platform.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  loader: ({ context }) => context.queryClient.ensureQueryData(settingsQuery()),
  component: CspAgenticAiPage,
});

const CAPABILITIES = [
  {
    title: "Proactive Interception",
    body: "Catches platform errors — dead clicks, API drops, database desyncs — before the customer ever opens a case.",
  },
  {
    title: "Cross-System Root Cause",
    body: "Correlates frontend console logs, API errors and identity/role mismatches into one diagnosed incident per account and session.",
  },
  {
    title: "Background Self-Resolution",
    body: "Executes scoped, reversible fixes against admin APIs for the top system sync failures, resolving in under 5 seconds.",
  },
  {
    title: "Silent Friction Reporting",
    body: "Surfaces unseen bugs and drop-offs to Product and R&D in daily and weekly automated friction reports.",
  },
] as const;

const FLOW = [
  {
    n: "1",
    tag: "INPUTS",
    title: "Signal & Event Real-Time Engine",
    lines: [
      {
        label: "Platform tower",
        text: "Frontend console logs, API errors, database sync flags and role-change events.",
      },
      {
        label: "Behavioural tower",
        text: "Pendo clicks, funnels, guides and search — rage clicks, dead clicks and abandonment on high-friction pages.",
      },
    ],
    accent: false,
  },
  {
    n: "2",
    tag: "CORE AI",
    title: "Agentic AI Reasoning Engine",
    lines: [
      {
        label: "Diagnose",
        text: "Cross-system anomaly detection and root cause analysis, ranked against known failure signatures and recent change events.",
      },
      {
        label: "Guardrail",
        text: "Diagnostic accuracy audited against CSR tickets; write actions gated behind a 95% RCA accuracy threshold.",
      },
    ],
    accent: true,
  },
  {
    n: "3",
    tag: "OUTPUT",
    title: "Mitigation, Dispatch & Feedback",
    lines: [
      {
        label: "User",
        text: "Inline banner explaining the issue and the ticket already raised on their behalf — or an autonomous repair completed in 1.4s.",
      },
      {
        label: "Support",
        text: "Auto-populated Jira / ServiceNow ticket in under 5 seconds, with evidence, severity and rollback state logged.",
      },
    ],
    accent: false,
  },
] as const;

const IMPACT = [
  {
    value: "40%",
    title: "MTTR Time Reduction",
    detail: "No more manual diagnostics for CSRs on role and sync failures.",
    tone: "positive",
  },
  {
    value: "<5s",
    title: "Autonomous Resolution",
    detail: "Zero-ticket workflows once Phase 2 write access is earned.",
    tone: "plain",
  },
  {
    value: "25%",
    title: "Escalation Deflection",
    detail: "Cut through proactive in-app status messaging before case creation.",
    tone: "plain",
  },
  {
    value: "100%",
    title: "Friction Visibility",
    detail: "Unseen bugs surfaced weekly via automated product friction reports.",
    tone: "plain",
  },
] as const;

const PHASES = [
  {
    label: "Phase 1 · Watch & Alert",
    text: "Detect and diagnose in real time. User gets an inline banner, support gets an auto-created ticket, R&D gets the silent friction report.",
  },
  {
    label: "Phase 2 · Autonomous Repair",
    text: "Write permissions enabled for background API fixes; self-healing for the top 3 system sync failures, every action reversible.",
  },
] as const;

const THRESHOLDS: {
  key: keyof Settings;
  label: string;
  help: string;
  suffix: string;
}[] = [
  {
    key: "rage_click_threshold",
    label: "Repeated clicks",
    help: "Clicks on the same element within 10 minutes before a signal is raised.",
    suffix: "clicks",
  },
  {
    key: "dropoff_pct_threshold",
    label: "Funnel drop-off",
    help: "Percentage-point rise in drop-off at any step, versus the previous week.",
    suffix: "%",
  },
  {
    key: "zero_result_threshold",
    label: "Zero-result searches",
    help: "Searches for the same term returning nothing, per day.",
    suffix: "searches",
  },
  {
    key: "error_rate_threshold",
    label: "Failed interactions",
    help: "Share of attempts ending in a validation or submit error.",
    suffix: "%",
  },
  {
    key: "time_on_task_minutes",
    label: "Time on task",
    help: "Minutes spent on a workflow before it counts as friction.",
    suffix: "min",
  },
  {
    key: "quiet_window_minutes",
    label: "Quiet window",
    help: "Repeat behaviour inside this window is folded into the existing signal.",
    suffix: "min",
  },
];

const TABS = [
  { id: "briefing", label: "Executive briefing" },
  { id: "pendo", label: "Pendo settings" },
] as const;

function CspAgenticAiPage() {
  const [tab, setTab] = useState<(typeof TABS)[number]["id"]>("briefing");

  return (
    <SynapseShell
      title="Agentic Support | Proactive Interception"
      subtitle="Replacing reactive case handling with real-time reasoning that resolves platform friction before a case exists."
      actions={
        <Badge variant="secondary" className="uppercase tracking-[0.14em]">
          CSP Agentic AI · Executive briefing
        </Badge>
      }
    >
      <div className="space-y-6">
        <div className="flex flex-wrap gap-2 border-b border-border pb-3">
          {TABS.map((t) => (
            <button
              key={t.id}
              type="button"
              onClick={() => setTab(t.id)}
              className={`rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
                tab === t.id
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
              }`}
            >
              {t.label}
            </button>
          ))}
        </div>

        {tab === "briefing" ? <Briefing /> : <PendoSettings />}
      </div>
    </SynapseShell>
  );
}

function Briefing() {
  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 rounded-xl border-l-4 border-l-primary bg-accent/40 p-5 sm:flex-row sm:gap-6">
        <p className="shrink-0 font-display text-lg font-bold text-primary">The Problem</p>
        <p className="text-sm leading-relaxed text-foreground/90">
          Platform errors — missing Salesforce contact IDs, entitlement failures, API drops and
          unhandled frontend exceptions — stay invisible until a customer complains. Users hit a
          dead end, CSRs re-diagnose the same sync issues by hand, and the underlying defects
          never reach R&amp;D.
        </p>
      </div>

      <div className="grid gap-5 lg:grid-cols-3">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <Cpu className="size-4 text-primary" />
              <h2 className="flex-1 text-center font-display text-lg font-bold">
                Agentic Capabilities
              </h2>
            </div>
            <ul className="mt-5 space-y-6">
              {CAPABILITIES.map((c) => (
                <li key={c.title}>
                  <p className="flex items-start gap-2 font-semibold">
                    <CheckCircle2 className="mt-0.5 size-4 shrink-0 text-primary" />
                    {c.title}
                  </p>
                  <p className="mt-2 pl-6 text-sm leading-relaxed text-muted-foreground">
                    {c.body}
                  </p>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <Network className="size-4 text-primary" />
              <h2 className="flex-1 text-center font-display text-lg font-bold">
                End-to-End Decision Flow
              </h2>
            </div>
            <div className="mt-5">
              {FLOW.map((step, i) => (
                <div key={step.n}>
                  <div
                    className={`rounded-lg border-l-4 p-4 ${
                      step.accent
                        ? "border-l-primary bg-accent/50"
                        : "border-l-primary/40 bg-muted/40"
                    }`}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <p className="font-display text-base font-bold">
                        {step.n}. {step.title}
                      </p>
                      <Badge variant="secondary" className="shrink-0 text-[10px] tracking-[0.12em]">
                        {step.tag}
                      </Badge>
                    </div>
                    <div className="mt-2 space-y-1.5 text-sm text-muted-foreground">
                      {step.lines.map((l) => (
                        <p key={l.label}>
                          <span className="font-semibold text-foreground">{l.label}:</span>{" "}
                          {l.text}
                        </p>
                      ))}
                    </div>
                  </div>
                  {i < FLOW.length - 1 ? (
                    <div className="flex justify-center py-4">
                      <ArrowDown className="size-4 text-muted-foreground" />
                    </div>
                  ) : null}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <LineChart className="size-4 text-primary" />
              <h2 className="flex-1 text-center font-display text-lg font-bold">
                Business &amp; IT Impact
              </h2>
            </div>
            <div className="mt-5 space-y-3">
              {IMPACT.map((m) => (
                <div key={m.title} className="rounded-lg border border-border bg-muted/30 p-4">
                  <p className="font-display text-lg font-bold">
                    <span className={m.tone === "positive" ? "text-primary" : ""}>{m.value}</span>{" "}
                    {m.title}
                  </p>
                  <p className="mt-1 text-sm text-muted-foreground">{m.detail}</p>
                </div>
              ))}
            </div>

            <div className="mt-5 rounded-lg bg-accent/40 p-4">
              <p className="flex items-center gap-2 text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                <CalendarCheck className="size-3.5" />
                Phased delivery commitment
              </p>
              <div className="mt-3 space-y-3 text-sm">
                {PHASES.map((p) => (
                  <p key={p.label}>
                    <span className="font-semibold">{p.label}:</span>{" "}
                    <span className="text-muted-foreground">{p.text}</span>
                  </p>
                ))}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center gap-2 border-b border-border pb-3">
            <BarChart3 className="size-4 text-primary" />
            <h2 className="font-display text-lg font-bold">Baselines needed before Phase 1</h2>
          </div>
          <div className="mt-4 grid gap-5 sm:grid-cols-3">
            <div>
              <p className="text-sm font-semibold">System error volume</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Identity and role mismatches, authorization and entitlement failures, integration
                and API drops, unhandled client-side exceptions.
              </p>
            </div>
            <div>
              <p className="text-sm font-semibold">Support case benchmarks</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Monthly CSR/TAC tickets that turn out to be role or sync mapping issues, and their
                current mean time to resolution.
              </p>
            </div>
            <div>
              <p className="text-sm font-semibold">Behavioural coverage</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Share of CSP sessions covered by Pendo, plus rage-click, dead-click and
                abandonment rates on high-friction pages (Pendo: 6.62%).
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

function PendoSettings() {
  const queryClient = useQueryClient();
  const { data } = useSuspenseQuery(settingsQuery());
  const [form, setForm] = useState<Settings>(data);
  const [origin, setOrigin] = useState("");

  useEffect(() => setForm(data), [data]);
  useEffect(() => setOrigin(window.location.origin), []);

  const save = useMutation({
    mutationFn: async (next: Settings) => {
      const { id, ...rest } = next;
      const { error } = await supabase.from("pendo_settings").update(rest).eq("id", id);
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pendo-settings"] });
      toast.success("Pendo settings saved");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const endpoint = `${origin}/api/public/pendo-alert`;
  const set = <K extends keyof Settings>(key: K, value: Settings[K]) =>
    setForm((f) => ({ ...f, [key]: value }));

  const copy = (text: string, what: string) => {
    void navigator.clipboard.writeText(text);
    toast.success(`${what} copied`);
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-muted-foreground">
          Paste your Pendo key, set the thresholds, and let matching behaviour create signals on
          its own.
        </p>
        <Button onClick={() => save.mutate(form)} disabled={save.isPending}>
          <Save className="size-4" />
          {save.isPending ? "Saving…" : "Save settings"}
        </Button>
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.1fr_1fr]">
        <Card>
          <CardHeader>
            <CardTitle className="font-display text-base">Connection</CardTitle>
          </CardHeader>
          <CardContent className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="api_key">Pendo integration key</Label>
              <Input
                id="api_key"
                type="password"
                autoComplete="off"
                placeholder="Paste the integration key from Pendo"
                value={form.api_key}
                onChange={(e) => set("api_key", e.target.value.slice(0, 500))}
              />
              <p className="text-xs text-muted-foreground">
                Stored with your CSP AI Capabilities data and only used to read aggregations.
              </p>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>Pendo region</Label>
                <Select value={form.region} onValueChange={(v) => set("region", v)}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="us">US (app.pendo.io)</SelectItem>
                    <SelectItem value="eu">EU (app.eu.pendo.io)</SelectItem>
                    <SelectItem value="us1">US1 (us1.app.pendo.io)</SelectItem>
                    <SelectItem value="jp">Japan (app.jpn.pendo.io)</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Default severity</Label>
                <Select
                  value={form.default_severity}
                  onValueChange={(v) => set("default_severity", v)}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {SEVERITIES.map((s) => (
                      <SelectItem key={s} value={s}>
                        {LABEL[s] ?? s}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="flex items-center justify-between rounded-xl border border-border p-4">
              <div>
                <p className="text-sm font-semibold">Auto-create signals</p>
                <p className="text-xs text-muted-foreground">
                  When a threshold trips, a signal appears in the inbox without anyone typing it.
                </p>
              </div>
              <Switch checked={form.auto_create} onCheckedChange={(v) => set("auto_create", v)} />
            </div>

            <p className="text-xs text-muted-foreground">
              Last sync:{" "}
              {form.last_synced_at
                ? new Date(form.last_synced_at).toLocaleString()
                : "no data received yet"}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="font-display text-base">Send Pendo alerts here</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Endpoint URL</Label>
              <div className="flex gap-2">
                <Input readOnly value={endpoint} className="font-mono text-xs" />
                <Button variant="outline" size="icon" onClick={() => copy(endpoint, "Endpoint")}>
                  <Copy className="size-4" />
                </Button>
              </div>
            </div>
            <div className="space-y-2">
              <Label>Shared secret (header x-pendo-secret)</Label>
              <div className="flex gap-2">
                <Input
                  value={form.webhook_secret}
                  onChange={(e) => set("webhook_secret", e.target.value.slice(0, 200))}
                  className="font-mono text-xs"
                />
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => copy(form.webhook_secret, "Secret")}
                >
                  <Copy className="size-4" />
                </Button>
              </div>
            </div>
            <div className="rounded-xl border border-border bg-muted/40 p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                Example body
              </p>
              <pre className="mt-2 overflow-x-auto text-xs leading-relaxed">
{`{
  "source": "Click & page events",
  "summary": "12 rage clicks on Apply policy",
  "customer": "Northwind Bank",
  "severity": "high",
  "reason_code": "Product not working as expected",
  "metric": "rage_clicks",
  "value": 12
}`}
              </pre>
              <p className="mt-2 text-xs text-muted-foreground">
                Only <span className="font-semibold">summary</span> is required. When a metric and
                value are sent, the signal is created only if the value passes the matching
                threshold below.
              </p>
            </div>
          </CardContent>
        </Card>

        <Card className="xl:col-span-2">
          <CardHeader>
            <CardTitle className="font-display text-base">Thresholds</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
            {THRESHOLDS.map((t) => (
              <div key={t.key} className="space-y-2">
                <Label htmlFor={t.key}>{t.label}</Label>
                <div className="flex items-center gap-2">
                  <Input
                    id={t.key}
                    type="number"
                    min={0}
                    max={100000}
                    value={String(form[t.key] ?? 0)}
                    onChange={(e) => set(t.key, Math.max(0, Number(e.target.value) || 0) as never)}
                  />
                  <span className="text-xs text-muted-foreground">{t.suffix}</span>
                </div>
                <p className="text-xs text-muted-foreground">{t.help}</p>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

```

## `src/routes/synapse.index.tsx`

```tsx
import { createFileRoute } from "@tanstack/react-router";
import { SynapseShell } from "@/components/synapse-shell";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export const Route = createFileRoute("/synapse/")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "CSP AI Capabilities Overview — Live Signals, Cases & Healing" },
      {
        name: "description",
        content:
          "Live view of incoming signals, open cases, active self-healing runbooks and resolution performance across the CSP AI Capabilities platform.",
      },
      { property: "og:title", content: "CSP AI Capabilities Overview — Live Signals, Cases & Healing" },
      {
        property: "og:description",
        content: "Signals, cases, runbooks and healing performance in one operating view.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: SynapseOverview,
});

const CAPABILITY_LANDSCAPE: {
  area: string;
  audience: string;
  objective: string;
  status: "In progress" | "Requirements gathering" | "Not started";
}[] = [
  {
    area: "AI Search & Welcome Center",
    audience: "Customers / Admins",
    objective:
      "Deliver AI search that understands intent and returns a single cited answer from knowledge, account context and product data — instead of a list of links",
    status: "In progress",
  },
  {
    area: "CSR AI Admin Assistant",
    audience: "CSRs / Support Operations",
    objective:
      "An AI agent that reads the case, diagnoses the account state, and executes the administrative fix end to end — asking a CSR only where judgement is needed",
    status: "In progress",
  },
  {
    area: "Ask CSP",
    audience: "Customers",
    objective: "Provide account-aware answers and resolve questions before case creation",
    status: "Requirements gathering",
  },
  {
    area: "Triage & Diagnosis",
    audience: "CSRs, L2, Engineering",
    objective: "Improve context, identify causes, and reduce investigation effort",
    status: "Not started",
  },
  {
    area: "Resolution & Automation",
    audience: "Customers, CSRs, IT Operations",
    objective: "Recommend or execute safe fixes",
    status: "Not started",
  },
  {
    area: "Predictive Intelligence",
    audience: "Product, Support, CSMs, IT",
    objective: "Detect emerging problems and intervene proactively",
    status: "Not started",
  },
  {
    area: "Learning & Optimization",
    audience: "Product, Support, Operations",
    objective: "Turn cases, feedback, and outcomes into continuous improvement",
    status: "Not started",
  },
];

function SynapseOverview() {
  return (
    <SynapseShell
      title="Overview"
      subtitle="Three-stage AI strategy for CSP: capture behavioral and system signals, diagnose customer or operational friction, and resolve issues autonomously where safe. The capabilities span customer self-service, CSR productivity, proactive issue detection, and continuous product learning."
    >
      <Card>
        <CardHeader>
          <CardTitle className="font-display text-base">Overall  AI capability landscape</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="border-y border-border bg-muted/50 text-left">
                  <th className="px-5 py-3 font-bold text-foreground">Capability area</th>
                  <th className="px-5 py-3 font-bold text-foreground">Primary audience</th>
                  <th className="px-5 py-3 font-bold text-foreground">Strategic objective</th>
                  <th className="px-5 py-3 font-bold text-foreground">Status</th>
                </tr>
              </thead>
              <tbody>
                {CAPABILITY_LANDSCAPE.map((row) => (
                  <tr key={row.area} className="border-b border-border last:border-0 align-top">
                    <td className="px-5 py-3 font-semibold">{row.area}</td>
                    <td className="px-5 py-3 text-muted-foreground">{row.audience}</td>
                    <td className="px-5 py-3 text-muted-foreground">{row.objective}</td>
                    <td className="px-5 py-3">
                      <span
                        className={`inline-flex whitespace-nowrap rounded-full px-2.5 py-1 text-xs font-bold ${
                          row.status === "In progress"
                            ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-400"
                            : row.status === "Requirements gathering"
                              ? "bg-amber-500/15 text-amber-700 dark:text-amber-400"
                              : "bg-red-500/15 text-red-700 dark:text-red-400"
                        }`}
                      >
                        {row.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

    </SynapseShell>
  );
}

```

## `src/routes/synapse.roadmap.tsx`

```tsx
import { createFileRoute } from "@tanstack/react-router";
import { SynapseShell } from "@/components/synapse-shell";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { CheckCircle2, Circle, CircleDot, ListChecks, Layers, ArrowDown, ArrowRight, ArrowUp, ChevronDown, ExternalLink, AlertTriangle, Sparkles, Command, Target } from "lucide-react";

export const Route = createFileRoute("/synapse/roadmap")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "CSP AI Capabilities Platform" },
      {
        name: "description",
        content:
          "Phased CSP AI Capabilities delivery roadmap: operational foundations, reactive case deflection and proactive agentic intelligence, with milestones and success measures per phase.",
      },
      { property: "og:title", content: "CSP AI Capabilities" },
      {
        property: "og:description",
        content:
          "Three-phase roadmap from signal capture to autonomous resolution, with milestones, owners and success measures.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  component: RoadmapPage,
});

type MonthStatus = "Done" | "In progress" | "Planned";

const MONTHS: {
  month: string;
  phase: string;
  focus: string;
  status: MonthStatus;
  milestones: string[];
  measure: string;
}[] = [
  {
    month: "January",
    phase: "Phase 1 · Operational foundations",
    focus: "Pendo ingestion live",
    status: "Done",
    milestones: [
      "Pendo alert endpoint and shared secret in production",
      "Thresholds and duplicate suppression configured",
    ],
    measure: "First real Pendo signals landing in the inbox daily.",
  },
  {
    month: "February",
    phase: "Phase 1 · Operational foundations",
    focus: "Signal inbox hardening",
    status: "Done",
    milestones: ["Themes, severity and reason codes applied to every signal", "Noise review with CSR leads"],
    measure: "90% of signals carry a theme and reason code.",
  },
  {
    month: "March",
    phase: "Phase 1 · Operational foundations",
    focus: "Case queue and resolution",
    status: "In progress",
    milestones: ["Resolution status, notes and close dates on cases", "Close-rate reporting on the metrics page"],
    measure: "Close rate reported weekly.",
  },
  {
    month: "April",
    phase: "Phase 1 · Operational foundations",
    focus: "Baseline measurement",
    status: "In progress",
    milestones: ["Friction rate and case-creation baselines captured", "Impact forecast calibrated to real volumes"],
    measure: "Baseline friction rate agreed and published.",
  },
  {
    month: "May",
    phase: "Phase 2 · Reactive case deflection",
    focus: "Correlation engine v1",
    status: "Planned",
    milestones: ["Frontend, API and identity failures correlated on one timeline", "Blast-radius calculation per incident"],
    measure: "Correlated evidence on 50% of new cases.",
  },
  {
    month: "June",
    phase: "Phase 2 · Reactive case deflection",
    focus: "Suggest-tier runbooks",
    status: "Planned",
    milestones: ["Top 10 runbooks published at suggest tier", "CSR-facing recommendation panel in the case view"],
    measure: "Runbook suggestion shown on 70% of eligible cases.",
  },
  {
    month: "July",
    phase: "Phase 2 · Reactive case deflection",
    focus: "Enriched escalation packet",
    status: "Planned",
    milestones: ["Evidence, blast radius and root-cause hypothesis attached automatically", "Packet accepted by L2 support"],
    measure: "40% MTTR reduction on correlated cases.",
  },
  {
    month: "August",
    phase: "Phase 2 · Reactive case deflection",
    focus: "In-app guidance",
    status: "Planned",
    milestones: ["Guides served at the point of friction via Pendo", "Deflection tracked per guide"],
    measure: "25% escalation deflection.",
  },
  {
    month: "September",
    phase: "Phase 3 · Proactive agentic intelligence",
    focus: "Auto-resolve pilot",
    status: "Planned",
    milestones: ["Top sync failure auto-resolved, reversible and logged", "Rollback and audit trail verified"],
    measure: "First failure class self-healed in under 5 seconds.",
  },
  {
    month: "October",
    phase: "Phase 3 · Proactive agentic intelligence",
    focus: "Auto-resolve expansion",
    status: "Planned",
    milestones: ["Top 3 sync failures on auto-resolve tier", "Autonomy guardrails reviewed with IT operations"],
    measure: "100% of auto-resolutions auditable.",
  },
  {
    month: "November",
    phase: "Phase 3 · Proactive agentic intelligence",
    focus: "Predictive interception",
    status: "Planned",
    milestones: ["Failures intercepted before the customer hits them", "Silent-friction digest to product and IT"],
    measure: "Interception ahead of contact on the top failure class.",
  },
  {
    month: "December",
    phase: "Phase 3 · Proactive agentic intelligence",
    focus: "Self-tuning and review",
    status: "Planned",
    milestones: ["Thresholds and autonomy tuned from outcome data", "Annual impact review with stakeholders"],
    measure: "Thresholds adjusted automatically from outcomes.",
  },
];

const STATUS_ICON = {
  Done: CheckCircle2,
  "In progress": CircleDot,
  Planned: Circle,
} as const;

const WELCOME_CENTER = {
  eyebrow: "Executive agentic briefing",
  title: "AI Search Welcome Center",
  subtitle:
    "Turning first-login confusion into guided, self-served activation with AI search over the live knowledge base.",
  quarter: "Nov '26",
  phase: " Welcome Center & AI Search",
   pipelineTitle: "3-Stage Activation Pipeline",
  capabilitiesTitle: "AI Search Capability",
  capabilities: [
    {
      name: "Core Search & Ingestion",
      body: "Semantic search via Vertex AI RAG, smart LLM routing, and automated daily sync of Knowledge Base assets.",
    },
    {
      name: "Security & Guardrails",
      body: "Dual-layer validation with Model Armor, metadata-preserving privilege validation, and similarity-based confidence gates.",
    },
    {
      name: "Feedback & Analytics",
      body: "Diagnostic-triggering user feedback, backend passive monitoring, Pendo friction metrics, and comprehensive audit logging.",
    },
    {
      name: "Actionable Answers ",
      body: "Quick Actions Suggestions; Embedded Actions Inside Responses; Conversational Workflow Completion; Natural Language Navigation; Source Transparency.",
    },
  ],
  stages: [
    {
      tag: "Stage 1",
      name: "1. Provision & Welcome",
       body: "New users are guided to the Welcome Center with role-aware onboarding steps and a clear next action.",
      accent: false,
    },
    {
      tag: "Stage 2-3",
       name: "2. Ask & Retrieve",
      body: "Admin questions hit AI Search; the RAG index pulls the right KB passages and returns a cited, task-shaped answer.",
      accent: false,
    },
    {
      tag: "Stage 4-5",
      name: "3. Deflect ",
      body: "Resolved journeys close in-product; unresolved ones hand off to case creation with full context attached.",
      accent: true,
    },
  ],
  impact: [
    { stat: "13.67%", label: "Case Mix Addressed", note: "Onboarding & activation drivers in scope" },
    { stat: "15–25%", label: "Structural Deflection", note: "Sustained 30 days before the next phase" },
    { stat: "~2,400", label: "Batch 1 Admins", note: "CYBR-migrated customers first" },
  ],
  cases: [
    { name: "New tenant activation confusion", pct: "5.8%" },
    { name: "Welcome email / invite comprehension", pct: "4.1%" },
    { name: "KB navigation failures", pct: "3.77%" },
  ],
  commitment:
    "Phase 1 scope (FY26 Q4): Welcome Center CMS build, AI Search over the live KB vector index, and case-deflection analytics wired into the admin journey.",
};

const CSR_AI_ADMIN = {
  eyebrow: "Executive agentic briefing",
  title: "CSR AI Admin Assistant",
  subtitle:
    "A deterministic, zero-hallucination admin assistant embedded in the case console — starting with Resend Activation Link.",
  quarter: "FY26 · Q4 POC",
  phase: "Phase 1 · Reactive case deflection",
  pipelineTitle: "Assistant Workflow",
  capabilitiesTitle: "Agentic Capabilities",
  capabilities: [
    {
      name: "Intent Classification",
      body: "The orchestrator parses case subject and description and confirms the request maps to a supported admin scenario, rejecting unrelated auth noise.",
    },
    {
      name: "Entity Extraction",
      body: "User email, org domain and identity parameters are pulled into a structured payload for the downstream connectors.",
    },
    {
      name: "Context Stitching",
      body: "Account status, user role and IDP status are hydrated from the identity provider and CSP so the agent sees ground truth before acting.",
    },
    {
      name: "Deterministic Guardrails",
      body: "Third-party IDP accounts are blocked from automated action; every other path follows a fixed diagnose → action → done wizard with no free-form generation.",
    },
  ],
  stages: [
    {
      tag: "Step 1",
      name: "1. Diagnose",
      body: "The assistant surfaces identity and portal signals on the case: account status, user role and IDP status, with the case email prefilled.",
      accent: false,
    },
    {
      tag: "Step 2",
      name: "2. Action",
      body: "Signals are evaluated deterministically: safe cases resend the activation link via the identity lifecycle call; external-IDP cases are blocked with an explanation.",
      accent: false,
    },
    {
      tag: "Step 3",
      name: "3. Done",
      body: "A confirmation is written back to the case, and the outcome is logged for deflection and accuracy measurement.",
      accent: true,
    },
  ],
  impact: [
    { stat: "41.1%", label: "Admin Ticket Pool", note: "Onboarding / activation token failures" },
    { stat: "~270", label: "Cases per Month", note: "Resend-link candidates in Phase 1 scope" },
    { stat: "Minutes", label: "Time to Resolution", note: "Replaces manual tool-switching by agents" },
  ],
  cases: [
    { name: "Expired activation token", pct: "Scenario 1" },
    { name: "Password reset / lockout", pct: "Next" },
    { name: "Verification code not received", pct: "Next" },
  ],
  commitment:
    "Phase 1 POC scope (FY26 Q4): single-scenario side-drawer in the case console, identity lifecycle integration, orchestrator classification, and outcome logging for accuracy review.",
};

const ASK_CSP = {
  eyebrow: "In-portal assistant",
  title: "Ask CSP",
  subtitle:
    "A conversational assistant embedded in the Customer Support Portal — answers account, asset and case questions in place, and only creates a case when it cannot resolve the request.",
  quarter: "FY27 Q1",
  phase: "Reactive Case Deflection",
  capabilitiesTitle: "Assistant Capabilities",
  capabilities: [
    {
      name: "Grounded answers",
      body: "Questions are answered from the knowledge base and the user's own account context, with the source article linked on every response.",
    },
    {
      name: "Account & asset awareness",
      body: "Resolves entitlement, migrated-asset and account-context questions using the signed-in user's licence and asset records rather than generic documentation.",
    },
    {
      name: "Confidence gating",
      body: "When verified data is thin, the assistant says so instead of guessing — offering a super-user contact, a knowledge article, or a prefilled case.",
    },
    {
      name: "Feedback capture",
      body: "Thumbs up/down with reason tags and free text on every answer, feeding accuracy review and content gaps back to the CSP team.",
    },
  ],
  pipelineTitle: "Conversation Flow",
  stages: [
    {
      tag: "Step 1",
      name: "1. Ask",
      body: "The user opens Ask CSP from the portal header and asks in natural language, with suggested starters for common intents.",
      accent: false,
    },
    {
      tag: "Step 2",
      name: "2. Retrieve & Reason",
      body: "The query is matched against the knowledge index and the user's account signals; the assistant scores its own confidence before replying.",
      accent: false,
    },
    {
      tag: "Step 3",
      name: "3. Deflect or Escalate",
      body: "High-confidence answers resolve in the drawer; low-confidence or zero-result queries hand off to a prefilled case with the full conversation attached.",
      accent: true,
    },
  ],
  impact: [
    { stat: "15–25%", label: "Target Deflection", note: "Portal questions resolved without a case" },
    { stat: "Minutes", label: "Time to Answer", note: "Replaces KB hunting and portal navigation" },
    { stat: "100%", label: "Answers Rated", note: "Feedback captured on every response" },
  ],
  cases: [
    { name: "Where are my migrated assets?", pct: "Scenario 1" },
    { name: "Licence & entitlement questions", pct: "Scenario 2" },
    { name: "Case status & next steps", pct: "Next" },
  ],
  commitment:
    "Scope: portal-embedded drawer, KB-grounded retrieval with confidence gating, account-context lookups, prefilled case handoff, and per-answer feedback logging.",
};

const PREDICTIVE_AI_AGENT = {
  eyebrow: "Anticipatory support",
  title: "Predictive AI Agent",
  subtitle:
    "An always-on agent that forecasts failures, entitlement gaps and support demand from behavioural and system telemetry — acting before the customer notices the problem.",
  quarter: "FY27 Q2",
  phase: "Proactive Agentic Intelligence",
  capabilitiesTitle: "Predictive Capabilities",
  capabilities: [
    {
      name: "Failure prediction",
      body: "Learns the error, retry and sync patterns that precede a case and flags at-risk accounts and journeys hours before the customer hits the wall.",
    },
    {
      name: "Demand forecasting",
      body: "Projects case volume by driver, product line and region so support capacity and content are staged ahead of the spike, not after it.",
    },
    {
      name: "Next-best action",
      body: "Recommends the highest-value intervention per account — a guide, a proactive message, a runbook, or a CSR outreach — ranked by predicted deflection.",
    },
    {
      name: "Model governance",
      body: "Every prediction carries a confidence score, the features behind it and an outcome record, so accuracy and drift are auditable over time.",
    },
  ],
  pipelineTitle: "Prediction Flow",
  stages: [
    {
      tag: "Step 1",
      name: "1. Observe",
      body: "Behavioural, API and case telemetry stream into the signal store with account, journey and severity context attached.",
      accent: false,
    },
    {
      tag: "Step 2",
      name: "2. Predict",
      body: "Models score likelihood of failure, escalation and churn risk per account, ranking what is about to go wrong and how costly it is.",
      accent: false,
    },
    {
      tag: "Step 3",
      name: "3. Act Ahead",
      body: "High-confidence predictions trigger a scoped preventive action or an alert to the owning team, with the outcome fed back for retraining.",
      accent: true,
    },
  ],
  impact: [
    { stat: "30%", label: "Cases Prevented", note: "Issues resolved before a case is raised" },
    { stat: "Hours", label: "Early Warning", note: "Lead time ahead of customer impact" },
    { stat: ">85%", label: "Prediction Precision", note: "Accuracy gate before autonomous action" },
  ],
  cases: [
    { name: "Entitlement & licence failures", pct: "Driver 1" },
    { name: "Sync and provisioning breakages", pct: "Driver 2" },
    { name: "Onboarding drop-off risk", pct: "Driver 3" },
  ],
  commitment:
    "Scope: telemetry feature store, failure and demand prediction models, next-best-action ranking, confidence gating and outcome logging for continuous retraining.",
};

type Briefing = typeof CSR_AI_ADMIN;



function BriefingSection({ data }: { data: Briefing }) {
  return (
    <section className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-3 border-b border-border pb-5">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">{data.eyebrow}</p>
          <h2 className="mt-1 font-display text-3xl font-bold tracking-tight">{data.title}</h2>
          <p className="mt-2 max-w-3xl text-sm text-muted-foreground">{data.subtitle}</p>
        </div>
        <div className="flex items-center gap-2">
          <Badge>{data.phase}</Badge>
        </div>

      </div>

      <div className="grid gap-5 lg:grid-cols-3">
        <Card className="h-full">
          <CardContent className="p-5">
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <ListChecks className="size-4 text-primary" />
              <h3 className="flex-1 text-center font-display text-lg font-bold">{data.capabilitiesTitle}</h3>
            </div>
            <div className="mt-4 space-y-4">
              {data.capabilities.map((c) => (
                <div key={c.name}>
                  <p className="text-sm font-bold">{c.name}</p>
                  <p className="mt-1 text-sm leading-relaxed text-muted-foreground">{c.body}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card className="h-full">
          <CardContent className="p-5">
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <Layers className="size-4 text-primary" />
              <h3 className="flex-1 text-center font-display text-lg font-bold">{data.pipelineTitle}</h3>
            </div>
            <div className="mt-4 space-y-2">
              {data.stages.map((s, i) => (
                <div key={s.name}>
                  <div
                    className={`rounded-lg border-l-4 p-4 ${
                      s.accent ? "border-l-primary bg-primary/10" : "border-l-primary/50 bg-muted/40"
                    }`}
                  >
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <p className="font-display text-base font-bold">{s.name}</p>
                    </div>

                    <p className="mt-1 text-sm leading-relaxed text-muted-foreground">{s.body}</p>
                  </div>
                  {i < data.stages.length - 1 && (
                    <div className="flex justify-center py-3">
                      <ArrowDown className="size-4 text-muted-foreground" />
                    </div>
                  )}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card className="h-full">
          <CardContent className="flex h-full flex-col p-5">
            <div className="border-b border-border pb-3">
              <h3 className="text-center font-display text-lg font-bold">Business &amp; IT Impact</h3>
            </div>
            <div className="mt-4 space-y-3">
              {data.impact.map((m) => (
                <div key={m.label} className="flex items-center gap-4 rounded-lg border border-border bg-muted/40 p-4">
                  <span className="font-display text-2xl font-bold text-primary">{m.stat}</span>
                  <span className="flex-1 text-right">
                    <span className="block text-sm font-bold">{m.label}</span>
                    <span className="block text-sm text-muted-foreground">{m.note}</span>
                  </span>
                </div>
              ))}
            </div>

            <div className="mt-4">
              <p className="text-[11px] font-bold uppercase tracking-[0.16em] text-muted-foreground">
                Case drivers addressed
              </p>
              <ul className="mt-2 space-y-1">
                {data.cases.map((c) => (
                  <li key={c.name} className="flex justify-between gap-3 text-sm">
                    <span className="text-muted-foreground">{c.name}</span>
                    <span className="font-semibold">{c.pct}</span>
                  </li>
                ))}
              </ul>
            </div>

            <div className="mt-4 rounded-lg border border-dashed border-border bg-muted/30 p-4">
              <p className="text-[11px] font-bold uppercase tracking-[0.16em] text-muted-foreground">
                IT delivery commitment
              </p>
              <p className="mt-1 text-sm leading-relaxed">{data.commitment}</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}

const FUTURE_PHASES: { label: string; tone: string; items: string[] }[] = [
  {
    label: "Next",
    tone: "border-emerald-500/40 bg-emerald-500/5",
    items: [
      "Personalized Assistant",
      "Context-Aware AI Responses",
      "AI Memory & Continuity",
      "Dynamic Knowledge Summarization",
      "Smart Search Filters",
      "Similar Case Search",
      "Root Cause Search",
      "AI Confidence",
    ],
  },
  {
    label: "Later",
    tone: "border-sky-500/40 bg-sky-500/5",
    items: [
      "Proactive AI Notifications",
      "Multi-Modal Search (uploads, screenshots)",
      "Intelligent Escalation",
      "AI Search Analytics Layer",
      "Personalized Home Feed",
      "Suggested Prompts (behavior-based)",
      "Voice & Command Support",
    ],
  },
];

function FutureEnhancements() {
  return (
    <section className="space-y-4">
      <div className="flex items-center gap-3">
        <span className="h-8 w-1.5 rounded-full bg-primary" />
        <h2 className="font-display text-2xl font-bold tracking-tight">Future Enhancements</h2>
      </div>

      <div className="grid gap-4 lg:grid-cols-[1fr_auto_1fr_auto_1fr] lg:items-center">
        {FUTURE_PHASES.map((phase, i) => (
          <div key={phase.label} className="contents">
            <div className={`h-full rounded-3xl border p-5 ${phase.tone}`}>
              <Badge className="mb-4">{phase.label}</Badge>
              <ul className="space-y-2">
                {phase.items.map((item) => (
                  <li key={item} className="flex gap-2 text-sm font-semibold">
                    <span className="mt-2 size-1.5 shrink-0 rounded-full bg-foreground/60" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </div>
            {i < FUTURE_PHASES.length - 1 ? (
              <div className="hidden justify-center text-muted-foreground lg:flex">
                <ArrowRight className="size-6" />
              </div>
            ) : null}
          </div>
        ))}
      </div>
    </section>
  );
}

type Pillar = {
  name: string;
  body: string;
  tags: string[];
  explanation: string;
  experience: string[];
  impact: { stat: string; label: string; note: string }[];
  steps?: { title: string; note: string }[];
  deckUrl?: string;
};

const PILLARS: Pillar[] = [
  {
    name: "Comms Orchestrator",
    body: "An agent that decides who needs to hear what, when and on which channel — proactive incident notices, case updates and follow-ups written and sent without a CSR chasing the thread.",
    tags: ["Guide", "Customer facing"],
    explanation:
      "The agent watches incidents, clusters and case state changes, works out the affected audience from account and entitlement data, drafts a message grounded in the confirmed facts, picks the channel (in-CSP banner, email, case note) and the timing, then sends it. It tracks acknowledgement, suppresses duplicate notices across a cluster, and schedules the resolution and follow-up messages on its own.",
    experience: [
      "Affected customers get an in-CSP notice before they open a case.",
      "One incident produces one message per account, not one per signal.",
      "Case updates land on schedule without the CSR writing them.",
      "Resolution and follow-up notes go out automatically when the fix verifies.",
      "The CSR sees every message sent on the case timeline and can override before send.",
    ],
    impact: [
      { stat: "-30%", label: "Inbound Case Volume", note: "Customers informed before they ask" },
      { stat: "-50%", label: "CSR Update Effort", note: "Status comms written and scheduled" },
      { stat: "Consistent", label: "Customer Messaging", note: "Same facts, same tone, every channel" },
      { stat: "Faster", label: "Incident Awareness", note: "Audience notified as the cluster forms" },
    ],
  },
  {
    name: "Case Summary on Arrival",
    body: "Every new case opens with a plain-language summary of what the customer did, what failed and what was already tried.",
    tags: ["Triage"],
    explanation:
      "On case creation the agent stitches the submitted description with session telemetry, recent errors and prior cases for that account, then writes a short structured brief: what happened, what broke, what has already been attempted.",
    experience: [
      "CSR opens a new case and reads a five-line brief instead of a raw description.",
      "Session events and error codes are already attached as evidence.",
      "Prior related cases for the same account are linked inline.",
      "The CSR starts investigating instead of re-reading the thread.",
    ],
    impact: [
      { stat: "-40%", label: "Time to First Action", note: "No manual context gathering" },
      { stat: "Consistent", label: "Case Quality", note: "Same brief structure on every case" },
      { stat: "Fewer", label: "Clarifying Replies", note: "Context captured up front" },
    ],
  },
  {
    name: "Duplicate, Sibling and Cluster Detection",
    body: "Group signals and cases that are really one incident, so one fix closes the whole group instead of ten separate investigations — a big noise reduction, since today every threshold breach is its own row.",
    tags: ["Triage"],
    explanation:
      "Signal and case text is embedded and compared across accounts on symptom, error signature, feature area, product and time window. Matches above threshold collapse into a single incident or parent case with a shared resolution and a member count.",
    experience: [
      "The inbox shows one incident with a count, not fifty rows.",
      "A new case is flagged as a sibling of an open investigation at intake.",
      "Expanding an incident lists every contributing signal and account.",
      "Resolving the parent closes the whole group with a shared response.",
      "New matching signals attach to the open incident automatically.",
      "Cluster growth rate signals an emerging incident early.",
    ],
    impact: [
      { stat: "-65%", label: "Inbox Volume", note: "Related breaches collapse into one" },
      { stat: "1 fix", label: "Closes Many Cases", note: "Group resolution instead of repeats" },
      { stat: "Earlier", label: "Incident Detection", note: "Cluster growth is the signal" },
      { stat: "-30%", label: "Duplicate Effort", note: "No parallel investigations" },
    ],
  },
  {
    name: "Draft Reply with Cited Evidence",
    body: "Suggest a response built only from knowledge base passages and case telemetry, with every claim linked to a source.",
    tags: ["Resolve"],
    explanation:
      "Retrieval is constrained to approved KB content and the case's own telemetry. The draft is assembled with inline citations; any statement without a source is dropped rather than generated.",
    experience: [
      "CSR opens the reply box and a grounded draft is already there.",
      "Each claim carries a link to the KB section or log line behind it.",
      "CSR edits and sends, or rejects the draft with a reason.",
      "Rejections feed back into retrieval quality.",
    ],
    impact: [
      { stat: "-50%", label: "Drafting Time", note: "Response starts written" },
      { stat: "0", label: "Uncited Claims", note: "No answer without a source" },
      { stat: "Higher", label: "Response Consistency", note: "Same approved content every time" },
    ],
  },
  {
    name: "Product Friction Digest",
    body: "An agent investigates friction, names the likely cause with a confidence level, drafts the work to fix it, answers follow-up questions live and verifies afterwards whether the fix actually worked.",
    tags: ["Top priority", "Detect", "Diagnose", "Learn"],
    deckUrl:
      "https://docs.google.com/presentation/d/17XGXOGtKtCK0Qbl6orfHtNVzZLSet6EA/edit?slide=id.p4#slide=id.p4",
    explanation:
      "Signals are rolled up by feature and journey step rather than by account. For each ranked item the agent runs its own investigation — pulling sessions, cases, entitlement and config state and recent releases — forms candidate explanations, tests each against the observed pattern and reports the winner with a confidence score plus the hypotheses it eliminated and why. It then drafts the resulting work item, estimates the case volume a fix would remove, stays available for follow-up questions against the live data, and re-checks the same friction after the fix ships.",
    experience: [
      "Each item states a hypothesised cause in plain language with a confidence level.",
      "The agent shows what it checked, what it ruled out and the evidence for each.",
      "The PM can ask follow-ups in natural language — 'is this new since June?', 'which accounts churned after hitting it?' — and the agent queries the underlying data live.",
      "Every item arrives as a drafted problem statement with affected users, rough sizing and a suggested fix path, ready to push to the backlog.",
      "A projected impact figure quantifies the ask: fixing this removes roughly N cases a month.",
      "After a fix ships the agent re-measures the same friction and reports back, unprompted, whether it worked.",
      "Low-confidence items are flagged as needing human judgement rather than presented as fact.",
    ],
    impact: [
      { stat: "Cause", label: "Not Just Correlation", note: "Hypothesis with confidence" },
      { stat: "Drafted", label: "Backlog Item", note: "Sizing and fix path included" },
      { stat: "Verified", label: "After the Fix", note: "Agent confirms it worked" },
    ],
  },


  {
    name: "Runbook Mining from Resolutions",
    body: "Learn repeat resolution patterns from closed cases and propose new runbooks for review instead of hand-authoring them.",
    tags: ["Learn"],
    explanation:
      "Closed cases are clustered by cause and resolution path. Where a path repeats reliably, a draft runbook is generated with its trigger conditions, steps, blast radius and rollback, and queued for human approval.",
    experience: [
      "Ops reviews a queue of proposed runbooks each cycle.",
      "Each proposal shows the cases it was learned from and its historical success rate.",
      "Approved runbooks enter the library at the lowest autonomy tier.",
      "Tier increases only after live accuracy is proven.",
    ],
    impact: [
      { stat: "Faster", label: "Runbook Coverage", note: "Authoring starts from real resolutions" },
      { stat: "Evidence", label: "Backed Proposals", note: "Success rate shown before approval" },
      { stat: "Compounding", label: "Automation Rate", note: "Library grows with case volume" },
    ],
  },
  {
    name: "Release-Aware Regression Watch",
    body: "Compare friction signals before and after each release and flag the features a deploy quietly broke.",
    tags: ["Detect"],
    explanation:
      "Each release is a boundary marker. Error rates, dead clicks, task completion and search misses are compared across the boundary per feature area, and statistically significant regressions are raised with the release attached.",
    experience: [
      "A regression alert names the release, the feature and the delta.",
      "Affected accounts and user counts are listed with the alert.",
      "Engineering gets the signal before the case volume arrives.",
      "Clean releases close the watch automatically.",
    ],
    impact: [
      { stat: "Hours", label: "Regression Detection", note: "Versus days of rising case volume" },
      { stat: "Attributed", label: "To a Release", note: "No guesswork on what changed" },
      { stat: "Lower", label: "Post-Release Cases", note: "Fixed before customers report" },
    ],
  },
  {
    name: "Voice of the Admin Synthesis",
    body: "Turn search misses, feedback and abandoned guides into a ranked product gap list refreshed every month.",
    tags: ["Learn"],
    explanation:
      "Unstructured admin behaviour — failed searches, dismissed guides, free-text feedback, repeated help-centre visits — is themed and ranked by affected accounts and revenue exposure into a monthly product gap report.",
    experience: [
      "Product receives a ranked gap list each month.",
      "Each theme carries verbatim examples and affected account counts.",
      "Gaps link back to the underlying signals for validation.",
      "Closed gaps are tracked to confirm the friction actually fell.",
    ],
    impact: [
      { stat: "Monthly", label: "Product Feedback Loop", note: "Behaviour, not opinion" },
      { stat: "Ranked", label: "By Exposure", note: "Accounts and revenue weighted" },
      { stat: "Traceable", label: "Gap to Signal", note: "Every theme has evidence" },
    ],
  },
  {
    name: "Simulation Before Autonomy",
    body: "Dry-run new auto-resolve actions against historical cases to prove accuracy before granting them autonomy.",
    tags: ["Learn", "Platform"],
    explanation:
      "A candidate action is replayed against a corpus of closed cases in shadow mode. Predicted outcomes are scored against the real resolutions, producing precision, false-positive rate and blast-radius estimates before any live execution.",
    experience: [
      "Ops selects a candidate action and runs a simulation.",
      "A scorecard shows accuracy, misses and worst-case blast radius.",
      "Only actions clearing the bar are promoted to a live tier.",
      "Simulations re-run on every runbook change.",
    ],
    impact: [
      { stat: "Proven", label: "Before Live", note: "No autonomy without evidence" },
      { stat: "Measured", label: "Blast Radius", note: "Worst case known in advance" },
      { stat: "Safe", label: "Tier Promotion", note: "Gated on simulation results" },
    ],
  },
  {
    name: "Visual Error Diagnosis",
    body: "An agent watches for failed screens, reads what the user sees, investigates across systems, tests competing explanations and then offers the fix — not just an explanation.",
    tags: ["Top priority", "Detect", "Diagnose", "Resolve", "Customer facing"],
    explanation:
      "A multimodal model reads the failed screen together with the last seconds of session activity that led to it, then the agent goes and fetches what it needs — entitlements, recent config changes, the failing API response, whether other accounts are hitting the same thing right now. It forms two or three candidate causes, tests each against the evidence and reports the winner with what it ruled out. Where the fix is safe and reversible it offers to run it; where the same failure recurs across accounts it clusters the instances and raises them to engineering on its own.",
    experience: [
      "The agent detects the failed state itself and offers an explanation — the user does not have to find a button.",
      "It reads the click-and-state sequence leading up to the error, not a single frozen frame.",
      "It pulls entitlements, config history, the failing API call and concurrent reports across accounts before concluding anything.",
      "Competing causes are tested against each other; the answer names what was ruled out and why.",
      "If the evidence is ambiguous it asks the single question that resolves it, instead of guessing.",
      "Once the cause is known it offers the action — request the entitlement, clear the stuck state, re-run the job — behind one confirm.",
      "When the same failure appears across accounts it clusters them and drafts the engineering write-up with sample screens, before the cases arrive.",
    ],
    impact: [
      { stat: "Resolved", label: "Not Just Explained", note: "Diagnosis ends in a one-click fix" },
      { stat: "2 min", label: "Time to Cause", note: "From failed screen to tested root cause" },
      { stat: "Ahead", label: "Of the Case Queue", note: "Clustered failures raised proactively" },
    ],
  },
  {
    name: "Voice-Enabled Error Capture",
    body: "Users describe the problem out loud instead of writing it up. The system records, transcribes and structures the spoken description into usable error detail.",
    tags: ["Detect", "Diagnose", "Customer facing"],
    explanation:
      "A speech-to-text model transcribes the spoken report in the browser, then a language model extracts the error text, affected feature, timing and repro hints from the transcript and attaches the page and telemetry context captured at the moment of recording.",
    experience: [
      "User hits a problem and taps 'Describe it' instead of typing.",
      "They speak naturally; the transcript appears live as they talk.",
      "Key details — feature, error, when it started — are pulled out and shown for confirmation.",
      "The structured report feeds diagnosis, or a pre-filled case if a human is needed.",
    ],
    impact: [
      { stat: "-70%", label: "Reporting Effort", note: "Speaking replaces written repro steps" },
      { stat: "Richer", label: "Detail Captured", note: "People say more than they type" },
      { stat: "Faster", label: "Time to Report", note: "Friction gets reported instead of abandoned" },
    ],
  },
  {
    name: "AI Knowledge Gap Prioritization",
    body: "Search misses, unresolved questions, case drivers and feedback combine into a ranked list of knowledge content to write or fix.",
    tags: ["Learn", "Platform"],
    explanation:
      "Zero-result and low-confidence queries are clustered by intent, then weighted by the case volume and customer impact attached to each cluster, producing a prioritized authoring queue.",
    experience: [
      "Content owners open a ranked gap list instead of guessing.",
      "Each gap shows the queries, cases and accounts behind it.",
      "Publishing an article closes the gap and the ranking updates.",
      "Impact is measured as deflection on those same queries.",
    ],
    impact: [
      { stat: "Ranked", label: "By Case Volume", note: "Highest-cost gaps first" },
      { stat: "Evidence", label: "Behind Every Gap", note: "Queries and cases attached" },
      { stat: "Measurable", label: "Deflection Lift", note: "Before and after per article" },
    ],
  },
  {
    name: "AI Journey Friction Scoring",
    body: "A per-journey score showing where customers struggle across onboarding, licensing, asset management and case creation.",
    tags: ["Detect", "Learn", "Platform"],
    explanation:
      "Behavioural events are stitched into journeys per account, then scored on drop-off, retries, errors and elapsed time, with the score tracked over time and comparable across accounts.",
    experience: [
      "Each journey carries a score and a trend arrow.",
      "Drilling in shows the exact step where users stall.",
      "Scores compare across segments, regions and account sizes.",
      "Improvements show up as the score moves.",
    ],
    impact: [
      { stat: "One", label: "Comparable Score", note: "Friction measured, not argued" },
      { stat: "Step-level", label: "Diagnosis", note: "Exact stall point identified" },
      { stat: "Trended", label: "Over Releases", note: "Regression visible early" },
    ],
  },
  {
    name: "Zero-Ticket Self-Resolution",
    body: "Once the cause is known, the agent composes and runs the fix in-CSP, proves it worked by re-running the user's own action, and rolls itself back if it did not.",
    tags: ["Top priority", "Resolve", "Customer facing"],
    explanation:
      "Where a runbook matches, it runs. Where none matches, the agent composes a fix from the primitives it is permitted to use rather than falling back to a case. Success is never assumed from an API returning 200 — the agent replays the user's original action and confirms it now completes. A failed verification triggers its own rollback with an account of what was attempted. Autonomy is earned per runbook from its own success history, not fixed by a human tier setting.",
    experience: [
      "The diagnosis card offers a single 'Fix this' action.",
      "No matching runbook is not a dead end — the agent composes one from permitted actions.",
      "The fix is verified by re-running the action the user was trying to complete.",
      "A failed verification rolls itself back and explains what it tried and why it stopped.",
      "Runbooks that keep succeeding gain autonomy; ones that regress lose it automatically.",
    ],
    impact: [
      { stat: "Zero", label: "Tickets Created", note: "Journey closes inside the product" },
      { stat: "Verified", label: "Not Assumed", note: "Success proven by replaying the user's action" },
      { stat: "Self-tuning", label: "Autonomy", note: "Earned from each runbook's own track record" },
    ],
  },
  {
    name: "Background Signal Monitoring",
    body: "Continuously listen for real-time frontend console errors, unhandled JavaScript 500s, dead clicks (representing ~6.28% of user events), rage clicks, and database/API desynchronizations.",
    tags: ["Detect", "Platform"],
    explanation:
      "A lightweight client listener plus server-side event stream feeds a single signal store. Events are normalised, deduplicated and scored against thresholds so only meaningful deviations surface as signals.",
    experience: [
      "Nothing to install or report — monitoring runs silently behind every session.",
      "Friction the user never reported still reaches the team as a signal.",
      "Signals arrive grouped by account, screen and reason code.",
      "Noise stays suppressed: repeated events roll into one row.",
    ],
    impact: [
      { stat: "6.28%", label: "Dead Clicks Captured", note: "Friction invisible in ticket data today" },
      { stat: "Real time", label: "Detection Latency", note: "Seconds from event to signal" },
      { stat: "Fewer", label: "Silent Failures", note: "Unreported breakage becomes visible" },
    ],
  },
  {
    name: "Automated Root Cause Analysis (RCA)",
    body: "The engine does not just correlate what was already collected — it gathers the evidence it needs, tests competing explanations, and says when it is not sure.",
    tags: ["Top priority", "Diagnose"],
    explanation:
      "Starting from product telemetry, identity and entitlement records, CRM data, past cases and recent deployment or config events, the agent issues its own follow-up queries: pulling the specific log window, re-running the failing call in a safe mode, checking whether other accounts share the condition. It holds several hypotheses at once with a confidence on each and states what evidence would disprove them. Below its confidence bar it names the missing evidence instead of committing to a weak cause. When the real fix lands, it compares that against its ranking and updates how it weighs similar evidence next time.",
    experience: [
      "An engineer opens a signal and sees ranked causes with confidence, not raw logs.",
      "The agent fetched what it needed — log windows, safe re-runs, cross-account checks — on its own.",
      "Each hypothesis states the evidence for it and what would rule it out.",
      "Low confidence reads as 'I need this to be sure', never as a guess.",
      "When the actual cause differs, the ranking corrects itself for the next occurrence.",
    ],
    impact: [
      { stat: "-70%", label: "Investigation Time", note: "Evidence gathered before anyone looks" },
      { stat: "Ranked", label: "Competing Hypotheses", note: "With confidence and disproof conditions" },
      { stat: "Improves", label: "With Every Case", note: "Outcomes feed back into the ranking" },
    ],
  },
  {
    name: "Contextual In-App Guidance",
    body: "Guidance is generated from the actual failure and this account's state, adapts when a step does not work, and steps in before the error where the agent can see it coming.",
    tags: ["Top priority", "Guide", "Customer facing"],
    explanation:
      "Instead of looking up a template, the agent writes the explanation from the specific failure, the account's entitlements and configuration, and what the user was trying to do. It guides through a sequence rather than a single banner, changing course when a step does not resolve the problem. Where it can see a user heading into a state that will fail, it intervenes before the error happens. Dismissals, retries and outcomes train it: guidance that does not work stops being shown.",
    experience: [
      "The page keeps working — only the affected card shows guidance.",
      "The explanation is written for this failure and this account, not a generic message.",
      "It walks the user through a sequence and adapts when a step does not resolve it.",
      "When the agent sees the user heading into a failing state, it intervenes first.",
      "Guidance that gets dismissed or fails to resolve is retired automatically.",
    ],
    impact: [
      { stat: "0", label: "Full-Page Crashes", note: "Failures contained to one surface" },
      { stat: "-40%", label: "Abandoned Journeys", note: "Users recover instead of leaving" },
      { stat: "Before", label: "The Error Happens", note: "Intervenes on a predicted failure" },
    ],
  },
  {
    name: "Resolution Path Decisioning",
    body: "Before anything executes, decide the path a signal takes: resolve it in place, run a ranked runbook, or open a case — a case only when a human touch point genuinely adds value.",
    tags: ["Triage", "Resolve", "Customer facing", "Predictive AI Agent"],
    explanation:
      "The agent scores the issue against what resolved the same signature historically: is there a known fix, a safe automated action, a definitive knowledge answer, and is the risk and blast radius acceptable? Where a runbook fits, historical run outcomes (succeeded / failed / rolled back) rank the top two or three with an expected success rate, which the Tier 1/2/3 autonomy model uses to decide automatic execution versus approval. High-confidence, low-risk issues are resolved in place and logged as a deflection; anything ambiguous, risky, contractual or emotionally charged is routed to a person with diagnosis, evidence and recommended action attached.",
    experience: [
      "The customer gets the fix or the definitive answer instead of a case number.",
      "Opening a signal shows two or three ranked runbooks, best first, each with its historical success rate and last outcome.",
      "Approving is one click; the autonomy tier is shown before you commit.",
      "When a person is needed, the case opens already diagnosed — no cold start for the CSR.",
      "The reason a case was or was not opened is recorded and auditable; outcomes feed straight back into the ranking.",
    ],
    impact: [
      { stat: "Case-Free", label: "Resolution by Default", note: "A case only when a human adds value" },
      { stat: "Top 3", label: "Ranked Runbooks", note: "No manual library search" },
      { stat: "Higher", label: "Run Success Rate", note: "Outcome-weighted recommendations" },
      { stat: "Right-First-Time", label: "Routing", note: "Human effort spent where it matters" },
    ],
  },

  {
    name: "Auto-Triage and Case Drafting",
    body: "When a signal becomes a case, AI writes the summary, impact statement, affected accounts, reproduction steps and severity recommendation — so escalations arrive pre-packaged rather than empty. These will be issues which need Human in the Loop",
    tags: ["Top priority", "Triage"],
    explanation:
      "The drafting agent assembles the case from the signal's evidence, then asks for the one missing detail that would otherwise cause a clarification loop. It recommends the route as well as the severity — team, on-call or known issue — with the reasoning shown, and attaches what it already attempted and ruled out so the human starts where the agent stopped. As new signals join the cluster it re-drafts the case unprompted.",
    experience: [
      "Promoting a signal opens a case that is already written.",
      "The agent asks for the one missing detail before the case lands, not after.",
      "Route and severity both come recommended, with the reasoning shown.",
      "What the agent already tried and ruled out travels with the case.",
      "The case re-drafts itself as new signals join the same cluster.",
    ],
    impact: [
      { stat: "-80%", label: "Case Authoring Time", note: "Draft arrives complete" },
      { stat: "Routed", label: "Not Just Written", note: "Team and severity both recommended" },
      { stat: "Fewer", label: "Clarification Loops", note: "Missing detail requested up front" },
    ],
  },
  {
    name: "Signal → Root Cause Explainer",
    body: "For each signal, generate a plain-language \u201cwhat likely broke and why\u201d, citing the events, config changes and similar past cases that support it, plus a confidence score. Turns a raw threshold breach into something an engineer can act on in seconds.",
    tags: ["Diagnose"],
    explanation:
      "A grounded generation step turns the correlated evidence into a short narrative, always citing its sources, and gates publication on a confidence threshold so unsupported explanations are withheld rather than guessed.",
    experience: [
      "Every signal carries a one-paragraph explanation at the top.",
      "Citations link to the events, changes and past cases behind it.",
      "A confidence score sets expectations before anyone acts.",
      "Low-confidence signals are flagged for human investigation instead.",
    ],
    impact: [
      { stat: "Seconds", label: "Time to Understand", note: "Plain language over raw thresholds" },
      { stat: "Cited", label: "Every Claim", note: "Evidence attached to each explanation" },
      { stat: "Gated", label: "Low Confidence", note: "No unsupported explanations shown" },
    ],
  },
  {
    name: "Ask CSP AI (Natural-Language Console)",
    body: "A chat box over your own data: \u201cwhich accounts had entitlement loops last week?\u201d, \u201cwhich runbooks fail most often?\u201d, \u201cshow critical signals with no owner\u201d. Read-only, query-generating, no new pages needed.",
    tags: ["Guide"],
    explanation:
      "Natural-language questions are translated into read-only queries against the signal, case and run tables, executed under the asker's permissions, and returned as tables or charts with the generated query visible.",
    experience: [
      "Ask a question in plain English from any CSP AI Capabilities page.",
      "Results render as a table or chart, with the query shown for trust.",
      "Follow-up questions keep the prior context.",
      "Useful answers can be pinned to a dashboard.",
    ],
    impact: [
      { stat: "No SQL", label: "Access Barrier", note: "Anyone can interrogate the data" },
      { stat: "Read only", label: "Blast Radius", note: "Queries can never mutate state" },
      { stat: "Fewer", label: "Ad-Hoc Report Asks", note: "Self-serve answers replace requests" },
    ],
  },
  {
    name: "Post-Incident Learning Loop",
    body: "After a run resolves, AI writes the retro: what fired, what fixed it, what threshold should change. Accepted suggestions feed back into the Pendo threshold settings.",
    tags: ["Learn", "Platform", "Predictive AI Agent"],
    explanation:
      "On resolution the agent compiles the timeline, the successful action and the noise-to-signal ratio of the triggering threshold, then proposes concrete threshold or runbook changes that an owner can accept or reject.",
    experience: [
      "A retro appears on the case the moment it resolves.",
      "Suggested threshold changes show the before and after effect.",
      "Accepting a suggestion updates the settings directly.",
      "Rejections are recorded so the same suggestion is not repeated.",
    ],
    impact: [
      { stat: "Every", label: "Case Reviewed", note: "Retro written automatically" },
      { stat: "Falling", label: "False Positives", note: "Thresholds tuned from outcomes" },
      { stat: "Compounding", label: "System Accuracy", note: "Each incident improves the next" },
    ],
  },
  {
    name: "Case-Reason-Code Prediction",
    body: "Predict the reason code a signal will turn into, so cases avoided can be measured per code instead of in aggregate.",
    tags: ["Triage", "Predictive AI Agent"],
    explanation:
      "A classifier trained on historical signal-to-case pairs predicts the eventual reason code at signal time, letting deflection be attributed to the specific driver it prevented.",
    experience: [
      "Each signal shows its predicted reason code and likelihood.",
      "Dashboards report cases avoided per reason code.",
      "Owners route work by driver rather than by volume.",
      "Prediction accuracy is tracked against actual outcomes.",
    ],
    impact: [
      { stat: "Per code", label: "Deflection Measured", note: "Attribution instead of aggregate" },
      { stat: "Earlier", label: "Routing Decision", note: "Driver known at signal time" },
      { stat: "Tracked", label: "Model Accuracy", note: "Predicted versus actual code" },
    ],
  },
  {
    name: "Proactive Customer Comms Drafting",
    body: "When a signal predicts customer impact, draft the in-app guide copy or outreach email for approval — closing the loop from detection to prevention.",
    tags: ["Guide", "Predictive AI Agent"],
    explanation:
      "Given an impact prediction and the affected segment, the agent drafts audience-appropriate copy for an in-app guide or email, with the affected account list attached, ready for human approval before anything is sent.",
    experience: [
      "A predicted-impact signal offers a 'Draft comms' action.",
      "The draft arrives with the audience and channel pre-selected.",
      "A human edits and approves before anything sends.",
      "Delivery and engagement come back onto the signal.",
    ],
    impact: [
      { stat: "Ahead", label: "Of the Complaint", note: "Customers told before they notice" },
      { stat: "Minutes", label: "To First Draft", note: "Copy and audience prepared together" },
      { stat: "Human", label: "Approval Gate", note: "Nothing sends unreviewed" },
    ],
  },
  {
    name: "Predict Emerging Problems",
    body: "The agent continuously analyses cases, telemetry, behaviour, product versions, KB searches, error codes, failed troubleshooting, escalations and engineering defects to spot that something is changing — before anyone notices the trend.",
    tags: ["Top priority", "Detect", "Predictive AI Agent", "Customer facing"],
    explanation:
      "Rather than waiting for a threshold to breach on one metric, the agent looks across every stream at once and surfaces the shape of an emerging issue: 47 customers hit a similar authentication failure in the last 24 hours, 83% of them on version X. It then recommends creating a known issue and alerting the affected accounts.",
    experience: [
      "An 'Emerging issue detected' card appears with the population, window and shared conditions.",
      "The correlation is spelled out — version, region, entitlement or configuration in common.",
      "One action drafts the known issue article and the affected-customer alert.",
      "The trend keeps updating as new cases land, and closes itself when it stops growing.",
    ],
    impact: [
      { stat: "24h", label: "Detection Window", note: "Trend named before escalation" },
      { stat: "83%", label: "Version Correlation", note: "Shared condition identified automatically" },
      { stat: "Zero", label: "Human Trigger", note: "Nobody has to notice first" },
    ],
  },
  {
    name: "Runbook Authoring Assistant",
    body: "Generate a draft runbook (steps, guardrails, rollback, blast radius) from a resolved case, so the library grows from real incidents.",
    tags: ["Learn"],
    explanation:
      "The assistant replays the actions taken on a resolved case and converts them into a structured runbook draft with explicit guardrails, rollback path and declared blast radius for review before publication.",
    experience: [
      "A resolved case offers 'Create runbook from this'.",
      "The draft lists the steps actually taken, in order.",
      "Guardrails, rollback and blast radius are pre-filled for editing.",
      "Publishing adds it to the library and the recommender.",
    ],
    impact: [
      { stat: "Growing", label: "Runbook Library", note: "Built from real resolutions" },
      { stat: "Declared", label: "Blast Radius", note: "Every draft carries its limits" },
      { stat: "Hours→min", label: "Authoring Effort", note: "Draft replaces blank page" },
    ],
  },
  {
    name: "Issue Prediction",
    body: "From product, error codes, telemetry, recent changes and previous cases, predict the likely problem — detecting the patterns that show a product or configuration is about to fail.",
    tags: ["Detect", "Predictive AI Agent"],
    explanation:
      "The agent scores each account and configuration against the failure patterns it has seen before: which product and version, which error codes are appearing, what changed recently, and how similar the shape is to past cases. A rising score becomes a named prediction with the evidence behind it.",
    experience: [
      "At-risk accounts appear with a predicted problem and a confidence score.",
      "The evidence is listed — version, error codes, recent change, similar past cases.",
      "A predicted window says how soon failure is likely.",
      "Predictions are scored against what actually happened, so accuracy is visible.",
    ],
    impact: [
      { stat: "Before", label: "The Failure", note: "Risk named while it is still preventable" },
      { stat: "Evidence", label: "Backed Score", note: "Every prediction cites its inputs" },
      { stat: "Tracked", label: "Hit Rate", note: "Predicted versus actual outcome" },
    ],
  },
  {
    name: "Root-Cause Prediction",
    body: "Identify the most likely root cause before a case is ever opened, so the first response starts from a diagnosis rather than a question.",
    tags: ["Diagnose", "Predictive AI Agent"],
    explanation:
      "Correlating telemetry, configuration state and the resolution history of similar cases, the agent ranks candidate root causes with a confidence score and the checks that would confirm or rule each one out.",
    experience: [
      "A predicted signal shows the top candidate causes, ranked.",
      "Each candidate lists the confirming check and the evidence supporting it.",
      "The matching runbook is attached to the leading candidate.",
      "Confirmed and rejected causes feed back to sharpen the ranking.",
    ],
    impact: [
      { stat: "Day one", label: "Diagnosis Ready", note: "Cause proposed before the case opens" },
      { stat: "Ranked", label: "Candidate Causes", note: "With confirming checks attached" },
      { stat: "Shorter", label: "Time to Fix", note: "Investigation starts mid-way" },
    ],
  },
  {
    name: "Incident Correlation",
    body: "Determine whether a customer's problem is related to a known incident, outage, vulnerability or product issue — rather than treating it as a fresh, isolated case.",
    tags: ["Diagnose", "Customer facing"],
    explanation:
      "The agent matches the symptom, product version, region and timing against open incidents, status-page events, published vulnerabilities and known product defects, and attaches the match with a confidence score and the shared conditions that justify it.",
    experience: [
      "A case or signal shows 'Related to a known issue' with the incident named.",
      "The shared conditions are listed — version, region, component, time window.",
      "The known-issue article, workaround and expected fix date are linked.",
      "If no match exists, the case is flagged as a potential new incident.",
    ],
    impact: [
      { stat: "Instant", label: "Known-Issue Match", note: "No duplicate investigation" },
      { stat: "Linked", label: "Workaround & ETA", note: "Answer ready at first response" },
      { stat: "Earlier", label: "New Incident Signal", note: "Unmatched clusters surface fast" },
    ],
  },
  {
    name: "Impact Prediction",
    body: "Estimate the severity and business impact of a problem — how many accounts and users are exposed, what is blocked, and what it is worth fixing first.",
    tags: ["Detect", "Predictive AI Agent"],
    explanation:
      "The agent sizes the blast radius from the affected population, the criticality of the blocked workflow and the value of the accounts involved, then proposes a severity with the reasoning shown so an owner can accept or override it.",
    experience: [
      "Each signal or case carries a predicted severity with the reasoning behind it.",
      "Exposed accounts, users and blocked workflows are quantified.",
      "The queue can be ordered by predicted business impact, not arrival time.",
      "Overrides are captured so the sizing model improves.",
    ],
    impact: [
      { stat: "Sized", label: "Blast Radius", note: "Accounts and users quantified" },
      { stat: "Ranked", label: "By Impact", note: "Highest-value work surfaces first" },
      { stat: "Consistent", label: "Severity Calls", note: "Same standard applied every time" },
    ],
  },
  {
    name: "Next-Best Action Agent",
    body: "Recommend the single most likely action that will resolve the issue — the step to take now, not a list of everything that could be tried.",
    tags: ["Resolve", "Predictive AI Agent"],
    deckUrl:
      "https://docs.google.com/presentation/d/1bWUGRIDSDqMDXXvP3Sd-ch9Q1mWFNcPs/edit?slide=id.p8#slide=id.p8",
    explanation:
      "The agent weighs the predicted cause, the account's configuration and what actually resolved similar cases before, then ranks candidate actions by expected success and shows the evidence behind the top pick.",
    steps: [
      {
        title: "Context & Signal Evaluation",
        note: "Weighs the predicted cause, account configuration, and past resolution outcomes.",
      },
      {
        title: "Success Rate Ranking",
        note: "Ranks candidate actions by expected success probability.",
      },
      {
        title: "Evidence-Backed Pick",
        note: "Displays the top recommendation alongside full supporting evidence.",
      },
    ],
    experience: [
      "Primary Recommendation — a single recommended action appears on the case with its expected success rate.",
      "Alternate Options — two alternates sit behind it for when the primary action does not apply.",
      "Inline Evidence — supporting past cases and config checks are directly linked inline.",
      "Adaptive Ranking — accepting or rejecting the recommendation trains future ranking.",
    ],
    impact: [
      { stat: "One", label: "Clear Next Step", note: "No hunting through options" },
      { stat: "Ranked", label: "By Success Rate", note: "Learned from past resolutions" },
      { stat: "Faster", label: "Time to Resolve", note: "Fewer failed attempts per case" },
    ],
  },
  {
    name: "Next-Best Article",
    body: "Proactively surface the exact KB article or documentation page that answers the problem, instead of making the customer search for it.",
    tags: ["Resolve", "Customer facing"],
    explanation:
      "The agent matches the symptom, product version and context against the knowledge base and documentation using semantic retrieval, and pushes the one passage that answers it — with a confidence gate so a weak match stays hidden rather than misleading.",
    experience: [
      "The relevant article appears in context, before any search is typed.",
      "The answering passage is quoted, not just the article title.",
      "Version and role-appropriate content only — nothing the user cannot act on.",
      "Helpful / not helpful feedback tunes future matches.",
    ],
    impact: [
      { stat: "Zero", label: "Search Needed", note: "Answer arrives in context" },
      { stat: "Gated", label: "By Confidence", note: "Weak matches never shown" },
      { stat: "Higher", label: "Self-Serve Deflection", note: "Fewer cases opened for documented issues" },
    ],
  },
  {
    name: "Next-Best Troubleshooting Step",
    body: "Dynamically generate the next diagnostic step based on what has already been tried, so troubleshooting moves forward instead of repeating itself.",
    tags: ["Resolve"],
    explanation:
      "The agent reads the case history — steps attempted, outcomes observed, data already collected — and proposes the single most informative next check, narrowing the cause space rather than restating a static checklist.",
    experience: [
      "The case shows one suggested next step, with what it will rule in or out.",
      "Steps already attempted are recognised and never suggested again.",
      "Each result reshapes the following suggestion.",
      "The full path taken is captured as a reusable troubleshooting trail.",
    ],
    impact: [
      { stat: "Adaptive", label: "Diagnostic Path", note: "Built from what was already tried" },
      { stat: "Fewer", label: "Repeat Steps", note: "No re-asking for known results" },
      { stat: "Shorter", label: "Investigation", note: "Each step maximises information gained" },
    ],
  },
  {
    name: "Missing Information Predictor",
    body: "Predict everything needed to actually solve an issue — not just case fields, but the diagnostics, access, approvals, environment detail and people required — and secure them upfront on every channel, not only the case form.",
    tags: ["Triage", "Customer facing"],
    explanation:
      "The agent learns from resolved issues what resolution genuinely depended on, then builds a live 'requirements to resolve' checklist for the issue in front of it: the symptom detail, the telemetry and log windows, the account, licence and version state, the permissions or approvals needed to act, and the team that must be involved. It runs wherever the issue surfaces — case form, AI search, chat, community, an escalation, or a signal raised before anyone reports it — and keeps re-evaluating as new facts arrive.",
    experience: [
      "Whatever the entry point — search, chat, case form or a proactive signal — only the questions this issue needs are asked.",
      "Anything already known from telemetry, account, licence or version data is pre-filled, never re-asked.",
      "Blockers beyond information are surfaced early: missing access, a pending approval, an unavailable environment, or the team that needs to be in the room.",
      "A readiness indicator shows what is still outstanding and who owns each item, updating as answers arrive.",
      "Escalations and engineering handoffs arrive complete, with no round trip to gather the basics.",
    ],
    impact: [
      { stat: "Upfront", label: "Complete Intake", note: "Right questions asked once, on any channel" },
      { stat: "Beyond Cases", label: "Access & Approvals Too", note: "Blockers named, not discovered late" },
      { stat: "Fewer", label: "Back-and-Forths", note: "No waiting on missing details" },
      { stat: "Faster", label: "First Response", note: "Work starts on arrival" },
    ],
  },
  {
    name: "File/Log Predictor",
    body: "Determine which logs, TSF bundles, screenshots and configuration files are likely to be needed for this issue, and request or collect them automatically.",
    tags: ["Triage", "Customer facing"],
    explanation:
      "The agent maps the symptom and component to the artefacts that resolved similar cases, then names the exact files, paths and time windows — collecting them automatically where the integration allows.",
    experience: [
      "The case lists the precise artefacts needed, with file names and time ranges.",
      "Available diagnostics are attached automatically without asking.",
      "Upload validation confirms the right file arrived before the case moves on.",
      "Irrelevant or oversized collections are avoided.",
    ],
    impact: [
      { stat: "Named", label: "Exact Artefacts", note: "No guessing which logs to pull" },
      { stat: "Auto", label: "Collected Where Possible", note: "Less customer effort" },
      { stat: "Fewer", label: "Rejected Uploads", note: "Validated at attach time" },
    ],
  },
  {
    name: "Dynamic Article Summaries",
    body: "Instead of returning a list of documents, the answer is an AI-written summary of the relevant knowledge, written for the question that was actually asked.",
    tags: ["Guide", "Customer facing"],
    explanation:
      "Retrieved KB passages are re-ranked against the question and condensed into a short grounded answer with citations back to the source article, so nothing is asserted that the knowledge base does not support.",
    experience: [
      "The user asks in their own words and reads one answer, not ten links.",
      "Every claim carries a citation back to the source article.",
      "The summary adapts to the product and version in context.",
      "A 'show the full article' option is always one click away.",
    ],
    impact: [
      { stat: "400/mo", label: "Cases Deflected", note: "~13% of monthly case volume" },
      { stat: "Seconds", label: "Time to Answer", note: "No reading through article lists" },
      { stat: "Grounded", label: "Cited Sources", note: "Answers traceable to the KB" },
    ],
  },
  {
    name: "Direct KB Deep Links",
    body: "Jump the user to the exact section of the exact article that answers their question, rather than the top of a long document.",
    tags: ["Guide", "Customer facing"],
    explanation:
      "Articles are chunked at heading level and indexed individually, so retrieval returns an anchor rather than a document — the link opens scrolled and highlighted at the passage that matched.",
    experience: [
      "The answer links straight to the paragraph that resolves the issue.",
      "The matched passage is highlighted on arrival.",
      "Related sections are offered as next steps.",
      "Link targets stay valid as articles are re-published.",
    ],
    impact: [
      { stat: "Section", label: "Precision", note: "Anchor-level, not document-level" },
      { stat: "Lower", label: "Abandonment", note: "Fewer users give up mid-article" },
      { stat: "Higher", label: "Self-Serve Success", note: "Answer found on first open" },
    ],
  },
  {
    name: "Live API Lookups",
    body: "Answers grounded in the customer's real state — their licences, entitlements, assets and open cases — read live at question time instead of described generically.",
    tags: ["Diagnose", "Platform", "Customer facing"],
    explanation:
      "The agent calls entitlement, tenant and case APIs under the asking user's own permissions, then reasons over the returned state so the answer reflects this account rather than the documented default.",
    experience: [
      "'Why can't I access this?' returns their actual licence state, not a generic article.",
      "Answers name the specific tenant, asset or seat involved.",
      "Permissions are respected: users only see their own account's data.",
      "Stale or unreachable systems are declared rather than guessed around.",
    ],
    impact: [
      { stat: "800/mo", label: "Cases Deflected", note: "~25% of monthly case volume" },
      { stat: "Account", label: "Specific Answers", note: "Real state, not documentation" },
      { stat: "Fewer", label: "Clarifying Questions", note: "Context already resolved" },
    ],
  },
  {
    name: "Proactive Diagnostics",
    body: "Run a health check across the customer's tenant before they ask — surfacing misconfiguration, expiring entitlements and broken syncs as findings with fixes.",
    tags: ["Detect", "Customer facing"],
    deckUrl:
      "https://docs.google.com/presentation/d/1NcHeR9JU4QKUPkV-IRiEWlgsEjlSj3SP/edit?slide=id.p1#slide=id.p1",
    explanation:
      "A scheduled diagnostic suite evaluates configuration, entitlement and integration state against known failure signatures, scoring each finding by likely customer impact and attaching the remediation path.",
    experience: [
      "A health panel shows what will break before it breaks.",
      "Each finding states the impact and the fix, not just a warning.",
      "Checks re-run automatically after a change is made.",
      "Clean results are shown too, so silence is not ambiguity.",
    ],
    impact: [
      { stat: "Ahead", label: "Of the Incident", note: "Issues found before users hit them" },
      { stat: "Scored", label: "By Impact", note: "Findings ranked, not dumped" },
      { stat: "Fewer", label: "Preventable Cases", note: "Config failures caught early" },
    ],
  },
  {
    name: "Automated State Changes",
    body: "The agent performs the fix itself — reissue a token, re-run a sync, re-apply a licence — as a reversible, audited action rather than instructions for someone else to follow.",
    tags: ["Resolve"],
    explanation:
      "Each remediation is a declared action with scope, preconditions, audit record and rollback. Tier 1 actions execute autonomously; anything wider requires approval before it runs.",
    experience: [
      "The issue is corrected in place, with a record of exactly what changed.",
      "Every action is reversible and attributable.",
      "Out-of-scope changes stop and escalate instead of improvising.",
      "The user is told what was done, not what to do.",
    ],
    impact: [
      { stat: "1300/mo", label: "Cases Deflected", note: "~45% of monthly case volume" },
      { stat: "100%", label: "Audited & Reversible", note: "Every change logged with rollback" },
      { stat: "Minutes", label: "Time to Fix", note: "No handoff to a queue" },
    ],
  },
  {
    name: "1-Click Execution Triggers",
    body: "Where autonomy is not appropriate, the agent prepares the complete action and the admin approves it with a single click.",
    tags: ["Resolve"],
    explanation:
      "The action is fully composed — target, parameters, blast radius and rollback shown up front — and held pending approval, so the human decision is a confirmation rather than a configuration exercise.",
    experience: [
      "A single button presents the exact change and who it affects.",
      "Blast radius and rollback are visible before approving.",
      "Approval and outcome are recorded against the case.",
      "Declined actions leave the diagnosis and evidence intact.",
    ],
    impact: [
      { stat: "1 click", label: "To Execute", note: "No manual steps to reassemble" },
      { stat: "Human", label: "In the Loop", note: "Control kept where risk is higher" },
      { stat: "Faster", label: "Approved Fixes", note: "Ready the moment it is confirmed" },
    ],
  },
  {
    name: "Behavioural Signal Ingestion",
    body: "Pendo behaviour and platform errors stream into one inbox automatically, so friction is recorded the moment it happens rather than when someone reports it.",
    tags: ["Detect", "Platform"],
    explanation:
      "A secured alert endpoint receives Pendo and platform events, applies thresholds so only meaningful behaviour is raised, and suppresses repeats inside a quiet window so one problem becomes one signal.",
    experience: [
      "Rage clicks, drop-offs and failed interactions appear as signals without anyone typing them.",
      "Repeat behaviour folds into the existing signal instead of flooding the inbox.",
      "Each signal carries source, customer and the metric that tripped it.",
      "Daily arrival of real behavioural evidence, not anecdotes.",
    ],
    impact: [
      { stat: "Daily", label: "Live Signal Flow", note: "Friction captured as it happens" },
      { stat: "Lower", label: "Inbox Noise", note: "Thresholds and duplicate suppression" },
      { stat: "0", label: "Manual Entry", note: "Signals create themselves" },
    ],
  },
  {
    name: "Signal Theming and Reason Coding",
    body: "Every signal is classified into a theme, severity and reason code so patterns are visible instead of buried in free text.",
    tags: ["Triage", "Platform"],
    explanation:
      "Incoming summaries are mapped to a stable theme set — case creation, account management, entitlements and the rest — with a severity and reason code that explains the fastest path to resolution.",
    experience: [
      "Inbox groups by theme rather than arrival time.",
      "Severity and reason code are suggested on arrival and editable.",
      "Filtering by theme shows where friction concentrates.",
      "Noise reviews with CSR leads become a short weekly pass.",
    ],
    impact: [
      { stat: "90%", label: "Signals Classified", note: "Theme and reason code on arrival" },
      { stat: "Faster", label: "Pattern Spotting", note: "Grouped rather than individual" },
      { stat: "Shared", label: "Vocabulary", note: "Same language across teams" },
    ],
  },
  {
    name: "Case Resolution Tracking",
    body: "Cases carry a resolution state, notes and close date, so how much actually closed is measurable rather than assumed.",
    tags: ["Resolve", "Platform"],
    explanation:
      "Each case can be marked resolved or reopened with a note and timestamp, and the linked signal resolves with it, producing open, resolved and close-rate reporting straight from live data.",
    experience: [
      "A CSR marks a case resolved and records what fixed it.",
      "Reopening restores context rather than starting again.",
      "Open, resolved and close-rate figures update as work happens.",
      "The linked signal closes alongside the case.",
    ],
    impact: [
      { stat: "Weekly", label: "Close Rate Reported", note: "Straight from recorded outcomes" },
      { stat: "Traceable", label: "Signal to Closure", note: "One thread end to end" },
      { stat: "Evidence", label: "For Runbooks", note: "Resolutions feed later automation" },
    ],
  },
  {
    name: "Friction Baseline and Calibration",
    body: "Establish the starting friction and case-creation rates so every later claim of deflection is measured against a real baseline.",
    tags: ["Learn", "Platform"],
    explanation:
      "Historic case volume, friction rates and behavioural coverage are captured as an agreed baseline, then the impact forecast is calibrated against observed volumes rather than assumed multipliers.",
    experience: [
      "A published baseline everyone references in reviews.",
      "Forecasts track the volumes the business actually sees.",
      "Improvements are attributable to a specific capability.",
      "Targets are set from evidence rather than ambition.",
    ],
    impact: [
      { stat: "Agreed", label: "Baseline Published", note: "Shared starting point" },
      { stat: "Calibrated", label: "Impact Forecast", note: "Tuned to real volumes" },
      { stat: "Defensible", label: "Benefit Claims", note: "Measured against the baseline" },
    ],
  },
  {
    name: "Silent Friction Digest",
    body: "A recurring digest sends the friction nobody reported — abandoned journeys, dead ends and near-misses — to product and IT.",
    tags: ["Learn"],
    explanation:
      "Signals that never became cases are summarised by theme and account impact, ranked by how many customers hit them, and delivered on a daily and weekly cadence to the teams who can remove the cause.",
    experience: [
      "Product receives a ranked list of unreported friction each week.",
      "IT sees which failures are growing before they escalate.",
      "Each entry links to the underlying signals and affected accounts.",
      "Fixes are prioritised by customers affected, not by who shouted.",
    ],
    impact: [
      { stat: "100%", label: "Friction Visibility", note: "Unreported issues surfaced" },
      { stat: "Weekly", label: "Cadence to Product", note: "Consistent feedback loop" },
      { stat: "Ranked", label: "By Customer Impact", note: "Prioritised objectively" },
    ],
  },
  {
    name: "Self-Tuning Thresholds and Autonomy",
    body: "Thresholds and autonomy tiers adjust themselves from outcome data, so the platform gets sharper without a manual retune.",
    tags: ["Learn", "Platform"],
    explanation:
      "Outcomes from signals, runs and case closures feed back into threshold values and autonomy levels: a runbook that keeps succeeding earns more autonomy, a noisy threshold tightens, and every change is logged for review.",
    experience: [
      "Noisy signal types quieten without anyone editing settings.",
      "Consistently successful runbooks progress from suggest to auto.",
      "Every adjustment is recorded with the evidence behind it.",
      "An annual review shows the tuning history and its effect.",
    ],
    impact: [
      { stat: "Automatic", label: "Threshold Tuning", note: "Driven by outcome data" },
      { stat: "Earned", label: "Autonomy Levels", note: "Proven before widened" },
      { stat: "Auditable", label: "Every Change", note: "Logged with rationale" },
    ],
  },
];

// Capabilities sequenced by business impact — biggest deflection and time saved first.
const IMPACT_SEQUENCE = [
  "Zero-Ticket Self-Resolution",
  "Automated Root Cause Analysis (RCA)",
  "Auto-Triage and Case Drafting",
  "Visual Error Diagnosis",
  "Contextual In-App Guidance",
  "Predict Emerging Problems",
  "Product Friction Digest",
  "Proactive Diagnostics",
  "Duplicate, Sibling and Cluster Detection",
  "Comms Orchestrator",
  "Resolution Path Decisioning",
  "Next-Best Action Agent",
  "Automated State Changes",
  "1-Click Execution Triggers",
  "Case Summary on Arrival",
  "Draft Reply with Cited Evidence",
  "Missing Information Predictor",
  "Next-Best Troubleshooting Step",
  "Next-Best Article",
  "Signal → Root Cause Explainer",
  "Incident Correlation",
  "Impact Prediction",
  "Issue Prediction",
  "Root-Cause Prediction",
  "Case-Reason-Code Prediction",
  "File/Log Predictor",
  "Direct KB Deep Links",
  "Dynamic Article Summaries",
  "Live API Lookups",
  "Ask CSP AI (Natural-Language Console)",
  "Proactive Customer Comms Drafting",
  "Voice-Enabled Error Capture",
  "AI Journey Friction Scoring",
  "AI Knowledge Gap Prioritization",
  "Behavioural Signal Ingestion",
  "Background Signal Monitoring",
  "Signal Theming and Reason Coding",
  "Friction Baseline and Calibration",
  "Self-Tuning Thresholds and Autonomy",
  "Simulation Before Autonomy",
  "Runbook Mining from Resolutions",
  "Runbook Authoring Assistant",
  "Post-Incident Learning Loop",
  "Case Resolution Tracking",
  "Silent Friction Digest",
  "Voice of the Admin Synthesis",
  "Release-Aware Regression Watch",
];

const impactRank = (name: string) => {
  const i = IMPACT_SEQUENCE.indexOf(name);
  return i === -1 ? IMPACT_SEQUENCE.length : i;
};

const PILLARS_BY_IMPACT = [...PILLARS].sort((a, b) => impactRank(a.name) - impactRank(b.name));

const TAG_COLORS: Record<string, string> = {
  "Top priority": "border-tag-top/50 bg-tag-top/15 text-tag-top",
  Detect: "border-tag-detect/40 bg-tag-detect/12 text-tag-detect",
  Diagnose: "border-tag-diagnose/40 bg-tag-diagnose/12 text-tag-diagnose",
  Triage: "border-tag-triage/40 bg-tag-triage/12 text-tag-triage",
  Resolve: "border-tag-resolve/40 bg-tag-resolve/12 text-tag-resolve",
  Guide: "border-tag-guide/40 bg-tag-guide/12 text-tag-guide",
  Learn: "border-tag-learn/40 bg-tag-learn/12 text-tag-learn",
  Platform: "border-tag-platform/35 bg-tag-platform/10 text-tag-platform",
  "Customer facing": "border-tag-customer/40 bg-tag-customer/12 text-tag-customer",
  "Predictive AI Agent": "border-tag-predictive/40 bg-tag-predictive/12 text-tag-predictive",
};

function tagClasses(tag: string) {
  return TAG_COLORS[tag] ?? "border-border bg-muted text-muted-foreground";
}

function PillarCard({ pillar }: { pillar: Pillar }) {
  const [open, setOpen] = useState(false);
  return (
    <Card className="border-l-4 border-l-primary">
      <div
        onClick={() => {
          if (typeof window !== "undefined" && window.getSelection()?.toString()) return;
          setOpen((o) => !o);
        }}
        className="w-full cursor-pointer select-text p-5 text-left"
      >
        <div className="flex items-start gap-3">
          <button
            type="button"
            aria-expanded={open}
            aria-label={open ? "Collapse" : "Expand"}
            onClick={(e) => {
              e.stopPropagation();
              setOpen((o) => !o);
            }}
            className="mt-1 shrink-0"
          >
            <ChevronDown className={`size-4 text-primary transition-transform ${open ? "" : "-rotate-90"}`} />
          </button>
          <div className="flex-1">
            <p className="font-display text-xl font-bold">{pillar.name}</p>
            <p className="mt-1 text-sm leading-relaxed text-muted-foreground">
              {pillar.body.split("Human in the Loop").map((part, i, arr) => (
                <span key={i}>
                  {part}
                  {i < arr.length - 1 ? <strong className="font-semibold text-foreground">Human in the Loop</strong> : null}
                </span>
              ))}
            </p>
            <div className="mt-3 flex flex-wrap gap-2">
              {pillar.tags.map((t) => (
                <span
                  key={t}
                  className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold ${tagClasses(t)}`}
                >
                  {t}
                </span>
              ))}
            </div>
            {pillar.deckUrl ? (
              <a
                href={pillar.deckUrl}
                target="_blank"
                rel="noreferrer"
                onClick={(e) => e.stopPropagation()}
                className="mt-3 inline-flex items-center text-sm font-semibold text-primary underline underline-offset-4"
              >
                View deck
              </a>
            ) : null}
          </div>
        </div>
      </div>


      {open && (
        <CardContent className="grid gap-5 border-t border-border p-5">
          <div>
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <ListChecks className="size-4 text-primary" />
              <h4 className="flex-1 text-center font-display text-lg font-bold">How it works</h4>
            </div>
            <p className="mt-4 text-sm leading-relaxed text-muted-foreground">{pillar.explanation}</p>
            {pillar.steps && (
              <ol className="mt-4 space-y-3">
                {pillar.steps.map((s, i) => (
                  <li key={s.title} className="flex gap-3">
                    <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground">
                      {i + 1}
                    </span>
                    <span>
                      <span className="block text-sm font-bold">{s.title}</span>
                      <span className="block text-sm leading-relaxed text-muted-foreground">{s.note}</span>
                    </span>
                  </li>
                ))}
              </ol>
            )}
          </div>

          <div>
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <Layers className="size-4 text-primary" />
              <h4 className="flex-1 text-center font-display text-lg font-bold">User experience</h4>
            </div>
            <div className="mt-4 space-y-2">
              {pillar.experience.map((step, i) => (
                <div key={step}>
                  <div className="rounded-lg border-l-4 border-l-primary/50 bg-muted/40 p-3">
                    <p className="text-sm leading-relaxed text-muted-foreground">{step}</p>
                  </div>
                  {i < pillar.experience.length - 1 && (
                    <div className="flex justify-center py-1">
                      <ArrowDown className="size-3 text-muted-foreground" />
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>

          <div>
            <div className="border-b border-border pb-3">
              <h4 className="text-center font-display text-lg font-bold">Business &amp; IT Impact</h4>
            </div>
            <div className="mt-4 space-y-3">
              {pillar.impact.map((m) => (
                <div key={m.label} className="flex items-center gap-4 rounded-lg border border-border bg-muted/40 p-4">
                  <span className="font-display text-xl font-bold text-primary">{m.stat}</span>
                  <span className="flex-1 text-right">
                    <span className="block text-sm font-bold">{m.label}</span>
                    <span className="block text-sm text-muted-foreground">{m.note}</span>
                  </span>
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      )}
    </Card>
  );
}

function AiIdeas() {
  const [pillarsOpen, setPillarsOpen] = useState(true);
  const [tag, setTag] = useState<string | null>(null);
  const allTags = Array.from(new Set(PILLARS.flatMap((p) => p.tags)));
  const visiblePillars = tag ? PILLARS_BY_IMPACT.filter((p) => p.tags.includes(tag)) : PILLARS_BY_IMPACT;
  return (
    <section className="space-y-6">
      <div className="border-b border-border pb-5">
        <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">Idea backlog</p>
        <h2 className="mt-1 font-display text-3xl font-bold tracking-tight">AI Ideas</h2>
        <p className="mt-2 max-w-3xl text-sm text-muted-foreground">
          Capabilities described by what each one does and the outcome it delivers — for customers, CSRs and the support operation.
        </p>
        <div className="mt-4 flex flex-wrap items-center gap-2">
          
          <button
            type="button"
            onClick={() => setTag(null)}
            aria-pressed={tag === null}
            className={`rounded-full border px-3 py-1 text-xs font-medium transition-colors ${
              tag === null
                ? "border-primary bg-primary text-primary-foreground"
                : "border-border bg-card text-muted-foreground hover:text-foreground"
            }`}
          >
            All ({PILLARS.length})
          </button>
          {allTags.map((t) => {
            const count = PILLARS.filter((p) => p.tags.includes(t)).length;
            const active = tag === t;
            return (
              <button
                key={t}
                type="button"
                onClick={() => setTag(active ? null : t)}
                aria-pressed={active}
                className={`rounded-full border px-3 py-1 text-xs font-semibold transition-colors ${tagClasses(t)} ${
                  active ? "ring-2 ring-primary/40" : "opacity-80 hover:opacity-100"
                }`}
              >
                {t} ({count})
              </button>
            );
          })}
        </div>
      </div>

      <div className="space-y-3">
        <button
          type="button"
          onClick={() => setPillarsOpen((o) => !o)}
          aria-expanded={pillarsOpen}
          className="flex w-full items-center gap-2 text-left text-xs font-bold uppercase tracking-[0.16em] text-muted-foreground transition-colors hover:text-foreground"
        >
          <ChevronDown
            className={`size-4 transition-transform ${pillarsOpen ? "" : "-rotate-90"}`}
          />
          Features
          <span className="font-normal tracking-normal normal-case">({visiblePillars.length})</span>
        </button>
        {pillarsOpen && (
          <div className="grid items-start gap-3 md:grid-cols-2">
            {visiblePillars.map((p) => (
              <PillarCard key={p.name} pillar={p} />
            ))}
          </div>
        )}
      </div>

    </section>
  );
}

const MATURITY = [
  { id: "agentic", label: "Fully Agentic", dot: "bg-maturity-agentic", desc: "Autonomous loops" },
  { id: "enhanced", label: "AI Enhanced", dot: "bg-maturity-enhanced", desc: "Copilot support" },
  { id: "ml", label: "ML Based", dot: "bg-maturity-ml", desc: "Predictive routing" },
  { id: "manual", label: "Manual ", dot: "bg-maturity-manual", desc: "Rep-led touch" },
] as const;

type MaturityId = (typeof MATURITY)[number]["id"];

type JourneyDetail = {
  entails: string;
  impact: string[];
  outcome: string;
  flow: string[];
};

// AI capabilities shown in the first column of each details modal: [title, description].
const JOURNEY_CAPABILITIES: Record<string, [string, string][]> = {
  "AI Search – Welcome Center": [
    ["Semantic Search", "Understands plain-language questions across KB, guides and docs."],
    ["Grounded Answers", "Returns the exact step with its source, not a list of links."],
    ["Role-Aware Results", "Tailors answers to the admin's role, products and entitlements."],
    ["Knowledge Gap Capture", "Logs unanswered questions for authoring."],
  ],
  "Onboarding Agent": [
    ["Prerequisite Validation", "Checks accounts, licences and access before setup starts."],
    ["Missing Info Detection", "Spots gaps in setup data and asks for the one missing detail."],
    ["Next-Step Recommendation", "Ranks the next setup step by likelihood of success."],
    ["Autonomous Setup", "Completes approved setup steps and verifies the result."],
  ],
  "Account & User Data Sanity": [
    ["Account Relationship Mapping", "Builds a clear view of ownership, hierarchy and related organisations."],
    ["Setup Anomaly Detection", "Flags duplicate, fragmented or misaligned account structures."],
    ["Plain-Language Guidance", "Explains which account should be used and what must change."],
    ["Assisted Remediation", "Recommends or executes approved corrections and validates them."],
  ],
  "Proactive Diagnostic Agent": [
    ["Telemetry Monitoring", "Watches asset, sync and API health signals continuously."],
    ["Anomaly Detection", "Spots drift and failures before customers notice."],
    ["Cause Hypothesis Testing", "Tests competing causes against evidence."],
    ["Pre-emptive Fix", "Applies or recommends the fix and confirms recovery."],
  ],
  "Visual Error Diagnosis": [
    ["Screen Understanding", "Reads error screens and screenshots with vision models."],
    ["Error Signature Matching", "Matches the error to known issues and past cases."],
    ["Cross-System Investigation", "Checks account, entitlement and config state for the cause."],
    ["Fix Recommendation", "Offers the fix inline and escalates recurring failures."],
  ],
  "Proactive Integrity Check": [
    ["Early Risk Detection", "Detects entitlement anomalies before customer-visible failure using account, entitlement and coverage signals."],
    ["Contextual Explanation", "Explains in plain language what is wrong, who is affected and what action is needed."],
    ["Automated Integrity Checks", "Continuously validates entitlement ownership, visibility and readiness conditions."],
    ["Guided / Autonomous Remediation", "Recommends or executes approved corrective actions and verifies the outcome."],
  ],
  "Comms Orchestrator": [
    ["Signal Monitoring", "Watches customer and CSP signals for events worth telling."],
    ["Audience Targeting", "Decides who needs to know what, and when."],
    ["Personalised Drafting", "Creates communications tailored to account and role."],
    ["Delivery & Tracking", "Sends via the right channel and tracks acknowledgement."],
  ],
  "NetSec Copilot": [
    ["Conversational Assist", "Answers NetSec product questions in context."],
    ["Config Understanding", "Reads customer configuration to ground answers."],
    ["Troubleshooting Guidance", "Walks through the next best diagnostic step."],
    ["Case Handoff", "Hands off with full context when a human is needed."],
  ],
  "CSR Admin Assist": [
    ["Intent Classification", "Understands the admin request from free text."],
    ["Entity Extraction", "Pulls accounts, users and assets from the request."],
    ["Guardrailed Actions", "Executes approved admin actions within policy."],
    ["Audit Logging", "Records every action for review."],
  ],
  "Next Best Action Agent": [
    ["Context & Signal Evaluation", "Weighs predicted cause, account config and past outcomes."],
    ["Success Rate Ranking", "Ranks candidate actions by expected success."],
    ["Evidence-Backed Pick", "Shows the top action with supporting evidence."],
    ["Adaptive Ranking", "Learns from accepted and rejected recommendations."],
  ],
  "Renewal Risk Agent": [
    ["Usage Analysis", "Reads adoption and usage trends per account."],
    ["Case History Signals", "Weighs support friction as churn risk."],
    ["Expiry Awareness", "Tracks entitlement and contract end dates."],
    ["Churn Risk Scoring", "Flags drifting accounts with the next best action."],
  ],
  "Purchase-to-Activation Gap": [
    ["Unactivated Purchase Detection", "Finds purchased licences, subscriptions and devices that were never activated or deployed."],
    ["Blocker Diagnosis", "Pinpoints why activation stalled: missing registration, entitlement, role or setup step."],
    ["Value-at-Risk Scoring", "Prioritises gaps by contract value and time to renewal."],
    ["Guided Activation", "Sends the admin the exact next step, or completes approved steps automatically."],
  ],
  "Consumption & Credit Agent": [
    ["Burn Forecasting", "Projects credit consumption from usage trends."],
    ["Threshold Alerts", "Warns before an outage or overage."],
    ["Allocation Guidance", "Recommends rebalancing credits across tenants."],
    ["Top-up Recommendation", "Suggests the right purchase ahead of time."],
  ],
  "Renewal & entitlement guidance": [
    ["Entitlement Mapping", "Shows what the customer owns and when it expires."],
    ["Renewal Guidance", "Explains the renewal path in plain language."],
    ["Mismatch Detection", "Flags entitlements that don't match deployment."],
    ["Guided Action", "Links to the exact renewal or transfer step."],
  ],
  "Continuous AI Training": [
    ["Feedback Capture", "Collects outcomes and corrections from every interaction."],
    ["Model Retraining", "Retrains models on validated resolutions."],
    ["Drift Detection", "Spots accuracy decline early."],
    ["Safe Rollout", "Shadow-tests changes before release."],
  ],
  "Audit & Compliance Agent": [
    ["Action Evidence", "Records who did what, when and why."],
    ["Policy Breach Detection", "Flags actions outside policy."],
    ["Audit Pack Assembly", "Prepares FedRAMP and enterprise audit packs."],
    ["Access Review", "Surfaces stale or excessive privileges."],
  ],
  "Agent Performance Supervisor": [
    ["Accuracy Scoring", "Scores every agent against ground truth."],
    ["Autonomy Tuning", "Adjusts thresholds per agent and theme."],
    ["Drift Demotion", "Demotes agents whose accuracy slips."],
    ["Conflict Arbitration", "Resolves conflicting agent recommendations."],
  ],
};

// Six-stage "How it works" framework (overrides flow steps where defined).
const NBA_STAGES: { label: string; items: string[] }[] = [
  { label: "Customer", items: ["Role", "Account", "Journey"] },
  { label: "Context", items: ["Products", "Assets", "Cases", "Entitlements"] },
  { label: "Signals", items: ["Activity", "Telemetry", "Searches", "Lifecycle"] },
  { label: "Decision", items: ["Intent", "Eligibility", "Confidence", "Guardrails"] },
  { label: "Action", items: ["Recommend", "Guide", "Execute", "Escalate"] },
  { label: "Learn", items: ["Outcome", "Feedback", "Effectiveness"] },
];
const JOURNEY_HOW: Record<string, { label: string; items: string[] }[]> = {
  "Next Best Action Agent": NBA_STAGES,
  "Proactive Diagnostic Agent": NBA_STAGES,
};

// CSP-specific scenarios shown in each capability's details modal.
const JOURNEY_SCENARIOS: Record<string, string[]> = {
  "AI Search – Welcome Center": [
    "A new Prisma Access admin asks \"how do I activate my licence?\" and gets the exact activation step instead of a KB list.",
    "A first-time support-portal user searches \"add a user to my account\" and is walked to the right user-management screen for their role.",
    "An admin who cannot find their Cortex XDR tenant is shown the asset-claim flow tied to their own sales order.",
    "A partner admin asks about ASC permissions and receives the entitlement-specific answer for their contract type.",
    "A question with no grounded answer (\"why is my FedRAMP tenant missing?\") is logged as a knowledge gap and routed to authoring.",
  ],
  "Onboarding Agent": [
    "A new account has assets purchased but never claimed — the agent detects it and completes the claim against the sales order.",
    "Setup stalls because no super-user was nominated; the agent identifies the gap and invites the named contact from the order.",
    "A customer uploads a user list with malformed emails — the agent corrects the format and re-runs the bulk invite.",
    "Support-level entitlement is missing on a newly registered device, so the agent applies the correct coverage before the customer notices.",
    "An onboarding that is 60% complete after 14 days triggers the agent to finish the remaining steps and confirm to the admin.",
  ],
  "Account & User Data Sanity": [
    "Two account records exist for the same customer after an acquisition; the agent flags the duplicate and proposes the merge.",
    "A departed employee still holds super-user rights — the agent detects dormant privileged access and recommends revocation.",
    "Domain mismatch between the CSP account and the support contract is caught before it blocks case creation.",
    "Orphaned users left with no account owner after a reorg are reassigned to the current admin.",
    "Mis-mapped customer hierarchies (child account under the wrong parent) are surfaced with the corrected structure.",
  ],
  "Proactive Diagnostic Agent": [
    "A licence activation silently fails in the backend; the agent detects the desync and repairs it before the admin retries.",
    "An expiring support contract on a production firewall is flagged 30 days out with the renewal path attached.",
    "A broken sync between CSP and Salesforce entitlements is detected on a health check and corrected automatically.",
    "A device registered with the wrong serial/SKU pairing is caught and the correct asset record proposed.",
    "Repeated 403s on the licence API for one account are diagnosed as a role misconfiguration, not an outage.",
  ],
  "Visual Error Diagnosis": [
    "An admin sees a blank Asset page — the agent reads the failed screen, traces it to a missing entitlement and offers the fix.",
    "The licence download button returns an error toast; the agent tests permission, contract and asset state and names the real cause.",
    "A customer screenshots a \"no access\" message; the agent matches it to the known role-scope issue and corrects the role.",
    "Repeated dead clicks on the Add Device screen are diagnosed as a validation rule mismatch and raised to engineering.",
    "An intermittent spinner on the Support Cases list is tied to a slow downstream call, with the workaround shown inline.",
  ],
  "Proactive Integrity Check": [
    "The agent spots a firewall reporting under an expired support contract and surfaces the coverage gap.",
    "Unregistered devices sitting in a customer's order are detected and offered as a one-click registration.",
    "Duplicate asset entries from a double RMA are identified and consolidated.",
    "Assets transferred to a new account but still linked to the old one are flagged with the corrected ownership.",
    "Credits burning faster than forecast on a Cortex tenant trigger an early warning to the admin.",
  ],
  "Comms Orchestrator": [
    "A Prisma Access maintenance window affects 40 accounts; only impacted admins are notified, in their preferred channel.",
    "A licensing outage is resolved — the agent sends the all-clear only to those who hit the error.",
    "A customer with an expiring FedRAMP entitlement receives a personalised renewal notice with their exact assets listed.",
    "A case pending customer input for 5 days triggers a tailored nudge rather than a generic reminder.",
    "Acknowledgement is tracked: admins who never opened the critical advisory are re-contacted through a second channel.",
  ],
  "NetSec Copilot": [
    "An admin troubleshooting a Prisma Access tunnel gets asset-aware guidance based on their own deployment.",
    "A failed software upgrade on a PAN-OS device returns the exact rollback and retry sequence for that version.",
    "Licence mismatch between purchased and deployed NetSec SKUs is explained with the correction step.",
    "A customer asks why a device is unreachable and the copilot checks registration, entitlement and telemetry in one pass.",
    "Recurring configuration errors across a customer's devices are summarised into a single remediation plan.",
  ],
  "CSR Admin Assist": [
    "A CSR opens an admin case in Salesforce and sees the triage summary, account state and suggested response already drafted.",
    "A user-access request arrives with details missing; the assistant asks the customer for the one missing field.",
    "The CSR gets a step-by-step runbook for an entitlement transfer instead of searching internal docs.",
    "Duplicate cases from the same customer are clustered so the CSR answers once.",
    "A complex licensing case is auto-routed to the licensing queue with severity and reason code pre-set.",
  ],
  "Next Best Action Agent": [
    "On a licence-activation case, the single highest-success action — re-apply entitlement from the order — is recommended with its success rate.",
    "For an asset-visibility complaint, the agent ranks \"re-sync asset feed\" above \"re-register device\" based on past outcomes.",
    "A case about a missing user shows the primary action plus two alternates for different role configurations.",
    "Past resolved cases and the account's config checks are linked inline as evidence behind the pick.",
    "A CSR rejecting the recommendation trains the ranking for that reason code going forward.",
  ],
  "Renewal Risk Agent": [
    "Declining Cortex XDR console logins in the last 90 days flag an account drifting toward non-renewal.",
    "A customer with rising severity-1 cases and an expiry in 60 days is surfaced with a save play.",
    "Unclaimed entitlements on a renewing contract are flagged as unrealised value before the renewal conversation.",
    "An account that never completed onboarding is escalated ahead of its first renewal.",
    "Support sentiment plus open escalations combine into a churn-risk score with the next best action.",
  ],
  "Purchase-to-Activation Gap": [
    "A Prisma Access licence bought 90 days ago has never been activated; the admin gets the activation steps with the blocker identified.",
    "Firewalls shipped and delivered but never registered are flagged with a one-click registration path.",
    "A Cortex XDR tenant was provisioned but no agents deployed — the account is escalated for onboarding help.",
    "Add-on subscriptions sitting unused on a renewing contract are surfaced as unrealised value before renewal.",
    "An activation stuck on a missing admin role is diagnosed and the correct role assignment proposed.",
  ],
  "Consumption & Credit Agent": [
    "Credit burn on a Cortex tenant is forecast to exhaust two months early; the admin is warned with the top consumers.",
    "An unexpected ingestion spike is flagged with the source before it turns into an overage bill.",
    "Under-consumption against a purchased commit is surfaced ahead of the renewal review.",
    "A customer nearing their log-storage ceiling is offered the expansion path before ingestion stops.",
    "Credits sitting unallocated across child accounts are highlighted for redistribution.",
  ],
  "Renewal & entitlement guidance": [
    "An admin sees exactly which devices lose coverage at expiry, with the renewal quote path attached.",
    "Mixed-term entitlements across one account are consolidated into a single guided renewal view.",
    "A customer upgrading SKUs is shown the entitlement impact on existing assets before they commit.",
    "Expired support on a device blocking case creation is explained with the reinstatement step.",
    "Partner-sold entitlements are routed to the ASC contact rather than the end customer.",
  ],
  "Continuous AI Training": [
    "Deflected licensing answers that later reopened as cases are fed back to retrain the routing model.",
    "A drop in first-person-fix on asset cases triggers a review of the responsible runbooks.",
    "CSR overrides of suggested severity are mined weekly to correct the classification thresholds.",
    "Knowledge gaps from unanswered Welcome Center searches become authoring tasks with volume ranking.",
    "Autonomy for an agent is raised only after its accuracy holds above threshold for a full quarter.",
  ],
  "Audit & Compliance Agent": [
    "Every automated entitlement change is evidenced with actor, reason and rollback path for audit.",
    "A FedRAMP customer's access review pack is assembled without a manual data pull.",
    "An agent action that bypassed a guardrail is flagged as a policy breach the same day.",
    "Privileged role grants outside policy windows are reported with the approving identity.",
    "Quarterly enterprise audit evidence for admin case handling is compiled and exported on schedule.",
  ],
  "Agent Performance Supervisor": [
    "The self-resolution agent's rollback rate rises; the supervisor demotes it to shadow mode automatically.",
    "Routing accuracy per reason code is scored weekly, with drifting codes sent back to human routing.",
    "Deflections that reopened within 7 days are attributed to the specific agent that made the call.",
    "An agent consistently above target on licensing cases is granted higher autonomy for that theme only.",
    "Conflicting recommendations between two agents on the same case are detected and arbitrated.",
  ],
};


const JOURNEY_PHASES: {
  phase: string;
  accent: string;
  cards: { name: string; note: string; level: MaturityId; detail: JourneyDetail }[];
}[] = [
  {
    phase: "1. Onboard",
    accent: "bg-primary",
    cards: [
      {
        name: "AI Search – Welcome Center",
        note: "Improves customer onboarding experience and operational efficiency.",
        level: "ml",
        detail: {
          entails:
            "A semantic search and welcome surface over the knowledge base, activation guides and account state, so a first-time admin gets the answer instead of a list of links.",
          impact: [
            "15–25% structural deflection of onboarding and activation cases",
            "Faster first successful login and first completed setup task",
            "Fewer duplicate 'how do I start' tickets in the admin queue",
          ],
          outcome:
            "New admins self-serve their first-week questions without opening a case.",
          flow: [
            "Admin lands in CSP for the first time and sees the welcome surface.",
            "They ask a question in plain language or pick a suggested task.",
            "Search returns the grounded answer with a direct link to the exact step.",
            "Unanswered questions are logged as knowledge gaps for authoring.",
          ],
        },
      },
      {
        name: "Onboarding Agent",
        note: "Guides customers through onboarding, validates prerequisites, detects missing information, and recommends or completes the next setup step autonomously.",
        level: "agentic",
        detail: {
          entails:
            "An agent that owns the whole onboarding journey: it reads where the account is in setup — users invited, assets claimed, entitlements applied — validates prerequisites, detects missing information, and either recommends or performs the next step.",
          impact: [
            "Shorter time-to-value on new accounts",
            "Fewer stalled onboardings handed to a CSR to rescue",
            "Fewer onboarding cases raised in the first 30 days",
            "Consistent activation quality regardless of who set the account up",
          ],
          outcome:
            "Onboarding finishes itself instead of stalling silently, and new accounts reach a fully working state without a human shepherding them.",
          flow: [
            "Agent evaluates account state after each setup event.",
            "It validates prerequisites and requests any missing information in one clear prompt.",
            "It picks the highest-value next step and explains why.",
            "Low-risk steps run automatically; sensitive ones wait for a click.",
            "It verifies the result and shows progress or any blocker on the welcome surface.",
          ],
        },
      },
    ],

  },
  {
    phase: "2. Provisioning",
    accent: "bg-accent-foreground",
    cards: [
      {
        name: "Account & User Data Sanity",
        note: "Guided setup of accounts, roles and access with agent-run validation.",
        level: "ml",
        detail: {
          entails:
            "Continuous validation of account hierarchy, duplicate records, orphaned admins, stale roles and mismatched access, with the correction proposed inline.",
          impact: [
            "Fewer access and permission cases downstream",
            "Cleaner account data for entitlement and renewal decisions",
            "Less manual data cleanup by support and operations",
          ],
          outcome: "Account and user data stays correct without a periodic cleanup project.",
          flow: [
            "Validation runs on every account or user change.",
            "Anomalies are grouped by account with a suggested fix.",
            "Owner approves the fix, or it applies automatically where rules allow.",
            "Corrections are logged for audit.",
          ],
        },
      },
      {
        name: "Proactive Diagnostic Agent",
        note: "Continuously runs health checks across account, asset and integration state, finding faults before the customer hits them.",
        level: "agentic",
        detail: {
          entails:
            "An agent that runs standing diagnostics over configuration, entitlements, sync jobs, API connections and telemetry, and pinpoints the fault with scoped, reversible repair.",
          impact: [
            "Fewer outage-shaped cases caused by silent misconfiguration or a broken pipe",
            "Lower MTTR on configuration and integration failures",
            "Less silent data drift between systems",
          ],
          outcome: "Faults are found and fixed before the customer sees them.",
          flow: [
            "Agent runs continuous diagnostics across setup, entitlements and connections.",
            "It diagnoses cause — credentials, schema, rate limit, downstream outage, bad config.",
            "It repairs within its scope and verifies the result.",
            "If it cannot fix it, it escalates with full evidence.",
          ],
        },
      },
      {
        name: "Visual Error Diagnosis",
        note: "Reads the failing screen, error banner or screenshot, correlates it with telemetry and explains the real cause.",
        level: "agentic",
        detail: {
          entails:
            "A multimodal agent that interprets what the user is seeing — a failed configuration screen, an error banner, a pasted screenshot — and correlates it with Pendo events, API errors and account state to name the actual cause.",
          impact: [
            "Removes the back-and-forth of asking customers to describe an error",
            "Cuts diagnosis time during configuration and activation",
            "Catches recurring setup failures and escalates them as product defects",
          ],
          outcome:
            "The customer gets the cause and the fix from the screen they are stuck on, without opening a case.",
          flow: [
            "User hits a failure while configuring or activating, or uploads a screenshot.",
            "Agent reads the visual and pulls matching telemetry for that session and account.",
            "It tests competing explanations and states the most likely cause with confidence.",
            "It offers the fix inline, or escalates with evidence when the pattern repeats.",
          ],
        },
      },
    ],
  },
  {
    phase: "3. Operate & monitor",
    accent: "bg-destructive",
    cards: [
      {
        name: '"Ask CSP"',
        note: "Customer-facing AI assistant handling admin requests.",
        level: "agentic",
        detail: {
          entails:
            "An account-aware conversational assistant that answers questions about accounts, assets, entitlements and cases using live lookups, not generic documentation.",
          impact: [
            "15–25% target deflection of administrative cases",
            "Answers in seconds instead of a case round trip",
            "Lower admin ticket load on CSRs",
          ],
          outcome: "Admins get account-specific answers without opening a case.",
          flow: [
            "Admin asks a question in the portal.",
            "The assistant retrieves live account and asset data plus grounded articles.",
            "It answers with confidence gating, or offers the action it can run.",
            "Low-confidence questions are handed to a case with context attached.",
          ],
        },
      },
      {
        name: "Proactive Integrity Check",
        note: "Entitlement integrity: detect issues before customer impact.",
        level: "ml",
        detail: {
          entails:
            "Moves CSP from reactive entitlement troubleshooting to proactive detection and resolution. Today customers discover entitlement issues only when activation, downloads, support eligibility or coverage checks fail — CSP has the data, but issues are detected too late and need manual investigation across account, entitlement and order state. AI replaces manual investigation with automated integrity analysis, human root-cause explanation with AI-generated explanations, and reactive correction after failure with proactive or agentic remediation.",
          impact: [
            "Higher post-purchase success — more customers complete post-purchase workflows without friction",
            "Fewer entitlement-related cases — less TAC/CSR and operations effort on late-discovered issues",
            "Faster time-to-value — customers access products and services sooner after purchase",
            "Better lifecycle health — the foundation for proactive account health management",
          ],
          outcome: "Entitlement issues are detected and resolved before the customer is impacted.",
          flow: [
            "Signal Ingestion (inputs): pulls CSP account, order, entitlement, support-coverage, expiration and visibility signals.",
            "Entitlement Integrity Agent (core AI): correlates anomalies, identifies likely failure conditions, prioritises impacted customers and determines the best remediation path.",
            "Resolution & Verification (output): notifies users, opens guided tasks or automatically executes approved fixes, then verifies the entitlement is healthy.",
          ],
        },
      },
      {
        name: "Comms Orchestrator",
        note: "Continuously monitors customer and CSP signals, determines who needs to know what and when, creates personalized communications, delivers them through the right channel, and tracks customer acknowledgement and follow-up actions.",
        level: "agentic",
        detail: {
          entails:
            "Continuously monitors customer and CSP signals, determines who needs to know what and when, creates personalized communications, delivers them through the right channel, and tracks customer acknowledgement and follow-up actions.",
          impact: [
            "Fewer 'any update?' follow-up contacts",
            "Consistent tone and accuracy across channels",
            "CSR time returned from manual status writing",
          ],
          outcome: "Customers stay informed without a rep writing each update.",
          flow: [
            "The agent monitors customer and CSP signals continuously.",
            "It determines who needs to know what, and when.",
            "It creates a personalized message and delivers it on the right channel.",
            "It tracks acknowledgement and follows up on any outstanding action.",
          ],
        },
      },
    ],
  },
  {
    phase: "4. Support & resolve",
    accent: "bg-status-progress",
    cards: [
      {
        name: "NetSec Copilot",
        note: "Support assistant that helps customers diagnose and resolve issues through contextual guidance and asset-aware troubleshooting.",
        level: "enhanced",
        detail: {
          entails:
            "A guided troubleshooting companion that knows the customer's assets and walks them through diagnosis step by step.",
          impact: [
            "Higher first-person-fix rate",
            "Shorter diagnosis time before a case is opened",
            "Fewer cases opened with insufficient information",
          ],
          outcome: "Customers resolve more issues themselves, and escalate better ones.",
          flow: [
            "Customer describes the symptom or hits an error.",
            "Copilot pulls asset and configuration context.",
            "It proposes the most likely cause and the check to confirm it.",
            "If unresolved, it hands a fully evidenced case to support.",
          ],
        },
      },
      {
        name: "CSR Admin Assist",
        note: "Equips internal CSR agents with real-time AI triage and inline response tools.",
        level: "agentic",
        detail: {
          entails:
            "Inline triage, drafted responses, suggested routing and one-click administrative actions in the CSR workspace.",
          impact: [
            "40% MTTR reduction target on administrative cases",
            "Fewer mis-routes and re-assignments",
            "Consistent resolutions across the CSR team",
          ],
          outcome: "CSRs spend their time on judgement, not on assembling context.",
          flow: [
            "A case arrives and is triaged with theme, severity and route.",
            "Evidence and history are summarised for the CSR.",
            "A response and the recommended action are drafted.",
            "CSR approves, edits or overrides — the override trains the model.",
          ],
        },
      },
      {
        name: "Next Best Action Agent",
        note: "Continuously evaluates customer context and recommends or executes the most relevant action across CSP.",
        level: "agentic",
        detail: {
          entails:
            "A decisioning agent across the whole CSP surface, choosing the single highest-value action for this customer right now.",
          impact: [
            "Higher resolution rate per interaction",
            "Fewer back-and-forth touches per case",
            "Consistent next steps regardless of who is handling it",
          ],
          outcome: "Every interaction ends with the right action taken, not just advised.",
          flow: [
            "Agent reads case, account, asset and behavioural context.",
            "It ranks candidate actions with confidence and expected impact.",
            "It executes within its autonomy tier or recommends to a human.",
            "The result is measured and fed back into ranking.",
          ],
        },
      },
    ],
  },

  {
    phase: "5. Renew & expand",
    accent: "bg-status-delivered",
    cards: [
      {
        name: "Purchase-to-Activation Gap",
        note: "Entitlement utilization: activate and allocate the value purchased, using CSP purchase counts and activation/allocation reports to find underconsumed entitlements before value is lost.",
        level: "ml",
        detail: {
          entails:
            "Customers may not activate all purchased entitlements or may under-consume the usage count they bought. CSP has the original purchased license count and receives activation/allocation reports showing what was activated, but gaps are not always surfaced early enough. AI brings purchased-vs-activated analysis, underutilization detection of unused or partially activated entitlements, value realization guidance on the next-best activation or allocation action, and agentic follow-through with guided tasks and owner notifications. AI replaces manual purchase vs. activation comparison with automated utilization analysis, human identification of gaps with AI-driven underuse detection, and passive reporting with proactive value-realization guidance.",
          impact: [
            "Higher entitlement utilization — more of what the customer bought is activated, allocated and used",
            "Less value leakage — reduces underconsumption that weakens perceived value and expansion readiness",
            "Better adoption visibility — customer teams get a clear view of unused purchased capacity",
            "Stronger renewal confidence — connects purchased value to actual usage before the sales-led renewal motion",
          ],
          outcome: "Purchased entitlements are activated, allocated and used before value is lost.",
          flow: [
            "License Signal Ingestion (inputs): pulls purchased count, entitlement details, activation status, allocation report and account context.",
            "Utilization Insight Agent (core AI): compares purchased vs. activated and allocated counts, identifies gaps and prioritises value leakage risk.",
            "Guide, Allocate & Verify (output): alerts owners and guides activation or allocation, then verifies the gap is closed.",
          ],
        },
      },
      {
        name: "Coverage Continuity",
        note: "Entitlement utilization: activate and allocate the value purchased, using CSP purchase counts and activation/allocation reports to find underconsumed entitlements before value is lost.",
        level: "ml",
        detail: {
          entails:
            "Customers may not activate all purchased entitlements or may under-consume the usage count they bought. CSP has the original purchased license count and receives activation/allocation reports showing what was activated, but gaps are not always surfaced early enough. AI brings purchased-vs-activated analysis, underutilization detection of unused or partially activated entitlements, value realization guidance on the next-best activation or allocation action, and agentic follow-through with guided tasks and owner notifications. AI replaces manual purchase vs. activation comparison with automated utilization analysis, human identification of gaps with AI-driven underuse detection, and passive reporting with proactive value-realization guidance.",
          impact: [
            "Higher entitlement utilization — more of what the customer bought is activated, allocated and used",
            "Less value leakage — reduces underconsumption that weakens perceived value and expansion readiness",
            "Better adoption visibility — customer teams get a clear view of unused purchased capacity",
            "Stronger renewal confidence — connects purchased value to actual usage before the sales-led renewal motion",
          ],
          outcome: "Purchased entitlements are activated, allocated and used before value is lost.",
          flow: [
            "License Signal Ingestion (inputs): pulls purchased count, entitlement details, activation status, allocation report and account context.",
            "Utilization Insight Agent (core AI): compares purchased vs. activated and allocated counts, identifies gaps and prioritises value leakage risk.",
            "Guide, Allocate & Verify (output): alerts owners and guides activation or allocation, then verifies the gap is closed.",
          ],
        },
      },
      {
        name: "Renewal & entitlement guidance",
        note: "Expiry, consumption and entitlement signals turned into next-best actions.",
        level: "manual",
        detail: {
          entails:
            "Consolidated view of expiry dates, consumption and entitlement coverage, turned into recommended renewal actions.",
          impact: [
            "Fewer lapsed entitlements",
            "Less manual renewal preparation",
            "Clearer expansion signals for account teams",
          ],
          outcome: "Renewal decisions are made from one evidenced view.",
          flow: [
            "Expiry and consumption signals are collected per account.",
            "Gaps and risks are ranked.",
            "A recommended renewal action is produced for the owner.",
            "Actions taken are recorded against the account.",
          ],
        },
      },
    ],
  },
  {
    phase: "6. Govern & report",
    accent: "bg-foreground",
    cards: [
      {
        name: "Continuous AI Training",
        note: "Ongoing telemetry and verification loop for routing and audit engine performance accuracy.",
        level: "enhanced",
        detail: {
          entails:
            "A verification loop that compares every AI decision against the eventual human outcome and retrains on the difference.",
          impact: [
            "Accuracy that improves rather than drifts",
            "Evidence for raising autonomy safely",
            "Faster detection of model regression",
          ],
          outcome: "The platform earns more autonomy from measured accuracy.",
          flow: [
            "Every decision is logged with its confidence.",
            "The final human outcome is captured.",
            "Disagreements are reviewed and become training data.",
            "Accuracy per capability is published.",
          ],
        },
      },
      {
        name: "Audit & Compliance Agent",
        note: "Evidences who did what, flags policy breaches and prepares FedRAMP/enterprise audit packs.",
        level: "agentic",
        detail: {
          entails:
            "A complete, queryable action record across humans and agents, with policy checks and pre-assembled audit evidence.",
          impact: [
            "Audit preparation time cut from weeks to hours",
            "Policy breaches caught at the time of action",
            "Enterprise and FedRAMP evidence always current",
          ],
          outcome: "Audit readiness is a standing state, not a project.",
          flow: [
            "Every action is recorded with actor, reason and result.",
            "Policy rules are evaluated as actions happen.",
            "Breaches are flagged to the owner immediately.",
            "Audit packs are generated on request.",
          ],
        },
      },
      {
        name: "Agent Performance Supervisor",
        note: "Scores every other agent's accuracy, tunes autonomy thresholds and demotes agents that drift.",
        level: "agentic",
        detail: {
          entails:
            "Supervision of the agent estate: accuracy scoring, autonomy threshold tuning, and automatic demotion when an agent drifts.",
          impact: [
            "Contained blast radius from a misbehaving agent",
            "Autonomy granted on evidence, not optimism",
            "One place to see how the estate is performing",
          ],
          outcome: "Agents are governed the way staff are — on measured performance.",
          flow: [
            "Supervisor scores each agent on accuracy, overrides and reopens.",
            "Thresholds are tuned per agent from its own history.",
            "Drifting agents are demoted to human-in-the-loop.",
            "Changes and reasons are reported to the owner.",
          ],
        },
      },
    ],
  },
];


function MaturityDots({ level }: { level: MaturityId }) {
  return (
    <span className="flex shrink-0 items-center gap-1">
      {MATURITY.map((m) => (
        <span
          key={m.id}
          title={m.label}
          className={`size-2.5 rounded-full border ${
            m.id === level ? `${m.dot} border-transparent` : "border-border bg-transparent"
          }`}
        />
      ))}
    </span>
  );
}

const IN_FLIGHT = new Set<string>([
  "AI Search – Welcome Center",
  '"Ask CSP"',
  "NetSec Copilot",
  "CSR Admin Assist",
]);

function AiEvolution() {
  const [open, setOpen] = useState<string | null>(null);
  return (

    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="font-display text-2xl font-bold tracking-tight">
            CSP Capabilities <span className="text-primary">| AI Transformation Map</span>
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Mapping today&apos;s CSP capabilities to the path from manual operations → AI-assisted → fully agentic
            experiences.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-4 rounded-full border border-border bg-card px-4 py-2">
          {MATURITY.map((m) => (
            <span key={m.id} className="flex items-center gap-2 text-xs font-semibold">
              <span className={`size-2.5 rounded-full ${m.dot}`} />
              {m.label}
            </span>
          ))}
          <span className="flex items-center gap-2 text-xs font-semibold">
            <span className="h-3 w-5 rounded-sm border-2 border-solid border-chart-2" />
            In- Flight
          </span>
          <span className="flex items-center gap-2 text-xs font-semibold">
            <span className="h-3 w-5 rounded-sm border border-dashed border-border" />
            Proposed capability
          </span>
        </div>
      </div>

      <div className="overflow-x-auto">
        <div className="flex min-w-[1100px] gap-4">
          {JOURNEY_PHASES.map((col) => (
            <div key={col.phase} className="flex-1 space-y-3">
              <div className="flex items-center gap-2 rounded-xl border border-border bg-card px-3 py-3">
                <span className={`h-6 w-1.5 rounded-full ${col.accent}`} />
                <span className="text-sm font-bold">{col.phase}</span>
              </div>
              <div className="min-h-[320px] space-y-3 rounded-xl border border-border bg-muted/30 p-3">
                {col.cards.map((c) => {
                  const key = `${col.phase}::${c.name}`;
                  const funded = IN_FLIGHT.has(c.name);
                  return (
                    <button
                      key={key}
                      type="button"
                      onClick={() => setOpen(key)}
                      className={`w-full rounded-xl bg-card p-3 text-left transition-colors ${
                        funded
                          ? "border-2 border-solid border-chart-2 hover:border-primary"
                          : "border border-dashed border-border hover:border-primary"
                      }`}
                    >
                      <div className="flex items-start justify-between gap-2">
                        <p className="text-sm font-bold">{c.name}</p>
                        <MaturityDots level={c.level} />
                      </div>
                      <p className="mt-2 text-xs leading-relaxed text-muted-foreground">{c.note}</p>
                      <span className="mt-2 inline-flex items-center gap-1 text-[11px] font-bold uppercase tracking-[0.12em] text-primary">
                        <ChevronDown className="size-3 -rotate-90" />
                        Details
                      </span>
                    </button>
                  );
                })}

              </div>
            </div>
          ))}
        </div>
      </div>

      <Dialog open={open !== null} onOpenChange={(o) => !o && setOpen(null)}>
        <DialogContent className="max-h-[90vh] w-[95vw] max-w-6xl overflow-y-auto sm:max-w-6xl">
          {(() => {
            const active = JOURNEY_PHASES.flatMap((col) =>
              col.cards.map((c) => ({ ...c, phase: col.phase, key: `${col.phase}::${c.name}` })),
            ).find((c) => c.key === open);
            if (!active) return null;
            const scenarios = JOURNEY_SCENARIOS[active.name] ?? [];
            const flow = active.detail.flow;
            const stepTag = (i: number) =>
              i === 0
                ? { label: "Inputs", cls: "bg-accent text-accent-foreground", bar: "border-l-primary" }
                : i === flow.length - 1
                  ? { label: "Output", cls: "bg-tag-learn/15 text-tag-learn", bar: "border-l-tag-learn" }
                  : { label: "Core AI", cls: "bg-status-delivered text-status-delivered-foreground", bar: "border-l-tag-resolve" };
            const colCls = "space-y-3 rounded-xl border border-border bg-muted/30 p-4";
            const colHead = "border-b border-border pb-2 font-display text-lg font-bold";
            return (
              <>
                <DialogHeader>
                  <div className="flex items-center gap-3">
                    <DialogTitle className="font-display text-xl font-bold">{active.name}</DialogTitle>
                    <MaturityDots level={active.level} />
                  </div>
                  <DialogDescription>{active.phase} · {active.note}</DialogDescription>
                </DialogHeader>

                <div className="flex flex-col gap-3 rounded-xl border border-destructive/30 bg-destructive/5 p-4 sm:flex-row sm:items-center">
                  <div className="flex shrink-0 items-center gap-2 font-display text-lg font-bold text-destructive sm:w-48 sm:border-r sm:border-destructive/30">
                    <AlertTriangle className="size-5" /> Overview
                  </div>
                  <p className="text-sm leading-relaxed text-foreground">{active.detail.entails}</p>
                </div>

                <div className="grid gap-4 md:grid-cols-3">
                  <div className={colCls}>
                    <p className={`${colHead} flex items-center gap-2`}>
                      <span className="font-extrabold text-primary">AI</span> AI Capabilities
                    </p>
                    {(JOURNEY_CAPABILITIES[active.name] ?? []).map(([title, desc]) => (
                      <div key={title} className="flex gap-3 rounded-lg border border-border bg-card p-3">
                        <span className="mt-0.5 flex size-7 shrink-0 items-center justify-center rounded-full bg-accent text-accent-foreground">
                          <Sparkles className="size-3.5" />
                        </span>
                        <div>
                          <p className="text-sm font-bold">{title}</p>
                          <p className="text-xs leading-relaxed text-muted-foreground">{desc}</p>
                        </div>
                      </div>
                    ))}
                  </div>

                  <div className={colCls}>
                    <p className={`${colHead} flex items-center gap-2`}>
                      <Command className="size-5 text-primary" /> How It Works
                    </p>
                    {JOURNEY_HOW[active.name] ? (
                      <>
                        {JOURNEY_HOW[active.name]!.map((st, i) => {
                          const bar = i < 3 ? "border-t-primary text-primary" : i === 3 ? "border-t-chart-2 text-chart-2" : "border-t-chart-4 text-chart-4";
                          return (
                            <div key={st.label} className={`rounded-lg border border-border border-t-4 bg-card p-3 ${bar}`}>
                              <div className="flex items-baseline gap-2">
                                <span className="text-xs font-bold">{String(i + 1).padStart(2, "0")}</span>
                                <span className="text-sm font-bold uppercase tracking-wide text-foreground">{st.label}</span>
                              </div>
                              <p className="mt-1 text-xs text-muted-foreground">{st.items.join(" · ")}</p>
                            </div>
                          );
                        })}
                        <div className="rounded-lg border-l-4 border-l-chart-4 bg-foreground p-3 text-background">
                          <p className="text-[10px] font-bold uppercase tracking-wider text-chart-4">Design principle</p>
                          <p className="mt-1 text-xs"><strong>Deterministic rules</strong> for eligibility &amp; safety | <strong>AI</strong> for interpretation, prioritization and contextual guidance.</p>
                        </div>
                      </>
                    ) : flow.map((f, i) => {
                      const t = stepTag(i);
                      return (
                        <div key={f}>
                          <div className={`flex gap-3 rounded-lg border border-border border-l-4 bg-card p-3 ${t.bar}`}>
                            <span className="flex size-8 shrink-0 items-center justify-center rounded-full bg-accent font-display font-bold text-accent-foreground">
                              {i + 1}
                            </span>
                            <div className="flex-1">
                              <span className={`mb-1 inline-block rounded px-1.5 py-0.5 text-[10px] font-bold uppercase tracking-wider ${t.cls}`}>
                                {t.label}
                              </span>
                              <p className="text-xs leading-relaxed text-muted-foreground">{f}</p>
                            </div>
                          </div>
                          {i < flow.length - 1 && (
                            <ArrowDown className="mx-auto mt-2 size-4 text-muted-foreground" />
                          )}
                        </div>
                      );
                    })}
                  </div>

                  <div className={colCls}>
                    <p className={`${colHead} flex items-center gap-2`}>
                      <span className="h-5 w-1.5 rounded-sm bg-primary" /> Business &amp; IT Impact
                    </p>
                    {active.detail.impact.map((imp) => {
                      const down = /^(fewer|lower|less|reduc|decreas|cut)/i.test(imp);
                      return (
                        <div key={imp} className="flex gap-3 rounded-lg border border-border bg-card p-3">
                          <span className={`flex size-8 shrink-0 items-center justify-center rounded-full ${down ? "bg-destructive/10 text-destructive" : "bg-status-delivered text-tag-resolve"}`}>
                            {down ? <ArrowDown className="size-4" /> : <ArrowUp className="size-4" />}
                          </span>
                          <p className="text-sm font-semibold leading-snug">{imp}</p>
                        </div>
                      );
                    })}
                  </div>
                </div>

                <div className="flex flex-col gap-3 rounded-xl border border-border bg-card p-4 sm:flex-row sm:items-center">
                  <div className="flex shrink-0 items-center gap-2 font-bold sm:w-56 sm:border-r sm:border-border">
                    <Target className="size-5 text-primary" /> Outcome
                  </div>
                  <p className="text-sm text-muted-foreground">{active.detail.outcome}</p>
                </div>

                {scenarios.length > 0 && (
                  <div className="rounded-xl border border-border bg-muted/30 p-4">
                    <p className={`${colHead} mb-3 flex items-center gap-2`}>
                      <Sparkles className="size-5 text-primary" /> CSP Scenarios
                    </p>
                    <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3">
                      {scenarios.map((s, i) => (
                        <div key={s} className="flex gap-3 rounded-lg border border-border bg-card p-3">
                          <span className="flex size-7 shrink-0 items-center justify-center rounded-full bg-accent text-xs font-bold text-accent-foreground">
                            {i + 1}
                          </span>
                          <p className="text-xs leading-relaxed text-muted-foreground">{s}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </>
            );
          })()}
        </DialogContent>
      </Dialog>

      <div className="grid gap-4 border-t-2 border-primary pt-4 lg:grid-cols-3">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.14em] text-primary">OVERVIEW</p>
          <p className="mt-2 text-sm text-muted-foreground">
            Mapping today’s CSP capabilities to the path from manual operations → AI-assisted → fully agentic
            experiences.
          </p>
        </div>
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.14em] text-primary">
            MATURITY CLASSIFICATION KEY
          </p>
          <div className="mt-2 grid gap-2 sm:grid-cols-2">
            {MATURITY.map((m) => (
              <p key={m.id} className="flex items-start gap-2 text-sm">
                <span className={`mt-1.5 size-2.5 shrink-0 rounded-full ${m.dot}`} />
                <span>
                  <span className="font-bold">{m.label}:</span>{" "}
                  <span className="text-muted-foreground">{m.desc}</span>
                </span>
              </p>
            ))}
          </div>
        </div>
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.14em] text-primary">STRATEGIC TARGETS</p>
          <p className="mt-2 text-sm text-muted-foreground">
            Map operational deliverables to maximize <strong className="text-foreground">Mean-Time-to-Resolution
            (MTTR)</strong> gains and elevate <strong className="text-foreground">First-Person-Fix (FPF)</strong> rates
            across critical platform support domains.
          </p>
        </div>
      </div>
    </div>
  );
}


function EvolutionContextGraph() {
  const [focus, setFocus] = useState<string | null>(null);
  const [level, setLevel] = useState<MaturityId | null>(null);
  const W = 1100;
  const H = 820;
  const cx = W / 2;
  const cy = H / 2;
  const phases = JOURNEY_PHASES;
  const all = phases.flatMap((p, pi) => p.cards.map((c) => ({ ...c, phase: p.phase, pi })));
  const total = all.length;
  const phaseR = 170;
  const capR = 340;
  const phaseNodes = phases.map((p, i) => {
    const start = all.findIndex((a) => a.pi === i);
    const mid = start + (p.cards.length - 1) / 2;
    const a = (mid / total) * Math.PI * 2 - Math.PI / 2;
    return { ...p, x: cx + phaseR * Math.cos(a), y: cy + phaseR * Math.sin(a) };
  });
  const capNodes = all.map((c, i) => {
    const a = (i / total) * Math.PI * 2 - Math.PI / 2;
    return { ...c, key: `${c.phase}::${c.name}`, a, x: cx + capR * Math.cos(a), y: cy + capR * Math.sin(a) };
  });
  const byName = new Map<string, typeof capNodes>();
  capNodes.forEach((n) => byName.set(n.name, [...(byName.get(n.name) ?? []), n]));
  const shared = [...byName.values()].filter((v) => v.length > 1);
  const color = (l: MaturityId) => `var(--maturity-${l === "agentic" ? "agentic" : l === "enhanced" ? "enhanced" : l === "ml" ? "ml" : "manual"})`;
  const active = focus ? capNodes.find((n) => n.key === focus) ?? null : null;
  const isLit = (n: (typeof capNodes)[number]) =>
    (!level || n.level === level) &&
    (!focus || n.key === focus || n.phase === focus || (active && (n.name === active.name || n.phase === active.phase)));
  const phaseLit = (p: string) => !focus || focus === p || active?.phase === p;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        {MATURITY.map((m) => (
          <button
            key={m.id}
            type="button"
            onClick={() => setLevel(level === m.id ? null : m.id)}
            className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-semibold ${
              level === m.id ? "border-primary bg-accent" : "border-border"
            }`}
          >
            <span className={`size-2.5 rounded-full ${m.dot}`} />
            {m.label} ({all.filter((a) => a.level === m.id).length})
          </button>
        ))}
        {(focus || level) && (
          <Button size="sm" variant="ghost" onClick={() => { setFocus(null); setLevel(null); }}>
            Reset
          </Button>
        )}
        <span className="ml-auto text-xs text-muted-foreground">
          {phases.length} journey phases · {total} capabilities · click a node to trace its connections
        </span>
      </div>
      <Card>
        <CardContent className="p-2">
          <svg viewBox={`0 0 ${W} ${H}`} className="h-auto w-full">
            {phaseNodes.map((p) => (
              <line key={p.phase} x1={cx} y1={cy} x2={p.x} y2={p.y} stroke="var(--border)" strokeWidth={2} opacity={phaseLit(p.phase) ? 1 : 0.2} />
            ))}
            {capNodes.map((n) => {
              const p = phaseNodes[n.pi]!;
              return (
                <line key={`l-${n.key}`} x1={p.x} y1={p.y} x2={n.x} y2={n.y} stroke={color(n.level)} strokeWidth={1.5} opacity={isLit(n) ? 0.7 : 0.08} />
              );
            })}
            {shared.map((grp) =>
              grp.slice(1).map((b, i) => {
                const a = grp[i]!;
                const lit = !focus || grp.some((g) => g.key === focus);
                return (
                  <path key={`s-${a.key}-${b.key}`} d={`M${a.x},${a.y} Q${cx},${cy} ${b.x},${b.y}`} fill="none" stroke="var(--destructive)" strokeDasharray="5 4" strokeWidth={1.5} opacity={lit ? 0.8 : 0.1} />
                );
              }),
            )}
            <circle cx={cx} cy={cy} r={58} fill="var(--primary)" />
            <text x={cx} y={cy - 4} textAnchor="middle" fill="var(--primary-foreground)" fontSize={14} fontWeight={700}>CSP customer</text>
            <text x={cx} y={cy + 14} textAnchor="middle" fill="var(--primary-foreground)" fontSize={11}>journey</text>
            {phaseNodes.map((p) => (
              <g key={p.phase} className="cursor-pointer" opacity={phaseLit(p.phase) ? 1 : 0.3} onClick={() => setFocus(focus === p.phase ? null : p.phase)}>
                <rect x={p.x - 70} y={p.y - 18} width={140} height={36} rx={18} fill="var(--card)" stroke="var(--primary)" strokeWidth={2} />
                <text x={p.x} y={p.y + 4} textAnchor="middle" fontSize={12} fontWeight={700} fill="var(--foreground)">{p.phase.replace(/^\d+\.\s*/, "")}</text>
              </g>
            ))}
            {capNodes.map((n) => {
              const right = Math.cos(n.a) >= 0;
              const inFlight = IN_FLIGHT.has(n.name);
              return (
                <g key={n.key} className="cursor-pointer" opacity={isLit(n) ? 1 : 0.2} onClick={() => setFocus(focus === n.key ? null : n.key)}>
                  <circle cx={n.x} cy={n.y} r={focus === n.key ? 11 : 8} fill={color(n.level)} stroke={inFlight ? "var(--chart-2)" : "var(--card)"} strokeWidth={inFlight ? 3 : 2} />
                  <text x={n.x + (right ? 14 : -14)} y={n.y + 4} textAnchor={right ? "start" : "end"} fontSize={11} fontWeight={600} fill="var(--foreground)">
                    {n.name.length > 30 ? `${n.name.slice(0, 29)}…` : n.name}
                  </text>
                </g>
              );
            })}
          </svg>
        </CardContent>
      </Card>
      <div className="flex flex-wrap gap-4 text-xs text-muted-foreground">
        <span className="inline-flex items-center gap-1.5"><span className="size-3 rounded-full border-[3px] border-chart-2" /> In-Flight</span>
        <span className="inline-flex items-center gap-1.5"><span className="h-0 w-6 border-t-2 border-dashed border-destructive" /> Same capability used in several phases</span>
      </div>
      {active ? (
        <Card>
          <CardContent className="space-y-2 p-5">
            <div className="flex flex-wrap items-center gap-2">
              <p className="font-display text-lg font-bold">{active.name}</p>
              <Badge variant="outline">{active.phase}</Badge>
              <Badge variant="outline">{MATURITY.find((m) => m.id === active.level)?.label}</Badge>
            </div>
            <p className="text-sm text-muted-foreground">{active.detail.entails}</p>
            <p className="text-sm"><span className="font-semibold">Outcome: </span>{active.detail.outcome}</p>
            <p className="text-xs text-muted-foreground">
              Connected in {active.phase}: {capNodes.filter((c) => c.phase === active.phase && c.key !== active.key).map((c) => c.name).join(", ") || "none"}
            </p>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}

const TABS = [
  
  { id: "welcome", label: "AI Search Welcome Center" },
  { id: "csr", label: "CSR AI Admin Assistant" },
  { id: "askcsp", label: "Ask CSP" },
  { id: "predictive", label: "Predictive AI Agent" },
  { id: "ideas", label: "AI Ideas" },
  { id: "evolution", label: "CSP Capabilities AI Evolution" },
  { id: "evograph", label: "AI Evolution Context Graph" },
  { id: "timeline", label: "Monthly delivery plan" },
] as const;


function RoadmapPage() {
  const phases = Array.from(new Set(MONTHS.map((m) => m.phase)));
  const [tab, setTab] = useState<(typeof TABS)[number]["id"]>("welcome");

  return (
    <SynapseShell
      title="CSP AI Capabilities"
      subtitle="Delivery plan from capturing behavioural signals to resolving them autonomously — with the measure that proves each month landed."
    >
      <div className="space-y-6">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="inline-flex flex-wrap gap-1 rounded-xl bg-muted p-1">
            {TABS.map((t) => (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className={`rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
                  tab === t.id
                    ? "bg-background text-foreground shadow-sm"
                    : "text-muted-foreground hover:text-foreground"
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>
          {tab === "welcome" ? (
            <a
              href="https://docs.google.com/presentation/d/1JutFeRqNzkrJ4_rQam_1OY_mW-57KYIisE-rYKvltN4/edit?slide=id.g3ece32fa191_0_2613#slide=id.g3ece32fa191_0_2613"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 rounded-lg border border-border px-4 py-2 text-sm font-semibold text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
            >
              Welcome Center overview deck
              <ExternalLink className="size-4" />
            </a>
          ) : null}
          {tab === "csr" ? (
            <a
              href="https://docs.google.com/presentation/d/1XoTJ7xYaHtNrrYBa3zRuam65F0Z9d2dhHaYoGJXcXKU/edit?slide=id.g3ece32fa191_0_2613#slide=id.g3ece32fa191_0_2613"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 rounded-lg border border-border px-4 py-2 text-sm font-semibold text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
            >
              CSR AI Admin Assistant deck
              <ExternalLink className="size-4" />
            </a>
          ) : null}
          {tab === "askcsp" ? (
            <a
              href="https://docs.google.com/presentation/d/1ifrsEJsVa6sxkRlIZZGKIFkWeeeMa5-vJWZycdPnCos/edit?slide=id.g3ece32fa191_0_2613#slide=id.g3ece32fa191_0_2613"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 rounded-lg border border-border px-4 py-2 text-sm font-semibold text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
            >
              Ask CSP deck
              <ExternalLink className="size-4" />
            </a>
          ) : null}
        </div>


        {tab === "welcome" ? (
          <div className="space-y-8">
            <BriefingSection data={WELCOME_CENTER} />
            <FutureEnhancements />
          </div>
        ) : null}

        {tab === "csr" ? <BriefingSection data={CSR_AI_ADMIN} /> : null}

        {tab === "askcsp" ? <BriefingSection data={ASK_CSP} /> : null}

        {tab === "predictive" ? <BriefingSection data={PREDICTIVE_AI_AGENT} /> : null}


        {tab === "ideas" ? <AiIdeas /> : null}

        {tab === "evolution" ? <AiEvolution /> : null}


        <div className={tab === "timeline" ? "space-y-8" : "hidden"}>
        {phases.map((phase) => (
          <div key={phase} className="space-y-3">
            <p className="text-xs font-bold uppercase tracking-[0.16em] text-muted-foreground">{phase}</p>
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              {MONTHS.filter((m) => m.phase === phase).map((m) => {
                const Icon = STATUS_ICON[m.status];
                return (
                  <Card key={m.month} className="flex h-full flex-col">
                    <CardContent className="flex flex-1 flex-col gap-3 p-5">
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex items-center gap-2">
                          <Icon className="size-4 text-primary" />
                          <span className="font-display text-lg font-bold leading-none">{m.month}</span>
                        </div>
                        <Badge variant="secondary">{m.status}</Badge>
                      </div>

                      <p className="text-sm font-semibold">{m.focus}</p>

                      <ul className="flex-1 space-y-2">
                        {m.milestones.map((item) => (
                          <li key={item} className="flex gap-2 text-sm text-muted-foreground">
                            <CheckCircle2 className="mt-0.5 size-4 shrink-0 text-primary/70" />
                            <span>{item}</span>
                          </li>
                        ))}
                      </ul>

                      <div className="rounded-xl border border-border bg-muted/40 p-3">
                        <p className="text-[11px] font-bold uppercase tracking-[0.16em] text-muted-foreground">
                          Exit measure
                        </p>
                        <p className="mt-1 text-sm font-semibold">{m.measure}</p>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          </div>
        ))}
        </div>
      </div>
    </SynapseShell>
  );
}

```

## `src/routes/synapse.signals.tsx`

```tsx
import { createFileRoute, Link } from "@tanstack/react-router";
import { useMutation, useQueryClient, useSuspenseQuery } from "@tanstack/react-query";
import { Fragment, useState } from "react";
import { Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { SynapseShell } from "@/components/synapse-shell";
import { TimeAgo } from "@/components/time-ago";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { supabase } from "@/integrations/supabase/client";
import {
  LABEL,
  PILL,
  SEVERITIES,
  SIGNAL_STATUSES,
  THEMES,
  casesQuery,
  signalTheme,
  type Signal,
  severityClass,
  signalsQuery,
  stateClass,
} from "@/lib/synapse";

export const Route = createFileRoute("/synapse/signals")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "Signal Inbox — CSP AI Capabilities" },
      {
        name: "description",
        content:
          "Every incoming CSP AI Capabilities signal with its source, customer, severity, reason code and current handling status.",
      },
      { property: "og:title", content: "Signal Inbox — CSP AI Capabilities" },
      {
        property: "og:description",
        content: "Triage incoming telemetry, log and behaviour signals in one inbox.",
      },
      { property: "og:type", content: "website" },
      { name: "twitter:card", content: "summary_large_image" },
    ],
  }),
  loader: ({ context }) => {
    context.queryClient.ensureQueryData(signalsQuery());
    context.queryClient.ensureQueryData(casesQuery());
  },
  component: SignalsPage,
});

const SOURCES: string[] = [
  "Click & page events",
  "Feature usage",
  "Guides & in-app messaging",
  "Search & feedback",
  "Visitor & account metadata",
  "Application logs",
  "Infrastructure telemetry",
  "Configuration & change events",
  "Integration & API health",
];


const REASON_CODES: string[] = [
  "Product not working as expected",
  "Configuration / setup assistance",
  "Access & authentication",
  "Licensing & entitlements",
  "How-to / documentation",
  "Enablement / adoption",
  "Unclassified",
];

const EMPTY = {
  source: "Click & page events",
  summary: "",
  severity: "medium",
  status: "new",
  customer: "",
  reason_code: "Unclassified",
};

function SignalsPage() {
  const { data: signals } = useSuspenseQuery(signalsQuery());
  const { data: cases } = useSuspenseQuery(casesQuery());
  const openCases = cases.filter((c) => c.status !== "resolved");
  const qc = useQueryClient();
  const [filter, setFilter] = useState("open");
  const [theme, setTheme] = useState("all");
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState({ ...EMPTY });


  const invalidate = () => qc.invalidateQueries({ queryKey: ["synapse"] });

  const create = useMutation({
    mutationFn: async () => {
      const { error } = await supabase.from("synapse_signals").insert({
        ...draft,
        customer: draft.customer.trim() || "Unknown",
      });
      if (error) throw error;
    },
    onSuccess: () => {
      setOpen(false);
      setDraft({ ...EMPTY });
      invalidate();
      toast.success("Signal added");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const update = useMutation({
    mutationFn: async ({ id, patch }: { id: string; patch: Partial<Signal> }) => {
      const { error } = await supabase.from("synapse_signals").update(patch).eq("id", id);
      if (error) throw error;
    },
    onSuccess: invalidate,
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: async (id: string) => {
      const { error } = await supabase.from("synapse_signals").delete().eq("id", id);
      if (error) throw error;
    },
    onSuccess: () => {
      invalidate();
      toast.success("Signal removed");
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const rows = signals.filter(
    (s) =>
      (filter === "all" ? true : filter === "open" ? s.status !== "resolved" : s.status === filter) &&
      (theme === "all" || signalTheme(s) === theme),
  );

  const groups = THEMES.map(
    (t) => [t, rows.filter((s) => signalTheme(s) === t)] as const,
  ).filter(([, items]) => items.length > 0);


  return (
    <SynapseShell
      title="Signal inbox"
      subtitle="Everything the agents picked up, newest first. Change a status inline as it moves through the loop."
      actions={
        <div className="flex items-center gap-2">
          <Select value={filter} onValueChange={setFilter}>
            <SelectTrigger className="w-[170px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="open">Open only</SelectItem>
              <SelectItem value="all">All signals</SelectItem>
              {SIGNAL_STATUSES.map((s) => (
                <SelectItem key={s} value={s}>
                  {LABEL[s]}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={theme} onValueChange={setTheme}>
            <SelectTrigger className="w-[190px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All themes</SelectItem>
              {THEMES.map((t) => (
                <SelectItem key={t} value={t}>
                  {t}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button onClick={() => setOpen(true)}>
            <Plus className="size-4" /> Add signal
          </Button>
        </div>
      }
    >
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] border-collapse">
              <thead>
                <tr className="border-b border-border">
                  {["Signal", "Theme", "Source", "Customer", "Reason code", "Severity", "Status", "Seen", ""].map(
                    (h) => (
                      <th
                        key={h}
                        className="px-4 py-3 text-left text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground"
                      >
                        {h}
                      </th>
                    ),
                  )}
                </tr>
              </thead>
              <tbody>
                {groups.map(([groupName, items]) => (
                  <Fragment key={groupName}>
                    <tr className="border-b border-border bg-muted/50">
                      <td
                        colSpan={9}
                        className="px-4 py-2 text-[11px] font-bold uppercase tracking-[0.12em] text-muted-foreground"
                      >
                        {groupName} · {items.length}
                      </td>
                    </tr>
                    {items.map((s) => (
                  <tr key={s.id} className="border-b border-border last:border-0">
                    <td className="px-4 py-3 text-sm font-semibold">{s.summary}</td>
                    <td className="px-4 py-3">
                      <span className={`${PILL} bg-accent text-accent-foreground`}>
                        {signalTheme(s)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">{s.source}</td>
                    <td className="px-4 py-3 text-sm">{s.customer}</td>
                    <td className="px-4 py-3 text-xs text-muted-foreground">{s.reason_code}</td>
                    <td className="px-4 py-3">
                      <span className={`${PILL} ${severityClass(s.severity)}`}>
                        {LABEL[s.severity] ?? s.severity}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <Select
                        value={s.status}
                        onValueChange={(v) => update.mutate({ id: s.id, patch: { status: v } })}
                      >
                        <SelectTrigger className="h-8 w-[150px]">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {SIGNAL_STATUSES.map((v) => (
                            <SelectItem key={v} value={v}>
                              {LABEL[v]}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </td>
                    <td className="px-4 py-3 text-xs text-muted-foreground">
                      <span className={`${PILL} ${stateClass(s.status)}`}>
                        <TimeAgo iso={s.detected_at} />
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="Delete signal"
                        onClick={() => remove.mutate(s.id)}
                      >
                        <Trash2 className="size-4" />
                      </Button>
                    </td>
                  </tr>
                    ))}
                  </Fragment>
                ))}

                {rows.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="px-4 py-10 text-center text-sm text-muted-foreground">
                      No signals in this view.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      <Card className="mt-6">
        <CardHeader className="flex-row items-center justify-between space-y-0">
          <CardTitle className="font-display text-base">Cases needing a human</CardTitle>
          <Link to="/synapse/about" className="text-xs font-semibold text-primary">
            Open queue
          </Link>
        </CardHeader>
        <CardContent className="space-y-3">
          {openCases.length === 0 ? (
            <p className="text-sm text-muted-foreground">Nothing open. All cases resolved.</p>
          ) : (
            openCases.slice(0, 6).map((c) => (
              <div
                key={c.id}
                className="flex items-start justify-between gap-3 border-b border-border pb-3 last:border-0 last:pb-0"
              >
                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold">{c.title}</p>
                  <p className="mt-0.5 text-xs text-muted-foreground">
                    {c.owner} · {c.customer}
                  </p>
                </div>
                <div className="flex shrink-0 gap-1.5">
                  <span className={`${PILL} ${severityClass(c.severity)}`}>
                    {LABEL[c.severity] ?? c.severity}
                  </span>
                  <span className={`${PILL} ${stateClass(c.status)}`}>
                    {LABEL[c.status] ?? c.status}
                  </span>
                </div>
              </div>
            ))
          )}
        </CardContent>
      </Card>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add signal</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4">
            <div className="grid gap-1.5">
              <Label>
                Summary <span className="text-destructive">*</span>
              </Label>
              <Input
                value={draft.summary}
                onChange={(e) => setDraft({ ...draft, summary: e.target.value })}
                placeholder="What was observed"
              />
            </div>
            <div className="grid gap-1.5">
              <Label>Source</Label>
              <Select value={draft.source} onValueChange={(v) => setDraft({ ...draft, source: v })}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {SOURCES.map((s) => (
                    <SelectItem key={s} value={s}>
                      {s}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid gap-1.5">
              <Label>Customer</Label>
              <Input
                value={draft.customer}
                onChange={(e) => setDraft({ ...draft, customer: e.target.value })}
                placeholder="Account name"
              />
            </div>
            <div className="grid gap-1.5">
              <Label>Reason code</Label>
              <Select
                value={draft.reason_code}
                onValueChange={(v) => setDraft({ ...draft, reason_code: v })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {REASON_CODES.map((r) => (
                    <SelectItem key={r} value={r}>
                      {r}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="grid gap-1.5">
                <Label>Severity</Label>
                <Select
                  value={draft.severity}
                  onValueChange={(v) => setDraft({ ...draft, severity: v })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {SEVERITIES.map((v) => (
                      <SelectItem key={v} value={v}>
                        {LABEL[v]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid gap-1.5">
                <Label>Status</Label>
                <Select
                  value={draft.status}
                  onValueChange={(v) => setDraft({ ...draft, status: v })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {SIGNAL_STATUSES.map((v) => (
                      <SelectItem key={v} value={v}>
                        {LABEL[v]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              onClick={() => create.mutate()}
              disabled={!draft.summary.trim() || create.isPending}
            >
              Save signal
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </SynapseShell>
  );
}

```

## `src/server.ts`

```ts
import "./lib/error-capture";

import { consumeLastCapturedError } from "./lib/error-capture";
import { renderErrorPage } from "./lib/error-page";

type ServerEntry = {
  fetch: (request: Request, env: unknown, ctx: unknown) => Promise<Response> | Response;
};

let serverEntryPromise: Promise<ServerEntry> | undefined;

async function getServerEntry(): Promise<ServerEntry> {
  if (!serverEntryPromise) {
    serverEntryPromise = import("@tanstack/react-start/server-entry").then(
      (m) => (m.default ?? m) as ServerEntry,
    );
  }
  return serverEntryPromise;
}

// h3 swallows in-handler throws into a normal 500 Response with body
// {"unhandled":true,"message":"HTTPError"} — try/catch alone never fires for those.
async function normalizeCatastrophicSsrResponse(response: Response): Promise<Response> {
  if (response.status < 500) return response;
  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) return response;

  const body = await response.clone().text();
  if (!isH3SwallowedErrorBody(body)) return response;

  console.error(consumeLastCapturedError() ?? new Error(`h3 swallowed SSR error: ${body}`));
  return new Response(renderErrorPage(), {
    status: 500,
    headers: { "content-type": "text/html; charset=utf-8" },
  });
}

function isH3SwallowedErrorBody(body: string): boolean {
  try {
    const payload = JSON.parse(body) as { unhandled?: unknown; message?: unknown };
    return payload.unhandled === true && payload.message === "HTTPError";
  } catch {
    return false;
  }
}

export default {
  async fetch(request: Request, env: unknown, ctx: unknown) {
    try {
      const handler = await getServerEntry();
      const response = await handler.fetch(request, env, ctx);
      return await normalizeCatastrophicSsrResponse(response);
    } catch (error) {
      console.error(error);
      return new Response(renderErrorPage(), {
        status: 500,
        headers: { "content-type": "text/html; charset=utf-8" },
      });
    }
  },
};

```

## `src/start.ts`

```ts
import { createStart, createCsrfMiddleware, createMiddleware } from "@tanstack/react-start";

import { renderErrorPage } from "./lib/error-page";
import { attachSupabaseAuth } from "@/integrations/supabase/auth-attacher";

const errorMiddleware = createMiddleware().server(async ({ next }) => {
  try {
    return await next();
  } catch (error) {
    if (error != null && typeof error === "object" && "statusCode" in error) {
      throw error;
    }
    console.error(error);
    return new Response(renderErrorPage(), {
      status: 500,
      headers: { "content-type": "text/html; charset=utf-8" },
    });
  }
});

// Start installs this automatically when src/start.ts is absent; defining the
// file opts out, so re-add it explicitly to keep server functions protected
// from cross-site requests.
const csrfMiddleware = createCsrfMiddleware({
  filter: (ctx) => ctx.handlerType === "serverFn",
});

export const startInstance = createStart(() => ({
  functionMiddleware: [attachSupabaseAuth],
  requestMiddleware: [errorMiddleware, csrfMiddleware],
}));

```

## `src/styles.css`

```css
@import "tailwindcss" source(none);
@source "../src";
@import "tw-animate-css";

@custom-variant dark (&:is(.dark *));

/*
 * Design system definition.
 *
 * The @theme inline block maps CSS custom properties to Tailwind utility
 * classes (e.g. --color-primary -> bg-primary, text-primary).
 *
 * The :root and .dark blocks define the actual color values using oklch.
 * All colors MUST use oklch format.
 *
 * To add a new semantic color:
 * 1. Add the variable to :root (light value) and .dark (dark value)
 * 2. Register it in @theme inline as --color-<name>: var(--<name>)
 */

@theme inline {
  --radius-sm: calc(var(--radius) - 4px);
  --radius-md: calc(var(--radius) - 2px);
  --radius-lg: var(--radius);
  --radius-xl: calc(var(--radius) + 4px);
  --radius-2xl: calc(var(--radius) + 8px);
  --radius-3xl: calc(var(--radius) + 12px);
  --radius-4xl: calc(var(--radius) + 16px);
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-card: var(--card);
  --color-card-foreground: var(--card-foreground);
  --color-popover: var(--popover);
  --color-popover-foreground: var(--popover-foreground);
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
  --color-primary-glow: var(--primary-glow);
  --color-secondary: var(--secondary);
  --color-secondary-foreground: var(--secondary-foreground);
  --color-muted: var(--muted);
  --color-muted-foreground: var(--muted-foreground);
  --color-accent: var(--accent);
  --color-accent-foreground: var(--accent-foreground);
  --color-destructive: var(--destructive);
  --color-destructive-foreground: var(--destructive-foreground);
  --color-border: var(--border);
  --color-input: var(--input);
  --color-ring: var(--ring);
  --color-ring-offset-background: var(--background);
  --color-chart-1: var(--chart-1);
  --color-chart-2: var(--chart-2);
  --color-chart-3: var(--chart-3);
  --color-chart-4: var(--chart-4);
  --color-chart-5: var(--chart-5);
  --color-sidebar: var(--sidebar);
  --color-sidebar-foreground: var(--sidebar-foreground);
  --color-sidebar-primary: var(--sidebar-primary);
  --color-sidebar-primary-foreground: var(--sidebar-primary-foreground);
  --color-sidebar-accent: var(--sidebar-accent);
  --color-sidebar-accent-foreground: var(--sidebar-accent-foreground);
  --color-sidebar-border: var(--sidebar-border);
  --color-sidebar-ring: var(--sidebar-ring);

  /* Tag palette: one distinct hue per capability tag */
  --color-tag-detect: var(--tag-detect);
  --color-tag-diagnose: var(--tag-diagnose);
  --color-tag-triage: var(--tag-triage);
  --color-tag-resolve: var(--tag-resolve);
  --color-tag-guide: var(--tag-guide);
  --color-tag-learn: var(--tag-learn);
  --color-tag-platform: var(--tag-platform);
  --color-tag-customer: var(--tag-customer);
  --color-tag-predictive: var(--tag-predictive);
  --color-tag-top: var(--tag-top);
  --color-tag-level1: var(--tag-level1);
  --color-tag-level2: var(--tag-level2);
  --color-tag-level3: var(--tag-level3);
}

:root {
  --radius: 0.75rem;

  /* Distinct hue per tag */
  --tag-detect: oklch(0.55 0.12 197);
  --tag-diagnose: oklch(0.5 0.13 268);
  --tag-triage: oklch(0.58 0.14 62);
  --tag-resolve: oklch(0.52 0.12 148);
  --tag-guide: oklch(0.55 0.15 25);
  --tag-learn: oklch(0.5 0.13 305);
  --tag-platform: oklch(0.45 0.05 255);
  --tag-customer: oklch(0.55 0.16 350);
  --tag-predictive: oklch(0.52 0.12 105);
  --tag-top: oklch(0.55 0.19 28);
  --tag-level1: oklch(0.6 0.11 230);
  --tag-level2: oklch(0.58 0.15 40);
  --tag-level3: oklch(0.5 0.16 15);

  /* Warm paper surface with ink text */
  --background: oklch(0.975 0.006 85);
  --foreground: oklch(0.26 0.014 255);
  --card: oklch(0.995 0.003 85);
  --card-foreground: oklch(0.26 0.014 255);
  --popover: oklch(0.995 0.003 85);
  --popover-foreground: oklch(0.26 0.014 255);

  /* Teal primary with an amber highlight */
  --primary: oklch(0.52 0.093 197);
  --primary-foreground: oklch(0.99 0.004 85);
  --primary-glow: oklch(0.66 0.09 190);
  --secondary: oklch(0.945 0.008 85);
  --secondary-foreground: oklch(0.32 0.014 255);
  --muted: oklch(0.95 0.007 85);
  --muted-foreground: oklch(0.52 0.012 255);
  --accent: oklch(0.93 0.03 190);
  --accent-foreground: oklch(0.34 0.07 197);
  --destructive: oklch(0.55 0.17 27);
  --destructive-foreground: oklch(0.99 0.004 85);

  --border: oklch(0.9 0.01 85);
  --input: oklch(0.9 0.01 85);
  --ring: oklch(0.52 0.093 197);

  /* Status system */
  --status-planned: oklch(0.94 0.012 255);
  --status-planned-foreground: oklch(0.44 0.02 255);
  --status-progress: oklch(0.93 0.06 80);
  --status-progress-foreground: oklch(0.46 0.11 68);
  --status-delivered: oklch(0.93 0.05 185);
  --status-delivered-foreground: oklch(0.42 0.08 194);

  /* Maturity dots: green / blue / amber / slate */
  --maturity-agentic: oklch(0.55 0.16 150);
  --maturity-enhanced: oklch(0.52 0.16 255);
  --maturity-ml: oklch(0.68 0.16 75);
  --maturity-manual: oklch(0.6 0.02 255);


  /* Chart ramp: teal → amber → ink-blue → clay → moss */
  --chart-1: oklch(0.55 0.1 197);
  --chart-2: oklch(0.72 0.14 68);
  --chart-3: oklch(0.48 0.09 255);
  --chart-4: oklch(0.6 0.13 32);
  --chart-5: oklch(0.6 0.09 145);

  --sidebar: oklch(0.985 0.007 85);
  --sidebar-foreground: oklch(0.32 0.014 255);
  --sidebar-primary: oklch(0.52 0.093 197);
  --sidebar-primary-foreground: oklch(0.99 0.004 85);
  --sidebar-accent: oklch(0.93 0.03 190);
  --sidebar-accent-foreground: oklch(0.34 0.07 197);
  --sidebar-border: oklch(0.9 0.01 85);
  --sidebar-ring: oklch(0.52 0.093 197);

  --gradient-surface: linear-gradient(
    160deg,
    oklch(0.995 0.004 85) 0%,
    oklch(0.965 0.008 85) 100%
  );
  --gradient-primary: linear-gradient(
    135deg,
    var(--primary) 0%,
    var(--primary-glow) 100%
  );
  --shadow-soft: 0 1px 2px oklch(0.3 0.02 255 / 0.05),
    0 6px 16px -10px oklch(0.3 0.02 255 / 0.18);
  --shadow-elevated: 0 1px 2px oklch(0.3 0.02 255 / 0.06),
    0 20px 40px -26px oklch(0.3 0.02 255 / 0.3);
  --shadow-glow: 0 0 0 1px color-mix(in oklab, var(--primary) 22%, transparent),
    0 14px 34px -22px color-mix(in oklab, var(--primary) 45%, transparent);

  --font-sans: "DM Sans", ui-sans-serif, system-ui, sans-serif;
  --font-display: "Space Grotesk", ui-sans-serif, system-ui, sans-serif;
}

.dark {
  color-scheme: dark;

  --background: oklch(0.21 0.012 255);
  --foreground: oklch(0.95 0.006 85);
  --card: oklch(0.25 0.014 255);
  --card-foreground: oklch(0.95 0.006 85);
  --popover: oklch(0.25 0.014 255);
  --popover-foreground: oklch(0.95 0.006 85);

  --primary: oklch(0.72 0.1 194);
  --primary-foreground: oklch(0.2 0.02 255);
  --primary-glow: oklch(0.8 0.09 188);
  --secondary: oklch(0.3 0.014 255);
  --secondary-foreground: oklch(0.93 0.006 85);
  --muted: oklch(0.29 0.012 255);
  --muted-foreground: oklch(0.72 0.01 85);
  --accent: oklch(0.34 0.04 197);
  --accent-foreground: oklch(0.9 0.06 190);
  --destructive: oklch(0.65 0.17 27);
  --destructive-foreground: oklch(0.2 0.02 255);

  --border: oklch(0.34 0.014 255);
  --input: oklch(0.34 0.014 255);
  --ring: oklch(0.72 0.1 194);

  --status-planned: oklch(0.33 0.014 255);
  --status-planned-foreground: oklch(0.82 0.01 85);
  --status-progress: oklch(0.38 0.07 70);
  --status-progress-foreground: oklch(0.87 0.11 80);
  --status-delivered: oklch(0.35 0.06 192);
  --status-delivered-foreground: oklch(0.86 0.1 190);

  --maturity-agentic: oklch(0.75 0.16 150);
  --maturity-enhanced: oklch(0.72 0.14 255);
  --maturity-ml: oklch(0.82 0.15 78);
  --maturity-manual: oklch(0.68 0.02 255);


  --chart-1: oklch(0.73 0.1 194);
  --chart-2: oklch(0.79 0.14 72);
  --chart-3: oklch(0.68 0.11 258);
  --chart-4: oklch(0.7 0.14 32);
  --chart-5: oklch(0.72 0.1 148);

  --sidebar: oklch(0.24 0.014 255);
  --sidebar-foreground: oklch(0.9 0.006 85);
  --sidebar-primary: oklch(0.72 0.1 194);
  --sidebar-primary-foreground: oklch(0.2 0.02 255);
  --sidebar-accent: oklch(0.34 0.04 197);
  --sidebar-accent-foreground: oklch(0.92 0.05 190);
  --sidebar-border: oklch(0.33 0.014 255);
  --sidebar-ring: oklch(0.72 0.1 194);

  --gradient-surface: linear-gradient(
    160deg,
    oklch(0.27 0.014 255) 0%,
    oklch(0.22 0.012 255) 100%
  );
  --shadow-soft: 0 1px 2px oklch(0.1 0.02 255 / 0.5),
    0 6px 18px -12px oklch(0.08 0.02 255 / 0.8);
  --shadow-elevated: 0 1px 2px oklch(0.1 0.02 255 / 0.5),
    0 22px 44px -26px oklch(0.08 0.02 255 / 0.9);
}





@theme inline {
  --font-sans: var(--font-sans);
  --font-display: var(--font-display);
  --color-status-planned: var(--status-planned);
  --color-status-planned-foreground: var(--status-planned-foreground);
  --color-status-progress: var(--status-progress);
  --color-status-progress-foreground: var(--status-progress-foreground);
  --color-status-delivered: var(--status-delivered);
  --color-status-delivered-foreground: var(--status-delivered-foreground);
  --color-maturity-agentic: var(--maturity-agentic);
  --color-maturity-enhanced: var(--maturity-enhanced);
  --color-maturity-ml: var(--maturity-ml);
  --color-maturity-manual: var(--maturity-manual);

  --shadow-soft: var(--shadow-soft);
  --shadow-elevated: var(--shadow-elevated);
}


@layer base {
  * {
    border-color: var(--color-border);
  }

  body {
    background-color: var(--color-background);
    background-image:
      radial-gradient(
        900px 520px at 10% -12%,
        color-mix(in oklab, var(--primary) 10%, transparent),
        transparent 70%
      ),
      radial-gradient(
        760px 440px at 98% -6%,
        color-mix(in oklab, var(--chart-2) 12%, transparent),
        transparent 70%
      );
    background-attachment: fixed;
    color: var(--color-foreground);
    font-family: var(--font-sans);
    -webkit-font-smoothing: antialiased;
  }


  h1, h2, h3, .font-display {
    font-family: var(--font-display);
  }
}

```

## `supabase/migrations/20260905151757_f427aecf-4352-464d-9da3-64e5fe18724f.sql`

```sql

CREATE TABLE public.speedboats (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL UNIQUE,
  sort_order int NOT NULL DEFAULT 0,
  created_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.speedboats TO authenticated;
GRANT ALL ON public.speedboats TO service_role;
ALTER TABLE public.speedboats ENABLE ROW LEVEL SECURITY;
CREATE POLICY "speedboats_all_authenticated" ON public.speedboats FOR ALL TO authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.product_lines (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL UNIQUE,
  speedboat_id uuid NOT NULL REFERENCES public.speedboats(id) ON DELETE CASCADE,
  sort_order int NOT NULL DEFAULT 0,
  created_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.product_lines TO authenticated;
GRANT ALL ON public.product_lines TO service_role;
ALTER TABLE public.product_lines ENABLE ROW LEVEL SECURITY;
CREATE POLICY "product_lines_all_authenticated" ON public.product_lines FOR ALL TO authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.npi_projects (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL,
  product_line_id uuid NOT NULL REFERENCES public.product_lines(id) ON DELETE CASCADE,
  is_standard boolean NOT NULL DEFAULT false,
  status text NOT NULL DEFAULT 'planned',
  start_quarter text NOT NULL DEFAULT 'Q1FY27',
  ga_quarter text NOT NULL DEFAULT 'Q1FY27',
  resources int NOT NULL DEFAULT 0,
  impactful boolean NOT NULL DEFAULT false,
  owner text,
  notes text,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.npi_projects TO authenticated;
GRANT ALL ON public.npi_projects TO service_role;
ALTER TABLE public.npi_projects ENABLE ROW LEVEL SECURITY;
CREATE POLICY "npi_projects_all_authenticated" ON public.npi_projects FOR ALL TO authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.quarter_stats (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  quarter text NOT NULL UNIQUE,
  sort_order int NOT NULL DEFAULT 0,
  standard_count int NOT NULL DEFAULT 0,
  nonstandard_count int NOT NULL DEFAULT 0
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.quarter_stats TO authenticated;
GRANT ALL ON public.quarter_stats TO service_role;
ALTER TABLE public.quarter_stats ENABLE ROW LEVEL SECURITY;
CREATE POLICY "quarter_stats_all_authenticated" ON public.quarter_stats FOR ALL TO authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.quarter_resources (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  quarter text NOT NULL UNIQUE,
  sort_order int NOT NULL DEFAULT 0,
  resources int NOT NULL DEFAULT 0
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.quarter_resources TO authenticated;
GRANT ALL ON public.quarter_resources TO service_role;
ALTER TABLE public.quarter_resources ENABLE ROW LEVEL SECURITY;
CREATE POLICY "quarter_resources_all_authenticated" ON public.quarter_resources FOR ALL TO authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.quarter_product_line_counts (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  quarter text NOT NULL,
  product_line_id uuid NOT NULL REFERENCES public.product_lines(id) ON DELETE CASCADE,
  count int NOT NULL DEFAULT 0,
  UNIQUE (quarter, product_line_id)
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.quarter_product_line_counts TO authenticated;
GRANT ALL ON public.quarter_product_line_counts TO service_role;
ALTER TABLE public.quarter_product_line_counts ENABLE ROW LEVEL SECURITY;
CREATE POLICY "qplc_all_authenticated" ON public.quarter_product_line_counts FOR ALL TO authenticated USING (true) WITH CHECK (true);

CREATE OR REPLACE FUNCTION public.update_updated_at_column() RETURNS TRIGGER AS $$
BEGIN NEW.updated_at = now(); RETURN NEW; END; $$ LANGUAGE plpgsql SET search_path = public;

CREATE TRIGGER update_npi_projects_updated_at BEFORE UPDATE ON public.npi_projects
FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

INSERT INTO public.speedboats (name, sort_order) VALUES ('NetSec', 1), ('Cortex', 2), ('Idira', 3);

INSERT INTO public.product_lines (name, speedboat_id, sort_order)
SELECT v.name, s.id, v.sort_order FROM (VALUES
  ('CDSS','NetSec',1),('NGFW','NetSec',2),('SASE','NetSec',3),('SWFW','NetSec',4),
  ('Cross-Platform','NetSec',5),('GCS','NetSec',6),('HWFW','NetSec',7),
  ('Cortex','Cortex',1),('Unit 42','Idira',1)
) AS v(name, sb, sort_order) JOIN public.speedboats s ON s.name = v.sb;

INSERT INTO public.npi_projects (name, product_line_id, status, start_quarter, ga_quarter, resources, impactful, is_standard)
SELECT v.name, pl.id, v.status, v.startq, v.gaq, v.res, v.impactful, v.std FROM (VALUES
  ('CDSS:AIPD ELA8','CDSS','delivered','Q3FY26','Q4FY26',3,true,true),
  ('New Precision AI Bundle - Phase 2','CDSS','delivered','Q3FY26','Q4FY26',4,true,true),
  ('NETSEC:Quantum Phase 2','CDSS','delivered','Q3FY26','Q4FY26',3,true,false),
  ('Single FedRamp Mod SKU for ELA1','CDSS','delivered','Q4FY26','Q4FY26',2,true,true),
  ('CDSS ELA2 – FedRamp Moderate','CDSS','delivered','Q4FY26','Q4FY26',2,true,true),
  ('4-HR RMA Add-On for ESA','CDSS','delivered','Q4FY26','Q4FY26',2,true,false),
  ('EA SDWAN','CDSS','delivered','Q3FY26','Q4FY26',3,true,true),
  ('SCM downgrade - NGFW SCM to SLS','NGFW','delivered','Q4FY26','Q4FY26',2,false,false),
  ('NGFW H/W: Atlas, Ocean Ranch, Spring Ranch, Antres Renewals','NGFW','delivered','Q3FY26','Q4FY26',5,true,true),
  ('NGFW:SCM:FEDRAMP MOD','NGFW','delivered','Q4FY26','Q4FY26',2,true,true),
  ('SCM Migration PS Offer','SASE','delivered','Q4FY26','Q4FY26',2,false,false),
  ('PA 6.2 Simplified deployment of Coffee shop in Prisma SASE branch - add 10mbps site','SASE','delivered','Q3FY26','Q4FY26',3,true,false),
  ('FedRamp mod phase 4 (SCR2) - RBI FedRAMP Moderate','SASE','delivered','Q4FY26','Q4FY26',2,false,true),
  ('PA 6.2 BYOIP SKU NPI for Prisma Access','SASE','delivered','Q4FY26','Q4FY26',2,false,false),
  ('Prisma SD-WAN Blackhawk Dual 5G Quoting readiness','SASE','delivered','Q3FY26','Q4FY26',3,true,true),
  ('PA 6.2 Licensing Flexibility (Multiple PA MU Editions in a single tenant) - Greenfield','SASE','delivered','Q3FY26','Q4FY26',4,true,true),
  ('Prisma SD-WAN vION 7132 enablement','SASE','delivered','Q4FY26','Q4FY26',2,true,true),
  ('PA 6.2 PRA to SAA Rebranding','SASE','delivered','Q4FY26','Q4FY26',1,false,false),
  ('Speedboat - SASE: Product - PB: PB UPG Pro Add On to Prisma Access','SASE','delivered','Q4FY26','Q4FY26',2,true,false),
  ('AI Gateway','SWFW','delivered','Q3FY26','Q4FY26',5,true,true),
  ('FW flex: VM-Series to AI Runtime Migration','SWFW','delivered','Q3FY26','Q4FY26',4,true,true),
  ('FW flex: Credit Unification for Cloud NGFW and VM Flex','SWFW','delivered','Q3FY26','Q4FY26',4,true,true),
  ('CNGFW: AWS Marketplace AI FW (AIRF)','SWFW','delivered','Q4FY26','Q4FY26',2,false,false),
  ('FW Flex: Prisma AIRS:KOI Licensing','SWFW','delivered','Q3FY26','Q4FY26',3,true,true),
  ('Red Teaming Pricing Changes','SWFW','delivered','Q4FY26','Q4FY26',1,false,false),
  ('Unit 42 OT Retainer SKU','Unit 42','delivered','Q4FY26','Q4FY26',2,false,false),
  ('Future Dated Upgrade (Agentix)','Cortex','delivered','Q3FY26','Q4FY26',4,true,true),
  ('PC+Cortex Migration: RPA Changes','Cortex','delivered','Q4FY26','Q4FY26',2,false,false),
  ('Cortex AES (Koi)','Cortex','delivered','Q3FY26','Q4FY26',4,true,true),
  ('Cortex Availability SKUs','Cortex','delivered','Q4FY26','Q4FY26',2,false,false),
  ('PC Only to Cortex migration Ramped (Ph 2)','Cortex','delivered','Q4FY26','Q4FY26',3,false,true),
  ('XDR/XSIAM FIM','Cortex','delivered','Q4FY26','Q4FY26',2,false,false),
  ('AGENTIX PS/DS SKUs','Cortex','delivered','Q4FY26','Q4FY26',2,false,false),
  ('PC Only to Cortex migration Non ramped (Ph 1)','Cortex','delivered','Q3FY26','Q4FY26',3,true,true),
  ('PC+Cortex to Cortex Cloud migration','Cortex','delivered','Q3FY26','Q4FY26',4,true,true),
  ('NGFW Hardware Releases - FY27 wave 1','NGFW','in_progress','Q4FY26','Q1FY27',6,false,true),
  ('SDWAN Releases - FY27 wave 1','SASE','in_progress','Q4FY26','Q1FY27',5,false,true),
  ('Prisma AIRS - Koi expansion','SWFW','in_progress','Q4FY26','Q2FY27',4,true,false),
  ('Agentix licensing phase 2','Cortex','in_progress','Q1FY27','Q2FY27',4,true,false),
  ('ELA1 Fedramp follow-on','CDSS','planned','Q1FY27','Q2FY27',3,false,true),
  ('VMFlex Credit Unification phase 2','SWFW','planned','Q1FY27','Q3FY27',3,false,false),
  ('PC to Cortex migration Ph 3','Cortex','planned','Q2FY27','Q3FY27',5,true,false),
  ('Unit 42 Retainer expansion','Unit 42','planned','Q2FY27','Q4FY27',2,false,false)
) AS v(name, pl_name, status, startq, gaq, res, impactful, std)
JOIN public.product_lines pl ON pl.name = v.pl_name;

INSERT INTO public.quarter_stats (quarter, sort_order, standard_count, nonstandard_count) VALUES
  ('Q1''26',1,14,13),('Q2''26',2,9,16),('Q3''26',3,13,22),('Q4''26',4,24,16);

INSERT INTO public.quarter_resources (quarter, sort_order, resources) VALUES
  ('Q3FY26',1,82),('Q4FY26',2,62),('Q1FY27',3,72),('Q2FY27',4,74),('Q3FY27',5,74),('Q4FY27',6,74);

INSERT INTO public.quarter_product_line_counts (quarter, product_line_id, count)
SELECT v.quarter, pl.id, v.count FROM (VALUES
  ('Q1''26','CDSS',1),('Q1''26','Cortex',6),('Q1''26','Cross-Platform',1),('Q1''26','GCS',1),('Q1''26','NGFW',9),('Q1''26','SASE',3),('Q1''26','SWFW',4),('Q1''26','Unit 42',2),
  ('Q2''26','CDSS',7),('Q2''26','Cortex',4),('Q2''26','Cross-Platform',2),('Q2''26','GCS',1),('Q2''26','HWFW',2),('Q2''26','NGFW',7),('Q2''26','SASE',2),
  ('Q3''26','CDSS',7),('Q3''26','Cortex',4),('Q3''26','Cross-Platform',2),('Q3''26','GCS',2),('Q3''26','NGFW',9),('Q3''26','SASE',5),('Q3''26','SWFW',3),('Q3''26','Unit 42',3),
  ('Q4''26','CDSS',7),('Q4''26','Cortex',9),('Q4''26','Cross-Platform',1),('Q4''26','HWFW',2),('Q4''26','NGFW',3),('Q4''26','SASE',11),('Q4''26','SWFW',6),('Q4''26','Unit 42',1)
) AS v(quarter, pl_name, count)
JOIN public.product_lines pl ON pl.name = v.pl_name;

```

## `supabase/migrations/20260905152811_f275ca24-74c3-497d-ab92-d3aea9fa8c6d.sql`

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON public.speedboats, public.product_lines, public.npi_projects, public.quarter_stats, public.quarter_resources, public.quarter_product_line_counts TO authenticated;
GRANT ALL ON public.speedboats, public.product_lines, public.npi_projects, public.quarter_stats, public.quarter_resources, public.quarter_product_line_counts TO service_role;
```

## `supabase/migrations/20260905153150_42447b9e-73db-4ba4-8ceb-c1e1c9307b5a.sql`

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON public.speedboats, public.product_lines, public.npi_projects, public.quarter_stats, public.quarter_resources, public.quarter_product_line_counts TO anon;

DROP POLICY IF EXISTS speedboats_all_authenticated ON public.speedboats;
DROP POLICY IF EXISTS product_lines_all_authenticated ON public.product_lines;
DROP POLICY IF EXISTS npi_projects_all_authenticated ON public.npi_projects;
DROP POLICY IF EXISTS quarter_stats_all_authenticated ON public.quarter_stats;
DROP POLICY IF EXISTS quarter_resources_all_authenticated ON public.quarter_resources;
DROP POLICY IF EXISTS quarter_product_line_counts_all_authenticated ON public.quarter_product_line_counts;

CREATE POLICY speedboats_all_public ON public.speedboats FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);
CREATE POLICY product_lines_all_public ON public.product_lines FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);
CREATE POLICY npi_projects_all_public ON public.npi_projects FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);
CREATE POLICY quarter_stats_all_public ON public.quarter_stats FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);
CREATE POLICY quarter_resources_all_public ON public.quarter_resources FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);
CREATE POLICY quarter_product_line_counts_all_public ON public.quarter_product_line_counts FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);
```

## `supabase/migrations/20260905154732_e464da53-ba8b-41cb-9f1d-a5599a1494cf.sql`

```sql
UPDATE public.npi_projects SET owner = 'Unassigned' WHERE owner IS NULL OR btrim(owner) = '';
ALTER TABLE public.npi_projects ALTER COLUMN owner SET NOT NULL;
ALTER TABLE public.npi_projects ADD CONSTRAINT npi_projects_owner_not_blank CHECK (btrim(owner) <> '');
```

## `supabase/migrations/20260907010254_b292594e-9a2b-428f-abf5-a2d60985011b.sql`

```sql
CREATE TABLE public.synapse_signals (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  source text NOT NULL,
  summary text NOT NULL,
  severity text NOT NULL DEFAULT 'medium',
  status text NOT NULL DEFAULT 'new',
  customer text NOT NULL DEFAULT 'Unknown',
  reason_code text NOT NULL DEFAULT 'Unclassified',
  detected_at timestamptz NOT NULL DEFAULT now(),
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.synapse_signals TO anon, authenticated;
GRANT ALL ON public.synapse_signals TO service_role;
ALTER TABLE public.synapse_signals ENABLE ROW LEVEL SECURITY;
CREATE POLICY synapse_signals_all_public ON public.synapse_signals FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.synapse_runbooks (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL,
  tier text NOT NULL DEFAULT 'tier1',
  trigger_signature text NOT NULL DEFAULT '',
  description text NOT NULL DEFAULT '',
  enabled boolean NOT NULL DEFAULT false,
  success_rate integer NOT NULL DEFAULT 0,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.synapse_runbooks TO anon, authenticated;
GRANT ALL ON public.synapse_runbooks TO service_role;
ALTER TABLE public.synapse_runbooks ENABLE ROW LEVEL SECURITY;
CREATE POLICY synapse_runbooks_all_public ON public.synapse_runbooks FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.synapse_cases (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  title text NOT NULL,
  signal_id uuid REFERENCES public.synapse_signals(id) ON DELETE SET NULL,
  severity text NOT NULL DEFAULT 'medium',
  status text NOT NULL DEFAULT 'triaged',
  owner text NOT NULL DEFAULT 'Unassigned',
  root_cause text NOT NULL DEFAULT '',
  next_step text NOT NULL DEFAULT '',
  customer text NOT NULL DEFAULT 'Unknown',
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.synapse_cases TO anon, authenticated;
GRANT ALL ON public.synapse_cases TO service_role;
ALTER TABLE public.synapse_cases ENABLE ROW LEVEL SECURITY;
CREATE POLICY synapse_cases_all_public ON public.synapse_cases FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);

CREATE TABLE public.synapse_runs (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  runbook_id uuid NOT NULL REFERENCES public.synapse_runbooks(id) ON DELETE CASCADE,
  signal_id uuid REFERENCES public.synapse_signals(id) ON DELETE SET NULL,
  result text NOT NULL DEFAULT 'succeeded',
  notes text NOT NULL DEFAULT '',
  ran_at timestamptz NOT NULL DEFAULT now()
);
GRANT SELECT, INSERT, UPDATE, DELETE ON public.synapse_runs TO anon, authenticated;
GRANT ALL ON public.synapse_runs TO service_role;
ALTER TABLE public.synapse_runs ENABLE ROW LEVEL SECURITY;
CREATE POLICY synapse_runs_all_public ON public.synapse_runs FOR ALL TO anon, authenticated USING (true) WITH CHECK (true);

CREATE TRIGGER synapse_signals_updated_at BEFORE UPDATE ON public.synapse_signals FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();
CREATE TRIGGER synapse_cases_updated_at BEFORE UPDATE ON public.synapse_cases FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();
CREATE TRIGGER synapse_runbooks_updated_at BEFORE UPDATE ON public.synapse_runbooks FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

INSERT INTO public.synapse_runbooks (name, tier, trigger_signature, description, enabled, success_rate) VALUES
('Clear stuck job queue','tier1','queue.depth > threshold for 15m','Drain and re-dispatch stuck jobs, verify depth returns to baseline, roll back on failure.',true,96),
('Refresh expiring tokens','tier1','auth.token.expiry < 24h','Rotate integration tokens ahead of expiry and verify a test call succeeds.',true,99),
('Restart failed worker','tier1','worker.heartbeat missing 3x','Restart the unhealthy worker, confirm heartbeat and drain backlog.',true,92),
('Re-run failed sync','tier1','connector.sync.failed','Replay the failed sync window and reconcile record counts.',true,88),
('Configuration correction','tier2','config.drift detected','Propose a config diff for approval, then apply and verify.',false,81),
('Entitlement repair','tier2','entitlement.mismatch','Realign licence allocation with the order record after approval.',false,77),
('Index rebuild','tier2','search.latency drift','Rebuild the affected index during a low-traffic window.',false,84),
('Security posture change','tier3','policy.risk flagged','Advisory only — prepare the plan and evidence for a human.',false,0);

INSERT INTO public.synapse_signals (source, summary, severity, status, customer, reason_code, detected_at) VALUES
('Application logs','Repeated 500s on /api/policy-sync stack signature PS-114','critical','triaged','Northwind Bank','Product not working as expected', now() - interval '2 hours'),
('Infrastructure telemetry','Latency drift 340ms → 1.2s on ingest cluster eu-west','high','healing','Contoso Retail','Product not working as expected', now() - interval '5 hours'),
('Integration & API health','Connector auth failures spiking for Okta integration','high','new','Globex','Access & authentication', now() - interval '40 minutes'),
('Configuration & change events','Cert rotation on gateway correlates with failed handshakes','critical','new','Initech','Configuration / setup assistance', now() - interval '25 minutes'),
('CSP behaviour','Rage clicks on entitlement page, 6 sessions today','medium','triaged','Umbrella Health','Licensing & entitlements', now() - interval '9 hours'),
('CSP behaviour','Onboarding funnel drop-off at step 3 for 12 new tenants','medium','resolved','Stark Industries','Configuration / setup assistance', now() - interval '2 days'),
('Application logs','Failed job retries on nightly report generator','low','resolved','Wayne Enterprises','Product not working as expected', now() - interval '3 days'),
('Infrastructure telemetry','Disk saturation 88% on log archive node','medium','healing','Soylent Corp','Product not working as expected', now() - interval '1 day'),
('Search & feedback','Zero-result searches for "SAML group mapping" up 4x','low','new','Multiple','How-to / documentation', now() - interval '6 hours'),
('Feature usage','SASE policy insights unused 30 days after purchase','low','triaged','Hooli','Enablement / adoption', now() - interval '4 days');

INSERT INTO public.synapse_cases (title, severity, status, owner, root_cause, next_step, customer)
SELECT 'Policy sync failing after 4.2 upgrade','critical','in_progress','A. Rivera','Schema migration left stale index on policy_map; sync writes fail on conflict.','Apply hotfix 4.2.1 and re-run backfill for affected tenants.','Northwind Bank'
UNION ALL SELECT 'Ingest latency drift in eu-west','high','triaged','M. Chen','Node pool undersized after traffic shift from eu-central.','Approve Tier 2 capacity runbook.','Contoso Retail'
UNION ALL SELECT 'Okta connector auth failures','high','triaged','Unassigned','Client secret rotated upstream without update in connector store.','Route to Identity team with evidence packet.','Globex'
UNION ALL SELECT 'Entitlement page confusion','medium','resolved','S. Patel','Licence counts displayed pre-allocation, not post.','Shipped in-app entitlement summary; monitor rage-click rate.','Umbrella Health'
UNION ALL SELECT 'SAML documentation gap','low','resolved','J. Okafor','No published guidance for group mapping in 4.x.','Article published and wired into in-app search.','Multiple';

INSERT INTO public.synapse_runs (runbook_id, result, notes, ran_at)
SELECT r.id, 'succeeded', 'Queue depth returned to baseline in 2m.', now() - interval '3 hours' FROM public.synapse_runbooks r WHERE r.name = 'Clear stuck job queue'
UNION ALL SELECT r.id, 'succeeded', 'Rotated 14 tokens, all verified.', now() - interval '1 day' FROM public.synapse_runbooks r WHERE r.name = 'Refresh expiring tokens'
UNION ALL SELECT r.id, 'rolled_back', 'Worker restart did not clear backlog; escalated.', now() - interval '2 days' FROM public.synapse_runbooks r WHERE r.name = 'Restart failed worker'
UNION ALL SELECT r.id, 'succeeded', 'Replayed 6h window, counts reconciled.', now() - interval '4 days' FROM public.synapse_runbooks r WHERE r.name = 'Re-run failed sync'
UNION ALL SELECT r.id, 'succeeded', 'Depth cleared, no rollback needed.', now() - interval '6 days' FROM public.synapse_runbooks r WHERE r.name = 'Clear stuck job queue';
```

## `supabase/migrations/20260907064425_d2b57c8e-5c3c-4ced-bce9-801b094e2ac6.sql`

```sql
CREATE TABLE public.pendo_settings (
  id uuid NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
  api_key text NOT NULL DEFAULT '',
  region text NOT NULL DEFAULT 'us',
  auto_create boolean NOT NULL DEFAULT true,
  webhook_secret text NOT NULL DEFAULT '',
  rage_click_threshold integer NOT NULL DEFAULT 3,
  dropoff_pct_threshold integer NOT NULL DEFAULT 25,
  zero_result_threshold integer NOT NULL DEFAULT 10,
  error_rate_threshold integer NOT NULL DEFAULT 5,
  time_on_task_minutes integer NOT NULL DEFAULT 5,
  quiet_window_minutes integer NOT NULL DEFAULT 30,
  default_severity text NOT NULL DEFAULT 'medium',
  last_synced_at timestamp with time zone,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now()
);

GRANT SELECT, INSERT, UPDATE, DELETE ON public.pendo_settings TO anon, authenticated;
GRANT ALL ON public.pendo_settings TO service_role;

ALTER TABLE public.pendo_settings ENABLE ROW LEVEL SECURITY;

CREATE POLICY pendo_settings_all_public ON public.pendo_settings FOR ALL USING (true) WITH CHECK (true);

CREATE TRIGGER pendo_settings_updated_at BEFORE UPDATE ON public.pendo_settings
FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

INSERT INTO public.pendo_settings (webhook_secret) VALUES (encode(gen_random_bytes(16), 'hex'));
```

## `supabase/migrations/20260910040429_f268bba5-0861-439c-826e-e5061c617e06.sql`

```sql
ALTER TABLE public.synapse_cases
  ADD COLUMN IF NOT EXISTS resolution text NOT NULL DEFAULT '',
  ADD COLUMN IF NOT EXISTS resolved_at timestamp with time zone;

CREATE OR REPLACE FUNCTION public.synapse_cases_set_resolved_at()
RETURNS trigger
LANGUAGE plpgsql
SET search_path = public
AS $$
BEGIN
  IF NEW.status = 'resolved' AND NEW.resolved_at IS NULL THEN
    NEW.resolved_at = now();
  ELSIF NEW.status <> 'resolved' THEN
    NEW.resolved_at = NULL;
  END IF;
  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS synapse_cases_resolved_at ON public.synapse_cases;
CREATE TRIGGER synapse_cases_resolved_at
BEFORE INSERT OR UPDATE ON public.synapse_cases
FOR EACH ROW EXECUTE FUNCTION public.synapse_cases_set_resolved_at();

UPDATE public.synapse_cases SET resolved_at = updated_at WHERE status = 'resolved' AND resolved_at IS NULL;
```

