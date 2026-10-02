# PRD Template: Welcome Center - Generic Onboarding Entry Point

**Status:** Ready for Product Review | **Recommended Solution:** Option 1 from Analysis

---

## Problem Statement

The "Welcome to CSP" email template cannot include a generic link to the OnBoarding page because:
- The current OnBoarding page (`/SupportAccount/OnBoarding/{id}`) requires authentication and a specific Support Account ID
- Removing the ID from the URL redirects to an error page
- New users receiving the welcome email cannot access the onboarding content without already being logged in

**Impact:** New users have a broken onboarding experience when clicking the welcome email link.

---

## Proposed Solution

Create a new, **unauthenticated Welcome Center** that serves as the generic entry point for new users, with an option to sign in for account-specific content.

---

## Requirements

### Functional Requirements

#### FR1: Unauthenticated Access
- The Welcome Center page must be accessible **without authentication**
- No Support Account ID required in the URL
- Route: `/SupportAccount/WelcomeCenter`

#### FR2: Generic Onboarding Content
- Display generic onboarding information (not account-specific)
- Include key resources:
  - Getting started guide
  - Feature overview
  - Support contact information
  - FAQ links
- Content should be the same for all users

#### FR3: Sign In Call-to-Action
- Include a prominent "Sign In" button
- Direct authenticated users to their account-specific onboarding
- After sign in, redirect to `/SupportAccount/OnBoarding/{accountId}`

#### FR4: Email Template Integration
- Update the "Welcome to CSP" email template to use the new generic URL
- Email link: `https://support.paloaltonetworks.com/SupportAccount/WelcomeCenter`
- No dynamic account ID required

#### FR5: Security
- No sensitive account data on the unauthenticated page
- No authentication bypass
- Maintain existing security model for authenticated pages

### Non-Functional Requirements

#### NFR1: Performance
- Page load time < 2 seconds
- No database queries required (static content)

#### NFR2: Accessibility
- WCAG 2.1 AA compliance
- Mobile-responsive design

#### NFR3: Analytics
- Track page views
- Track "Sign In" button clicks
- Track email link clicks (if possible)

---

## Acceptance Criteria

### AC1: Unauthenticated Access Works
**Given** a user receives the welcome email  
**When** they click the link to `/SupportAccount/WelcomeCenter`  
**Then** the page loads without requiring authentication

### AC2: Generic Content Displays
**Given** the Welcome Center page loads  
**When** the page renders  
**Then** generic onboarding content is displayed (no account-specific data)

### AC3: Sign In Redirects Correctly
**Given** an authenticated user is on the Welcome Center  
**When** they click "Sign In" or navigate to their account  
**Then** they are redirected to `/SupportAccount/OnBoarding/{accountId}` with their account ID

### AC4: Email Template Works
**Given** a new user receives the "Welcome to CSP" email  
**When** they click the link  
**Then** they are taken to `/SupportAccount/WelcomeCenter` (not an error page)

### AC5: No Security Bypass
**Given** an unauthenticated user is on the Welcome Center  
**When** they attempt to access account-specific pages  
**Then** they are redirected to the login page

### AC6: Error Handling
**Given** the Welcome Center page encounters an error  
**When** the error occurs  
**Then** a user-friendly error message is displayed (not a generic error page)

---

## Technical Specifications

### Controller Changes

**File:** `Csp.Web/Controllers/SupportAccountController.cs`

**New Action:**
```csharp
[HttpGet]
public ActionResult WelcomeCenter()
{
    return View("../SupportAccount/WelcomeCenter");
}
```

**Key Points:**
- No `[CspAuthorize]` attribute (unauthenticated access)
- No `id` parameter required
- Returns a new view: `Views/SupportAccount/WelcomeCenter.cshtml`

### View Changes

**New File:** `Csp.Web/Views/SupportAccount/WelcomeCenter.cshtml`

**Content:**
- Generic onboarding information
- Feature overview
- Getting started guide
- Support resources
- "Sign In" button (links to `/SupportAccount/MyAccounts`)

### Email Template Changes

**File:** Email template in database (template ID TBD)

**Current:** `https://support.paloaltonetworks.com/SupportAccount/OnBoarding/{accountId}`

**Updated:** `https://support.paloaltonetworks.com/SupportAccount/WelcomeCenter`

### Routing

**No routing changes required** — uses default route pattern:
```
{controller}/{action}/{id}
```

Route will match: `/SupportAccount/WelcomeCenter`

---

## Edge Cases & Error States

| Scenario | Current Behavior | Desired Behavior |
|----------|------------------|------------------|
| User clicks email link without logging in | Error page | Welcome Center loads |
| User is already logged in, clicks email link | Error page | Welcome Center loads (or redirect to account onboarding) |
| User clicks "Sign In" without an account | Redirects to login | Redirects to login, then to account selection |
| Page fails to load | Generic error | User-friendly error message with support contact |
| User has multiple accounts | N/A | After sign in, show account selector, then redirect to account onboarding |

---

## Empty States

- **No content available:** Display a message: "Welcome content is temporarily unavailable. Please try again later or contact support."
- **Page not found:** Display a message: "This page doesn't exist. Return to the Customer Support Portal."

---

## Permission Roles

| Role | Access | Notes |
|------|--------|-------|
| Unauthenticated User | ✅ Full access | Can view all content |
| Authenticated User | ✅ Full access | Can view all content + "Sign In" redirects to account |
| Super User | ✅ Full access | Same as authenticated user |
| Admin | ✅ Full access | Same as authenticated user |

---

## Success Metrics

1. **Email Click-Through Rate:** Track clicks from welcome email to Welcome Center
2. **Sign In Conversion:** Track % of users who click "Sign In" after visiting Welcome Center
3. **Page Load Time:** Ensure < 2 seconds
4. **Error Rate:** Monitor for any errors on the page
5. **User Feedback:** Collect feedback on onboarding experience

---

## Implementation Timeline

| Phase | Duration | Deliverables |
|-------|----------|--------------|
| **Design** | 1-2 days | Wireframes, content outline |
| **Development** | 2-3 days | Controller action, view, email template update |
| **Testing** | 1-2 days | Functional testing, security review, accessibility audit |
| **Deployment** | 1 day | Deploy to staging, then production |
| **Monitoring** | Ongoing | Track metrics, gather feedback |

**Total:** ~1 week

---

## Dependencies

- **Design:** Confirm content and layout with design team
- **Content:** Finalize onboarding content with product team
- **Email:** Coordinate with email template management system
- **Analytics:** Ensure tracking is implemented

---

## Risks & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Unauthenticated page exposes sensitive data | Low | High | Security review before deployment |
| Email template not updated | Medium | High | Add to deployment checklist |
| Users confused by generic content | Medium | Medium | A/B test content, gather feedback |
| Page performance issues | Low | Medium | Load test before deployment |

---

## Open Questions

1. **Content:** What specific onboarding content should be on the Welcome Center?
2. **Design:** Should it match the existing CyberArk onboarding design?
3. **Analytics:** What metrics are most important to track?
4. **Localization:** Should the page support multiple languages?
5. **Mobile:** Any mobile-specific considerations?

---

## Appendix: Current Code References

### OnBoarding Controller (Current)
**File:** `Csp.Web/Controllers/SupportAccountController.cs` (line 2258)
```csharp
[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
public ActionResult Onboarding(long id)
{
    return View("../CyberArk/Index");
}
```

### Email Template Pattern
**File:** `Csp.Api/HelperClasses/EmailHelper.cs` (line 528)
```csharp
and click on the <a href='https://support.paloaltonetworks.com/SupportAccount/AccountUsers/{3}'>Members</a> tab.
```

### CyberArk View (Current)
**File:** `Csp.Web/Views/CyberArk/Index.cshtml`
```html
@{
    Layout = "~/Views/Shared/_SiteLayout.cshtml";
    ViewBag.Title = "CyberArk Onboarding - Customer Support - Palo Alto Networks";
}
<div id="cyberark-onboarding-container-home"></div>
```

---

## Sign-Off

- **Product Manager:** _______________  Date: _______
- **Engineering Lead:** _______________  Date: _______
- **Design Lead:** _______________  Date: _______
- **Security Review:** _______________  Date: _______

