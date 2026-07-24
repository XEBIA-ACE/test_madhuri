# Functional Specification: Start Single Sign-On from HRIS Login Page

## User Story
As an employee, I want to be able to log in to the HRIS using Single Sign-On (SSO) mechanisms so that I can access my HRIS account without remembering separate login credentials.

## Acceptance Criteria
- A "Sign in with SSO" button is available on the HRIS login page.
- Initiating SSO follows environment-configured identity provider settings.
- Only pre-registered redirect URIs are used in the authentication flow.
- Security vulnerabilities such as Open Redirect are mitigated.
- The authentication request is performed correctly according to the identity provider's specifications.

## Out-of-Scope
- Support for multiple identity providers that are not pre-configured.
- Custom redirect URIs not listed in environment settings.

## Cross-Service Dependencies
- Integration depends on the correct configuration of identity provider settings within environment variables.
- Requires coordination with IT for identity provider details and testing.